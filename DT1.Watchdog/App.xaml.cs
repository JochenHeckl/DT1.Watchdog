using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Util;
using DT1.Watchdog.Command;
using DT1.Watchdog.Common;
using DT1.Watchdog.Common.Logging;
using DT1.Watchdog.ViewModel;
using Plugin.BluetoothLE;
using Xamarin.Forms;

namespace DT1.Watchdog
{
	public partial class App : Application, INavigationAccess
	{
        // needed to make Xamarin Preview work
        public App()
        {
            InitializeComponent();
        }

        public App ( ContainerBuilder builder )
		{
			BootstrapApp( builder );

            InitializeComponent();
			MainPage = new NavigationPage( new MainPage() );
		}

		public INavigation Navigation => MainPage.Navigation;

        private void BootstrapApp(ContainerBuilder builder)
        {
            builder.RegisterType<MainPageViewModel>().SingleInstance().PropertiesAutowired();
			builder.RegisterType<SettingsPageViewModel>().SingleInstance().PropertiesAutowired();

			builder.RegisterType<ApplySettingsCommand>();
			builder.RegisterType<OpenSettingsCommand>().PropertiesAutowired();
			builder.RegisterType<ScanForDeviceCommand>();

			builder.RegisterInstance( this ).As<INavigationAccess>();

            Bootstrap.Container = builder.Build();
        }

        protected override void OnStart ()
		{
            base.OnStart();

			var bleService = Bootstrap.Container.Resolve<IBleDeviceService>();
			bleService.ScanForDevice();
        }

		protected override void OnSleep ()
		{
            base.OnSleep();
		}

		protected override void OnResume ()
		{
            base.OnResume();
        }

        // private ILog log;
	}
}
