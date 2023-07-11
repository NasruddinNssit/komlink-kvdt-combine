using Microsoft.AspNetCore.SignalR.Client;
using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Train.Kiosk.Common.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WFmKTMBWebApiSignalRTest
{
    public partial class FrmMainForm : Form
    {
        private LibShowMessageWindow.MessageWindow _msg = null;
        private HubConnection _connection;

        public FrmMainForm()
        {
            InitializeComponent();
            _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;

            Setting setting = Setting.GetSetting();
            setting.HashSecretKey = @"b7edee98eb074d8eb67b8b20f5a3ab13";
            setting.AesEncryptKey = @"0a4ef44c211f4c8dbb4f8ca73aabb5d3";
            setting.TVMKey = @"9eksd92ks9378qwjs92ks92ls02ls02l";
            setting.TimeZoneId = @"Singapore Standard Time";
        }

        private void FrmMainForm_Load(object sender, EventArgs e)
        {
            cboSignalRSvrUrl.SelectedIndex = 0;
        }

        private async void connectButton_Click(object sender, EventArgs e)
        {
            UpdateState(connected: false);

            string snrUrl = cboSignalRSvrUrl.SelectedItem.ToString();

            //_connection = new HubConnectionBuilder()
            //    .WithUrl(snrUrl)
            //    .Build();

            ///// My Custom Connetion to KTMBWebApi ----------------------
            _connection = new HubConnectionBuilder()
                .WithUrl(snrUrl,
                    new Action<Microsoft.AspNetCore.Http.Connections.Client.HttpConnectionOptions>((httpOptions) =>
                    {
                        httpOptions.Headers.Add("RequestSignature", SecurityHelper.getSignature());
                        //httpOptions.Headers.Add("RequestSignature", "");
                    }))
                .WithAutomaticReconnect(new[] { TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5) })
                .Build();

            _connection.Reconnected += _connection_Reconnected;
 
            /////----------------------------------------------------------

            _connection.On<string, string>("broadcastMessage", (s1, s2) => OnSend(s1, s2));
            _connection.On<string>("deliverMessage", (s1) => OnMessageReceived(s1));
            _connection.On<string>("collectReceipt", (s1) => OnCollectReceipt(s1));

            Log("Starting connection...");
            try
            {
                await _connection.StartAsync();

                txtConnectionId.Text = _connection.ConnectionId ?? "";

                UpdateState(connected: true);
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                return;
            }

            Log("End Connection established.");

            

            messageTextBox.Focus();
        }

        private Task _connection_Reconnected(string arg)
        {
            return Task.Factory.StartNew(() => {
                this.Invoke(new Action(() => {
                    txtConnectionId.Text = _connection.ConnectionId;
                    _msg.ShowMessage($@"SignalR Client Reconnection; New Connection Id : {txtConnectionId.Text}; Parameter : {arg}");
                }));
            });
        }

        private async void disconnectButton_Click(object sender, EventArgs e)
        {
            Log("Stopping connection...");
            try
            {
                await _connection.StopAsync();
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            Log("Connection terminated.");

            UpdateState(connected: false);
        }

        private void messageTextBox_Enter(object sender, EventArgs e)
        {
            AcceptButton = sendButton;
        }

        private async void sendButton_Click(object sender, EventArgs e)
        {
            try
            {
                await _connection.InvokeAsync<string>("Send", "WinFormsApp", messageTextBox.Text);
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        private async void btnReadServerTime_Click(object sender, EventArgs e)
        {
            try
            {
                string resStr = await _connection.InvokeAsync<string>("ReadTime", "WinFormsApp", "Aquire Server time");
                Log($@"Server Time : {resStr}");
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        private void UpdateState(bool connected)
        {
            disconnectButton.Enabled = connected;
            connectButton.Enabled = !connected;
            cboSignalRSvrUrl.Enabled = !connected;

            messageTextBox.Enabled = connected;
            sendButton.Enabled = connected;
            btnReadServerTime.Enabled = connected;
        }

        private void OnSend(string name, string message)
        {
            Log(name + ": " + message);
        }

        private void OnMessageReceived(string message)
        {
            Log($@"Received Receipt From Server : {message}");
        }

        private void OnCollectReceipt(string message)
        {
            Log($@"Received SnR Message From Server : {message}");
        }

        private void Log(string message)
        {
            _msg.ShowMessage(message);
        }
    }
}
