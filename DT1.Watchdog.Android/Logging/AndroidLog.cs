using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using DT1.Watchdog.Common.Logging;

namespace DT1.Watchdog.Droid.Logging
{
    class AndroidLog : ILog
    {
        private static readonly string DT1WatchdogAndroidTag = "DT1.Watchdog.Android";

        public void Debug(string format, params object[] parameters)
        {
            Log.Debug( DT1WatchdogAndroidTag, format, parameters );
        }
    }
}