
using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Autofac;
using DT1.Watchdog.Common;
using DT1.Watchdog.Common.Logging;
using DT1.Watchdog.Droid.Logging;
using DT1.Watchdog.Droid.Service;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace DT1.Watchdog.Droid
{
	[Activity(
		Label = "DT1 Watchdog",
		Icon = "@drawable/icon",
		Theme = "@style/MainTheme",
		MainLauncher = true,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation )]
	public class MainActivity : FormsAppCompatActivity
	{
		protected override void OnCreate( Bundle bundle )
		{
			base.OnCreate( bundle );

			Forms.Init( this, bundle );
			LoadApplication( new App( SetupContainer() ) );

			UpdatePermissions();
			TestDeviceCapabilities();
		}

		private void UpdatePermissions()
		{
			RequestPermissions(
				new string[]
				{
					Manifest.Permission.Bluetooth,
					Manifest.Permission.BluetoothAdmin,
					Manifest.Permission.WakeLock,
					Manifest.Permission.SendSms,
					Manifest.Permission.ReadContacts,
					Manifest.Permission.AccessCoarseLocation
				}, 0 );
		}

		private void TestDeviceCapabilities()
		{
			var bleSupported = PackageManager.HasSystemFeature( PackageManager.FeatureBluetoothLe );
			var bluetoothService = GetSystemService( BluetoothService ) as BluetoothManager;

			if ( !bleSupported )
			{
				Toast.MakeText( this, EmbeddedResource.NoBle, ToastLength.Long ).Show();
				FinishAffinity();
				return;
			}

			if ( !bluetoothService.Adapter.IsEnabled )
			{
				var dialogBuilder = new AlertDialog.Builder( this );

				dialogBuilder
					.SetTitle( EmbeddedResource.EnableBluetoothTitle )
					.SetMessage( EmbeddedResource.EnableBluetoothBody )
					.SetNegativeButton( EmbeddedResource.No, NoBluetoothExitApp )
					.SetPositiveButton( EmbeddedResource.Yes, TurnOnBluetooth );

				dialogBuilder.Create().Show();
			}
		}

		private void TurnOnBluetooth( object sender, DialogClickEventArgs e )
		{
			var bluetoothService = GetSystemService( BluetoothService ) as BluetoothManager;
			bluetoothService.Adapter.Enable();
		}

		private void NoBluetoothExitApp( object sender, DialogClickEventArgs e )
		{
			FinishAffinity();
		}

		private ContainerBuilder SetupContainer()
		{
			var containerBuilder = new ContainerBuilder();

			containerBuilder.RegisterType<AndroidLog>().As<ILog>().SingleInstance();
			containerBuilder.RegisterType<BleScanServicePluginBluetoothLE>().As<IBleScanService>().SingleInstance();
			containerBuilder.RegisterType<DT1WatchdogDataServiceLocalStorage>().As<IDT1WatchdogDataService>().SingleInstance().WithParameter( TypedParameter.From<Context>( this ) );

			return containerBuilder;
		}
	}
}

