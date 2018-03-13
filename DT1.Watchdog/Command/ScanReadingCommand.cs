using DT1.Watchdog.Common.Logging;
using System;
using System.Windows.Input;
using DT1.Watchdog.Common;

namespace DT1.Watchdog.Command
{
	class ScanReadingCommand : ICommand
    {
        public event EventHandler CanExecuteChanged = delegate { };

		public ILog Log { get; set; }
		public IBleDeviceService ScanService { get; set; }

		public bool CanExecute(object parameter)
        {
            return ScanService.IsDeviceDetected;
        }

        public void Execute(object parameter)
        {
            Log.Debug("Executing Scan...");
			ScanService.ScanForDevice();
		}
	}
}
