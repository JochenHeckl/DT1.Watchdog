using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using DT1.Watchdog.Common;

namespace DT1.Watchdog.Droid.Service
{
    class DataServiceLocalStorage : IDataService
    {
		private static readonly string DT1WatchdogDataFile = "DT1.Watchdog.Data";
		private static readonly string WatchdogDeviceNameKey = "WatchdogDeviceName";

		public DataServiceLocalStorage()
        {

		}

        public GlucoseReading MostRecentValidReading { get; private set; }
		public GlucoseReading MostRecentReading { get; private set; }
		public BleConnectionStatus DT1HardwareConnectionStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string MostRecentDT1DeviceDeviceAddress { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string WatchdogDeviceName
        {
			get
			{
				var contextPref = Application.Context.GetSharedPreferences( DT1WatchdogDataFile, FileCreationMode.Private );
				return contextPref.GetString( WatchdogDeviceNameKey, String.Empty );
			}
			set
			{
				var contextPref = Application.Context.GetSharedPreferences( DT1WatchdogDataFile, FileCreationMode.Private );
				var contextEdit = contextPref.Edit();

				contextEdit.PutString( WatchdogDeviceNameKey, value );
				contextEdit.Commit();
			}
		}

        public void PersistReading(GlucoseReading reading)
        {
            throw new NotImplementedException();
        }
    }
}