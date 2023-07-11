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

namespace PosiFlexMotionSensorTest01
{
    public partial class Form1 : Form
    {
        private LibShowMessageWindow.MessageWindow _msg = new LibShowMessageWindow.MessageWindow();
        private NssIT.Kiosk.Device.PosiFlex.MotionSensor1.OrgAPI.PosiFlexMotionSensorDLLWrapper _sensor = null;

        public Form1()
        {
            InitializeComponent();

            string[] ports = SerialPort.GetPortNames();
            cboComPort.Items.AddRange(ports);

            if (cboComPort.Items.Count > 0)
            {
                cboComPort.Text = (string)cboComPort.Items[cboComPort.Items.Count - 1];
            }

            
        }

        private void btnUseCOM_Click(object sender, EventArgs e)
        {
            try
            {
                string comPort = cboComPort.Text;
                _msg.ShowMessage($@"Use COM Port : {comPort}");

                _sensor = new NssIT.Kiosk.Device.PosiFlex.MotionSensor1.OrgAPI.PosiFlexMotionSensorDLLWrapper(comPort, 1);
                _sensor.OnSensorMotion += _sensor_OnSensorMotion;
                _sensor.OnNotifySensorMotion += _sensor_OnNotifySensorMotion;

                _sensor.OnPresentCustomer += _sensor_OnPresentCustomer;
                _sensor.OnAbsentCustomer += _sensor_OnAbsentCustomer;

                btnUseCOM.Enabled = false;
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error on btnUseCOM_Click; {"\r\n"}{ex.ToString()}");
            }
        }

        private void _sensor_OnAbsentCustomer(object sender, EventArgs e)
        {
            try
            {
                _msg.ShowMessage($@"_sensor_OnAbsentCustomer; Customer Absent ....................................................................................");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error on _sensor_OnAbsentCustomer; {"\r\n"}{ex.ToString()}");
            }
        }

        private void _sensor_OnPresentCustomer(object sender, EventArgs e)
        {
            try
            {
                _msg.ShowMessage($@"_sensor_OnPresentCustomer; Customer Present +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                _msg.ShowMessage($@"+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error on _sensor_OnPresentCustomer; {"\r\n"}{ex.ToString()}");
            }
        }

        private void _sensor_OnNotifySensorMotion(object sender, NssIT.Kiosk.Device.PosiFlex.MotionSensor1.OrgAPI.NotificationEventArgs e)
        {
            try
            {
                _msg.ShowMessage($@"_sensor_OnNotifySensorMotion; Message: {e.Message}");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error on _sensor_OnNotifySensorMotion; {"\r\n"}{ex.ToString()}");
            }
        }

        private void _sensor_OnSensorMotion(object sender, NssIT.Kiosk.Device.PosiFlex.MotionSensor1.OrgAPI.SensorMotionEventArgs e)
        {
            try
            {
                _msg.ShowMessage($@"_sensor_OnSensorMotion; RangeValue: {e.RangeValue};{"\t\t"}IsSomeoneNearBy: {e.IsSomeoneNearBy}");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error on _sensor_OnSensorMotion; {"\r\n"}{ex.ToString()}");
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                if (_sensor is null)
                    throw new Exception("Please select and use a COM port");

                _sensor.StartMotionDetector();
                btnStart.Enabled = false;
                btnEnd.Enabled = true;
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error on btnStart_Click; {"\r\n"}{ex.ToString()}" );
            }
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            try
            {
                if (_sensor is null)
                    throw new Exception("Please select and use a COM port");

                _sensor.StopMotionDetector();
                btnEnd.Enabled = false;
                btnStart.Enabled = true;
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error on btnEnd_Click; {"\r\n"}{ex.ToString()}");
            }
        }

        
    }
}
