
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Autofac;
using DT1.Watchdog.Common.Logging;
using DT1.Watchdog.Droid.Logging;

namespace DT1.Watchdog.Droid
{
    [Activity(Label = "DT1.Watchdog", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App(SetupIoCContainer()));
        }


        private IContainer SetupIoCContainer()
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<AndroidLog>().As<ILog>().SingleInstance();

            return containerBuilder.Build();
        }
    }
}

