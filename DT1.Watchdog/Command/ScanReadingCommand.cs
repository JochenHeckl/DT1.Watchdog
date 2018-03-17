using DT1.Watchdog.Common.Logging;
using System;
using System.Windows.Input;
using DT1.Watchdog.Common;

namespace DT1.Watchdog.Command
{
	class ScanReadingCommand : ICommand
    {
        public event EventHandler CanExecuteChanged = delegate { };

		public ScanReadingCommand( IBleDeviceService deviceServiceIn )
		{
			deviceService = deviceServiceIn;
			deviceService.DeviceDetected += OnDeviceDetected;
		}

		private void OnDeviceDetected()
		{
			CanExecuteChanged( this, null );
		}

		public ILog Log { get; set; }
		
		public bool CanExecute(object parameter)
        {
            return deviceService.IsDeviceDetected && !deviceService.IsDeviceConnected;
        }

        public void Execute(object parameter)
        {
            Log.Debug("Executing Scan...");
			deviceService.ScanReadingsAsync();
		}

		private IBleDeviceService deviceService;
	}
}
