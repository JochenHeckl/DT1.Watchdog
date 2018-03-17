using System;
using System.Windows.Input;
using DT1.Watchdog.Common;
using DT1.Watchdog.ViewModel;

namespace DT1.Watchdog.Command
{
	class ApplySettingsCommand : ICommand
	{
		public SettingsPageViewModel SettingsPageViewModel { get; set; }
		public IBleDeviceService DeviceService { get; set; }
		public IDataService DataService { get; set; }

		public event EventHandler CanExecuteChanged;

		public bool CanExecute( object parameter )
		{
			return (dataService != null && viewModel != null);
		}

		public void Execute( object parameter )
		{
			if( dataService.WatchdogDeviceName != viewModel.WatchdogBleDeviceName )
			{
				dataService.WatchdogDeviceName = viewModel.WatchdogBleDeviceName;
				scanService.ScanForDevice();
			}
		}

		private IDataService dataService;
		private SettingsPageViewModel viewModel;
		private IBleDeviceService scanService;
	}
}
