using kvdt_kiosk.Models;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace kvdt_kiosk.Views.Kvdt.ReturnJourney
{
    /// <summary>
    /// Interaction logic for PassengerInfoTextbox.xaml
    /// </summary>
    public partial class ChildPassengerInfoTextbox : UserControl
    {
        public ChildPassengerInfoTextbox()
        {
            InitializeComponent();
            CheckData();
        }

        private void CheckData()
        {
            DispatcherTimer timer = new DispatcherTimer();

            timer.Tick += async (sender, e) =>
            {

                if (TxtPassengerName.Text == "Child Passenger 1")
                {
                    while (PassengerInfo.PassengerName == null || PassengerInfo.PassengerName == "")
                    {
                        await Task.Delay(100); break;
                    }
                    TxtPassengerName.Text = PassengerInfo.PassengerName;
                    PassengerInfo.PassengerName = "";


                }

                if (TxtPassengerName.Text == "Child Passenger 2")
                {
                    while (PassengerInfo.PassengerName != null || PassengerInfo.PassengerName != "")
                    {
                        await Task.Delay(100); break;
                    }
                    TxtPassengerName.Text = PassengerInfo.PassengerName;
                    PassengerInfo.PassengerName = "";


                }

                if (TxtPassengerName.Text == "Child Passenger 3")
                {
                    while (PassengerInfo.PassengerName != null || PassengerInfo.PassengerName != "")
                    {
                        await Task.Delay(5000);
                    }

                    TxtPassengerName.Text = PassengerInfo.PassengerName;
                    PassengerInfo.PassengerName = "";


                }

                if (TxtPassengerName.Text == "Child Passenger 4")
                {
                    while (PassengerInfo.PassengerName == null || PassengerInfo.PassengerName == "")
                    {
                        await Task.Delay(5000); break;
                    }

                    TxtPassengerName.Text = PassengerInfo.PassengerName;
                    PassengerInfo.PassengerName = "";

                    while (PassengerInfo.PassengerName != "")
                    {
                        await Task.Delay(50);
                    }
                }

                if (TxtPassengerName.Text == "Child Passenger 5")
                {
                    while (PassengerInfo.PassengerName == null || PassengerInfo.PassengerName == "")
                    {
                        await Task.Delay(100);
                    }

                    TxtPassengerName.Text = PassengerInfo.PassengerName;
                    PassengerInfo.PassengerName = "";


                    PassengerInfo.PassengerName = "";

                    while (PassengerInfo.PassengerName != "")
                    {
                        await Task.Delay(100);
                    }
                }

                if (TxtPassengerName.Text == "Child Passenger 6")
                {
                    while (PassengerInfo.PassengerName == null || PassengerInfo.PassengerName == "")
                    {
                        await Task.Delay(100);
                    }

                    TxtPassengerName.Text = PassengerInfo.PassengerName;
                    PassengerInfo.PassengerName = "";

                    while (PassengerInfo.PassengerName != "")
                    {
                        await Task.Delay(100);
                    }
                }

            };

            timer.Start();
        }

    }
}
