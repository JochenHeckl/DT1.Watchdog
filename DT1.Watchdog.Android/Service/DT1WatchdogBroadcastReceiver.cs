using Android.App;
using Android.Content;

namespace DT1.Watchdog.Service
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { DT1WatchDogService.IntentIncommingData })]

    public class DT1WatchdogBroadcastReceiver : BroadcastReceiver
    {
        public delegate void DT1DataBroadcastDelegate();
        public event DT1DataBroadcastDelegate IncommingData = delegate { };

        public override void OnReceive( Context context, Intent intent )
        {
            switch ( intent.Action )
            {
                case DT1WatchDogService.IntentIncommingData:
                    {
                        IncommingData();
                        break;
                    }
            }
        }
    }
}