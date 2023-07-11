using kvdt_kiosk.Models.Komlink;
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

namespace kvdt_kiosk.Views.Komlink
{
    /// <summary>
    /// Interaction logic for PaymentAmountOption.xaml
    /// </summary>
    public partial class PaymentAmountOption : UserControl
    {
        public event EventHandler TextBoxClicked;

        PaymentOption PaymentOption;

        private Border selectedBorder;
        public PaymentAmountOption()
        {
            InitializeComponent();
            InitializeButtonEvents();

            LoadLanguage();
        }

        private void LoadLanguage()
        {
            try
            {
                if (App.Language == "ms")
                {
                    AmountRmText.Text = "Amaun (RM)";
                }

            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in PaymentAmountOption.xaml.cs");

            }
        }

        private void InitializeButtonEvents()
        {
            List<Button> buttons = new List<Button> { Btn1, Btn5, Btn10, Btn20, Btn50 };

            foreach (Button button in buttons)
            {
                button.Click += Button_Clicked;
            }
        }
        private void TextBox_Click(object sender, RoutedEventArgs e)
        {
            TextBoxClicked?.Invoke(this, e);
            SystemConfig.IsResetIdleTimer = true;
        }
        public void SetText(int value)
        {
            if (value == 0)
            {
                if (AmountText.Text.Length == 0)
                {
                    AmountText.Text = "";
                    return;
                }
            }

            AmountText.Text += value.ToString();
        }

        public void cancelKey()
        {
            AmountText.Text = "";
        }

        public void SetAllButtonUnSelected()
        {
            if (selectedBorder != null)
            {
                selectedBorder.Background = Brushes.White;
            }
        }

        public void SetTextAfterDelete(int value)
        {

            if (value == 0)
            {
                AmountText.Text = "";

                return;
            }

            AmountText.Text = value.ToString();
        }

        public void ClearText()
        {
            if (AmountText.Text != "")
            {
                AmountText.Text = "";
            }
        }

        public string GetText()
        {
            string digits = AmountText.Text; ;

            return digits;
        }

        public void ResetSelectedButton()
        {
            if (selectedBorder != null)
            {
                selectedBorder.Background = Brushes.White;
            }
        }

        private void Button_Clicked(object sender, RoutedEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;

            Button button = (Button)sender;
            Border border = button.Parent as Border;

            if (selectedBorder != null)
            {
                selectedBorder.Background = Brushes.White;
            }

            if (border != null)
            {

                SolidColorBrush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FBD012"));
                border.Background = brush;
                selectedBorder = border;
            }

            string value = button.Tag.ToString();

            AmountText.Text = value;

        }

    }
}
