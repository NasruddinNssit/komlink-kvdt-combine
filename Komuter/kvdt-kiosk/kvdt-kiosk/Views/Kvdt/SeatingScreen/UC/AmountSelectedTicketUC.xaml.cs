﻿using System;
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

namespace kvdt_kiosk.Views.Kvdt.SeatingScreen.UC
{
    /// <summary>
    /// Interaction logic for AmountSelectedTicketUC.xaml
    /// </summary>
    public partial class AmountSelectedTicketUC : UserControl
    {
        public AmountSelectedTicketUC()
        {
            InitializeComponent();
        }

        public void UpdateAmountTicket(string value)
        {
            TicketAmount.Text = value;
        }
    }
}
