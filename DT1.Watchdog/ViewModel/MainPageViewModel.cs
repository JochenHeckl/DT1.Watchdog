using DT1.Watchdog.Command;
using DT1.Watchdog.Common.Logging;
using System.ComponentModel;
using System.Windows.Input;
using DT1.Watchdog.Common;
using System;
using Xamarin.Forms;

namespace DT1.Watchdog.ViewModel
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel(ILog logIn, IBleScanService scanServiceIn, IDT1WatchdogDataService dataServiceIn)
        {
            log = logIn;
            scanService = scanServiceIn;
            dataService = dataServiceIn;

            scanServiceIn.DeviceDetected += OnDeviceDetected;
        }

        public string SettingsLabel { get { return EmbeddedResource.SettingsLabel; } }
        public string DevicePresenceLabel { get { return ResolveDevicePresenceLabel(); } }

        public ICommand OpenSettingsCommand { get { return new OpenSettingsCommand(); } }
        public ICommand ScanNowCommand { get { return new ScanNowCommand(); } }

        private string ResolveDevicePresenceLabel()
        {
            var deviceName = scanService.BleDeviceName;

            if ( string.IsNullOrEmpty( deviceName ) )
            {
                return EmbeddedResource.NoDevicePresent;
            }
            else
            {
                return string.Format(EmbeddedResource.BleDevicePresentFormatName, deviceName);
            }
        }

        private void OnDeviceDetected(string deviceName)
        {
            NotifytPropertyChanged(() => DevicePresenceLabel);
        }

        private ILog log;
        private IBleScanService scanService;
        private IDT1WatchdogDataService dataService;
    }
}
