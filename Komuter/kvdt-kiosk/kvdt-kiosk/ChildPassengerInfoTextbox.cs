using kvdt_kiosk.Models;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt.ReturnJourney
{
    /// <summary>
    /// Interaction logic for PassengerInfoTextbox.xaml
    /// </summary>
    public partial class ChildPassengerInfoTextbox : UserControl, INotifyPropertyChanged
    {
        public ChildPassengerInfoTextbox()
        {
            InitializeComponent();
            CheckData();
            CheckPassengerNameData();
        }

        private void CheckData()
        {

        }
        private void CheckPassengerNameData()
        {
            Timer timer = new Timer(2000); //2 seconds
            timer.Elapsed += (sender, e) =>
            {
                if (PassengerInfo.PassengerName != "")
                {
                    MessageBox.Show("Passenger Name: " + PassengerInfo.PassengerName);

                    TxtPassengerName.Text = PassengerInfo.PassengerName;
                    OnPropertyChanged(nameof(PassengerName));
                }

            };
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string PassengerName { get { return PassengerInfo.PassengerName; } set { PassengerInfo.PassengerName = value; } }

    }
}
