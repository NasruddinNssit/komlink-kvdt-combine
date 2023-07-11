using Komlink.Models;
using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Komlink.Views
{
    /// <summary>
    /// Interaction logic for CardDetail.xaml
    /// </summary>
    public partial class CardDetail : UserControl
    {

        KomlinkCardDetailModel _model;

        public CardDetail()
        {
            InitializeComponent();

            LoadLanguage();


        }

        private void LoadLanguage()
        {
            try
            {
                if (App.Language == "ms")
                {
                    NameText.Text = "Nama";
                    CardTypeText.Text = "Jenis Kad";
                    CardBalanceText.Text = "Baki Kad";
                    SeasonPassText.Text = "Pas Musim Aktif";

                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in CardDetail.xaml.cs");

            }
        }

        public void UpdateAmountAfterTopUP()
        {
            decimal currBalance = UserSession.CurrentUserSession.CardBalance.Value;
            decimal balanceAfter = UserSession.TotalTicketPrice + currBalance;
            UserSession.CurrentUserSession.CardBalance = balanceAfter;

            KomlinkCardBalance.Text = "MYR " + balanceAfter.ToString("F2");
        }

        private void SetKomlinkCardDetail()
        {
            SystemConfig.IsResetIdleTimer = true;

            if (_model != null)
            {
                KomlinkCardId.Text = _model.PNRNo;
                KomlinkCardName.Text = _model.Name;
                KomlinkCardType.Text = _model.TicketType;



                KomlinkCardBalance.Text = "MYR " + _model.CardBalance?.ToString("F2");

                WpnSeason.Children.Clear();

                if (_model.SeasonPasses.Count > 0)
                {



                    for (int i = 0; i < _model.SeasonPasses.Count; i++)
                    {
                        SeasonPass sessionPass = _model.SeasonPasses[i];

                        //Grid itemGrid = new Grid();
                        //Grid.SetColumn(itemGrid, i);
                        Pass pass = new Pass(sessionPass.SeasonPassId, sessionPass.ValidFrom.ToString("d MMM yy"), sessionPass.ValidTo.ToString("d MMM yy"));
                        //itemGrid.Children.Add(pass);

                        //PassSessionGrid.Children.Add(itemGrid);

                        WpnSeason.Children.Add(pass);
                    }



                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _model = UserSession.CurrentUserSession;

            SetKomlinkCardDetail();
        }
    }
}
