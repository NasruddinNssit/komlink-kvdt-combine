using NPrinterCLib;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Printing;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Nippon.NP3611BD.OrgAPI
{
    public class NP3611BDPrinter : IDisposable 
    {
        private const string LogChannel = "PRINTER_NP3611BD_API";

        private static SemaphoreSlim _lock = new SemaphoreSlim(1);
        private static NP3611BDPrinter _printQueue = null;

        private string _printerName = null;
        private NClassLib _nipPrinterLib;

        private bool _checkStatus_PaperLow = true;

        public static NP3611BDPrinter GetPrintQueue(string printerName, bool checkPrinterPaperLowState = true)
        {
            if (_printQueue != null)
                return _printQueue;

            //--------------------------------------------------------------------------------
            // Printer Name Verification
            if (string.IsNullOrWhiteSpace(printerName))
            {
                NssIT.Kiosk.Log.DB.DbLog.GetDbLog().LogText(LogChannel, "*", $@"Blank/Null parameter Printer Name", "A02", "NP3611BDPrinter.GetPrintQueue");
                printerName = AutoDetectDefaultPrinterName();
                NssIT.Kiosk.Log.DB.DbLog.GetDbLog().LogText(LogChannel, "*", $@"Detected Default Printer : {printerName}", "A03", "NP3611BDPrinter.GetPrintQueue");
            }
            else
            {
                NssIT.Kiosk.Log.DB.DbLog.GetDbLog().LogText(LogChannel, "*", $@"printerName : {printerName}", "A07", "NP3611BDPrinter.GetPrintQueue");

                if (IsRightPrinter(printerName) == false)
                {
                    throw new Exception("Wrong printer driver refer to parameter");
                }
            }

            if (string.IsNullOrWhiteSpace(printerName) == true)
                throw new Exception("Invalid Printer Name specification;") ;
            //--------------------------------------------------------------------------------
            // Create Instant of  NP3611BDPrinter.
            try
            {
                _lock.WaitAsync().Wait();

                if (_printQueue != null)
                    return _printQueue;

                _printQueue = new NP3611BDPrinter(printerName, checkPrinterPaperLowState);

                return _printQueue;
            }
            catch (Exception ex)
            {
                Exception ex2 = new Exception($@"Error create printer instance; (EXIT1050000101); {ex.Message}", ex);
                NssIT.Kiosk.Log.DB.DbLog.GetDbLog().LogError(LogChannel, "*", ex2, "EX01", "NP3611BDPrinter.GetPrintQueue");
                throw ex2;
            }
            finally
            {
                if (_lock.CurrentCount == 0)
                    _lock.Release();
            }

            return null;
        }

        public static bool IsInstanceAlreadyCreated
        {
            get
            {
                return (_printQueue is NP3611BDPrinter);
            }
        }

        /// <summary>
        /// Return instance (NP3611BDPrinter) that already created; Else null.
        /// </summary>
        public static NP3611BDPrinter Instance
        {
            get
            {
                return _printQueue;
            }
        }

        private static bool IsRightPrinter(string printerNameX)
        {
            PrintServer psvr = new PrintServer();
            psvr.Refresh();
            PrintQueueCollection pqColl = psvr.GetPrintQueues();

            string prtName = (printerNameX ?? "").Trim();
            foreach (PrintQueue pq3 in pqColl)
            {
                if (pq3.FullName?.ToString().Equals(prtName, StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    if (pq3.QueueDriver?.Name?.ToString().Equals("NPI Integration Driver", StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        return true;
                    }
                    else if (pq3.QueueDriver?.Name?.ToString().Equals("NPI NP Series Integration Driver", StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string AutoDetectDefaultPrinterName()
        {
            string retPrtName = null;

            LocalPrintServer localPrintServer = new LocalPrintServer();
            PrintQueue pq5 = localPrintServer.DefaultPrintQueue;

            if (pq5 != null)
            {
                retPrtName = pq5.FullName;

                NssIT.Kiosk.Log.DB.DbLog.GetDbLog().LogText(LogChannel, "*", $@"OS Default Printer : {retPrtName}", "A02", "NP3611BDPrinter.AutoDetectDefaultPrinterName");

                if (IsRightPrinter(retPrtName) == false)
                {
                    throw new Exception("Wrong printer driver refer to OS Default Printer setting");
                }
            }
            else
                throw new Exception("No default printer found in operation system");

            return retPrtName;
        }

        private NP3611BDPrinter(string printerName, bool checkStatus_PaperLow)
        {
            _printerName = printerName.Trim();
            _checkStatus_PaperLow = checkStatus_PaperLow;
            _nipPrinterLib = new NClassLib();
        }

        public string PrinterName => _printerName;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="returnStatus"></param>
        /// <param name="binaryStatus"></param>
        /// <param name="isPrinterError"></param>
        /// <param name="isPrinterWarning"></param>
        /// <param name="standardStatusDescription"></param>
        /// <param name="localStatusDescription"></param>
        public void GetPrinterStatus(out bool isPrinterError, out bool isPrinterWarning, out string standardStatusDescription, out string localStatusDescription)
        {
            string binaryStatus = "11111111";

            standardStatusDescription = "";
            localStatusDescription = "";
            isPrinterError = false;
            isPrinterWarning = false;

            int retVal = _nipPrinterLib.NGetStatus(_printerName, out long status);

            localStatusDescription = $@"NClassLib.NGetStatus => Return Value : {retVal}; Status Value : {status}; ";

            if (retVal == 0)
            {
                if (status == 0)
                {
                    isPrinterError = false;
                    isPrinterWarning = false;
                    standardStatusDescription = "";
                }
                else if (status < 0)
                {
                    isPrinterError = true;
                    isPrinterWarning = true;

                    standardStatusDescription = $@"Printer error ({status}); ";
                    localStatusDescription += $@"Printer error ({status}); ";
                }

                var binString = Convert.ToString(status, 2).Trim();

                if (binString.Length <= 8)
                    binString = binString.PadLeft(8, '0');
                else
                    binString = binString.Substring(binString.Length - 8);

                binaryStatus = binString;
                standardStatusDescription += GetStatusDescription(binaryStatus, out bool isPrtErr, out bool isPrtWarnX);
                localStatusDescription = $@"{localStatusDescription} Binary Status : {binaryStatus}; Status Description : {standardStatusDescription}";

                if (isPrinterError == false)
                    isPrinterError = isPrtErr;

                if (isPrinterWarning == false)
                    isPrinterWarning = isPrtWarnX;
            }
            
            else
            {
                isPrinterError = true;
                isPrinterWarning = true;

                if (retVal == -5)
                {
                    standardStatusDescription = $@"Printer offine";
                    localStatusDescription += $@"Printer offine";

                    //Below case will not occur since creation of instance must check printer driver
                    if (status == 0)
                    {
                        standardStatusDescription += $@"; Suspect printer setting; Wrong printer driver; Please check printer setting and parameter setting";
                        localStatusDescription += $@"; Suspect printer setting; Wrong printer driver; Please check printer setting and parameter setting";
                    }
                }
                else if (retVal < -100)
                {
                    standardStatusDescription = $@"Printer service error (E{status}#{retVal})";
                    localStatusDescription += $@"Printer service error (E{status}#{retVal})";
                }
                else if (retVal < 0)
                {
                    standardStatusDescription = $@"Printer communication error (E{status}#{retVal})";
                    localStatusDescription += $@"Printer communication error (E{status}#{retVal})";
                }
                else
                {
                    standardStatusDescription = $@"Printer connection error (E{status}#{retVal})";
                    localStatusDescription += $@"Printer connection error (E{status}#{retVal})";
                }
            }

            //if ((isPrinterError == false) && (isPrinterWarning == false))
            //{
            //    PrintServer psvr = new PrintServer();
            //    PrintQueueCollection pqColl = psvr.GetPrintQueues();

            //    foreach (PrintQueue pq3 in pqColl)
            //    {
            //        if (pq3.FullName?.ToString().Equals(_printerName,StringComparison.InvariantCultureIgnoreCase) == true)
            //        {
            //            return;
            //        }
            //    }

            //    isPrinterError = true;
            //    isPrinterWarning = true;
            //    standardStatusDescription = $@"Selected printer not found in operation system";
            //    localStatusDescription = $@"Selected printer not found in operation system";
            //}

            return;
            ////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            ///<summary>
            ///</summary>
            /// <param name="binaryStatusX">8 chars binary string</param>
            string GetStatusDescription(string binaryStatusX, out bool isPrinterErrorX, out bool isPrinterWarningX)
            {
                isPrinterErrorX = false;
                isPrinterWarningX = false;
                string retStateDesc = "";

                //if (binaryStatusX.Substring(0, 1).Equals("1"))
                //{
                //    isPrinterErrorX = true;
                //    isPrinterWarningX = true;
                //    //retStateDesc += "Print Start Status (C7);";
                //    retStateDesc += (retStateDesc.Length > 0 ? "; " : "") + "Printer Other Error (C7)";
                //}
                //if (binaryStatusX.Substring(1, 1).Equals("1"))
                //{
                //    isPrinterErrorX = true;
                //    isPrinterWarningX = true;
                //    retStateDesc += (retStateDesc.Length > 0 ? "; " : "") + "Printer Other Error (C6)";
                //}
                //if (binaryStatusX.Substring(2, 1).Equals("1"))
                //{
                //    isPrinterErrorX = true;
                //    isPrinterWarningX = true;
                //    retStateDesc += (retStateDesc.Length > 0 ? "; " : "") + "Printer Other Error (C5)";
                //}
                //if (binaryStatusX.Substring(3, 1).Equals("1"))
                //{
                //    isPrinterErrorX = true;
                //    isPrinterWarningX = true;
                //    retStateDesc += (retStateDesc.Length > 0 ? "; " : "") + "Printer Other Error (C4)";
                //}
                if (binaryStatusX.Substring(4, 1).Equals("1"))
                {
                    isPrinterErrorX = true;
                    isPrinterWarningX = true;
                    retStateDesc += (retStateDesc.Length > 0 ? "; " : "") + "Print Head Temp. Error";
                }
                if (binaryStatusX.Substring(5, 1).Equals("1"))
                {
                    isPrinterErrorX = true;
                    isPrinterWarningX = true;
                    retStateDesc += (retStateDesc.Length > 0 ? "; " : "") + "No Paper";
                }
                if (binaryStatusX.Substring(6, 1).Equals("1"))
                {
                    isPrinterErrorX = true;
                    isPrinterWarningX = true;
                    retStateDesc += (retStateDesc.Length > 0 ? "; " : "") + "Print Head Open";
                }
                if (binaryStatusX.Substring(7, 1).Equals("1") && (binaryStatusX.Substring(5, 1).Equals("1") == false) && (_checkStatus_PaperLow == true))
                {
                    int lowPaperCount = 0;

                    // Below looping used to confirm printer is paper low.
                    for(int cnt = 0; cnt < 10; cnt++)
                    {
                        Thread.Sleep(300);

                        int retValX = _nipPrinterLib.NGetStatus(_printerName, out long statusX);

                        string binStringX = Convert.ToString(statusX, 2).Trim();

                        if (binStringX.Length <= 8)
                            binStringX = binStringX.PadLeft(8, '0');
                        else
                            binStringX = binStringX.Substring(binStringX.Length - 8);

                        if (binStringX.Substring(7, 1).Equals("1"))
                            lowPaperCount++;
                    }
                    //-----------------------------------------------------------------------------
                    if (lowPaperCount >= 10)
                    {
                        isPrinterWarningX = true;
                        retStateDesc += (retStateDesc.Length > 0 ? "; " : "") + "Low Paper";
                    }
                }
                if (retStateDesc.Length == 0)
                {
                    retStateDesc = "Normal";
                }

                return retStateDesc;
            }
        }

        public void Dispose()
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_nipPrinterLib);
            }
            catch (Exception ex)
            {
                string tt1 = ex.Message;
            }
            finally 
            {
                _nipPrinterLib = null;
            }
            
            _printQueue = null;
        }

        //public bool IsPrinterPrinting
        //{
        //    get
        //    {
        //        string binaryStatus = "00000000";

        //        int retVal = _nipPrinterLib.NGetStatus(_printerName, out long status);

        //        if (retVal == 0)
        //        {
        //            if (status == 0)
        //            {
        //                var binString = Convert.ToString(status, 2).Trim();

        //                if (binString.Length <= 8)
        //                    binString = binString.PadLeft(8, '0');
        //                else
        //                    binString = binString.Substring(binString.Length - 8);

        //                binaryStatus = binString;

        //                if (binaryStatus.Substring(0, 1).Equals("1"))
        //                {
        //                    return true;
        //                }
        //            }
        //        }
        //        return false;
        //    }
        //}
    }
}
