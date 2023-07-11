using kvdt_kiosk.Models;
using kvdt_kiosk.Services;
using NssIT.Train.Kiosk.Common.Constants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Parcel
{
    /// <summary>
    /// Interaction logic for ParcelScreen.xaml
    /// </summary>
    public partial class ParcelScreen : UserControl
    {
        public ParcelScreen()
        {
            InitializeComponent();
            DateTime dateTime = DateTime.Now;

            string date = dateTime.ToString("dd MMM yyyy");
            lblDate.Text = date;

            SetupParcel();

            LoadLanguage();
        }

        private void SetupParcel()
        {
            //for (int i = 0; i < 2; i++)
            //{
            //    RowDefinition row = new RowDefinition();
            //    ParcelContainer.RowDefinitions.Add(row);
            //}


            //for (int i = 0; i < 2; i++)
            //{
            //    Grid parcelGrid = new Grid();
            //    Parcel parcel = new Parcel();


            //    Grid.SetRow(parcelGrid, i);

            //    parcelGrid.Children.Add(parcel);

            //    ParcelContainer.Children.Add(parcelGrid);
            //}


            APIServices aPIServices = new APIServices(new APIURLServices(), new APISignatureServices());

            var result = aPIServices.GetAFCAddOn(UserSession.AFCService).Result;

            if (!(result?.Data?.Count > 0)) return;
            foreach (var parcel in result.Data.Select(item => new Parcel
            {
                ParcelName =
                         {
                             Text = item.AddOnName
                         },
                ParcelPrice =
                         {
                             Text = "RM" + item.UnitPrice.ToString(CultureInfo.InvariantCulture)
                         },
                ParcelId =
                         {
                             Text = item.AddOnId
                         }
            }))
            {
                ParcelContainer.Rows = ParcelContainer.Rows + 1;

                ParcelContainer.Children.Add(parcel);
            }

        }

        private void Main_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;
        }

        private async void RequestBooking()
        {

            try
            {
                var apiService = new APIServices(new APIURLServices(), new APISignatureServices());


                AFCBookingModel bookingModel = new AFCBookingModel();

                bookingModel.MCounters_Id = UserSession.MCounters_Id;
                bookingModel.CounterUserId = null; // null
                bookingModel.HandheldUserId = null; //null
                bookingModel.AFCServiceHeaders_Id = UserSession.AFCService; //session
                bookingModel.Channel = "TVM";
                bookingModel.PurchaseStationId = UserSession.PurchaseStationId;
                bookingModel.PackageJourney = UserSession.JourneyTypeId;
                bookingModel.From_MStations_Id = UserSession.FromStationId;
                bookingModel.To_MStations_Id = UserSession.ToStationId;

                List<AFCBookingDetailModel> bookingDetails = new List<AFCBookingDetailModel>();
                List<AFCBookingAddOnDetailModel> parcels = new List<AFCBookingAddOnDetailModel>();

                foreach (TicketOrderType ticketOrderType in UserSession.TicketOrderTypes)
                {

                    for (int i = 0; i < ticketOrderType.NoOfPax; i++)
                    {
                        AFCBookingDetailModel aFCBookingDetail = new AFCBookingDetailModel();
                        aFCBookingDetail.TicketTypes_Id = ticketOrderType.TicketTypeId;
                        aFCBookingDetail.PassengerName = "";
                        aFCBookingDetail.PassengerIC = "";
                        aFCBookingDetail.PNR = "";
                        bookingDetails.Add(aFCBookingDetail);
                    }
                }


                bookingModel.AFCBookingDetailModels = bookingDetails;

                if (UserSession.IsParcelCheckOut)
                {
                    if (UserSession.UserAddons != null)
                    {
                        foreach (UserAddon userAddon in UserSession.UserAddons)
                        {
                            for (int i = 0; i < userAddon.AddOnCount; i++)
                            {
                                AFCBookingAddOnDetailModel aFCBookingAddOnDetailModel = new AFCBookingAddOnDetailModel();
                                aFCBookingAddOnDetailModel.AFCAddOns_Id = userAddon.AddOnId;
                                parcels.Add(aFCBookingAddOnDetailModel);
                            }
                        }

                        bookingModel.AFCBookingAddOnDetailModels = parcels;
                    }
                }
                else
                {
                    bookingModel.AFCBookingAddOnDetailModels = null;

                }

                var result = await apiService.RequestAFCBooking(bookingModel);


                if (result.Error == YesNo.No)
                {
                    UserSession.UpdateAFCBookingResultModel = result;
                }




            }
            catch (Exception ex)
            {

            }

        }

        private async void BtnSkip_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;
            BtnSkip.Content = "Please Wait...";
            BtnSkip.IsEnabled = false;

            await Task.Delay(500);

            //Dispatcher.BeginInvoke((System.Action)(() =>
            //{
            //    UserSession.IsCheckOut = true;

            //    Window parentWindow = Window.GetWindow(this);
            //    parentWindow.Effect = null;
            //    parentWindow.Opacity = 1;
            //    parentWindow.Close();


            //    RequestBooking();
            //}));

            BtnSkip.Content = "Skip";

            UserSession.IsCheckOut = true;
            RequestBooking();

            BtnSkip.IsEnabled = true;
        }

        private async void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;

            BtnOk.Content = "Please Wait...";
            BtnOk.IsEnabled = false;

            await Task.Delay(500);

            if (UserSession.UserAddons == null)
            {
                //MessageBox.Show("Please select at least one parcel", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //return;
                UserSession.UserAddons = null;
            }

            UserSession.IsParcelCheckOut = true;
            UserSession.IsCheckOut = true;
            //Window parentWindow = Window.GetWindow(this);
            //parentWindow.Effect = null;
            //parentWindow.Opacity = 1;
            //parentWindow.Close();           

            BtnOk.Content = "OK";

            RequestBooking();

            BtnOk.IsEnabled = true;
        }

        private void LoadLanguage()
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (App.Language == "ms")
                {
                    BtnOk.Content = "OK";
                    BtnSkip.Content = "Langkau";
                }

            });
        }

        private void Grid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;
        }
    }
}