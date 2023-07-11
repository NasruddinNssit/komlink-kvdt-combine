
namespace WFmKTMBWebApiSignalRTest
{
    partial class FrmMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.disconnectButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.connectButton = new System.Windows.Forms.Button();
            this.btnReadServerTime = new System.Windows.Forms.Button();
            this.sendButton = new System.Windows.Forms.Button();
            this.messageTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtConnectionId = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cboSignalRSvrUrl = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // disconnectButton
            // 
            this.disconnectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.disconnectButton.Enabled = false;
            this.disconnectButton.Location = new System.Drawing.Point(697, 10);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(75, 23);
            this.disconnectButton.TabIndex = 10;
            this.disconnectButton.Text = "Disconnect";
            this.disconnectButton.UseVisualStyleBackColor = true;
            this.disconnectButton.Click += new System.EventHandler(this.disconnectButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Url :";
            // 
            // connectButton
            // 
            this.connectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.connectButton.Location = new System.Drawing.Point(611, 10);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 23);
            this.connectButton.TabIndex = 8;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // btnReadServerTime
            // 
            this.btnReadServerTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReadServerTime.Enabled = false;
            this.btnReadServerTime.Location = new System.Drawing.Point(114, 94);
            this.btnReadServerTime.Name = "btnReadServerTime";
            this.btnReadServerTime.Size = new System.Drawing.Size(658, 23);
            this.btnReadServerTime.TabIndex = 13;
            this.btnReadServerTime.Text = "Read Server Time";
            this.btnReadServerTime.UseVisualStyleBackColor = true;
            this.btnReadServerTime.Click += new System.EventHandler(this.btnReadServerTime_Click);
            // 
            // sendButton
            // 
            this.sendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sendButton.Enabled = false;
            this.sendButton.Location = new System.Drawing.Point(611, 65);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(161, 23);
            this.sendButton.TabIndex = 12;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // messageTextBox
            // 
            this.messageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.messageTextBox.Enabled = false;
            this.messageTextBox.Location = new System.Drawing.Point(114, 67);
            this.messageTextBox.Name = "messageTextBox";
            this.messageTextBox.Size = new System.Drawing.Size(491, 20);
            this.messageTextBox.TabIndex = 11;
            this.messageTextBox.Enter += new System.EventHandler(this.messageTextBox_Enter);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Message to Send : ";
            // 
            // txtConnectionId
            // 
            this.txtConnectionId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConnectionId.Location = new System.Drawing.Point(114, 38);
            this.txtConnectionId.Name = "txtConnectionId";
            this.txtConnectionId.ReadOnly = true;
            this.txtConnectionId.Size = new System.Drawing.Size(491, 20);
            this.txtConnectionId.TabIndex = 15;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 41);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Connection ID : ";
            // 
            // cboSignalRSvrUrl
            // 
            this.cboSignalRSvrUrl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSignalRSvrUrl.FormattingEnabled = true;
            this.cboSignalRSvrUrl.Items.AddRange(new object[] {
            "https://localhost:44305/chat",
            "https://ktmb-dev-api.azurewebsites.net/chat"});
            this.cboSignalRSvrUrl.Location = new System.Drawing.Point(51, 7);
            this.cboSignalRSvrUrl.Name = "cboSignalRSvrUrl";
            this.cboSignalRSvrUrl.Size = new System.Drawing.Size(554, 21);
            this.cboSignalRSvrUrl.TabIndex = 17;
            // 
            // FrmMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 181);
            this.Controls.Add(this.cboSignalRSvrUrl);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtConnectionId);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnReadServerTime);
            this.Controls.Add(this.sendButton);
            this.Controls.Add(this.messageTextBox);
            this.Controls.Add(this.disconnectButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.connectButton);
            this.Name = "FrmMainForm";
            this.Text = "Main Form - KTMBWebAPI SignalR Test";
            this.Load += new System.EventHandler(this.FrmMainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button disconnectButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Button btnReadServerTime;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.TextBox messageTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtConnectionId;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cboSignalRSvrUrl;
    }
}

