using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NssIT.Kiosk.Client.ViewPage.Language
{
	/// <summary>
	/// Interaction logic for pgLanguage.xaml
	/// </summary>
	public partial class pgLanguage : Page
	{
		private const string LogChannel = "ViewPage";

		private bool _selectedFlag = false;
		public pgLanguage()
		{
			InitializeComponent();
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			_selectedFlag = false;

			System.Windows.Forms.Application.DoEvents();
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
			_selectedFlag = false;
		}

		private void Malay_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				DoSelectLanguage(LanguageCode.Malay);
				//DoSelectLanguage(LanguageCode.English);
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", ex, classNMethodName: "pgIntro._introAniHelp_OnIntroBegin");
			}
		}

		private void English_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				DoSelectLanguage(LanguageCode.English);
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", ex, classNMethodName: "pgIntro._introAniHelp_OnIntroBegin");
			}
		}

		
		private void DoSelectLanguage(LanguageCode langCode)
		{
			try
			{
				if (_selectedFlag == false)
				{
					_selectedFlag = true;

					Submit(langCode);
					//App.NetClientSvc.SalesService.SubmitLanguage(langCode, out bool isServerResponded);

					//if (isServerResponded == false)
					//	App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000101)");
				}
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000102)", ex), "EX01", classNMethodName: "pgLanguage.DoSelectLanguage");
				App.MainScreenControl.Alert(detailMsg: $@"Error on language selection; (EXIT10000102)");
			}

		}

		private void Submit(LanguageCode langCode)
		{
			System.Windows.Forms.Application.DoEvents();

			Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
				try
				{
					App.NetClientSvc.SalesService.SubmitLanguage(langCode, out bool isServerResponded);

					if (isServerResponded == false)
						App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000101)");
				}
				catch (Exception ex)
				{
					App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000103)");
					App.Log.LogError(LogChannel, "", new Exception("(EXIT10000103)", ex), "EX01", "pgLanguage.Submit");
				}
			})));
			submitWorker.IsBackground = true;
			submitWorker.Start();
		}
	}
}
