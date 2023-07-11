using kvdt_kiosk.Models;
using kvdt_kiosk.Services;
using kvdt_kiosk.Views.Kvdt;
using System;
using System.IO;
using System.Windows;

namespace kvdt_kiosk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            LanguageScreen languageScreen = new LanguageScreen();

            MyDispatcher.Invoke(() =>
            {
                this.Content = languageScreen;
            });

            // OpenTestWindow();
        }

        private void OpenTestWindow()
        {
            TestWindow testWindow = new TestWindow();
            testWindow.Show();
        }

        private void GetConnString()
        {
            string connString = ConnectionString.GetConnectionString();

            MessageBox.Show(connString);
        }

        private void TempData()
        {
            UserSession.SessionId = DateTime.Now.ToString("ddMMyyyyhhmmss");

            //string fileName = UserSession.SessionId + ".txt";
            //string path = @"UserSessionLog\" + fileName;

            //string[] lines = { "User Session: Tanjung Malim " + UserSession.SessionId, "Date: " + DateTime.Now.ToString(), "", "", "" };

            //File.WriteAllLines(path, lines);


        }

        private void TempLogFolder()
        {
            if (!Directory.Exists("UserSessionLog"))
            {
                Directory.CreateDirectory("UserSessionLog");
            }
        }
    }
}
