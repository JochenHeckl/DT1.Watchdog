using DT1.Watchdog.Common.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

using Autofac;
using IContainer = Autofac.IContainer;
using DT1.Watchdog.Common;

namespace DT1.Watchdog.Command
{
    class ScanNowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged = delegate { };

        public ScanNowCommand()
        {
            log = Bootstrap.Container.Resolve<ILog>();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            log.Debug("Executing Scan...");
            var scanService = Bootstrap.Container.Resolve<IBleScanService>();
            scanService.ScanReadings();
        }

        private ILog log;
    }
}
