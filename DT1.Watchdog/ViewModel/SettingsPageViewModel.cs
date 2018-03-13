using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Autofac;
using DT1.Watchdog.Command;
using DT1.Watchdog.Common;
using Xamarin.Forms;

namespace DT1.Watchdog.ViewModel
{
	class SettingsPageViewModel
	{
		public IDataService DataService { get; set; }
		public ICommand ApplySettingsCommand { get; set; }

		public string WatchdogBleDeviceName
		{
			get
			{
				return DataService.WatchdogDeviceName;
			}
		}
	}
}
