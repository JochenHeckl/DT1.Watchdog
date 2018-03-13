using DT1.Watchdog.Common.Logging;
using System;
using System.Windows.Input;
using DT1.Watchdog.Common;

namespace DT1.Watchdog.Command
{
	class ScanForDeviceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged = delegate { };

        public ScanForDeviceCommand( IBleDeviceService scanServiceIn, ILog logIn )
        {
			log = logIn;
			scanService = scanServiceIn;
		}

        public bool CanExecute(object parameter)
        {
            return scanService.IsScanningForDevice;
        }

        public void Execute(object parameter)
        {
            log.Debug("Executing Scan...");
			scanService.ScanForDevice();
		}

		private ILog log;
		private IBleDeviceService scanService;
	}
}
