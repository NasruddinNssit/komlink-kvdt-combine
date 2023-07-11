using NssIT.Kiosk.Log.DB.MarkingLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WFmMarkingLogTest.TestLib;

namespace WFmMarkingLogTest
{
    public partial class Form1 : Form
    {
        private LogTest01 _testLogA = null;
        private LogTest01 _testLogB = null;
        private LibShowMessageWindow.MessageWindow _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;

        public Form1()
        {
            InitializeComponent();

            _testLogA = new LogTest01("testLogA", intervalLogReqSec: 10, minDataCountOnLog: 5, maxIntervalLogReqMinutes: 1, validLogIntervalSec: 0);
            _testLogB = new LogTest01("testLogB", intervalLogReqSec: 10, minDataCountOnLog: 5, maxIntervalLogReqMinutes: 1, validLogIntervalSec: 2);
        }

        private void btnAMark01_Click(object sender, EventArgs e)
        {
            try
            {
                _msg.ShowMessage("Start Send M01");
                _testLogA.SendLog("M01");
                _msg.ShowMessage("End Send M01");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void btnAMark02_Click(object sender, EventArgs e)
        {
            try
            {
                _msg.ShowMessage("Start Send M02");
                _testLogA.SendLog("M02");
                _msg.ShowMessage("End Send M02");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void btnAMark03_Click(object sender, EventArgs e)
        {
            try
            {
                _msg.ShowMessage("Start Send M03");
                _testLogA.SendLog("M03");
                _msg.ShowMessage("End Send M03");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void btnAMark04_Click(object sender, EventArgs e)
        {
            try
            {
                _msg.ShowMessage("Start Send M04");
                _testLogA.SendLog("M04");
                _msg.ShowMessage("End Send M04");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void btnAMark05_Click(object sender, EventArgs e)
        {
            try
            {
                _msg.ShowMessage("Start Send M05");
                _testLogA.SendLog("M05", isLogTriggered: true);
                _msg.ShowMessage("End Send M05");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void btnBMark01_Click(object sender, EventArgs e)
        {
            try
            {
                _msg.ShowMessage("Start Send B-M01");
                _testLogB.SendLog("BM01");
                _msg.ShowMessage("End Send B-M01");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void btnBMark02_Click(object sender, EventArgs e)
        {
            try
            {
                _msg.ShowMessage("Start Send B-M02");
                _testLogB.SendLog("BM02");
                _msg.ShowMessage("End Send B-M02");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void btnBMark03_Click(object sender, EventArgs e)
        {
            try
            {
                _msg.ShowMessage("Start Send B-M03");
                _testLogB.SendLog("BM03");
                _msg.ShowMessage("End Send B-M03");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void btnBMark04_Click(object sender, EventArgs e)
        {
            try
            {
                _msg.ShowMessage("Start Send B-M04");
                _testLogB.SendLog("BM04");
                _msg.ShowMessage("End Send B-M04");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void btnBMark05_Click(object sender, EventArgs e)
        {
            try
            {
                _msg.ShowMessage("Start Send B-M05");
                _testLogB.SendLog("BM05", isLogTriggered: true);
                _msg.ShowMessage("End Send B-M05");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void btnEndMarkingLog_Click(object sender, EventArgs e)
        {
            try
            {
                _msg.ShowMessage("------- Start BaseMarkingLog Dispose -----");
                _testLogA.SendOutstanding();
                _testLogB.SendOutstanding();
                Thread.Sleep(700);
                BaseMarkingLog.GetDbLog().Dispose();
                _msg.ShowMessage("------- End BaseMarkingLog Dispose -----");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void btnASendMultiple_Click(object sender, EventArgs e)
        {
            try
            {
                if (int.TryParse(txtASendMultiple.Text, out int rInt))
                {
                    if (rInt < 0)
                        throw new Exception("A - Interger number must bigger then 0");

                    else
                    {
                        _msg.ShowMessage("End SendMultiple - A");
                        _testLogA.SendMultiple(rInt, chkATriggerLog.Checked);
                        _msg.ShowMessage("End SendMultiple - B");
                    }
                }
                else
                    throw new Exception("A - Invalid interger number. Number must bigger then 0");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void btnBSendMultiple_Click(object sender, EventArgs e)
        {
            try
            {
                if (int.TryParse(txtBSendMultiple.Text, out int rInt))
                {
                    if (rInt < 0)
                        throw new Exception("B - Interger number must bigger then 0");

                    else
                    {
                        _msg.ShowMessage("Start SendMultiple - B");
                        _testLogB.SendMultiple(rInt, chkBTriggerLog.Checked);
                        _msg.ShowMessage("End SendMultiple - B");
                    }
                }
                else
                    throw new Exception("B - Invalid interger number. Number must bigger then 0");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }
    }
}
