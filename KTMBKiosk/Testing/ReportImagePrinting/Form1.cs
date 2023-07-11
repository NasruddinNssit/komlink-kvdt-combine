using Microsoft.Reporting.WinForms;
using NssIT.Kiosk.Client.Reports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReportImagePrinting
{
    public partial class Form1 : Form
    {
        private LibShowMessageWindow.MessageWindow _msg = new LibShowMessageWindow.MessageWindow();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.reportViewer1.RefreshReport();
        }

        private void btnReadData_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = GetData();

                grdData.DataSource = dt;

                grdData.Refresh();

                _msg.ShowMessage("btnReadData_Click : Done");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"btnReadData_Click error : {ex.ToString()}");
            }
        }

        private void btnShowReport_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = GetData();
                ReportDataSource ds = new ReportDataSource("DataSet1", dt);

                this.reportViewer1.LocalReport.ReportPath = "ReportX2.rdlc";
                this.reportViewer1.LocalReport.DataSources.Clear();
                this.reportViewer1.LocalReport.DataSources.Add(ds);
                this.reportViewer1.RefreshReport();

            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"btnShowReport_Click error : {ex.ToString()}");
            }
        }

        private void btnPrintReport_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = GetData();
                LocalReport rep = RdlcImageRendering.CreateLocalReport("ReportX2.rdlc", new ReportDataSource[] { new ReportDataSource("DataSet1", dt) });
                ReportImageSize repSize = new ReportImageSize(8.27M, 11.69M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                Stream[] streamList = RdlcImageRendering.Export(rep, repSize);
                ImagePrintingTools.InitService();
                ImagePrintingTools.AddPrintDocument(streamList, "**", repSize);
                ImagePrintingTools.ExecutePrinting("TestingX0001");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"btnPrintReport_Click error : {ex.ToString()}");
            }
            
        }

        private DataTable GetData()
        {
            DataSetX2.DataTable1DataTable dt = new DataSetX2.DataTable1DataTable();

            DataSetX2.DataTable1Row rw = dt.NewDataTable1Row();
            rw.aId = "1";
            rw.aName = "Lee";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "2";
            rw.aName = "Chong";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "3";
            rw.aName = "Jason";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "4";
            rw.aName = "Kelvin";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "5";
            rw.aName = "YiHan";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "6";
            rw.aName = "Wei Lun";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "7";
            rw.aName = "Jimmy";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "8";
            rw.aName = "Sing";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "9";
            rw.aName = "Afifa";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "10";
            rw.aName = "Wira";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "11";
            rw.aName = "Lee";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "12";
            rw.aName = "Chong";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "13";
            rw.aName = "Jason";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "14";
            rw.aName = "Kelvin";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "15";
            rw.aName = "YiHan";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "16";
            rw.aName = "Wei Lun";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "17";
            rw.aName = "Jimmy";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "18";
            rw.aName = "Sing";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "19";
            rw.aName = "Afifa";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            rw = dt.NewDataTable1Row();
            rw.aId = "20";
            rw.aName = "Wira";
            rw.aDepartment = "Software Department";
            dt.AddDataTable1Row(rw);

            dt.AcceptChanges();

            return dt;
        }

        
    }
}
