using kvdt_kiosk.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace kvdt_kiosk.Views.Kvdt.SeatingScreen.UC
{
    /// <summary>
    /// Interaction logic for SeatSeniorCitizenUc.xaml
    /// </summary>
    /// 

    public partial class SeatSeniorCitizenUc : UserControl
    {

        private Border selectedBorder;
        public event EventHandler SeatButtonClicked;
        public SeatSeniorCitizenUc()
        {
            InitializeComponent();

            InitializeSeatButtonEvents();
        }

        private void InitializeSeatButtonEvents()
        {

            List<Button> buttonList = new List<Button> { seat_1, seat_2, seat_3, seat_4, seat_5, seat_6 };

            //foreach (Button seatButton in buttonList)
            //{
            //    seatButton.Click += Button_Click;
            //}

            foreach (var seatButton in buttonList)
            {
                seatButton.Click += (sender, args) =>
                {
                    if (selectedBorder != null)
                        selectedBorder.Background = Brushes.White;
                    if (seatButton.Parent != null && seatButton.Content != null)
                    {
                        selectedBorder = seatButton.Parent as Border;
                        UserSession.SeniorSeat = Convert.ToInt32(seatButton.Content);
                        if (selectedBorder != null)
                            selectedBorder.Background = Brushes.Yellow;
                        SeatButtonClicked?.Invoke(seatButton, EventArgs.Empty);
                    }
                };
            }
        }
        public void Reset_Clicked()
        {
            if (selectedBorder != null)
            {
                selectedBorder.Background = Brushes.White;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {


            Button button = (Button)sender;

            Border border = button.Parent as Border;


            if (selectedBorder != null)
            {
                selectedBorder.Background = Brushes.White;
            }

            if (border != null)
            {
                border.Background = Brushes.Yellow;
                selectedBorder = border;

            }

            SeatButtonClicked?.Invoke(button, EventArgs.Empty);


        }
    }


}
