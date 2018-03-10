using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Runtime;
using System.Linq;
using System;
using Android.Util;
using System.Collections.Generic;
using DT1.Watchdog.Common.Logging;

namespace DT1.Watchdog.Service
{
    internal class DT1WatchdogScanCallback : ScanCallback
    {
        public DT1WatchdogScanCallback( DT1WatchDogService serviceIn, ILog logIn )
        {
            log = logIn;
            service = serviceIn;
        }

        public override void OnScanFailed( [GeneratedEnum] ScanFailure errorCode )
        {
            log.Debug( "Scan for Watchdog failed." );
            DT1WatchDogService.Scan = null;
        }

        public override void OnBatchScanResults( IList<ScanResult> results )
        {
            base.OnBatchScanResults(results);
        }

        public override void OnScanResult( [GeneratedEnum] ScanCallbackType callbackType, ScanResult result )
        {
            base.OnScanResult(callbackType, result);

            if ( IsWatchdogDevice( result.Device.Name ) )
            {
                log.Debug("Watchdog detected.");

                service.ConnectGatt(result.Device);
                service.StopScan();
            }
        }

        private bool IsWatchdogDevice( string name )
        {
            return
                !string.IsNullOrEmpty(name)
                && name.Split(':').First() == DT1WatchDogService.BleDeviceName;
        }

        private readonly ILog log;
        private DT1WatchDogService service;
    }
}