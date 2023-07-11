using kvdt_kiosk.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Parcel
{
    /// <summary>
    /// Interaction logic for Parcel.xaml
    /// </summary>
    public partial class Parcel : UserControl
    {
        int parcelQuantiy;

        public Parcel()
        {
            InitializeComponent();
            DisableButtonUponCheckOut();
        }

        private async void DisableButtonUponCheckOut()
        {
            while (!UserSession.IsParcelCheckOut)
            {
                await System.Threading.Tasks.Task.Delay(50);
            }
            BtnMinus.IsEnabled = false;
            BtnPlus.IsEnabled = false;
        }


        private void Btn_Plus_Clicked(object sender, RoutedEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;

            parcelQuantiy++;
            SetParcelQuantityAsync();
        }
        private void Btn_Minus_Clicked(object sender, RoutedEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;

            if (parcelQuantiy > 0)
            {
                parcelQuantiy--;
                SetParcelQuantityAsync();
            }

        }

        private async Task SetParcelQuantityAsync()
        {
            SystemConfig.IsResetIdleTimer = true;

            ParcelQuantityText.Text = parcelQuantiy.ToString();

            if (UserSession.UserAddons == null)
            {
                UserSession.UserAddons = new List<UserAddon>();
            }


            if (UserSession.UserAddons.Count > 0)
            {
                bool isExist = false;
                string price = ParcelPrice.Text;
                price = price.Replace("RM", "");

                foreach (var item in UserSession.UserAddons)
                {
                    if (item.AddOnId == ParcelId.Text)
                    {
                        item.AddOnName = ParcelName.Text;
                        item.AddOnCount = parcelQuantiy;
                        item.AddOnPrice = decimal.Parse(price);
                        isExist = true;
                    }
                }

                if (!isExist)
                {
                    UserSession.UserAddons.Add(new UserAddon()
                    {
                        AddOnId = ParcelId.Text,
                        AddOnName = ParcelName.Text,
                        AddOnCount = parcelQuantiy,
                        AddOnPrice = decimal.Parse(price),
                    });
                }

                if (parcelQuantiy == 0)
                {
                    foreach (var item in UserSession.UserAddons)
                    {
                        if (item.AddOnId == ParcelId.Text)
                        {
                            UserSession.UserAddons.Remove(item);
                            UserSession.isParcelHaveClicked = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                string price = ParcelPrice.Text;
                price = price.Replace("RM", "");

                UserSession.UserAddons.Add(new UserAddon()
                {
                    AddOnId = ParcelId.Text,
                    AddOnName = ParcelName.Text,
                    AddOnCount = parcelQuantiy,
                    AddOnPrice = decimal.Parse(price),

                });
                UserSession.isParcelHaveClicked = true;

            }

        }

        private void Grid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;
        }
    }
}
