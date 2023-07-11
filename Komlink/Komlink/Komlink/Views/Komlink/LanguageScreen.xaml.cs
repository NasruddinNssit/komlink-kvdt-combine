
using Komlink.Models;
using Microsoft.Reporting.WinForms;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using QRCoder;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;



namespace Komlink.Views.Komlink
{
    /// <summary>
    /// Interaction logic for LanguageScreen.xaml
    /// </summary>
    public partial class LanguageScreen : System.Windows.Controls.UserControl
    {
        private static List<Stream> m_streams;
        private static int m_currentPageIndex = 0;
        public LanguageScreen()
        {
            InitializeComponent();
            InfoMessage();
            //TestPrint();
            UserSession.SessionId = System.DateTime.Now.ToString("ddMMyyyyHHmmss");
            //PrintReport("C:\\Users\\moham\\source\\repos\\NSSIT\\KTMB_KTMBKiosk\\Komlink\\Komlink\\Komlink\\Reports\\TopUpKomlinkReceipt.rdlc");

            //PrintReportQrCode("C:\\Users\\moham\\source\\repos\\NSSIT\\KTMB_KTMBKiosk\\Komlink\\Komlink\\Komlink\\Reports\\Ticket.rdlc");
        }

        private void BtnEnglish_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dispatcher.InvokeAsync(() =>
                {
                    Window.GetWindow(this).Content = new KomlinkCardScanScreen();

                    Log.Logger.Information(UserSession.SessionId + " English Language Selected");
                });
            }
            catch (System.Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in LanguageScreen.xaml.cs");
            }
        }

        private void BtnMalay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dispatcher.InvokeAsync(() =>
                {
                    App.Language = "ms";
                    Window.GetWindow(this).Content = new KomlinkCardScanScreen();
                });
                Log.Logger.Information(UserSession.SessionId + " Malay Language Selected");

            }
            catch(Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in LanguageScreen.xaml.cs");

            }
        }


        private void InfoMessage()
        {
            var msg = "This application still under development and testing. You may experience some unexpected behaviour of the UI and services";

            TxtInfo.Text = msg;

            var blink = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new System.Windows.Duration(System.TimeSpan.FromSeconds(1)),
                AutoReverse = true,
                RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever
            };

            TxtInfo.BeginAnimation(System.Windows.Controls.TextBlock.OpacityProperty, blink);

        }



        public void PrintReportQrCode(string reportPath)
        {
            LocalReport report = new LocalReport();
            string path = reportPath;
            report.ReportPath = path;
            List<ReportParameter> reportParameters = new List<ReportParameter>();


            QRCodeGenerator qrGenerator = new QRCodeGenerator();

            // Generate the QR code with your desired content
            QRCodeData qrCodeData = qrGenerator.CreateQrCode("Your QR Code Content", QRCodeGenerator.ECCLevel.Q);

            // Create a QR code based on the QR code data
            QRCode qrCode = new QRCode(qrCodeData);

            // Convert the QR code to an image
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            string qrImage = Convert.ToBase64String(ImageToByteArray(qrCodeImage));

            reportParameters.Add(new ReportParameter("QrCode", qrImage));
            reportParameters.Add(new ReportParameter("Address", "KTMB Corporate Headquaters, Jalan Sultan Hishamuddin, 50621 Wilayah Persekutuan, kuala Lumpur"));
            reportParameters.Add(new ReportParameter("Destination", "BUTTERWOTH-KODIANG"));
            reportParameters.Add(new ReportParameter("Date", "02/05/2023"));
            reportParameters.Add(new ReportParameter("TicketType", "Adult"));
            reportParameters.Add(new ReportParameter("JourneyType", "Sehala"));
            reportParameters.Add(new ReportParameter("Fare", "MYR 9.10"));
            reportParameters.Add(new ReportParameter("DepartureDate", "02/05/2023"));
            reportParameters.Add(new ReportParameter("DepartureTime", "07:53PM"));
            reportParameters.Add(new ReportParameter("TransactionDate", "05/12/2022 02:55PM"));
            reportParameters.Add(new ReportParameter("CreationId", "KLGTVM01"));
            reportParameters.Add(new ReportParameter("Term", "Sila berada 30 minit sebelum tren berlepas\n Tertakluk kepada Akta PAD2010 & Syarat Pengangkutan Penumpang KTMB\n KTMB CALL CENTER +603-22671200"));
            reportParameters.Add(new ReportParameter("TransactionRefNumber", "KB212123123213**KT123123123"));
            reportParameters.Add(new ReportParameter("PaymentMethod", "Debit Card"));
            report.EnableExternalImages = true;

            report.SetParameters(reportParameters);
            PrintToPrinter(report);
        }

        private byte[] ImageToByteArray(System.Drawing.Image image)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }


        public void PrintReport(string reportPath)
        {
            LocalReport report = new LocalReport();
            string path = reportPath;
            report.ReportPath = path;
            //report.DataSources.Add(new ReportDataSource(dsName, ds.table[0]));
            //report.SetParameters(parameters);
            List<ReportParameter> reportParameters = new List<ReportParameter>();
            reportParameters.Add(new ReportParameter("TransactionType", "Komlink Card - Deduct"));
            reportParameters.Add(new ReportParameter("PaymentMethod", "Cash"));
            reportParameters.Add(new ReportParameter("TopUpDate", "02/02/2023"));
            reportParameters.Add(new ReportParameter("TransactionType", "Komlink Card-Top Up"));
            reportParameters.Add(new ReportParameter("KomlinkCardNumber", "KM1213123"));
            reportParameters.Add(new ReportParameter("TopUpAmount", "MYR 12.05"));
            reportParameters.Add(new ReportParameter("CardBalance", "MYR 12.05"));
            reportParameters.Add(new ReportParameter("Term", "Sila berada 30 minit sebelum tren berlepas\n Tertakluk kepada Akta PAD2010 & Syarat Pengangkutan Penumpang KTMB\n KTMB CALL CENTER +603-22671200"));
            reportParameters.Add(new ReportParameter("Address", "KTMB Corporate Headquaters, Jalan Sultan Hishamuddin, 50621 Wilayah Persekutuan, kuala Lumpur"));
            reportParameters.Add(new ReportParameter("TransactionDate", "02/02/2023"));
            report.SetParameters(reportParameters);
            PrintToPrinter(report);
            //========================================================
        }
            public static void PrintToPrinter(LocalReport report)
            {
                Export(report);

            }

            public static void Export(LocalReport report, bool print = true)
            {
                string deviceInfo =
                 @"<DeviceInfo>
                <OutputFormat>EMF</OutputFormat>
                <PageWidth>8in</PageWidth>
                <PageHeight>12in</PageHeight>
                <MarginTop>0in</MarginTop>
                <MarginLeft>0in</MarginLeft>
                <MarginRight>0in</MarginRight>
                <MarginBottom>0in</MarginBottom>
            </DeviceInfo>";
                Warning[] warnings;
                m_streams = new List<Stream>();
                report.Render("Image", deviceInfo, CreateStream, out warnings);
                foreach (Stream stream in m_streams)
                    stream.Position = 0;

                if (print)
                {
                    Print();
                }
            }


            public static void Print()
            {
                if (m_streams == null || m_streams.Count == 0)
                    throw new Exception("Error: no stream to print.");
                PrintDocument printDoc = new PrintDocument();
                if (!printDoc.PrinterSettings.IsValid)
                {
                    throw new Exception("Error: cannot find the default printer.");
                }
                else
                {
                    printDoc.PrintPage += new PrintPageEventHandler(PrintPage);
                    m_currentPageIndex = 0;
                    printDoc.Print();
                }
            }

            public static Stream CreateStream(string name, string fileNameExtension, Encoding encoding, string mimeType, bool willSeek)
            {
                Stream stream = new MemoryStream();
                m_streams.Add(stream);
                return stream;
            }

            public static void PrintPage(object sender, PrintPageEventArgs ev)
            {
                Metafile pageImage = new
                   Metafile(m_streams[m_currentPageIndex]);

                // Adjust rectangular area with printer margins.
                Rectangle adjustedRect = new Rectangle(
                    ev.PageBounds.Left - (int)ev.PageSettings.HardMarginX,
                    ev.PageBounds.Top - (int)ev.PageSettings.HardMarginY,
                    ev.PageBounds.Width,
                    ev.PageBounds.Height);

                // Draw a white background for the report
                ev.Graphics.FillRectangle(System.Drawing.Brushes.White, adjustedRect);

                // Draw the report content
                ev.Graphics.DrawImage(pageImage, adjustedRect);

                // Prepare for the next page. Make sure we haven't hit the end.
                m_currentPageIndex++;
            ev.HasMorePages = false;
            }

            public static void DisposePrint()
            {
                if (m_streams != null)
                {
                    foreach (Stream stream in m_streams)
                        stream.Close();
                    m_streams = null;
                }
            }
        }


}

