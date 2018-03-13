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
    class DT1WatchdogDataServiceLocalStorage : IDT1WatchdogDataService
    {
        public DT1WatchdogDataServiceLocalStorage()
        {
            
        }

        public string MostRecentDT1DeviceDeviceAddress { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTimeOffset LastValidReading { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public BleConnectionStatus DT1HardwareConnectionStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string WatchdogDeviceName
        {
            get;
            set;
        }

        public void PersistReading(GlucoseReading reading)
        {
            throw new NotImplementedException();
        }
    }
}