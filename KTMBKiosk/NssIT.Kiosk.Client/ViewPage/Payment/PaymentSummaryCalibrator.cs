using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NssIT.Kiosk.Client.ViewPage.Payment
{
    public class PaymentSummaryItemCalibrator
    {
        private StackPanel _stkSummaryItemContainer = null;
        private List<uscSummaryItem> _uscSummaryItemList = null;

        private ResourceDictionary _languageResource = null;

        public PaymentSummaryItemCalibrator(StackPanel stkSummaryItem)
        {
            _stkSummaryItemContainer = stkSummaryItem;
            _uscSummaryItemList = new List<uscSummaryItem>();
        }

        public void LoadCalibrator(UserSession userSession, ResourceDictionary languageRes)
        {
            int overrallTotalTicket = 0;
            ClearAllPassengerInfo();

            _languageResource = languageRes;

            int departNoOfTicket = userSession.DepartPassengerSeatDetailList.Length;
            string departTripDesc = $@"{userSession.OriginStationName} > {userSession.DestinationStationName}";
            decimal departTotalTicketTypeAmt = (from seat in userSession.DepartPassengerSeatDetailList
                                                select seat.OriginalTicketPrice).Sum();
            //decimal departTotalTicketTypeAmt = userSession.DepartTotalAmount;
            string departCurrency = userSession.DepartPassengerSeatDetailList[0].Currency;
            uscSummaryItem departSummItem = GetFreeUscSummaryItem();
            overrallTotalTicket += departNoOfTicket;
            string trainNo = string.Format(_languageResource["TRAIN_NO_Label"].ToString(), 1);
            string noOfTicket = string.Format(_languageResource["NUMBER_OF_TICKETS_Label"].ToString(), departNoOfTicket);  
            departSummItem.ShowSummaryItem(trainNo, departTripDesc, noOfTicket
                , "", departCurrency, departTotalTicketTypeAmt, _languageResource);

            _stkSummaryItemContainer.Children.Add(departSummItem);

            // Load Depart Insurance
            decimal departTotalInsuranceAmt = (from seat in userSession.DepartPassengerSeatDetailList
                                                select seat.InsuranceAmount).Sum();
            if (departTotalInsuranceAmt > 0)
            {
                uscSummaryItem departInsurance = GetFreeUscSummaryItem();
                departInsurance.ShowInsurance(userSession.TradeCurrency, departTotalInsuranceAmt, _languageResource);
                _stkSummaryItemContainer.Children.Add(departInsurance);
            }
            //----- ----- ----- ----- -----
            //Load Depart Promo Discount
            decimal departTotalPromoDiscount = (from seat in userSession.DepartPassengerSeatDetailList
                                                select seat.PromoDiscountAmount).Sum();
            if (departTotalPromoDiscount > 0)
            {
                uscSummaryItem departPromoDist = GetFreeUscSummaryItem();
                departPromoDist.ShowPromoDiscount(userSession.TradeCurrency, departTotalPromoDiscount, _languageResource);
                _stkSummaryItemContainer.Children.Add(departPromoDist);
            }
            //----- ----- ----- ----- -----

            if (userSession.ReturnPassengerSeatDetailList?.Length > 0)
            {
                uscSummaryItem blank = GetFreeUscSummaryItem();
                blank.ShowBlankSpace();
                _stkSummaryItemContainer.Children.Add(blank);

                int returnNoOfTicket = userSession.ReturnPassengerSeatDetailList.Length;
                string returnTripDesc = $@"{userSession.DestinationStationName} > {userSession.OriginStationName}";
                decimal returnTotalTotalTicketTypeAmt = (from seat in userSession.ReturnPassengerSeatDetailList
                                                            select seat.OriginalTicketPrice).Sum();
                //decimal returnTotalTotalTicketTypeAmt = userSession.ReturnTotalAmount;
                string returnCurrency = userSession.ReturnPassengerSeatDetailList[0].Currency;
                uscSummaryItem returnSummItem = GetFreeUscSummaryItem();
                overrallTotalTicket += returnNoOfTicket;

                trainNo = string.Format(_languageResource["TRAIN_NO_Label"].ToString(), 2);
                noOfTicket = string.Format(_languageResource["NUMBER_OF_TICKETS_Label"].ToString(), returnNoOfTicket);
                returnSummItem.ShowSummaryItem(trainNo, returnTripDesc, noOfTicket
                    , "", returnCurrency, returnTotalTotalTicketTypeAmt, _languageResource);
                _stkSummaryItemContainer.Children.Add(returnSummItem);

                // Load Return Insurance
                decimal returnTotalInsuranceAmt = (from seat in userSession.ReturnPassengerSeatDetailList
                                                   select seat.InsuranceAmount).Sum();
                if (returnTotalInsuranceAmt > 0)
                {
                    uscSummaryItem departInsurance = GetFreeUscSummaryItem();
                    departInsurance.ShowInsurance(userSession.TradeCurrency, returnTotalInsuranceAmt, _languageResource);
                    _stkSummaryItemContainer.Children.Add(departInsurance);
                }
                //----- ----- ----- ----- -----
                //Load Return Promo Discount
                decimal returnTotalPromoDiscount = (from seat in userSession.ReturnPassengerSeatDetailList
                                                    select seat.PromoDiscountAmount).Sum();
                if (returnTotalPromoDiscount > 0)
                {
                    uscSummaryItem returnPromoDist = GetFreeUscSummaryItem();
                    returnPromoDist.ShowPromoDiscount(userSession.TradeCurrency, returnTotalPromoDiscount, _languageResource);
                    _stkSummaryItemContainer.Children.Add(returnPromoDist);
                }
                //----- ----- ----- ----- -----
            }

            uscSummaryItem grossSummItem = GetFreeUscSummaryItem();
            string ttNoOfTicket = string.Format(_languageResource["NUMBER_OF_TICKETS_Label"].ToString(), overrallTotalTicket);
            grossSummItem.ShowGrossTotal(ttNoOfTicket, userSession.TradeCurrency, userSession.GrossTotal, _languageResource);
            _stkSummaryItemContainer.Children.Add(grossSummItem);
        }

        private uscSummaryItem GetFreeUscSummaryItem()
        {
            uscSummaryItem retCtrl = null;
            if (_uscSummaryItemList.Count == 0)
                retCtrl = new uscSummaryItem();
            else
            {
                retCtrl = _uscSummaryItemList[0];
                _uscSummaryItemList.RemoveAt(0);
            }
            return retCtrl;
        }

        private void ClearAllPassengerInfo()
        {
            int nextCtrlInx = 0;
            do
            {
                if (_stkSummaryItemContainer.Children.Count > nextCtrlInx)
                {
                    if (_stkSummaryItemContainer.Children[nextCtrlInx] is uscSummaryItem ctrl)
                    {
                        _stkSummaryItemContainer.Children.RemoveAt(nextCtrlInx);
                        _uscSummaryItemList.Add(ctrl);
                    }
                    else
                        nextCtrlInx++;
                }
            } while (_stkSummaryItemContainer.Children.Count > nextCtrlInx);
        }
    }
}
