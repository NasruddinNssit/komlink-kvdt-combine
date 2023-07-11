using Komlink.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Komlink.Views
{
    /// <summary>
    /// Interaction logic for DateOption.xaml
    /// </summary>
    public partial class DateOption : UserControl
    {

        private Border selectedBorder;
        public event EventHandler DateRangeClicked;

        public event EventHandler Past30DayClickded;
        public event EventHandler Past1WeekClicked;
        public event EventHandler TodayClicked;
        public DateOption()
        {
            InitializeComponent();
            InitializeButtonEvent();

            LoadLanguage();
        }

        private void LoadLanguage()
        {
            try
            {
                if (App.Language == "ms")
                {
                    Btn_Today.Content = "Hari ini";
                    Btn_Past1Week.Content = "Minggu Lepas";
                    Btn_Past30Day.Content = "30 Hari lepas";
                    Btn_DateRange.Content = "Julat Tarikh";
                }

            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in BackButton.xaml.cs");

            }
        }


        private void InitializeButtonEvent()
        {
            List<Button> buttons = new List<Button> { Btn_Today, Btn_Past1Week, Btn_Past30Day, Btn_DateRange };

            foreach (Button button in buttons)
            {
                button.Click += Button_Clicked;
            }
        }

        private void Button_Clicked(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Border border = button.Parent as Border;

            if (selectedBorder != null)
            {
                SolidColorBrush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#074481"));

                selectedBorder.Background = brush;

            }

            if (border != null)
            {

                SolidColorBrush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#445A71"));
                border.Background = brush;
                selectedBorder = border;

            }
        }
        private void Btn_DateRange_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            DateRangeClicked?.Invoke(btn, EventArgs.Empty);
        }

        private void Btn_Today_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            TodayClicked?.Invoke(btn, EventArgs.Empty);
        }
        private void Btn_Past30Day_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            Past30DayClickded?.Invoke(btn, EventArgs.Empty);
        }
        private void Btn_Past1Week_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            Past1WeekClicked?.Invoke(btn, EventArgs.Empty);
        }
    }
}
