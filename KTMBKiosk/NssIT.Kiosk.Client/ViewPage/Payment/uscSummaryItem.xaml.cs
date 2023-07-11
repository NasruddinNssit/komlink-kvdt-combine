using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NssIT.Kiosk.Client.ViewPage.Payment
{
    /// <summary>
    /// Interaction logic for uscSummaryItem.xaml
    /// </summary>
    public partial class uscSummaryItem : UserControl
    {
        public uscSummaryItem()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) { }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) { }

        /// <summary></summary>
        /// <param name="trainNoStr">Like "TRAIN 1"</param>
        /// <param name="tripDesc">Like "Kuala Lumpur > Gemas"</param>
        /// <param name="noOfTicketStr">Like "2 tickets"</param>
        /// <param name="addOnStr">Like "5 Add-Ons"</param>
        /// <param name="currency">Like "RM"</param>
        /// <param name="ticketAmount"></param>
        public void ShowSummaryItem(string trainNoStr, string tripDesc, string noOfTicketStr, string addOnStr, string currency, decimal ticketAmount, ResourceDictionary languageRes)
        {
            if (languageRes != null)
            {
                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(languageRes);
            }
            
            BdTrainNo.Visibility = Visibility.Visible;
            TxtTripDesc.Visibility = Visibility.Visible;
            TxtNoOfTicket.Visibility = Visibility.Visible;
            TxtAddOn.Visibility = Visibility.Visible;
            BdTicketAmt.Visibility = Visibility.Visible;

            HideBlankSpace();
            HidePromoDiscount();
            HideInsurance();
            HideGrossTotal();
            HidePaymentSummary();

            TxtTrainNo.Text = trainNoStr ?? "";
            TxtTripDesc.Text = tripDesc ?? "";
            TxtNoOfTicket.Text = noOfTicketStr ?? "";
            TxtAddOn.Text = addOnStr ?? "";
            TxtCurrency.Text = currency ?? "";
            TxtTicketAmt.Text = $@"{ticketAmount:#,###.00}";
        }


        private void HideSummaryItem()
        {
            BdTrainNo.Visibility = Visibility.Collapsed;
            TxtTripDesc.Visibility = Visibility.Collapsed;
            TxtNoOfTicket.Visibility = Visibility.Collapsed;
            TxtAddOn.Visibility = Visibility.Collapsed;
            BdTicketAmt.Visibility = Visibility.Collapsed;
        }

        public void ShowBlankSpace()
        {
            BdSpace.Visibility = Visibility.Visible;

            HideSummaryItem();
            HidePromoDiscount();
            HideInsurance();
            HideGrossTotal();
            HidePaymentSummary();
        }

        private void HideBlankSpace()
        {
            BdSpace.Visibility = Visibility.Collapsed;
        }

        public void ShowPromoDiscount(string currency, decimal promoDiscountAmount, ResourceDictionary languageRes)
        {
            if (languageRes != null)
            {
                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(languageRes);
            }

            TxtPromoDiscount.Visibility = Visibility.Visible;
            TxtPromoDiscountCurrency.Visibility = Visibility.Visible;
            BdPromoDiscount.Visibility = Visibility.Visible;

            HideSummaryItem();
            HideBlankSpace();
            HideInsurance();
            HideGrossTotal();
            HidePaymentSummary();

            TxtPromoDiscountCurrency.Text = "- " + currency;
            TxtPromoDiscountAmt.Text = $@"{promoDiscountAmount:#,###.00}";
        }

        public void HidePromoDiscount()
        {
            TxtPromoDiscount.Visibility = Visibility.Collapsed;
            TxtPromoDiscountCurrency.Visibility = Visibility.Collapsed;
            BdPromoDiscount.Visibility = Visibility.Collapsed;
        }

        public void ShowInsurance(string currency, decimal insuranceAmount, ResourceDictionary languageRes)
        {
            if (languageRes != null)
            {
                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(languageRes);
            }

            TxtInsurance.Visibility = Visibility.Visible;
            TxtInsuranceCurrency.Visibility = Visibility.Visible;
            BdInsurance.Visibility = Visibility.Visible;

            HideSummaryItem();
            HideBlankSpace();
            HidePromoDiscount();
            HideGrossTotal();
            HidePaymentSummary();

            TxtInsuranceCurrency.Text = currency;
            TxtInsuranceAmt.Text = $@"{insuranceAmount:#,###.00}";
        }

        public void HideInsurance()
        {
            TxtInsurance.Visibility = Visibility.Collapsed;
            TxtInsuranceCurrency.Visibility = Visibility.Collapsed;
            BdInsurance.Visibility = Visibility.Collapsed;
        }

        /// <summary></summary>
        /// <param name="totalNoOfTicketStr">Like "10 tickets"</param>
        /// <param name="currency"></param>
        /// <param name="totalTicketAmount"></param>
        public void ShowGrossTotal(string totalNoOfTicketStr, string currency, decimal totalTicketAmount, ResourceDictionary languageRes)
        {
            if (languageRes != null)
            {
                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(languageRes);
            }

            TxtTotalLabel.Visibility = Visibility.Visible;
            TxtTotalNoOfTicket.Visibility = Visibility.Visible;
            BdTotalTicketAmt.Visibility = Visibility.Visible;

            HideSummaryItem();
            HideBlankSpace();
            HidePromoDiscount();
            HideInsurance();
            HidePaymentSummary();

            TxtTotalNoOfTicket.Text = totalNoOfTicketStr ?? "";
            TxtTotalCurrency.Text = currency ?? "";
            TxtTotalTicketAmt.Text = $@"{totalTicketAmount:#,###.00}";
        }

        private void HideGrossTotal()
        {
            TxtTotalLabel.Visibility = Visibility.Collapsed;
            TxtTotalNoOfTicket.Visibility = Visibility.Collapsed;
            BdTotalTicketAmt.Visibility = Visibility.Collapsed;
        }

        public void ShowPaymentSummary(string currency, decimal paymentAmount, ResourceDictionary languageRes)
        {
            if (languageRes != null)
            {
                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(languageRes);
            }

            TxtPaySummLabel.Visibility = Visibility.Visible;
            BdPayAmount.Visibility = Visibility.Visible;

            HideSummaryItem();
            HideBlankSpace();
            HidePromoDiscount();
            HideInsurance();
            HideGrossTotal();

            TxtPayAmountCurrency.Text = currency;
            TxtPayAmount.Text = $@"{paymentAmount:#,###.00}";
        }

        private void HidePaymentSummary()
        {
            TxtPaySummLabel.Visibility = Visibility.Collapsed;
            BdPayAmount.Visibility = Visibility.Collapsed;
        }

    }
}
