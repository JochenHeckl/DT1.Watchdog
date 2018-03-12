using DT1.Watchdog.Command;
using DT1.Watchdog.Common.Logging;
using System.ComponentModel;
using System.Windows.Input;
using DT1.Watchdog.Common;
using System;

namespace DT1.Watchdog.ViewModel
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public MainPageViewModel( ILog logIn, IBleScanService scanServiceIn, IDT1WatchdogDataService dataServiceIn )
        {
            log = logIn;
            scanService = scanServiceIn;
            dataService = dataServiceIn;

            scanServiceIn.DeviceDetected += OnDeviceDetected;

            DevicePresence = dataService.GetDevicePresentString(scanService.BleDeviceName);
        }

        public string DevicePresence { get; set; }

        public ICommand ScanNowCommand
        {
            get
            {
                return new ScanNowCommand();
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnDeviceDetected(string deviceName)
        {
            DevicePresence = dataService.GetDevicePresentString(scanService.BleDeviceName);
            OnPropertyChanged( nameof( DevicePresence ) );
        }

        private ILog log;
        private IBleScanService scanService;
        private IDT1WatchdogDataService dataService;
    }
}
