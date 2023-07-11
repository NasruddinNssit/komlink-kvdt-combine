using NssIT.Kiosk.AppDecorator.Config.ConfigConstant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Config
{
	public class Setting
	{
		private bool _isDebugMode = false;
		private bool _disablePrinterTracking = false;

		private string _appVersion = "*VerDev*";
		private string _logDbConnectionStr = null;
		private string _webServiceURL = null;
		private string _webApiURL = null;
		private string _eWalletWebApiBaseURL = null;
		private string _ipAddress = null;
		private string _payMethod = "C";
		private int _localServicePort = -1;
		private string _kioskId = null;
		private bool _noCardSettlement = false;

		private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
		private static Setting _setting = null;

		private Setting()
		{ }

		public static Setting GetSetting()
		{
			if (_setting == null)
			{
				try
				{
					_manLock.WaitAsync().Wait();
					if (_setting == null)
					{
						_setting = new Setting();
					}
					return _setting;
				}
				finally
				{
					if (_manLock.CurrentCount == 0)
						_manLock.Release();
				}
			}
			else
				return _setting;
		}

		public static void Shutdown()
		{
			try
			{
				_manLock.WaitAsync().Wait();
				_setting = null;
			}
			finally
			{
				if (_manLock.CurrentCount == 0)
					_manLock.Release();
			}
		}

		/// <summary>
		/// Version string not allows Blank/NULL.
		/// </summary>
		public string ApplicationVersion
        {
			get => _appVersion;
            set
            {
				if (value?.Trim().Length > 0)
					_appVersion = value.Trim();
			}
        }

		public string LogDbConnectionStr
		{
			get
			{
				return _logDbConnectionStr;
			}
			set
			{
				_logDbConnectionStr = string.IsNullOrWhiteSpace(value) ? null: value.Trim();
			}
		}

		public bool IsDebugMode
		{
			get
			{
				return _isDebugMode;
			}
			set
			{
				_isDebugMode = value;
			}
		}

		public bool DisablePrinterTracking
		{
			get
			{
				return _disablePrinterTracking;
			}
			set
			{
				_disablePrinterTracking = value;
			}
		}

		//public string WebServiceURL
		//{
		//	get
		//	{
		//		return _webServiceURL;
		//	}
		//	set
		//	{
		//		_webServiceURL = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
		//	}
		//}

		public string KioskId
		{
            get
            {
				return _kioskId;
			}
            set
            {
				_kioskId = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
			}
        }

		public string WebApiURL
		{
			get
			{
				return _webApiURL;
			}
			set
			{
				_webApiURL = FixWebApiURL(value);
				EvaWebApiInfo();
			}
		}

		private string FixWebApiURL(string webApiURL)
        {
			string retUrl = webApiURL;

			retUrl = string.IsNullOrEmpty(retUrl) ? "" : retUrl.Trim();

			if (retUrl.Substring(retUrl.Length - 1, 1).Equals(@"/") || retUrl.Substring(retUrl.Length - 1, 1).Equals(@"\"))
				retUrl = retUrl;
			else
				retUrl = retUrl + "/";

			return retUrl;

		}

		public bool IsLiveWebApi => (WebAPICode == WebAPISiteCode.Live);
		public WebAPISiteCode WebAPICode { get; private set; } = WebAPISiteCode.Unknown;

		/// <summary>
		/// 'Boost/Touch n Go' minimum waiting period in seconds. This period used for waiting response after 2D Barcode has shown.
		/// </summary>
		public int BTnGMinimumWaitingPeriod { get; set; } = 90;

		/// <summary>
		/// For Testing only
		/// </summary>
		public string EWalletWebApiBaseURL
		{
			get
			{
				return _eWalletWebApiBaseURL;
			}
			set
			{
				_eWalletWebApiBaseURL = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
			}
		}

		/// <summary>
		/// Assign IP for Local Machine/PC
		/// </summary>
		public string IPAddress
		{
			get
			{
				return _ipAddress;
			}
			set
			{
				_ipAddress = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
			}
		}

		public string PayMethod
		{
			get
			{
				return _payMethod;
			}
			set
			{
				_payMethod = string.IsNullOrWhiteSpace(value) ? "C" : value.Trim();
			}
		}

		public bool NoCardSettlement
        {
			get => _noCardSettlement;
            set
            {
				_noCardSettlement = value;
			}
        }

		public int LocalServicePort
		{
			get
			{
				return _localServicePort;
			}
			set
			{
				_localServicePort = value;
			}
		}

		/// <summary>
		/// Refer to KTM Trip Query
		/// </summary>
		public string PurchaseChannel { get; } = "TVM";
		//public string PurchaseChannel { get; } = "TVM";

		public string TVMKey { get; set; }
		public string AesEncryptKey { get; set; }
		public string HashSecretKey { get; set; }
		public string TimeZoneId { get; set; }

		public static string GetLocalIPAddress()
		{
			UnicastIPAddressInformation ucIP = null;

			NetworkInterface[] netIntfArr = NetworkInterface.GetAllNetworkInterfaces();

			IEnumerable<UnicastIPAddressInformation[]> unicastIPList
				= from ntInf in netIntfArr
				  where (ntInf.OperationalStatus == OperationalStatus.Up) &&
				  (ntInf.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ntInf.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
				  select ntInf.GetIPProperties().UnicastAddresses.ToArray();

			foreach (UnicastIPAddressInformation[] unicIPArr in unicastIPList)
			{
				UnicastIPAddressInformation ucIPX = (from ip in unicIPArr
													 where (ip != null && ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
													 select ip).Take(1).ToArray()[0];

				if (ucIPX != null)
				{
					ucIP = ucIPX;
					break;
				}
			}

			if (ucIP != null)
			{
				return ucIP.Address.ToString();
			}
			else
			{
				return null;
			}

			//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
			//var host = Dns.GetHostEntry(Dns.GetHostName());
			//foreach (var ip in host.AddressList)
			//{
			//	if (ip.AddressFamily == AddressFamily.InterNetwork)
			//	{
			//		return ip.ToString();
			//	}
			//}
			//throw new Exception("No network adapters with an IPv4 address in the system!");
		}

		public void GetWebApiSummary(out bool isLiveWebApi, out string webApiTag, out WebAPISiteCode webAPISiteCode)
        {
			isLiveWebApi = IsLiveWebApi;
			webApiTag = WebAPICode.ToString();
			webAPISiteCode = WebAPICode;
		}

		private void EvaWebApiInfo()
		{
			WebAPICode = WebAPISiteCode.Live;

			if (string.IsNullOrWhiteSpace(_webApiURL) == false)
			{
				if (_webApiURL.IndexOf(@"ktmb-staging-api.azurewebsites.net", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					WebAPICode = WebAPISiteCode.Staging;
				}
				else if (_webApiURL.IndexOf(@"192.168.0.126/KTMBWebAPI", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					WebAPICode = WebAPISiteCode.Staging;
				}
				else if (_webApiURL.IndexOf(@"91d90a7bc859.sn.mynetname.net:9000/KTMBWebAPI", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					WebAPICode = WebAPISiteCode.Staging;
				}
				else if (_webApiURL.IndexOf(@"ktmb-tvm-api-training.azurewebsites.net", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					WebAPICode = WebAPISiteCode.Training;
				}
				else if (_webApiURL.IndexOf(@"ktmb-dev-api.azurewebsites.net", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					WebAPICode = WebAPISiteCode.Development;
				}
				else if (_webApiURL.IndexOf(@"https://localhost", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					WebAPICode = WebAPISiteCode.Local_Host;
				}
				else if (_webApiURL.IndexOf(@"https://127.0.0.1", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					WebAPICode = WebAPISiteCode.Local_Host;
				}
			}
		}
	}
}
