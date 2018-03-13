using System;
using System.Windows.Input;
using Autofac;
using Xamarin.Forms;

namespace DT1.Watchdog.Command
{
	public class OpenSettingsCommand : ICommand
    {
		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
			var navigation = Bootstrap.Container.Resolve<INavigation>();
			navigation.PushAsync( new SettingsPage() );
        }
	}
}
