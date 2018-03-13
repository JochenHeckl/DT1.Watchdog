using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Autofac;
using Xamarin.Forms;

namespace DT1.Watchdog.Command
{
	class ApplySettingsCommand : ICommand
	{
		public event EventHandler CanExecuteChanged;

		public bool CanExecute( object parameter )
		{
			return true;
		}

		public void Execute( object parameter )
		{
			var navigation = Bootstrap.Container.Resolve<INavigation>();
		}
	}
}
