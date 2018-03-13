using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using DT1.Watchdog.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DT1.Watchdog
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SettingsPage : ContentPage
	{
		public SettingsPage()
		{
			InitializeComponent ();
			BindingContext = Bootstrap.Container.Resolve<SettingsPageViewModel>();
		}
	}
}