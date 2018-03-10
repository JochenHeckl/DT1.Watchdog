using System;

using Android.App;
using Android.Content;
using Android.OS;
using Java.Lang;

namespace DT1.Watchdog.Service
{
    static class WatchDogServiceScheduler
    {
        private static TimeSpan WatchDogServiceInterval = TimeSpan.FromSeconds(150);
        // private static readonly string WakeLockTag = "DT1WatchDogServicePartialWakeLockTag";
        // private static PowerManager.WakeLock WakeLock { get;set; }

        private static PowerManager PowerManager
        {
            get
            {
                return Application.Context.GetSystemService(Context.PowerService) as PowerManager;
            }
        }

        private static AlarmManager AlarmManager
        {
            get
            {
                return Application.Context.GetSystemService(Context.AlarmService) as AlarmManager;
            }
        }

        public static void StartWatchdog()
        {
            StopWatchdog();

            // WakeLock = PowerManager.NewWakeLock(WakeLockFlags.Partial, WakeLockTag);
            // WakeLock.Acquire();

            ScheduleWatchdog();
        }

        public static void ScheduleWatchdog()
        {
            //AlarmManager.SetExactAndAllowWhileIdle(
            //    AlarmType.ElapsedRealtimeWakeup,
            //    SystemClock.ElapsedRealtime() + Convert.ToInt64(WatchDogServiceInterval.TotalMilliseconds),
            //    MakeAlarmIntent(PendingIntentFlags.UpdateCurrent));

            var pendingIntent = MakeAlarmIntent(PendingIntentFlags.UpdateCurrent);
            var milliseconds = JavaSystem.CurrentTimeMillis() + Convert.ToInt64(WatchDogServiceInterval.TotalMilliseconds);
            AlarmManager.SetAlarmClock(new AlarmManager.AlarmClockInfo(milliseconds, null), pendingIntent);
        }

        public static void StopWatchdog()
        {
            if ( IsWatchdogServiceStarted() )
            {
                var alarmIntent = MakeAlarmIntent(PendingIntentFlags.NoCreate);

                if ( alarmIntent != null )
                {
                    alarmIntent.Cancel();
                }
            }

            //if ( WakeLock != null && WakeLock.IsHeld )
            //{
            //    WakeLock.Release();
            //    WakeLock = null;
            //}
        }

        public static bool IsWatchdogServiceStarted()
        {
            return MakeAlarmIntent(PendingIntentFlags.NoCreate) != null;
        }

        private static PendingIntent MakeAlarmIntent( PendingIntentFlags flags )
        {
            var intent = new Intent(Application.Context, typeof(DT1WatchDogService));
            intent.SetAction(DT1WatchDogService.IntentDT1WatchdogServiceScanForDevice);
            return PendingIntent.GetService(Application.Context, 0, intent, flags);
        }
    }
}