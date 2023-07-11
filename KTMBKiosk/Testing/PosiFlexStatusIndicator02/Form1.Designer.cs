namespace PosiFlexStatusIndicator02
{
    partial class Form1
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
            this.cboComPort = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.BtnTestRedColor = new System.Windows.Forms.Button();
            this.BtnSwitchOff = new System.Windows.Forms.Button();
            this.BtnTestGreenColor = new System.Windows.Forms.Button();
            this.BtnTestBlueColor = new System.Windows.Forms.Button();
            this.BtnTestAquaColor = new System.Windows.Forms.Button();
            this.BtnTestYellowColor = new System.Windows.Forms.Button();
            this.BtnTestMagentaColor = new System.Windows.Forms.Button();
            this.BtnTestWhiteColor = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cboComPort
            // 
            this.cboComPort.FormattingEnabled = true;
            this.cboComPort.Location = new System.Drawing.Point(71, 6);
            this.cboComPort.Name = "cboComPort";
            this.cboComPort.Size = new System.Drawing.Size(112, 21);
            this.cboComPort.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "COM Port";
            // 
            // BtnTestRedColor
            // 
            this.BtnTestRedColor.Location = new System.Drawing.Point(71, 33);
            this.BtnTestRedColor.Name = "BtnTestRedColor";
            this.BtnTestRedColor.Size = new System.Drawing.Size(112, 23);
            this.BtnTestRedColor.TabIndex = 2;
            this.BtnTestRedColor.Text = "Test Red Color";
            this.BtnTestRedColor.UseVisualStyleBackColor = true;
            this.BtnTestRedColor.Click += new System.EventHandler(this.BtnTestRedColor_Click);
            // 
            // BtnSwitchOff
            // 
            this.BtnSwitchOff.Location = new System.Drawing.Point(189, 6);
            this.BtnSwitchOff.Name = "BtnSwitchOff";
            this.BtnSwitchOff.Size = new System.Drawing.Size(112, 108);
            this.BtnSwitchOff.TabIndex = 3;
            this.BtnSwitchOff.Text = "Test Switch Off";
            this.BtnSwitchOff.UseVisualStyleBackColor = true;
            this.BtnSwitchOff.Click += new System.EventHandler(this.BtnSwitchOff_Click);
            // 
            // BtnTestGreenColor
            // 
            this.BtnTestGreenColor.Location = new System.Drawing.Point(71, 62);
            this.BtnTestGreenColor.Name = "BtnTestGreenColor";
            this.BtnTestGreenColor.Size = new System.Drawing.Size(112, 23);
            this.BtnTestGreenColor.TabIndex = 4;
            this.BtnTestGreenColor.Text = "Test Green Color";
            this.BtnTestGreenColor.UseVisualStyleBackColor = true;
            this.BtnTestGreenColor.Click += new System.EventHandler(this.BtnTestGreenColor_Click);
            // 
            // BtnTestBlueColor
            // 
            this.BtnTestBlueColor.Location = new System.Drawing.Point(71, 91);
            this.BtnTestBlueColor.Name = "BtnTestBlueColor";
            this.BtnTestBlueColor.Size = new System.Drawing.Size(112, 23);
            this.BtnTestBlueColor.TabIndex = 5;
            this.BtnTestBlueColor.Text = "Test Blue Color";
            this.BtnTestBlueColor.UseVisualStyleBackColor = true;
            this.BtnTestBlueColor.Click += new System.EventHandler(this.BtnTestBlueColor_Click);
            // 
            // BtnTestAquaColor
            // 
            this.BtnTestAquaColor.Location = new System.Drawing.Point(71, 140);
            this.BtnTestAquaColor.Name = "BtnTestAquaColor";
            this.BtnTestAquaColor.Size = new System.Drawing.Size(112, 23);
            this.BtnTestAquaColor.TabIndex = 6;
            this.BtnTestAquaColor.Text = "Test Aqua Color";
            this.BtnTestAquaColor.UseVisualStyleBackColor = true;
            this.BtnTestAquaColor.Click += new System.EventHandler(this.BtnTestAquaColor_Click);
            // 
            // BtnTestYellowColor
            // 
            this.BtnTestYellowColor.Location = new System.Drawing.Point(71, 169);
            this.BtnTestYellowColor.Name = "BtnTestYellowColor";
            this.BtnTestYellowColor.Size = new System.Drawing.Size(112, 23);
            this.BtnTestYellowColor.TabIndex = 7;
            this.BtnTestYellowColor.Text = "Test Yellow Color";
            this.BtnTestYellowColor.UseVisualStyleBackColor = true;
            this.BtnTestYellowColor.Click += new System.EventHandler(this.BtnTestYellowColor_Click);
            // 
            // BtnTestMagentaColor
            // 
            this.BtnTestMagentaColor.Location = new System.Drawing.Point(71, 198);
            this.BtnTestMagentaColor.Name = "BtnTestMagentaColor";
            this.BtnTestMagentaColor.Size = new System.Drawing.Size(112, 23);
            this.BtnTestMagentaColor.TabIndex = 8;
            this.BtnTestMagentaColor.Text = "Test Magenta Color";
            this.BtnTestMagentaColor.UseVisualStyleBackColor = true;
            this.BtnTestMagentaColor.Click += new System.EventHandler(this.BtnTestMagentaColor_Click);
            // 
            // BtnTestWhiteColor
            // 
            this.BtnTestWhiteColor.Location = new System.Drawing.Point(189, 140);
            this.BtnTestWhiteColor.Name = "BtnTestWhiteColor";
            this.BtnTestWhiteColor.Size = new System.Drawing.Size(112, 23);
            this.BtnTestWhiteColor.TabIndex = 9;
            this.BtnTestWhiteColor.Text = "Test White Color";
            this.BtnTestWhiteColor.UseVisualStyleBackColor = true;
            this.BtnTestWhiteColor.Click += new System.EventHandler(this.BtnTestWhiteColor_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(341, 245);
            this.Controls.Add(this.BtnTestWhiteColor);
            this.Controls.Add(this.BtnTestMagentaColor);
            this.Controls.Add(this.BtnTestYellowColor);
            this.Controls.Add(this.BtnTestAquaColor);
            this.Controls.Add(this.BtnTestBlueColor);
            this.Controls.Add(this.BtnTestGreenColor);
            this.Controls.Add(this.BtnSwitchOff);
            this.Controls.Add(this.BtnTestRedColor);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboComPort);
            this.Name = "Form1";
            this.Text = "Form1 (API)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboComPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BtnTestRedColor;
        private System.Windows.Forms.Button BtnSwitchOff;
        private System.Windows.Forms.Button BtnTestGreenColor;
        private System.Windows.Forms.Button BtnTestBlueColor;
        private System.Windows.Forms.Button BtnTestAquaColor;
        private System.Windows.Forms.Button BtnTestYellowColor;
        private System.Windows.Forms.Button BtnTestMagentaColor;
        private System.Windows.Forms.Button BtnTestWhiteColor;
    }
}

