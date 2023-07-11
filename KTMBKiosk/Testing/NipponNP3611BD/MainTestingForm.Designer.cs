namespace NipponNP3611BD
{
    partial class MainTestingForm
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
            this.cboPrinters = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnChkPrinter = new System.Windows.Forms.Button();
            this.btnAutoDetectPrinter = new System.Windows.Forms.Button();
            this.txtPrinterName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkNoPaperLow = new System.Windows.Forms.CheckBox();
            this.btnPrintingTracking = new System.Windows.Forms.Button();
            this.lblPrintingTracking = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cboPrinters
            // 
            this.cboPrinters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboPrinters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPrinters.FormattingEnabled = true;
            this.cboPrinters.Location = new System.Drawing.Point(212, 60);
            this.cboPrinters.Name = "cboPrinters";
            this.cboPrinters.Size = new System.Drawing.Size(277, 21);
            this.cboPrinters.TabIndex = 0;
            this.cboPrinters.SelectedIndexChanged += new System.EventHandler(this.cboPrinters_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(209, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select Printer";
            // 
            // btnChkPrinter
            // 
            this.btnChkPrinter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChkPrinter.Location = new System.Drawing.Point(337, 114);
            this.btnChkPrinter.Name = "btnChkPrinter";
            this.btnChkPrinter.Size = new System.Drawing.Size(152, 23);
            this.btnChkPrinter.TabIndex = 2;
            this.btnChkPrinter.Text = "Check Printer";
            this.btnChkPrinter.UseVisualStyleBackColor = true;
            this.btnChkPrinter.Click += new System.EventHandler(this.btnChkPrinter_Click);
            // 
            // btnAutoDetectPrinter
            // 
            this.btnAutoDetectPrinter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAutoDetectPrinter.Location = new System.Drawing.Point(12, 58);
            this.btnAutoDetectPrinter.Name = "btnAutoDetectPrinter";
            this.btnAutoDetectPrinter.Size = new System.Drawing.Size(166, 23);
            this.btnAutoDetectPrinter.TabIndex = 3;
            this.btnAutoDetectPrinter.Text = "Auto Detect Default Printer";
            this.btnAutoDetectPrinter.UseVisualStyleBackColor = true;
            this.btnAutoDetectPrinter.Click += new System.EventHandler(this.btnAutoDetectPrinter_Click);
            // 
            // txtPrinterName
            // 
            this.txtPrinterName.Location = new System.Drawing.Point(12, 114);
            this.txtPrinterName.Name = "txtPrinterName";
            this.txtPrinterName.ReadOnly = true;
            this.txtPrinterName.Size = new System.Drawing.Size(308, 20);
            this.txtPrinterName.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 98);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Printer Name";
            // 
            // chkNoPaperLow
            // 
            this.chkNoPaperLow.AutoSize = true;
            this.chkNoPaperLow.Location = new System.Drawing.Point(12, 13);
            this.chkNoPaperLow.Name = "chkNoPaperLow";
            this.chkNoPaperLow.Size = new System.Drawing.Size(175, 17);
            this.chkNoPaperLow.TabIndex = 6;
            this.chkNoPaperLow.Text = "No Paper Low Status Checking";
            this.chkNoPaperLow.UseVisualStyleBackColor = true;
            // 
            // btnPrintingTracking
            // 
            this.btnPrintingTracking.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPrintingTracking.Location = new System.Drawing.Point(12, 164);
            this.btnPrintingTracking.Name = "btnPrintingTracking";
            this.btnPrintingTracking.Size = new System.Drawing.Size(152, 23);
            this.btnPrintingTracking.TabIndex = 7;
            this.btnPrintingTracking.Text = "Start Printing Tracking";
            this.btnPrintingTracking.UseVisualStyleBackColor = true;
            this.btnPrintingTracking.Click += new System.EventHandler(this.btnPrintingTracking_Click);
            // 
            // lblPrintingTracking
            // 
            this.lblPrintingTracking.AutoSize = true;
            this.lblPrintingTracking.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrintingTracking.Location = new System.Drawing.Point(177, 164);
            this.lblPrintingTracking.Name = "lblPrintingTracking";
            this.lblPrintingTracking.Size = new System.Drawing.Size(35, 37);
            this.lblPrintingTracking.TabIndex = 8;
            this.lblPrintingTracking.Text = "0";
            // 
            // MainTestingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 210);
            this.Controls.Add(this.lblPrintingTracking);
            this.Controls.Add(this.btnPrintingTracking);
            this.Controls.Add(this.chkNoPaperLow);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtPrinterName);
            this.Controls.Add(this.btnAutoDetectPrinter);
            this.Controls.Add(this.btnChkPrinter);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboPrinters);
            this.Name = "MainTestingForm";
            this.Text = "Nippon NP-3611BD-3 Printer Testing";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainTestingForm_FormClosing);
            this.Load += new System.EventHandler(this.MainTestingForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboPrinters;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnChkPrinter;
        private System.Windows.Forms.Button btnAutoDetectPrinter;
        private System.Windows.Forms.TextBox txtPrinterName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkNoPaperLow;
        private System.Windows.Forms.Button btnPrintingTracking;
        private System.Windows.Forms.Label lblPrintingTracking;
    }
}

