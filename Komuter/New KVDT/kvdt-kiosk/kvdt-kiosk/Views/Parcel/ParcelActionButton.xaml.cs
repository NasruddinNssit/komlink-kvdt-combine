
using kvdt_kiosk.Models;
using kvdt_kiosk.Services;
using NssIT.Train.Kiosk.Common.Constants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Parcel
{
    /// <summary>
    /// Interaction logic for ParcelActionButton.xaml
    /// </summary>
    public partial class ParcelActionButton : UserControl
    {
        public ParcelActionButton()
        {
            InitializeComponent();
            LoadLanguage();
        }

        private async void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            BtnOk.Content = "Please Wait...";
            BtnOk.IsEnabled = false;
            await Task.Delay(500);
            BtnOk.IsEnabled = true;

            SystemConfig.IsResetIdleTimer = true;
            RequestBooking();
        }

        private async void BtnSkip_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            BtnSkip.Content = "Please Wait...";
            BtnSkip.IsEnabled = false;
            await Task.Delay(500);
            BtnSkip.IsEnabled = true;

            SystemConfig.IsResetIdleTimer = true;
            UserSession.IsCheckOut = true;
            RequestBooking();
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
