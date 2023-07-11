using kvdt_kiosk.Models;
using kvdt_kiosk.Views.Kvdt.SeatingScreen.UC;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt.SeatingScreen
{
    public partial class SeatScreen : UserControl
    {
        private SeatingTitleUc seatingTitle;
        private SeatAdultUc seatAdult;
        private SeatingResetUc seatingReset;
        private SeatChildUc seatChild;
        private SeatSeniorCitizenUc seatSeniorCitizen;

        private AmountSelectedTicketUC seatAmount;

        private CancelOkButtonUc cancelOkButton;

        List<KeyValuePair<string, int>> SeatTypeAmountPairs = new List<KeyValuePair<string, int>>();

        List<Border> selectedBorders = new List<Border>();



        public SeatScreen()
        {
            InitializeComponent();
            InitializeChildUserControls();
        }

        private void InitializeChildUserControls()
        {
            // Create and initialize child user controls
            seatingTitle = new SeatingTitleUc { Text = UserSession.JourneyType };
            MainTop1.Children.Add(seatingTitle);

            seatingReset = new SeatingResetUc();
            seatingReset.ResetButtonClicked += Reset_Click;
            MainTop2.Children.Add(seatingReset);

            seatAdult = new SeatAdultUc();
            seatAdult.SeatButtonClicked += SeatButton_Click;
            MainTop3_Adult.Children.Add(seatAdult);


            seatChild = new SeatChildUc();
            seatChild.SeatButtonClicked += SeatButton_Click;
            MainTop3_Child.Children.Add(seatChild);

            seatSeniorCitizen = new SeatSeniorCitizenUc();
            seatSeniorCitizen.SeatButtonClicked += SeatButton_Click;
            MainTop3_SeniorCitizen.Children.Add(seatSeniorCitizen);

            seatAmount = new AmountSelectedTicketUC();
            MainTop4.Children.Add(seatAmount);

            cancelOkButton = new CancelOkButtonUc();
            MainTop5.Children.Add(cancelOkButton);



        }


        public void Reset_Click(object sender, EventArgs e)
        {
            seatAdult.Reset_Clicked();
            seatChild.Reset_Clicked();
            seatSeniorCitizen.Reset_Clicked();

            SeatTypeAmountPairs.Clear();

            seatAmount.UpdateAmountTicket("0");
        }

        private void SeatButton_Click(object sender, EventArgs e)
        {
            Button seatButton = (Button)sender;

            if (seatButton != null)
            {
                Border lowerParent = seatButton.Parent as Border;


                if (lowerParent != null)
                {
                    Grid upperParent = lowerParent.Parent as Grid;
                    var upperLevelName = upperParent.Name;


                    if (upperLevelName != null)
                    {
                        int existingIndex = SeatTypeAmountPairs.FindIndex(pair => pair.Key == upperLevelName);

                        if (existingIndex != -1)
                        {
                            if (int.TryParse(seatButton.Content.ToString(), out int seatValue))
                            {
                                SeatTypeAmountPairs[existingIndex] = new KeyValuePair<string, int>(upperLevelName, seatValue);
                            }
                        }
                        else
                        {
                            if (int.TryParse(seatButton.Content.ToString(), out int seatValue))
                            {
                                SeatTypeAmountPairs.Add(new KeyValuePair<string, int>(upperLevelName, seatValue));

                            }
                        }


                    }

                }



            }


            int totalSeat = 0;
            foreach (KeyValuePair<string, int> pair in SeatTypeAmountPairs)
            {
                totalSeat += pair.Value;
            }

            UpdateTotalSelectedTicket(totalSeat);

        }


        private void UpdateTotalSelectedTicket(int totalSeat)
        {
            seatAmount.UpdateAmountTicket(totalSeat.ToString());
        }
    }
}
