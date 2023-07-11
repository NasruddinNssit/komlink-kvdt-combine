using Komlink.Models;
using Serilog;
using System;
using System.Windows.Controls;

namespace Komlink.Views
{
    /// <summary>
    /// Interaction logic for TransactionDate.xaml
    /// </summary>
    public partial class TransactionDate : UserControl
    {


        DateOption DateOption = new DateOption();

        public event EventHandler DateRangeClicked;

        public event EventHandler Past30DayClicked;
        public event EventHandler Past1WeekClicked;
        public event EventHandler TodayClicked;
        public TransactionDate()
        {
            InitializeComponent();

            DateOptionGrid.Children.Add(DateOption);

            DateOption.DateRangeClicked += Btn_DateRangeClicked;
            DateOption.Past1WeekClicked += Btn_Past1WeekClicked;
            DateOption.Past30DayClickded += Btn_Past30DayClicked;
            DateOption.TodayClicked += Btn_TodayClicked;

            LoadLanguage();

        }

        private void LoadLanguage()
        {
            try
            {
                if (App.Language == "ms")
                {
                    DateFromText.Text = "Tarikh Dari";
                    DateToText.Text = "Tarikh Hingga";

                }

            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in TransactionDate.xaml.cs");

            }
        }

        private void Btn_DateRangeClicked(object sender, EventArgs e)
        {



            DateRangeClicked?.Invoke(this, EventArgs.Empty);
            SystemConfig.IsResetIdleTimer = true;
        }



        public void setStartDateText(string value)
        {
            StartDate.Text = value;
        }

        public void setEndDateText(string value)
        {
            EndDate.Text = value;
        }



        private void Btn_Past30DayClicked(object sender, EventArgs e)
        {

            Past30DayClicked?.Invoke(this, EventArgs.Empty);
            SystemConfig.IsResetIdleTimer = true;
        }
        private void Btn_Past1WeekClicked(object sender, EventArgs e)
        {

            Past1WeekClicked?.Invoke(this, EventArgs.Empty);
            SystemConfig.IsResetIdleTimer = true;
        }
        private void Btn_TodayClicked(object sender, EventArgs e)
        {

            TodayClicked?.Invoke(this, EventArgs.Empty);
            SystemConfig.IsResetIdleTimer = true;
        }


    }
}
