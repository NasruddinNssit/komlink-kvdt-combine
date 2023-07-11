using NssIT.Kiosk.Device.PosiFlex.StatusIndicator1.AccessSDK;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosiFlexStatusIndicator02
{
    public partial class Form2 : Form
    {
        private StatusIndicator1Access _towerLight = null;
        private LibShowMessageWindow.MessageWindow _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            cboComPort.Items.AddRange(ports);
        }

        private StatusIndicator1Access TowerLight
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
                            _towerLight = StatusIndicator1Access.GetStatusIndicator(comPort);
                            StatusIndicator1Access.AssignShowMessageLogHandle(new NssIT.Kiosk.AppDecorator.Log.ShowMessageLogDelg(this.ShowMessage));
                        }
                        else
                        {
                            ShowMsg($@"No COM port selected");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowMsg(ex.ToString());
                }
                return _towerLight;
            }
        }

        private void ShowMessage(string msg)
        {
            try
            {
                _msg.ShowMessage(msg);
            }
            catch { }
        }

        private void ShowColorLight(StatusIndicator1Access.IndicatorColor colorArrCode)
        {
            try
            {
                if (ChkBlinking.Checked == true)
                {
                    ShowMsg("Done - Blinking");
                    TowerLight.ShowColor(colorArrCode, StatusIndicator1Access.LightMode.Blinking);
                }
                else
                {
                    TowerLight.ShowColor(colorArrCode);
                    ShowMsg("Done - Normal");
                }

            }
            catch (Exception ex)
            {
                ShowMsg(ex.ToString());
            }
        }

        private void BtnTestRedColor_Click(object sender, EventArgs e)
        {
            ShowColorLight(StatusIndicator1Access.IndicatorColor.Red);
        }

        private void BtnTestGreenColor_Click(object sender, EventArgs e)
        {
            ShowColorLight(StatusIndicator1Access.IndicatorColor.Green);
        }

        private void BtnTestBlueColor_Click(object sender, EventArgs e)
        {
            ShowColorLight(StatusIndicator1Access.IndicatorColor.Blue);
        }

        private void BtnTestAquaColor_Click(object sender, EventArgs e)
        {
            ShowColorLight(StatusIndicator1Access.IndicatorColor.Aqua);
        }

        private void BtnTestYellowColor_Click(object sender, EventArgs e)
        {
            ShowColorLight(StatusIndicator1Access.IndicatorColor.Yellow);
        }

        private void BtnTestMagentaColor_Click(object sender, EventArgs e)
        {
            ShowColorLight(StatusIndicator1Access.IndicatorColor.Magenta);
        }

        private void BtnTestWhiteColor_Click(object sender, EventArgs e)
        {
            ShowColorLight(StatusIndicator1Access.IndicatorColor.White);
        }

        private void BtnSwitchOff_Click(object sender, EventArgs e)
        {
            try
            {
                TowerLight.SwitchOff();
                ShowMsg("Done");
            }
            catch (Exception ex)
            {
                ShowMsg(ex.ToString());
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                TowerLight.SwitchOff();
                Thread.Sleep(1000);
                _towerLight?.Dispose();
            }
            catch (Exception ex)
            { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                DbLog log = DbLog.GetDbLog();

                log.LogText("PosiFlexMotionStatusIndicator01_Test", "*", "Test xxxxx", "ABC001", "Form2.button1_Click");
            }
            catch (Exception ex)
            {
                ShowMsg(ex.ToString());
            }
        }

        private void ShowMsg(string msg)
        {
            this.Invoke(new Action(() => {
                if (string.IsNullOrWhiteSpace(msg))
                    this.txtMsg.AppendText($@"{"\r\n"}");
                else
                {
                    this.txtMsg.AppendText($@"{DateTime.Now.ToString("HH:mm:ss")} - {msg}{"\r\n"}");
                }

                if (this.txtMsg.Text.Length > 10)
                {
                    this.txtMsg.SelectionStart = (this.txtMsg.Text.Length - 1);
                    this.txtMsg.SelectionLength = 1;
                    this.txtMsg.ScrollToCaret();
                }
            }));
        }

        private void btnReboot_Click(object sender, EventArgs e)
        {
            try
            {
                _towerLight?.Dispose();
            }
            catch (Exception ex)
            { }

            Thread.Sleep(500);

            try
            {
                var cmd = new System.Diagnostics.ProcessStartInfo("shutdown.exe", "-r -f -t 0");
                cmd.CreateNoWindow = true;
                cmd.UseShellExecute = false;
                cmd.ErrorDialog = false;
                System.Diagnostics.Process.Start(cmd);
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString()); 
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            try
            {
                var cmd = new System.Diagnostics.ProcessStartInfo("shutdown.exe", "-l");
                cmd.CreateNoWindow = true;
                cmd.UseShellExecute = false;
                cmd.ErrorDialog = false;
                System.Diagnostics.Process.Start(cmd);
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }
    }
}
