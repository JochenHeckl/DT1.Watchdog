using System;
using System.Windows.Input;
using Autofac;
using Xamarin.Forms;

namespace DT1.Watchdog.Command
{
	class OpenSettingsCommand : ICommand
    {
		public event EventHandler CanExecuteChanged;

		public INavigationAccess NavigationAccess { get; set; }

		public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
			NavigationAccess.Navigation.PushAsync( new SettingsPage() );
        }
	}
}
