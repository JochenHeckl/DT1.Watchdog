using System;
using Android.Bluetooth;
using Android.App;
using Android.Content;
using Android.Bluetooth.LE;
using Android.Telephony;
using Android.Media;
using Android.OS;
using Android.Util;

namespace DT1.Watchdog.Service
{
    [Service]
    public class DT1WatchDogService : IntentService
    {
        public enum AlertType
        {
            Hypoglycemia,
            PredictedHypoglycemia,
            Hyperglycemia,
            PredictedHyperglycemia,
            LowBattery,
            MissingReading,
        };

        public const string IntentIncommingData = "IntentIncommingData";
        public static readonly string BleDeviceName = "DT1Watchdog";

        // 0000ffe0-0000-1000-8000-00805f9b34fb
        public static readonly Java.Util.UUID DT1WatchdogUUID = new Java.Util.UUID(281337537761280, -9223371485494954757);

        // 0000ffe1-0000-1000-8000-00805f9b34fb
        public static readonly Java.Util.UUID DT1WatchdogDataCharacteristicUUID = new Java.Util.UUID(281341832728576, -9223371485494954757);

        public static readonly string IntentDT1WatchdogServiceAcknowledgeAlert = "IntentDT1WatchdogServiceAcknowledgeAlert";
        public static readonly string IntentDT1WatchdogServiceScanForDevice = "IntentDT1WatchdogServiceScanForDevice";
        public static readonly string IntentDT1ManualScanForDevice = "IntentDT1ManualScanForDevice";


        internal static readonly int TargetGlucoseLevel = 110;
        public static readonly int NonConditionalHypoglycemiaThresholdmgdl = 50;
        public static readonly int ConditionalHypoglycemiaThresholdmgdl = 80;
        public static readonly int HyperglycemiaThresholdmgdl = 170;
        public static readonly int LowPowerThresholdPerCent = 20;
        public readonly double MissingReadingTimeoutMinutes = 15;

        public static readonly TimeSpan PredictionInterval = TimeSpan.FromMinutes(10);

        public static ScanCallback Scan { get; internal set; }
        public static BluetoothGatt Gatt { get; internal set; }

        private static MediaPlayer dt1WatchdogAudioAlert;
        private static PowerManager.WakeLock AlertWakeLock;
        
        protected override void OnHandleIntent( Intent intent )
        {
            InitAudio();
            CheckForMissingReading();

            if ( intent.Action == IntentDT1WatchdogServiceAcknowledgeAlert )
            {
                HandleIntentAcknowledgeAlert();
            }

            if ( intent.Action == IntentDT1WatchdogServiceScanForDevice )
            {
                HandleScheduledIntentScanForDevice();
            }

            if ( intent.Action == IntentDT1ManualScanForDevice )
            {
                HandleIntentScanForService();
            }
        }

        public void TriggerAlert( AlertType alertType, String optionalText, bool isConditional )
        {
            var alertText = alertType.ToString();

            if ( !string.IsNullOrEmpty(optionalText) )
            {
                alertText = alertText + System.Environment.NewLine + optionalText;
            }

            DT1WatchdogData.SetAlert(alertText);

            if ( ( !isConditional ) || ( DT1WatchdogData.MuteAlertTimeout <= DateTimeOffset.UtcNow ) )
            {
                EnterAlertMode();

                switch ( DT1WatchdogData.AlertNotificationType )
                {
                    case DT1WatchdogData.AlertNotification.SMSAlert:
                        {
                            if ( !string.IsNullOrEmpty(DT1WatchdogData.SMSAlertContact) )
                            {
                                SmsManager.Default.SendTextMessage(DT1WatchdogData.SMSAlertContact, null, alertText, null, null);

                                if ( !isConditional )
                                {
                                    // non conditional trigger audio also
                                    dt1WatchdogAudioAlert.Start();
                                }
                            }
                            else
                            {
                                // fallback trigger audio alert
                                dt1WatchdogAudioAlert.Start();
                            }

                            break;
                        }

                    case DT1WatchdogData.AlertNotification.AudioAlert:
                        {
                            dt1WatchdogAudioAlert.Start();
                            break;
                        }
                }
            }
        }

        public void TriggerValidInRangeReading()
        {
            ClearAlert();
        }

        private void HandleIntentAcknowledgeAlert()
        {
            ClearAlert();

            DT1WatchdogData.MuteAlertTimeout = DateTimeOffset.UtcNow + DT1WatchdogData.DeferAlertInterval;
        }

