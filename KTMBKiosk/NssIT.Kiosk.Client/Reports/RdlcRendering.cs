using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Reports
{
	public class RdlcRendering
	{
		private bool _processIsActive = true;

		private string _folder = "";


		/// <summary>
		/// </summary>
		/// <param name="outputfolder"></param>
		///		Like 'yyyyMMdd HH_mm_ss'  OR 'HH_mm_ss'. ':' char cannot be used.
		/// </param>
		public RdlcRendering(string outputfolder)
		{
			outputfolder = (outputfolder ?? "").Trim();

			if (outputfolder.Length == 0)
				throw new Exception("Invalid output folder; Module:ReportRendering;");

			if ((outputfolder.Length > 0) && (outputfolder.Substring((outputfolder.Length - 1), 1).Equals(@"\") == false))
				outputfolder = outputfolder + @"\";

			if (!Directory.Exists(outputfolder))
				Directory.CreateDirectory(outputfolder);

			_folder = outputfolder;

		}

		/// <summary>
		///		
		/// </summary>
		/// <param name="format"></param>
		/// <param name="reportName"></param>
		/// <param name="dataSourceName"></param>
		/// <param name="dataSourceValue"></param>
		/// <param name="reportParameters"></param>
		/// <returns>Return a full file path of a result file.</returns>
		public string RenderReportFile(RdlcOutputFormat format, string reportSourceFolder, string reportName, string reportMidName, string dataSourceName, Object dataSourceValue, ReportParameter[] reportParameters = null)
		{

			reportSourceFolder = (reportSourceFolder ?? "").Trim();
			if (reportSourceFolder.Trim().Length == 0)
				throw new Exception("Invalid report folder.");

			reportName = (reportName ?? "").Trim();
			if (reportName.Trim().Length == 0)
				throw new Exception("Invalid report name.");

			if ((reportSourceFolder.Length > 0) && (reportSourceFolder.Substring((reportSourceFolder.Length - 1), 1).Equals(@"\") == false))
				reportSourceFolder = reportSourceFolder + @"\";

			// ----- ----- ----- ----- ----- 
			// Send Data To Report
			LocalReport localReport = new LocalReport();
			localReport.ReportPath = $@"{reportSourceFolder}{reportName}.rdlc";

			ReportDataSource reportDataSource = new ReportDataSource(dataSourceName, dataSourceValue);
			localReport.DataSources.Add(reportDataSource);
			localReport.EnableHyperlinks = true;
			localReport.EnableExternalImages = true;
			if (reportParameters?.Length > 0)
			{
				localReport.SetParameters(reportParameters);
			}
			// ----- ----- ----- ----- ----- 
			// Render Report
			Warning[] warnings = null;
			byte[] resBytes = null;
			switch (format)
			{
				case RdlcOutputFormat.PotraitPDF:
					resBytes = PotraitPDFRender(ref localReport, out warnings);
					break;
				case RdlcOutputFormat.LandscapePDF:
					resBytes = LandscapePDFRender(ref localReport, out warnings);
					break;
				case RdlcOutputFormat.PotraitExcel:
					resBytes = PotraitExcelRender(ref localReport, out warnings);
					break;
				case RdlcOutputFormat.LandscapeExcel:
					resBytes = LandscapeExcelRender(ref localReport, out warnings);
					break;
				case RdlcOutputFormat.LandscapeTicketPDFRender:
					resBytes = LandscapeTicketPDFRender(ref localReport, out warnings);
					break;

				case RdlcOutputFormat.PotraitTicketPDF1:
					resBytes = PotraitTicketPDF1Render(ref localReport, out warnings);
					break;

				case RdlcOutputFormat.PotraitTicketErrorPDF1:
					resBytes = PotraitTicketErrorPDF1Render(ref localReport, out warnings);
					break;

				case RdlcOutputFormat.PotraitPDF3INx3IN:
					resBytes = PotraitPDF3INx3INRender(ref localReport, out warnings);
					break;

				case RdlcOutputFormat.LandscapeTicketErrorPDFRender:
					resBytes = LandscapeTicketErrorPDFRender(ref localReport, out warnings);
					break;

				default:
					throw new Exception($@"Unregconized report format {format.ToString()}; Module:ReportRendering;");
			}
			// ----- ----- ----- ----- ----- 
			// Check Result
			if (resBytes?.Length > 0)
				return SaveDataToFile(resBytes, reportName, reportMidName, format);
			else
			{
				throw new Exception(TranslateWarnings(warnings));
			}				
		}

		private string SaveDataToFile(byte[] data, string reportName, string reportMidName, RdlcOutputFormat format)
		{
			string filePath = null;
			string postFix = $@"{DateTime.Now.ToString("yyyyMMddHHmmss_fffffff")}";
			switch (format)
			{
				case RdlcOutputFormat.PotraitPDF:
				case RdlcOutputFormat.PotraitTicketPDF1:
				case RdlcOutputFormat.PotraitTicketErrorPDF1:
				case RdlcOutputFormat.PotraitPDF3INx3IN:
				case RdlcOutputFormat.LandscapePDF:
				case RdlcOutputFormat.LandscapeTicketPDFRender:
				case RdlcOutputFormat.LandscapeTicketErrorPDFRender:
					filePath = $@"{_folder}{reportName}_{reportMidName}_{postFix}.pdf";
					break;
				case RdlcOutputFormat.PotraitExcel:
				case RdlcOutputFormat.LandscapeExcel:
					filePath = $@"{_folder}{reportName}_{reportMidName}_{postFix}.xls";
					break;
				default:
					throw new Exception($@"Unregconized report format {format.ToString()}; Module:ReportRendering;");
			}

			File.WriteAllBytes(filePath, data);

			return filePath;
		}

		private string TranslateWarnings(Warning[] warnings)
		{
			if ((warnings == null) || (warnings.Length == 0))
				return "Unable to output report. Unknown exception when render report; Module:ReportRendering;";

			string retMsg = "";
			foreach (Warning wn in warnings)
				retMsg += $@"{wn.Message ?? ".."}-{wn.Severity.ToString()}; ";

			if (retMsg.Length == 0)
				return "Unable to output report. Unknown exception (II) when render report; Module:ReportRendering;";
			else
				return $@"Unable to output report. {retMsg}; Module:ReportRendering;";
		}

		private byte[] PotraitPDFRender(ref LocalReport localReport, out Warning[] warnings)
		{
			warnings = null;

			string reportType = "PDF";
			string mimeType;
			string encoding;
			string fileNameExtension;

			string deviceInfo = "<DeviceInfo>" +
			  "  <OutputFormat>PDF</OutputFormat>" +
			  "  <PageWidth>8.27in</PageWidth>" +
			  "  <PageHeight>11.69in</PageHeight>" +
			  "  <MarginTop>0in</MarginTop>" +
			  "  <MarginLeft>0in</MarginLeft>" +
			  "  <MarginRight>0in</MarginRight>" +
			  "  <MarginBottom>0in</MarginBottom>" +
			  "</DeviceInfo>";

			string[] streams;

			return localReport.Render(reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
		}

		private byte[] LandscapePDFRender(ref LocalReport localReport, out Warning[] warnings)
		{
			warnings = null;

			string reportType = "PDF";
			string mimeType;
			string encoding;
			string fileNameExtension;

			string deviceInfo = "<DeviceInfo>" +
			  "  <OutputFormat>PDF</OutputFormat>" +
			  "  <PageWidth>11.69in</PageWidth>" +
			  "  <PageHeight>8.27in</PageHeight>" +
			  "  <MarginTop>0in</MarginTop>" +
			  "  <MarginLeft>0in</MarginLeft>" +
			  "  <MarginRight>0in</MarginRight>" +
			  "  <MarginBottom>0in</MarginBottom>" +
			  "</DeviceInfo>";

			string[] streams;

			return localReport.Render(reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
		}

		private byte[] PotraitExcelRender(ref LocalReport localReport, out Warning[] warnings)
		{
			warnings = null;

			string reportType = "excel";
			string mimeType;
			string encoding;
			string fileNameExtension;

			string deviceInfo = "<DeviceInfo>" +
			  "  <OutputFormat>PDF</OutputFormat>" +
			  "  <PageWidth>8.27in</PageWidth>" +
			  "  <PageHeight>11.69in</PageHeight>" +
			  "  <MarginTop>0in</MarginTop>" +
			  "  <MarginLeft>0in</MarginLeft>" +
			  "  <MarginRight>0in</MarginRight>" +
			  "  <MarginBottom>0in</MarginBottom>" +
			  "</DeviceInfo>";

			string[] streams;

			return localReport.Render(reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
		}

		private byte[] LandscapeExcelRender(ref LocalReport localReport, out Warning[] warnings)
		{
			warnings = null;

			string reportType = "excel";
			string mimeType;
			string encoding;
			string fileNameExtension;

			string deviceInfo = "<DeviceInfo>" +
			  "  <OutputFormat>PDF</OutputFormat>" +
			  "  <PageWidth>11.69in</PageWidth>" +
			  "  <PageHeight>8.27in</PageHeight>" +
			  "  <MarginTop>0in</MarginTop>" +
			  "  <MarginLeft>0in</MarginLeft>" +
			  "  <MarginRight>0in</MarginRight>" +
			  "  <MarginBottom>0in</MarginBottom>" +
			  "</DeviceInfo>";

			string[] streams;

			return localReport.Render(reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
		}

		private byte[] LandscapeTicketPDFRender(ref LocalReport localReport, out Warning[] warnings)
		{
			warnings = null;

			string reportType = "PDF";
			string mimeType;
			string encoding;
			string fileNameExtension;

			string deviceInfo = "<DeviceInfo>" +
			  "  <OutputFormat>PDF</OutputFormat>" +
			  "  <PageWidth>8.0in</PageWidth>" +
			  "  <PageHeight>3.0in</PageHeight>" +
			  "  <MarginTop>0in</MarginTop>" +
			  "  <MarginLeft>0in</MarginLeft>" +
			  "  <MarginRight>0in</MarginRight>" +
			  "  <MarginBottom>0in</MarginBottom>" +
			  "</DeviceInfo>";

			//string deviceInfo = "<DeviceInfo>" +
			//  "  <OutputFormat>PDF</OutputFormat>" +
			//  "  <PageWidth>0in</PageWidth>" +
			//  "  <PageHeight>0in</PageHeight>" +
			//  "  <MarginTop>0in</MarginTop>" +
			//  "  <MarginLeft>0in</MarginLeft>" +
			//  "  <MarginRight>0in</MarginRight>" +
			//  "  <MarginBottom>0in</MarginBottom>" +
			//  "</DeviceInfo>";

			string[] streams;

			return localReport.Render(reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
		}

		private byte[] PotraitTicketPDF1Render(ref LocalReport localReport, out Warning[] warnings)
		{
			warnings = null;

			string reportType = "PDF";
			string mimeType;
			string encoding;
			string fileNameExtension;

			string deviceInfo = "<DeviceInfo>" +
			  "  <OutputFormat>PDF</OutputFormat>" +
			  "  <PageWidth>3.2in</PageWidth>" +
			  "  <PageHeight>8.2in</PageHeight>" +
			  "  <MarginTop>0in</MarginTop>" +
			  "  <MarginLeft>0in</MarginLeft>" +
			  "  <MarginRight>0in</MarginRight>" +
			  "  <MarginBottom>0in</MarginBottom>" +
			  "</DeviceInfo>";

			//string deviceInfo = "<DeviceInfo>" +
			//  "  <OutputFormat>PDF</OutputFormat>" +
			//  "  <PageWidth>0in</PageWidth>" +
			//  "  <PageHeight>0in</PageHeight>" +
			//  "  <MarginTop>0in</MarginTop>" +
			//  "  <MarginLeft>0in</MarginLeft>" +
			//  "  <MarginRight>0in</MarginRight>" +
			//  "  <MarginBottom>0in</MarginBottom>" +
			//  "</DeviceInfo>";

			string[] streams;

			return localReport.Render(reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
		}

		private byte[] PotraitTicketErrorPDF1Render(ref LocalReport localReport, out Warning[] warnings)
		{
			warnings = null;

			string reportType = "PDF";
			string mimeType;
			string encoding;
			string fileNameExtension;

			string deviceInfo = "<DeviceInfo>" +
			  "  <OutputFormat>PDF</OutputFormat>" +
			  "  <PageWidth>3.2in</PageWidth>" +
			  "  <PageHeight>3.2in</PageHeight>" +
			  "  <MarginTop>0in</MarginTop>" +
			  "  <MarginLeft>0in</MarginLeft>" +
			  "  <MarginRight>0in</MarginRight>" +
			  "  <MarginBottom>0in</MarginBottom>" +
			  "</DeviceInfo>";

			//string deviceInfo = "<DeviceInfo>" +
			//  "  <OutputFormat>PDF</OutputFormat>" +
			//  "  <PageWidth>0in</PageWidth>" +
			//  "  <PageHeight>0in</PageHeight>" +
			//  "  <MarginTop>0in</MarginTop>" +
			//  "  <MarginLeft>0in</MarginLeft>" +
			//  "  <MarginRight>0in</MarginRight>" +
			//  "  <MarginBottom>0in</MarginBottom>" +
			//  "</DeviceInfo>";

			string[] streams;

			return localReport.Render(reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
		}

		private byte[] PotraitPDF3INx3INRender(ref LocalReport localReport, out Warning[] warnings)
		{
			warnings = null;

			string reportType = "PDF";
			string mimeType;
			string encoding;
			string fileNameExtension;

			string deviceInfo = "<DeviceInfo>" +
			  "  <OutputFormat>PDF</OutputFormat>" +
			  "  <PageWidth>3in</PageWidth>" +
			  "  <PageHeight>3in</PageHeight>" +
			  "  <MarginTop>0in</MarginTop>" +
			  "  <MarginLeft>0in</MarginLeft>" +
			  "  <MarginRight>0in</MarginRight>" +
			  "  <MarginBottom>0in</MarginBottom>" +
			  "</DeviceInfo>";

			string[] streams;

			return localReport.Render(reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
		}

		private byte[] LandscapeTicketErrorPDFRender(ref LocalReport localReport, out Warning[] warnings)
		{
			warnings = null;

			string reportType = "PDF";
			string mimeType;
			string encoding;
			string fileNameExtension;

			string deviceInfo = "<DeviceInfo>" +
			  "  <OutputFormat>PDF</OutputFormat>" +
			  "  <PageWidth>8.0in</PageWidth>" +
			  "  <PageHeight>3.0in</PageHeight>" +
			  "  <MarginTop>0in</MarginTop>" +
			  "  <MarginLeft>0in</MarginLeft>" +
			  "  <MarginRight>0in</MarginRight>" +
			  "  <MarginBottom>0in</MarginBottom>" +
			  "</DeviceInfo>";

			//string deviceInfo = "<DeviceInfo>" +
			//  "  <OutputFormat>PDF</OutputFormat>" +
			//  "  <PageWidth>0in</PageWidth>" +
			//  "  <PageHeight>0in</PageHeight>" +
			//  "  <MarginTop>0in</MarginTop>" +
			//  "  <MarginLeft>0in</MarginLeft>" +
			//  "  <MarginRight>0in</MarginRight>" +
			//  "  <MarginBottom>0in</MarginBottom>" +
			//  "</DeviceInfo>";

			string[] streams;

			return localReport.Render(reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
		}

		public enum RdlcOutputFormat
		{
			PotraitPDF = 1,
			LandscapePDF = 2,
			PotraitExcel = 3,
			LandscapeExcel = 4,

			LandscapeTicketPDFRender = 5,
			LandscapeTicketErrorPDFRender = 6,

			/// <summary>
			/// Size : 3in x 6in
			/// </summary>
			PotraitTicketPDF1 = 7,
			/// <summary>
			/// Size : 3in x 3in
			/// </summary>
			PotraitTicketErrorPDF1 = 8,
			/// <summary>
			/// Size : 3in x 3in
			/// </summary>
			PotraitPDF3INx3IN,
		}
	}
}