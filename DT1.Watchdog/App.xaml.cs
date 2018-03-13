using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using DT1.Watchdog.Common;
using DT1.Watchdog.Common.Logging;
using DT1.Watchdog.ViewModel;
using Plugin.BluetoothLE;
using Xamarin.Forms;

namespace DT1.Watchdog
{
	public partial class App : Application
	{
        // needed to make Xamarin Preview work
        public App()
        {
            InitializeComponent();
        }

        public App ( ContainerBuilder builder )
		{
            BootstrapApp(builder);

            InitializeComponent();

			MainPage = new NavigationPage( new MainPage() );
        }

        private void BootstrapApp(ContainerBuilder builder)
        {
            builder.RegisterType<MainPageViewModel>().SingleInstance();
			builder.RegisterType<SettingsPageViewModel>().SingleInstance();
			builder.Register( (x) => MainPage.Navigation ).As<INavigation>();

            Bootstrap.Container = builder.Build();
        }

        protected override void OnStart ()
		{
            base.OnStart();

            var bleService = Bootstrap.Container.Resolve<IBleScanService>();
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
