using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NssIT.Kiosk.Client.ViewPage.Komuter
{
    public class JourneyTypeSelector
    {
        public event EventHandler<JourneyTypeChangeEventArgs> OnJourneyTypeChanged;

        private string _logChannel = "ViewPage";

        private List<uscJourneyTypeButton> _uscJourneyTypeButtonList = new List<uscJourneyTypeButton>();
        private StackPanel _stkTicketTypeContainer = null;
        private Page _parentPage = null;
        private KomuterSummaryModel _komuterSummary = null;

        public JourneyTypeSelector(Page parentPage, StackPanel stkTicketTypeContainer)
        {
            _parentPage = parentPage;
            _stkTicketTypeContainer = stkTicketTypeContainer;
            _stkTicketTypeContainer.Children.Clear();
        }

        public KomuterPackageModel SelectedPackage { get; private set; }

        private void Journey_OnJourneyTypeChanged(object sender, JourneyTypeChangeEventArgs e)
        {
            try
            {
                if (SelectedPackage is null)
                {
                    SelectedPackage = e.KomuterPackage;
                    e.AgreeChanged = true;
                    RaiseOnJourneyTypeChanged(e);
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001151); {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001151);", ex), "EX01", "JourneyTypeSelector.Journey_OnJourneyTypeChanged");
            }

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void RaiseOnJourneyTypeChanged(JourneyTypeChangeEventArgs e2)
            {
                try
                {
                    OnJourneyTypeChanged?.Invoke(null, e2);
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"(EXIT10001150); {ex.Message}");
                    App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001150);", ex), "EX01", "JourneyTypeSelector.RaiseOnJourneyTypeChanged");
                }
            }
        }

        public void HideJourneyTypeList()
        {
            _stkTicketTypeContainer.Visibility = System.Windows.Visibility.Hidden;
        }

        public void LoadJourneyTypeList()
        {
            _parentPage.Dispatcher.Invoke(new Action(() => {
                _stkTicketTypeContainer.Visibility = System.Windows.Visibility.Visible;
                ClearAllJourneyType();

                if ((_komuterSummary != null) && (_komuterSummary.KomuterPackages?.Length > 0))
                {
                    foreach (KomuterPackageModel pack in _komuterSummary.KomuterPackages)
                    {
                        uscJourneyTypeButton usc = GetFreeUscJourneyTypeButton();
                        usc.Visibility = System.Windows.Visibility.Visible;
                        usc.OnJourneyTypeChanged += Journey_OnJourneyTypeChanged;
                        usc.InitJournetType(pack);
                        usc.LoadJournetType();
                        _stkTicketTypeContainer.Children.Add(usc);
                    }
                }
            }));
            System.Windows.Forms.Application.DoEvents();
        }

        /// <summary>
        /// Return true if JournetType has found
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        public bool ShowOnlySelectedJourneyType(string packageId)
        {
            bool found = false;
            foreach (var ctrl in _stkTicketTypeContainer.Children)
            { 
                if (ctrl is uscJourneyTypeButton jnyBtn)
                {
                    if (jnyBtn.KomuterPackage.Id.Equals(packageId))
                    {
                        found = true;
                    }
                    else
                        jnyBtn.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            return found;
        }

        public bool GetPackageData(string packageId, out string description, out string duration)
        {
            description = null;
            duration = null;
            bool found = false;
            foreach (var ctrl in _stkTicketTypeContainer.Children)
            {
                if (ctrl is uscJourneyTypeButton jnyBtn)
                {
                    if (jnyBtn.KomuterPackage.Id.Equals(packageId))
                    {
                        description = jnyBtn.KomuterPackage.Description;
                        duration = jnyBtn.KomuterPackage.Duration;
                        found = true;
                        break;
                    }
                }
            }
            return found;
        }

        public void InitJourneyTypeList(KomuterSummaryModel komuterSummary)
        {
            SelectedPackage = null;
            _komuterSummary = komuterSummary;
        }
        
        public void ResetJourneyTypeSelection()
        {
            SelectedPackage = null;
            _parentPage.Dispatcher.Invoke(new Action(() => 
            { 
                foreach(var ctrl in _stkTicketTypeContainer.Children)
                    if (ctrl is uscJourneyTypeButton jBtn)
                        jBtn.NormalEnabled();
            }));
            System.Windows.Forms.Application.DoEvents();
        }

        private uscJourneyTypeButton GetFreeUscJourneyTypeButton()
        {
            uscJourneyTypeButton retCtrl = null;
            if (_uscJourneyTypeButtonList.Count == 0)
                retCtrl = new uscJourneyTypeButton();
            else
            {
                retCtrl = _uscJourneyTypeButtonList[0];
                _uscJourneyTypeButtonList.RemoveAt(0);
            }
            return retCtrl;
        }

        private void ClearAllJourneyType()
        {
            int nextCtrlInx = 0;
            do
            {
                if (_stkTicketTypeContainer.Children.Count > nextCtrlInx)
                {
                    if (_stkTicketTypeContainer.Children[nextCtrlInx] is uscJourneyTypeButton ctrl)
                    {
                        ctrl.OnJourneyTypeChanged -= Journey_OnJourneyTypeChanged;
                        _stkTicketTypeContainer.Children.RemoveAt(nextCtrlInx);
                        _uscJourneyTypeButtonList.Add(ctrl);
                    }
                    else
                        nextCtrlInx++;
                }
            } while (_stkTicketTypeContainer.Children.Count > nextCtrlInx);
        }

    }
}
