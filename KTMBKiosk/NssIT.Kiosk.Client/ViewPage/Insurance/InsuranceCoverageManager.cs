using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.Client.Base;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Train.Kiosk.Common.Data.Response;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace NssIT.Kiosk.Client.ViewPage.Insurance
{
    public class InsuranceCoverageManager
    {
        private string _logChannel = "ViewPage";

        private Dispatcher _pageDispatcher = null;
        private StackPanel _stpInsrCollection = null;
        private LanguageCode _language = LanguageCode.English;

        private List<uscCoverageGroup> _uscCoverageGroupList = new List<uscCoverageGroup>();
        private WebImageCacheX.GetImageFromCache _getImageFromCacheDelgHandle = null;

        private string selectedInsuranceHeaderId = null;

        //private TextBlock _txtOriginNettTotalTicketAmount = null;
        private int _departSeatCount = 0;
        private decimal _departTotalAmount = 0M;
        private string _departCurrency = "RM";

        //private TextBlock _txtReturnNettTotalTicketAmount = null;
        private int _returnSeatCount = 0;
        private decimal _returnTotalAmount = 0M;
        private string _returnCurrency = "RM";
        private string _bookingId = null;

        private Run _txtInsurancePricePerPerson = null;
        private Run _txtInsuranceShortDescNm = null;
        private Run _txtInsuranceCost = null;

        public InsuranceCoverageManager(Page page, StackPanel stpInsrCollection,
            Run txtInsurancePricePerPerson, Run txtInsuranceCost, Run txtInsuranceShortDescNm)
        {
            _pageDispatcher = page.Dispatcher;
            _stpInsrCollection = stpInsrCollection;

            _txtInsurancePricePerPerson = txtInsurancePricePerPerson;
            _txtInsuranceShortDescNm = txtInsuranceShortDescNm;
            _txtInsuranceCost = txtInsuranceCost;
        }

        public void LoadInsurance(LanguageCode language, GetInsuranceModel insuranceColl, 
            string currency,
            int departSeatCount, decimal departTotalAmount, string departCurrency,
            int returnSeatCount, decimal returnTotalAmount, string returnCurrency,
            WebImageCacheX.GetImageFromCache getImageFromCacheDelgHandle)
        {
            _language = language;
            _getImageFromCacheDelgHandle = getImageFromCacheDelgHandle;

            _departSeatCount = departSeatCount;
            _departTotalAmount = departTotalAmount;
            _departCurrency = departCurrency;

            _returnSeatCount = returnSeatCount;
            _returnTotalAmount = returnTotalAmount;
            _returnCurrency = returnCurrency;

            ClearAllPassengerInfo();

            _bookingId = insuranceColl.Booking_Id;

            string firstInsrId = insuranceColl.InsuranceModels[0].MInsuranceHeaders_Id;
            selectedInsuranceHeaderId = firstInsrId;

            List<string> insuranceHeaderList = (from insr in insuranceColl.InsuranceModels
                                                group insr by insr.MInsuranceHeaders_Id into grpInsr
                                                orderby grpInsr.Key
                                                select grpInsr.Key).ToList();
            insuranceHeaderList.Remove(firstInsrId);

            InsuranceModel[] firstCoverageArr = GetInsuranceGroup(firstInsrId);
            uscCoverageGroup newUscInsrGrp = GetFreeUscCoverageGroup();

            newUscInsrGrp.InitData(_language, firstInsrId, firstCoverageArr, currency, firstCoverageArr[0].Price, 
                firstCoverageArr[0].Cost, firstCoverageArr[0].CostFormat,
                _getImageFromCacheDelgHandle);
            newUscInsrGrp.OnInsuranceSelected += UscInsrGrp_OnInsuranceSelected;
            _stpInsrCollection.Children.Add(newUscInsrGrp);

            System.Windows.Forms.Application.DoEvents();
            newUscInsrGrp.IsInsuranceSelected = true;
            System.Windows.Forms.Application.DoEvents();

            InsuranceModel[] coverageArr = null;
            foreach (string insrHdId in insuranceHeaderList)
            {
                coverageArr = GetInsuranceGroup(insrHdId);
                newUscInsrGrp = GetFreeUscCoverageGroup();

                newUscInsrGrp.InitData(_language, insrHdId, coverageArr, currency, coverageArr[0].Price, 
                    coverageArr[0].Cost, coverageArr[0].CostFormat, 
                    _getImageFromCacheDelgHandle);
                newUscInsrGrp.OnInsuranceSelected += UscInsrGrp_OnInsuranceSelected;
                _stpInsrCollection.Children.Add(newUscInsrGrp);
                newUscInsrGrp.IsInsuranceSelected = false;
            }

            if (App.SysParam.PrmIsDebugMode)
            {
                ///// CYA-TEST xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                int testingCount = 0;

                ///// CYA-TEST testingCount = 5;

                for (int testInx = 0; testInx < testingCount; testInx++)
                {
                    firstCoverageArr = GetInsuranceGroup(firstInsrId);
                    newUscInsrGrp = GetFreeUscCoverageGroup();

                    newUscInsrGrp.InitData(_language, firstInsrId, firstCoverageArr, currency, firstCoverageArr[0].Price,
                        firstCoverageArr[0].Cost, firstCoverageArr[0].CostFormat,
                        _getImageFromCacheDelgHandle);
                    newUscInsrGrp.OnInsuranceSelected += UscInsrGrp_OnInsuranceSelected;
                    _stpInsrCollection.Children.Add(newUscInsrGrp);

                    foreach (string insrHdId in insuranceHeaderList)
                    {
                        coverageArr = GetInsuranceGroup(insrHdId);
                        newUscInsrGrp = GetFreeUscCoverageGroup();

                        newUscInsrGrp.InitData(_language, insrHdId, coverageArr, currency, coverageArr[0].Price,
                            coverageArr[0].Cost, coverageArr[0].CostFormat,
                            _getImageFromCacheDelgHandle);
                        newUscInsrGrp.OnInsuranceSelected += UscInsrGrp_OnInsuranceSelected;
                        _stpInsrCollection.Children.Add(newUscInsrGrp);
                        newUscInsrGrp.IsInsuranceSelected = false;
                    }
                }
                /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            }


            _txtInsurancePricePerPerson.Text = $@"{firstCoverageArr[0].Price:#,##0.00}";
            _txtInsuranceCost.Text = $@"{firstCoverageArr[0].Cost:#,##0.00}";

            if (_language == LanguageCode.Malay)
                _txtInsuranceShortDescNm.Text = $@"({firstCoverageArr[0].ShortDescription2})";
            else
                _txtInsuranceShortDescNm.Text = $@"({firstCoverageArr[0].ShortDescription})";
            
            System.Windows.Forms.Application.DoEvents();
            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            InsuranceModel[] GetInsuranceGroup(string insuranceGroupId)
            {
                InsuranceModel[] insrGrp = (from insr in insuranceColl.InsuranceModels
                                            where insr.MInsuranceHeaders_Id.Equals(insuranceGroupId)
                                            select insr).ToArray();
                return insrGrp;
            }
        }

        private void UscInsrGrp_OnInsuranceSelected(object sender, SelectInsuranceEventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();

                int nextCtrlInx = 0;
                do
                {
                    if (_stpInsrCollection.Children.Count > nextCtrlInx)
                    {
                        if (_stpInsrCollection.Children[nextCtrlInx] is uscCoverageGroup ctrl)
                        {
                            if (ctrl.IsInsuranceSelected)
                                ctrl.IsInsuranceSelected = false;
                        }
                        nextCtrlInx++;
                    }
                } while (_stpInsrCollection.Children.Count > nextCtrlInx);
                e.SelectedInsurance.IsInsuranceSelected = true;

                _txtInsurancePricePerPerson.Text = $@"{e.SelectedInsurance.Price:#,##0.00}";
                _txtInsuranceShortDescNm.Text = $@"({e.SelectedInsurance.CoverageShortDesc})";
                _txtInsuranceCost.Text = $@"{e.SelectedInsurance.Cost:#,##0.00}";
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, _bookingId, ex, "EX01", "InsuranceCoverageManager.UscInsrGrp_OnInsuranceSelected");
            }
        }

        public string GetSelectedInsuranceHeaderId()
        {
            int nextCtrlInx = 0;
            do
            {
                if (_stpInsrCollection.Children.Count > nextCtrlInx)
                {
                    if (_stpInsrCollection.Children[nextCtrlInx] is uscCoverageGroup ctrl)
                    {
                        if (ctrl.IsInsuranceSelected && (string.IsNullOrWhiteSpace(ctrl.InsuranceHeadersId) == false))
                        {
                            return ctrl.InsuranceHeadersId;
                        }
                    }
                    nextCtrlInx++;
                }
            } while (_stpInsrCollection.Children.Count > nextCtrlInx);

            return null;
        }

        private uscCoverageGroup GetFreeUscCoverageGroup()
        {
            uscCoverageGroup retCtrl = null;
            if (_uscCoverageGroupList.Count == 0)
                retCtrl = new uscCoverageGroup();
            else
            {
                retCtrl = _uscCoverageGroupList[0];
                _uscCoverageGroupList.RemoveAt(0);
            }
            return retCtrl;
        }

        private void ClearAllPassengerInfo()
        {
            int nextCtrlInx = 0;
            do
            {
                if (_stpInsrCollection.Children.Count > nextCtrlInx)
                {
                    if (_stpInsrCollection.Children[nextCtrlInx] is uscCoverageGroup ctrl)
                    {
                        ctrl.OnInsuranceSelected -= UscInsrGrp_OnInsuranceSelected;
                        _stpInsrCollection.Children.RemoveAt(nextCtrlInx);
                        _uscCoverageGroupList.Add(ctrl);
                    }
                    else
                        nextCtrlInx++;
                }
            } while (_stpInsrCollection.Children.Count > nextCtrlInx);
        }
    }
}
