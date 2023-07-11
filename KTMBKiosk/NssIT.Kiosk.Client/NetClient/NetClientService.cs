using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.Common.AppService.Network.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.NetClient
{
	public class NetClientService : IDisposable
	{
		private string _logChannel = "NetClientService";

		private NssIT.Kiosk.Log.DB.DbLog _log = null;
		private INetMediaInterface _netInterface;

		public NetClientCashPaymentService CashPaymentService { get; private set; } = null;
		public NetClientSalesService SalesService { get; private set; } = null;
		public NetClientSalesServiceV2 SalesServiceV2 { get; private set; } = null;
		public NetClientBTnGService BTnGService { get; private set; } = null;
		public NetClientStatusMonitorService StatusMonitorService { get; private set; } = null;

		public NetClientService()
		{
			_netInterface = new LocalTcpService(App.SysParam.PrmClientPort);

			//CashPaymentService = new NetClientCashPaymentService(_netInterface);
			SalesService = new NetClientSalesService(_netInterface);
			SalesServiceV2 = new NetClientSalesServiceV2(_netInterface);
			BTnGService = new NetClientBTnGService(_netInterface);
			StatusMonitorService = new NetClientStatusMonitorService(_netInterface);
		}

		public INetMediaInterface NetInterface { get => _netInterface; }

		private bool _disposed = false;
		public void Dispose()
		{
			if (_disposed == false)
			{
				_disposed = true;

				try
				{
					CashPaymentService?.Dispose();
				}
				catch { }
				try
				{
					SalesServiceV2?.Dispose();
				}
				catch { }
				try
				{
					BTnGService?.Dispose();
				}
				catch { }
				try
				{
					StatusMonitorService?.Dispose();
				}
				catch { }

				CashPaymentService = null;
				SalesService = null;
				SalesServiceV2 = null;
				BTnGService = null;
				StatusMonitorService = null;

				if (_netInterface != null)
				{
					try
					{
						_netInterface.ShutdownService();
					}
					catch { }
					try
					{
						_netInterface.Dispose();
					}
					catch { }
					_netInterface = null;
				}
			}
		}
	}
}
