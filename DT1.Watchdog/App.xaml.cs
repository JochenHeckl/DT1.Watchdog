using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using DT1.Watchdog.Common;
using DT1.Watchdog.Common.Logging;
using DT1.Watchdog.ViewModel;
using Xamarin.Forms;

namespace DT1.Watchdog
{
	public partial class App : Application
	{
        public App ( ContainerBuilder builder )
		{
            BootstrapApp(builder);

            InitializeComponent();

            MainPage = new DT1.Watchdog.MainPage();
            MainPage.BindingContext = Bootstrap.Container.Resolve<MainPageViewModel>();

            log = Bootstrap.Container.Resolve<ILog>();
            log.Debug("Starting DT1.Watchdog Application");

        }

        private void BootstrapApp(ContainerBuilder builder)
        {
            builder.RegisterType<MainPageViewModel>().SingleInstance();
            Bootstrap.Container = builder.Build();
        }

        protected override void OnStart ()
		{
            var bleService = Bootstrap.Container.Resolve<IBleScanService>();
            bleService.ScanForDevice();
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}

        private ILog log;
	}
}
