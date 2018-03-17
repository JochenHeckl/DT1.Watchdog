using DT1.Watchdog.Command;
using DT1.Watchdog.Common.Logging;
using System.ComponentModel;
using System.Windows.Input;
using DT1.Watchdog.Common;
using System;
using Xamarin.Forms;
using Autofac;

namespace DT1.Watchdog.ViewModel
{
    class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel( IBleDeviceService bleDeviceServiceIn )
        {
			BleDeviceService = bleDeviceServiceIn;
			BleDeviceService.DeviceDetected += OnDeviceDetected;
			BleDeviceService.DeviceConnectionStateChanged += OnDeviceConnectionChanged;

		}

		public string SettingsLabel { get { return EmbeddedResource.SettingsLabel; } }
        public string DeviceStatus { get { return ResolveDeviceStatus(); } }
		
		public OpenSettingsCommand OpenSettingsCommand { get; set; }
		public ScanForDeviceCommand ScanForDeviceCommand { get; set; }
		public ScanReadingCommand ScanReadingCommand { get; set; }

		public IDataService DataService { get; set; }
		public IBleDeviceService BleDeviceService { get; private set; }
		
		private string ResolveDeviceStatus()
        {
			if( BleDeviceService.IsDeviceDetected )
			{
				var mostRecentReading = DataService.MostRecentReading;
				var knownCharge = (mostRecentReading != null) && (mostRecentReading.ScanTime - DateTimeOffset.Now) < TimeSpan.FromHours( 1 );
				var charge = knownCharge ? mostRecentReading.PerCentCharge : 0;

				if ( BleDeviceService.IsDeviceConnected )
				{
					return string.Format( EmbeddedResource.BleDeviceConnectedFormatName, DataService.WatchdogDeviceName );
				}
				
				if ( knownCharge )
				{
					return string.Format( EmbeddedResource.BleDevicePresentFormatNamePerCentCharge, DataService.WatchdogDeviceName, mostRecentReading.PerCentCharge );
				}
				else
				{
					return string.Format( EmbeddedResource.BleDevicePresentFormatName, DataService.WatchdogDeviceName );
				}
			}

			return EmbeddedResource.NoDevicePresent;
		}

        private void OnDeviceDetected()
        {
			NotifytViewModelChanged();
        }

		private void OnDeviceConnectionChanged( bool isConnected )
		{
			NotifytViewModelChanged();
		}
	}
}
