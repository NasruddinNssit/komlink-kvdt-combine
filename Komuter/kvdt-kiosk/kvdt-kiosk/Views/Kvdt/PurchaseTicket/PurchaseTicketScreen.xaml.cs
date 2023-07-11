using kvdt_kiosk.Models;
using kvdt_kiosk.Services;
using kvdt_kiosk.Views.Kvdt.SeatingScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace kvdt_kiosk.Views.Kvdt.PurchaseTicket
{
    /// <summary>
    /// Interaction logic for PurchaseTicketScreen.xaml
    /// </summary>
    public partial class PurchaseTicketScreen : UserControl
    {
        APIServices aPIServices = new APIServices(new APIURLServices(), new APISignatureServices());

        DestinationScreen destinationScreen = new DestinationScreen();
        AllRouteButton allRouteButton = new AllRouteButton();
        StationRouteButton stationRouteButton = new StationRouteButton();
        JourneyTypeButton JourneyTypeButton = new JourneyTypeButton();
        SeatScreen SeatScreen = new SeatScreen();

        public const string service = "KOMUTERKV";
        public List<string> RoutesId = new List<string>();

        public List<AFCStationDetails> listOfStations = new List<AFCStationDetails>();

        public PurchaseTicketScreen()
        {
            InitializeComponent();

            GridJourney.Children.Add(destinationScreen);

            CurrentDate();
            CurrentTime();

            GetAFCRoutes();
            GetAllStations(service);
        }

        public void CurrentTime()
        {
            TxtTime.Text = DateTime.Now.ToString("HH:mm:ss");

            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 1);
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)

        {
            TxtTime.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void CurrentDate()
        {
            TxtDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void GetAFCRoutes()
        {
            MyDispatcher.Invoke(new Action(() =>
            {
                var result = aPIServices.GetAFCStations(service).Result;

                if (result?.AFCRouteModels?.Count > 0)
                {
                    GridRoutesModel.Children.Clear();

                    foreach (var item in result.AFCRouteModels)
                    {
                        if (item.IsInterchange == "1")
                        {
                            allRouteButton = new AllRouteButton()
                            {
                                Margin = new Thickness(5, 3, 5, 3),
                                Padding = new Thickness(5, 3, 5, 3)
                            };

                            allRouteButton.TxtDescription.Text = item.Description;
                            GridRoutesModel.Children.Add(allRouteButton);

                        }
                        else
                        {
                            stationRouteButton = new StationRouteButton()
                            {
                                Margin = new Thickness(5, 3, 5, 3),
                                Padding = new Thickness(5, 3, 5, 3)
                            };

                            stationRouteButton.TxtDesription.Text = item.Description;
                            stationRouteButton.Style = Resources["BtnDefaultStation"] as Style;
                            stationRouteButton.BorderColor.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(item.ColorCode));

                            GridRoutesModel.Children.Add(stationRouteButton);
                            stationRouteButton.BtnRoute.Click += (s, e) =>
                            {
                                destinationScreen.txtDestination.TextWrapping = TextWrapping.WrapWithOverflow;
                                destinationScreen.TxtToStation.Text = "";

                                var routesId = result.AFCRouteModels.Select(x => x.Id).ToList();

                                GetStations(item.Id);
                            };
                        }

                        allRouteButton.Style = Resources["BtnDefaultAll"] as Style;
                        allRouteButton.BtnAll.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(item.ColorCode));
                    }

                    allRouteButton.BtnAll.Click += (s, e) =>
                    {
                        GetAllStations(service);
                    };
                }
            }));

        }

        private void GetAllStations(string service)
        {

            MyDispatcher.Invoke(new Action(() =>
            {
                GridStation.Children.Clear();
                TxtTextBox.Text = "";

                var result = aPIServices.GetAFCStations(service).Result;

                if (result.AFCRouteModels.Count > 0)
                {
                    foreach (var item in result.AFCRouteModels)
                    {
                        if (item.IsInterchange == "1")
                        {
                            var subStations = aPIServices.GetAFCStations(service).Result;

                            foreach (var station in subStations.AFCStationModels)
                            {
                                GenericStationButton genericStation = new GenericStationButton()
                                {
                                    lblStationName =
                                {
                                    Text = station.Station
                                },
                                    lblStationId =
                                {
                                    Text = station.Id
                                },
                                    StationColorCode =
                                {
                                    Background = new SolidColorBrush((Color)ColorConverter .ConvertFromString(station.ColorCode))
                                },
                                    Margin = new Thickness(5, 3, 5, 3),
                                    Padding = new Thickness(5, 3, 5, 3),
                                    Height = 60,
                                };

                                GridStation.Children.Add(genericStation);

                                genericStation.BtnGenericStation.Click += (s, e) =>
                                {

                                    destinationScreen.TxtToStation.Text = genericStation.lblStationName.Text;
                                    var bg = genericStation.GridBtnStation.Background;
                                    var brush = bg;

                                    foreach (var btnStation in GridStation.Children)
                                    {
                                        if (btnStation is GenericStationButton)
                                        {
                                            var btn = btnStation as GenericStationButton;
                                            var btnBg = btn?.GridBtnStation.Background;
                                            var btnBrush = btnBg;
                                            if (btnBrush?.ToString() == "#FFFBD012")
                                            {
                                                btn.GridBtnStation.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                                            }
                                        }
                                    }
                                    genericStation.GridBtnStation.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFBD012"));

                                    GetAFCPackage();

                                    GridJourneyButton.Visibility = Visibility.Visible;

                                    UserSession.FromStation = "TANJUNG MALIM";
                                    UserSession.ToStation = genericStation.lblStationName.Text;
                                };

                                listOfStations.Add(station);
                            }
                        }
                    }
                }
            }));
        }

        private void GetStations(string routeId)
        {
            MyDispatcher.Invoke(new Action(() =>
            {
                var dispatcher = Application.Current.Dispatcher;
                dispatcher.BeginInvoke(new Action(() =>
                {
                    GridStation.Children.Clear();
                    TxtTextBox.Text = "";

                    var result = aPIServices.GetAFCStations(service).Result;

                    foreach (var station in result.AFCStationModels)
                    {
                        if (station.RouteId[0].Equals(routeId) || station.IsInterchange == true)
                        {
                            GenericStationButton genericStation = new GenericStationButton()
                            {
                                lblStationName =
                                {
                                    Text = station.Station
                                },
                                lblStationId =
                                {
                                    Text = station.Id
                                },
                                StationColorCode =
                                {
                                    Background = new SolidColorBrush((Color)ColorConverter .ConvertFromString(station.ColorCode))
                                },
                                Margin = new Thickness(5, 3, 5, 3),
                                Padding = new Thickness(5, 3, 5, 3),
                                Height = 60,
                            };


                            GridStation.Children.Add(genericStation);

                            genericStation.BtnGenericStation.Click += (s, e) =>
                            {

                                destinationScreen.TxtToStation.Text = genericStation.lblStationName.Text;
                                var bg = genericStation.GridBtnStation.Background;
                                var brush = bg;

                                foreach (var btnStation in GridStation.Children)
                                {
                                    if (btnStation is GenericStationButton)
                                    {
                                        var btn = btnStation as GenericStationButton;
                                        var btnBg = btn?.GridBtnStation.Background;
                                        var btnBrush = btnBg;
                                        if (btnBrush?.ToString() == "#FFFBD012")
                                        {
                                            btn.GridBtnStation.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                                        }
                                    }
                                }
                                genericStation.GridBtnStation.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFBD012"));

                                GetAFCPackage();

                                GridJourneyButton.Visibility = Visibility.Visible;

                                UserSession.FromStation = "TANJUNG MALIM";
                                UserSession.ToStation = genericStation.lblStationName.Text;
                            };

                        }
                    }

                }));
            }));

        }

        private void GetAFCPackage()
        {
            MyDispatcher.Invoke(new Action(() =>
            {
                var result = aPIServices.GetAFCPackage().Result;
                GridJourneyButton1.Children.Clear();
                if (result.Data.Count > 0)
                {
                    foreach (var item in result.Data)
                    {
                        JourneyTypeButton = new JourneyTypeButton()
                        {
                            Margin = new Thickness(5, 3, 5, 3),
                            Padding = new Thickness(5, 3, 5, 3)
                        };

                        JourneyTypeButton.TxtJourney.Text = item.PackageName;
                        JourneyTypeButton.TxtJourneyDate.Text = DateTime.Now.ToString("ddd dd - MM");

                        GridJourneyButton1.Children.Add(JourneyTypeButton);
                    }
                }
            }));

        }

        public void FilterStationByKeyboardInput(string input)
        {

            MyDispatcher.Invoke(new Action(() =>
            {
                GridStation.Children.Clear();
                foreach (var item in listOfStations)
                {
                    if (item.Station.StartsWith(input))
                    {
                        GenericStationButton genericStation = new GenericStationButton()
                        {
                            lblStationName =
                {
                    Text = item.Station
                },
                            lblStationId =
                {
                    Text = item.Id
                },
                            StationColorCode =
                {
                    Background = new SolidColorBrush((Color)ColorConverter .ConvertFromString(item.ColorCode))
                },
                            Margin = new Thickness(5, 3, 5, 3),
                            Padding = new Thickness(5, 3, 5, 3),
                            Height = 60,
                        };

                        GridStation.Children.Add(genericStation);

                        genericStation.BtnGenericStation.Click += (s, e) =>
                        {
                            destinationScreen.TxtToStation.Text = genericStation.lblStationName.Text;
                            var bg = genericStation.GridBtnStation.Background;
                            var brush = bg;

                            foreach (var btnStation in GridStation.Children)
                            {
                                if (btnStation is GenericStationButton)
                                {
                                    var btn = btnStation as GenericStationButton;
                                    var btnBg = btn?.GridBtnStation.Background;
                                    var btnBrush = btnBg;
                                    if (btnBrush?.ToString() == "#FFFBD012")
                                    {
                                        btn.GridBtnStation.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                                    }
                                }
                            }
                            genericStation.GridBtnStation.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFBD012"));

                            GetAFCPackage();

                            GridJourneyButton.Visibility = Visibility.Visible;

                            UserSession.FromStation = "TANJUNG MALIM";
                            UserSession.ToStation = genericStation.lblStationName.Text;
                        };
                    }
                }
            }));
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            GridStation.Children.Clear();
            destinationScreen.TxtToStation.Text = "";
            GetAllStations(service);
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {

            var dispatcher = Application.Current.Dispatcher;

            dispatcher.BeginInvoke(new Action(() =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                Application.Current.Shutdown();
            }));

        }

        private void BtnViewMap_Click(object sender, RoutedEventArgs e)
        {

        }


        private void TxtTest_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void BtnA_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "A";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnS_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "S";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnD_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "D";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnF_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "F";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnG_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "G";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnH_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "H";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnJ_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "J";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnK_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "K";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnL_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "L";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnZ_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "Z";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnX_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "X";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnC_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "C";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnV_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "V";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnB_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "B";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnN_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "N";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnM_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtTextBox.Text += "M";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnQ_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnQ.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnQ_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnQ.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnW_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnW.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnW_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnW.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnE_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnE.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnE_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnE.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnR_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnR.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnR_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnR.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnT_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnT.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnT_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnT.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnY_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnY.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnY_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnY.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnU_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnU.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnU_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnU.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnI_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnI.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnI_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnI.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnO_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnO.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnO_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnO.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnP_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnP.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnP_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnP.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnA_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnA.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnA_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnA.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnS_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnS.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnS_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnS.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnD_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnD.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnD_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnD.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnF_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnF.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnF_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnF.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnG_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnG.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnG_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnG.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnH_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnH.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnH_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnH.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnJ_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnJ.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnJ_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnJ.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnK_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnK.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnK_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnK.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnL_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnL.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnL_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnL.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnZ_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnZ.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnZ_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnZ.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnX_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnX.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnX_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnX.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnC_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnC.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnC_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnC.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnV_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnV.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnV_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnV.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnB_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnB.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnB_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnB.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnN_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnN.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnN_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnN.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnM_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnM.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnM_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnM.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnClear_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnClear.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnClear_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnClear.Background = System.Windows.Media.Brushes.White;
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            GridStation.Children.Clear();
            TxtTextBox.Text = "";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnQ_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "Q";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnW_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "W";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnE_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "E";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnR_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "R";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnT_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "T";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnY_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "Y";
            FilterStationByKeyboardInput(TxtTextBox.Text);

        }

        private void BtnU_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "U";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnI_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "I";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnO_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "O";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }

        private void BtnP_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "P";
            FilterStationByKeyboardInput(TxtTextBox.Text);
        }
    }
}
