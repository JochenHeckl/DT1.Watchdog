using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using DT1.Watchdog.Command;

namespace DT1.Watchdog.ViewModel
{
    class SettingsPageViewModel
    {
		public ICommand ApplySettingsCommand
		{
			get { return new ApplySettingsCommand(); }
		}
	}
}
