using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DT1.Watchdog.Common;
using Plugin.BluetoothLE;

namespace DT1.Watchdog.Droid.Service
{
    class BleScanServicePluginBluetoothLE : IBleScanService
    {
        public event Action<string> DeviceDetected = delegate { };
        public event Action<string> ScanDataReceived = delegate { };

        private IDT1WatchdogDataService dataService;
        private IDevice watchdogDevice;


        public BleScanServicePluginBluetoothLE( IDT1WatchdogDataService dataServiceIn)
        {
            dataService = dataServiceIn;
        }

        public string BleDeviceName
        {
            get { return watchdogDevice?.Name; }
        }

        public void ScanForDevice()
        {
            CrossBleAdapter.Current.ScanWhenAdapterReady().Subscribe(FilterWatchdogDevice);
        }

        public async Task<string> ScanReadings()
        {
            if( watchdogDevice != null)
            {
                await watchdogDevice.Connect();
            }

            return "connected";
        }

        // Once finding the device/scanresult you want
        

        //Device.WhenAnyCharacteristicDiscovered().Subscribe(characteristic => {
        //    // read, write, or subscribe to notifications here
        //    var result = await characteristic.Read(); // use result.Data to see response
        //    await characteristic.Write(bytes);

        //    characteristic.EnableNotifications();
        //    characteristic.WhenNotificationReceived().Subscribe(result => {
        //        //result.Data to get at response
        //    });
        //});

        private void FilterWatchdogDevice( IScanResult scanResult )
        {
            if (scanResult.Device.Name == dataService.WatchdogDeviceName)
            {
                if (watchdogDevice != scanResult.Device)
                {
                    watchdogDevice = scanResult.Device;
                    DeviceDetected(watchdogDevice.Name);
                }
            }
        }
    }
}