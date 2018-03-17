using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DT1.Watchdog.Common;
using DT1.Watchdog.Common.Logging;
using Plugin.BluetoothLE;

namespace DT1.Watchdog.Droid.Service
{
	class BleScanServicePluginBluetoothLE : IBleDeviceService
	{
		// 0000ffe0-0000-1000-8000-00805f9b34fb
		// public static readonly Java.Util.UUID DT1WatchdogUUID = new Java.Util.UUID( 281337537761280, -9223371485494954757 );
		public static readonly Guid DT1WatchdogSericeUuid = new Guid( "0000ffe0-0000-1000-8000-00805f9b34fb" );

		// 0000ffe1-0000-1000-8000-00805f9b34fb
		// public static readonly Java.Util.UUID DT1WatchdogDataCharacteristicUUID = new Java.Util.UUID( 281341832728576, -9223371485494954757 );
		public static readonly Guid DT1WatchdogDataCharacteristicUUID = new Guid( "0000ffe1-0000-1000-8000-00805f9b34fb" );

		public event Action DeviceDetected = delegate { };
		public event Action<bool> DeviceConnectionStateChanged = delegate { };

		public BleScanServicePluginBluetoothLE( IDataService dataServiceIn, ILog logIn )
		{
			dataService = dataServiceIn;
			log = logIn;
		}

		public bool IsScanningForDevice
		{
			get
			{
				return CrossBleAdapter.Current.IsScanning;
			}
		}

		public bool IsDeviceDetected
		{
			get
			{
				return watchdogDevice != null;
			}
		}

		public bool IsDeviceConnected
		{
			get
			{
				return (watchdogDevice != null) && watchdogDevice.Status == ConnectionStatus.Connected;
			}
		}

		public string DeviceName
		{
			get
			{
				if ( watchdogDevice == null )
				{
					throw new InvalidOperationException( "DeviceName is not available before a device was detected." );
				}

				return watchdogDevice.Name;
			}

		}


		public void ScanForDevice()
		{
			if ( CrossBleAdapter.Current.Status == AdapterStatus.PoweredOn )
			{
				CrossBleAdapter.Current.Scan().Subscribe( FilterWatchdogDevice );
			}
			else
			{
				CrossBleAdapter.Current.ScanWhenAdapterReady().Subscribe( FilterWatchdogDevice );
			}
		}

		public async Task ScanReadingsAsync()
		{
			if ( watchdogDevice == null )
			{
				throw new InvalidOperationException( "ScanReadings can only be executed after a device was detected." );
			}

			if ( (watchdogDevice.Status == ConnectionStatus.Connected) || (watchdogDevice.Status == ConnectionStatus.Connecting) )
			{
				return;
			}

			await watchdogDevice.Connect();

			var service = await watchdogDevice.GetKnownService( DT1WatchdogSericeUuid );

			service.WhenCharacteristicDiscovered().Subscribe( async x =>
			{
				if ( x.Uuid == DT1WatchdogDataCharacteristicUUID )
				{
					if( await x.EnableNotifications() )
					{
						x.WhenNotificationReceived().Subscribe( (result) =>
						{
							result.Characteristic.DisableNotifications();
							result.Characteristic.Service.Device.CancelConnection();

							var reading = GlucoseReading.ParseRawCharacteristicData( result.Data );
						} );
					}
				}
			} );
		}

		private void FilterWatchdogDevice( IScanResult scanResult )
		{
			if ( scanResult.Device.Name == dataService.WatchdogDeviceName )
			{
				if ( watchdogDevice != scanResult.Device )
				{
					watchdogDevice = scanResult.Device;

					watchdogDevice.WhenStatusChanged().Subscribe( x =>
					{
						log.Debug( "{0} device state changed: {1}", watchdogDevice.Name, x );
						DeviceConnectionStateChanged( x == ConnectionStatus.Connected );
					} );

					DeviceDetected();
				}
			}
		}

		private ILog log;
		private IDataService dataService;
		private IDevice watchdogDevice;
	}
}