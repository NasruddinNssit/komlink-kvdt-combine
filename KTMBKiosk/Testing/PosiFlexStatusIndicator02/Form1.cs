using NssIT.Kiosk.Device.PosiFlex.StatusIndicator1.OrgAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosiFlexStatusIndicator02
{
    public partial class Form1 : Form
    {
        private StatusTowerIndicator _towerLight = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            cboComPort.Items.AddRange(ports);
        }

        private StatusTowerIndicator TowerLight
        {
            get
            {
                try
                {
                    if (_towerLight == null)
                    {
                        string comPort = cboComPort.SelectedItem.ToString();

                        if (string.IsNullOrWhiteSpace(comPort) == false)
                        {
                            _towerLight = new StatusTowerIndicator(comPort);
                        }
                        else
                        {
                            MessageBox.Show($@"No COM port selected");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                return _towerLight;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colorArrCode">Refer to NssIT.Kiosk.Device.PosiFlex.StatusIndicator1.OrgAPI.StatusTowerIndicator.ColorBytes_.. </param>
        private void ShowColorLight(byte[] colorArrCode)
        {
            try
            {
                StatusTowerIndicator towerLight = TowerLight;

                if (towerLight != null)
                {
                    towerLight.ShowLight(colorArrCode);

                    MessageBox.Show("Done");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void BtnTestRedColor_Click(object sender, EventArgs e)
        {
            ShowColorLight(StatusTowerIndicator.ColorBytes_Red);
        }

        private void BtnTestGreenColor_Click(object sender, EventArgs e)
        {
            ShowColorLight(StatusTowerIndicator.ColorBytes_Green);
        }

        private void BtnTestBlueColor_Click(object sender, EventArgs e)
        {
            ShowColorLight(StatusTowerIndicator.ColorBytes_Blue);
        }

        private void BtnTestAquaColor_Click(object sender, EventArgs e)
        {
            ShowColorLight(StatusTowerIndicator.ColorBytes_Aqua);
        }

        private void BtnTestYellowColor_Click(object sender, EventArgs e)
        {
            ShowColorLight(StatusTowerIndicator.ColorBytes_Yellow);
        }

        private void BtnTestMagentaColor_Click(object sender, EventArgs e)
        {
            ShowColorLight(StatusTowerIndicator.ColorBytes_Magenta);
        }

        private void BtnTestWhiteColor_Click(object sender, EventArgs e)
        {
            ShowColorLight(StatusTowerIndicator.ColorBytes_White);
        }

        private void BtnSwitchOff_Click(object sender, EventArgs e)
        {
            try
            {
                NssIT.Kiosk.Device.PosiFlex.StatusIndicator1.OrgAPI.StatusTowerIndicator towerLight = TowerLight;

                if (towerLight != null)
                {
                    towerLight.SwitchOffLight();

                    MessageBox.Show("Done");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                NssIT.Kiosk.Device.PosiFlex.StatusIndicator1.OrgAPI.StatusTowerIndicator towerLight = TowerLight;

                if (towerLight != null)
                    towerLight.Dispose();
            }
            catch (Exception ex)
            { }
        }
    }
}
