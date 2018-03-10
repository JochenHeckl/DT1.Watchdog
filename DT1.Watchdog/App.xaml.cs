using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using DT1.Watchdog.Common.Logging;
using Xamarin.Forms;

namespace DT1.Watchdog
{
	public partial class App : Application
	{

        public App ( IContainer ioCContainerIn )
		{
            ioCContainer = ioCContainerIn;

            log = ioCContainer.Resolve<ILog>();
            log.Debug("Starting DT1.Watchdog Application");


            InitializeComponent();

			MainPage = new DT1.Watchdog.MainPage();
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}

        private IContainer ioCContainer;
        private ILog log;
	}
}
