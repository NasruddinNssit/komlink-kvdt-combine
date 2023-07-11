﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NssIT.Train.Kiosk.Security.Live.AdminUI
{
    public partial class FrmMain : Form
    {
        private LiveRegistrySetting _liveRegSett = new LiveRegistrySetting();

        private LibShowMessageWindow.MessageWindow _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;

        public FrmMain()
        {
            InitializeComponent();
        }

        private void btnLiveVerify_Click(object sender, EventArgs e)
        {
            try
            {
                _liveRegSett.VerifyRegistry(out string resultMsg);
                _msg.ShowMessage(resultMsg);
                _msg.ShowMessage("----- Live.End -----");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void btnLiveWrite_Click(object sender, EventArgs e)
        {
            try
            {
                _liveRegSett.WriteRegistry(out string resultMsg);
                _msg.ShowMessage(resultMsg);
                _msg.ShowMessage("----- Live.End -----");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }
    }
}
