using DT1.Watchdog.Common.Logging;
using System;
using System.Windows.Input;
using DT1.Watchdog.Common;

namespace DT1.Watchdog.Command
{
	class ScanForDeviceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged = delegate { };

        public ScanForDeviceCommand( IBleDeviceService scanServiceIn )
        {
			scanService = scanServiceIn;
			scanService.DeviceDetected += OnDeviceDetected;
		}

		public ILog Log { get; set; }

		private void OnDeviceDetected()
		{
			scanService.DeviceDetected -= OnDeviceDetected;
			CanExecuteChanged( this, new EventArgs() );
		}

		public bool CanExecute(object parameter)
        {
            return !scanService.IsDeviceDetected && !scanService.IsScanningForDevice;
        }

        public void Execute(object parameter)
        {
            Log.Debug("Searching for available devices...");
			scanService.ScanForDevice();
		}

		private IBleDeviceService scanService;
	}
}