        private static void ClearAlert()
        {
            DT1WatchdogData.ClearAlert();

            if ( dt1WatchdogAudioAlert != null && dt1WatchdogAudioAlert.IsPlaying )
            {
                dt1WatchdogAudioAlert.Pause();
            }

            ExitAlertMode();
        }

        private void EnterAlertMode()
        {
            if ( AlertWakeLock == null )
            {
                PowerManager powerManager = (PowerManager) GetSystemService(PowerService);
                AlertWakeLock = powerManager.NewWakeLock(WakeLockFlags.Full | WakeLockFlags.AcquireCausesWakeup, nameof(DT1WatchDogService));
            }

            if ( !AlertWakeLock.IsHeld )
            {
                AlertWakeLock.Acquire();

                var watchDogIntent = new Intent(this, typeof(MainActivity));
                StartActivity(watchDogIntent);
            }
        }

        private static void ExitAlertMode()
        {
            if ( AlertWakeLock != null && AlertWakeLock.IsHeld )
            {
                AlertWakeLock.Release();
            }
        }

        private void HandleScheduledIntentScanForDevice()
        {
            Log.Debug(Logging.WatchdogTag, "Entering DT1WatchdogService:HandleScheduledIntentScanForDevice()");

            // reschedule
            WatchDogServiceScheduler.ScheduleWatchdog();

            HandleIntentScanForService();
        }

        private void HandleIntentScanForService()
        {
            // do not use Gatt.Connect() here to avoid background processing
            // we want to pick up the connection asap before the device shuts down again.

            if ( Gatt != null )
            {
                var device = Gatt.Device;
                Gatt.Close();

                Gatt = device.ConnectGatt(this, false, new DT1WatchDogGattCallback(this));
            }
            else
            {
                StartScan();
            }
        }

        public void ConnectGatt( BluetoothDevice device )
        {
            if ( device == null )
            {
                if ( Gatt != null )
                {
                    Gatt.Close();
                    Gatt = null;
                }

                return;
            }

            Gatt = device.ConnectGatt(this, false, new DT1WatchDogGattCallback(this));
            Gatt.RequestConnectionPriority(GattConnectionPriority.High);
        }

        public void StartScan()
        {
            StopScan();

            var bluetoothService = GetSystemService(BluetoothService) as BluetoothManager;

            Scan = new DT1WatchdogScanCallback(this);

            var scanSettings = new ScanSettings.Builder()
                .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency)
                .Build();

            Log.Debug(Logging.WatchdogTag, "DT1WatchdogService starting scan...");
            bluetoothService.Adapter.BluetoothLeScanner.StartScan(null, scanSettings, Scan);
        }

        public void StopScan()
        {
            if ( Scan != null )
            {
                var bluetoothService = GetSystemService(BluetoothService) as BluetoothManager;

                Log.Debug(Logging.WatchdogTag, "DT1WatchdogService stopping scan...");
                bluetoothService.Adapter.BluetoothLeScanner.StopScan(Scan);
                bluetoothService.Adapter.BluetoothLeScanner.FlushPendingScanResults(Scan);
            }
        }

        private void InitAudio()
        {
            if ( dt1WatchdogAudioAlert == null )
            {
                dt1WatchdogAudioAlert = MediaPlayer.Create(this, Resource.Raw.Alert);
                dt1WatchdogAudioAlert.Looping = true;
                dt1WatchdogAudioAlert.SetVolume(1.0f, 1.0f);
            }
        }

        private void CheckForMissingReading()
        {
            Log.Debug(Logging.WatchdogTag, "DT1WatchdogService checking for missing reading");

            var latesValidReading = DT1WatchdogData.GetLatestValidReading();

            if ( latesValidReading != null )
            {
                var timeSinceLastValidReading = DateTimeOffset.UtcNow - latesValidReading.ScanTime;

                if ( timeSinceLastValidReading.TotalMinutes > MissingReadingTimeoutMinutes )
                {
                    // disconnect everything to go again
                    if ( Gatt != null )
                    {
                        Gatt.Close();
                        Gatt = null;
                    }

                    StartScan();

                    var additionalInfo = String.Format(
                        Resources.GetString(Resource.String.MissingReadingFormatMinutes),
                        timeSinceLastValidReading.TotalMinutes.ToString("0.00"));

                    TriggerAlert(AlertType.MissingReading, additionalInfo, true);
                }
            }
        }
    }
}