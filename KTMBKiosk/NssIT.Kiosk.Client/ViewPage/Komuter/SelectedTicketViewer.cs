using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace NssIT.Kiosk.Client.ViewPage.Komuter
{
    public class SelectedTicketViewer
    {
        private string _logChannel = "ViewPage";

        private Brush _foregroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC));
        private double _textSize = 16;

        private List<TextBlock> _ticketItemTextBlockBuffer = new List<TextBlock>();
        private StackPanel _stkTicketLineItemContainer = null;
        private Page _parentPage = null;
        private KomuterTicket[] _selectedTicketList = null;

        public SelectedTicketViewer(Page parentPage, StackPanel stkTicketLineItemContainer)
        {
            _stkTicketLineItemContainer = stkTicketLineItemContainer;
            _parentPage = parentPage;
        }

        public void HideJourneyTypeList()
        {
            _stkTicketLineItemContainer.Visibility = System.Windows.Visibility.Hidden;
        }

        public void LoadSelectedTicketList()
        {
            _parentPage.Dispatcher.Invoke(new Action(() => {
                _stkTicketLineItemContainer.Visibility = System.Windows.Visibility.Visible;
                ClearAllTicketItem();

                if ((_selectedTicketList != null) && (_selectedTicketList?.Length > 0))
                {
                    foreach (KomuterTicket pack in _selectedTicketList)
                    {
                        TextBlock txt = GetFreeUscJourneyTypeButton();
                        txt.Text = $@"     * {pack.TicketTypeDescription}  ({pack.SelectedNoOfPax})";
                        _stkTicketLineItemContainer.Children.Add(txt);
                    }
                }
            }));
            System.Windows.Forms.Application.DoEvents();
        }

        public void InitJourneyTypeList(KomuterTicket[] selectedTicketList)
        {
            _selectedTicketList = selectedTicketList;
        }

        private TextBlock GetFreeUscJourneyTypeButton()
        {
            TextBlock retCtrl = null;
            if (_ticketItemTextBlockBuffer.Count == 0)
                retCtrl = new TextBlock();
            else
            {
                retCtrl = _ticketItemTextBlockBuffer[0];
                _ticketItemTextBlockBuffer.RemoveAt(0);
            }
            retCtrl.Foreground = _foregroundColor;
            retCtrl.FontSize = _textSize;
            retCtrl.TextWrapping = System.Windows.TextWrapping.WrapWithOverflow;
            return retCtrl;
        }

        private void ClearAllTicketItem()
        {
            int nextCtrlInx = 0;
            do
            {
                if (_stkTicketLineItemContainer.Children.Count > nextCtrlInx)
                {
                    if (_stkTicketLineItemContainer.Children[nextCtrlInx] is TextBlock ctrl)
                    {
                        _stkTicketLineItemContainer.Children.RemoveAt(nextCtrlInx);
                        _ticketItemTextBlockBuffer.Add(ctrl);
                    }
                    else
                        nextCtrlInx++;
                }
            } while (_stkTicketLineItemContainer.Children.Count > nextCtrlInx);
        }
    }
}
