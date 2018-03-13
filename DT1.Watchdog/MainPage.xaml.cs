using Autofac;
using DT1.Watchdog.ViewModel;
using Xamarin.Forms;

namespace DT1.Watchdog
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
			BindingContext = Bootstrap.Container.Resolve<MainPageViewModel>();
		}
	}
}
