using kvdt_kiosk.Services;
using System;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt.Parcel
{
    /// <summary>
    /// Interaction logic for ParcelScreen.xaml
    /// </summary>
    public partial class ParcelScreen : UserControl
    {
        ParcelActionButton parcelActionButton;
        public string KvdtServices { get; set; } = "KOMUTERKV";
        public ParcelScreen()
        {
            InitializeComponent();

            parcelActionButton = new ParcelActionButton();
            ParcelActionButtonGrid.Children.Add(parcelActionButton);
            DateTime dateTime = DateTime.Now;

            DateNow.Text = dateTime.ToString("ddd, dd-MM");

            SetupParcel();
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

            var result = aPIServices.GetAFCAddOn(KvdtServices).Result;

            if (result?.Data?.Count > 0)
            {
                foreach (var item in result.Data)
                {
                    Parcel parcel = new Parcel();
                    parcel.ParcelName.Text = item.AddOnName;
                    parcel.ParcelPrice.Text = item.UnitPrice.ToString("C2");

                    ParcelContainer.Children.Add(parcel);
                }
            }

        }

    }
}
