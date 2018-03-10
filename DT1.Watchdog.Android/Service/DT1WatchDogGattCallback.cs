using Android.Bluetooth;
using System.Linq;
using System.Threading;
using Android.Content;
using Android.Util;
using System;
using DT1.Watchdog.Common.Data;
using DT1.Watchdog.Common.Logging;

namespace DT1.Watchdog.Service
{
    internal class DT1WatchDogGattCallback : BluetoothGattCallback
    {
        public DT1WatchDogGattCallback( DT1WatchDogService serviceIn,  IDT1WatchdogDataService dataServiceIn, ILog logIn )
        {
            log = logIn;
            dataService = dataServiceIn;
            service = serviceIn;
        }

        public override void OnServicesDiscovered( BluetoothGatt gatt, GattStatus status )
        {
            log.Debug( "Watchdog services discovered." );

            base.OnServicesDiscovered(gatt, status);

            var characteristic = gatt.Services
                .Where(x => x.Uuid.Equals(DT1WatchDogService.DT1WatchdogUUID))
                .SelectMany(x => x.Characteristics)
                .FirstOrDefault(x => x.Uuid.Equals(DT1WatchDogService.DT1WatchdogDataCharacteristicUUID));

            if ( characteristic != null )
            {
                log.Debug("Watchdog data characteristic discovered.");

                dataService.MostRecentDT1DeviceDeviceAddress = gatt.Device.Address;

                if ( characteristic.Properties == GattProperty.Notify )
                {
                    var CLIENT_CHARACTERISTIC_CONFIG = Java.Util.UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");
                    var clientCharacteristiConfigDescriptor = characteristic.GetDescriptor(CLIENT_CHARACTERISTIC_CONFIG);
                    clientCharacteristiConfigDescriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());

                    while ( !gatt.WriteDescriptor(clientCharacteristiConfigDescriptor) )
                    {
                        Thread.Yield();
                    }

                    log.Debug( "Gatt descriptor written.");


                    while ( !gatt.SetCharacteristicNotification(characteristic, true) )
                    {
                        Thread.Yield();
                    }

                    log.Debug( "registerd for data characteristic changes.");
                }


            }
        }

        public override void OnCharacteristicChanged( BluetoothGatt gatt, BluetoothGattCharacteristic characteristic )
        {
            if ( characteristic.Uuid.Equals(DT1WatchDogService.DT1WatchdogDataCharacteristicUUID) )
            {
                log.Debug( "Watchdog data characteristic changed.");

                // we got data - fine we disconnect and wait for the next alarm

                var CLIENT_CHARACTERISTIC_CONFIG = Java.Util.UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");
                var clientCharacteristiConfigDescriptor = characteristic.GetDescriptor(CLIENT_CHARACTERISTIC_CONFIG);
                clientCharacteristiConfigDescriptor.SetValue(BluetoothGattDescriptor.DisableNotificationValue.ToArray());
                gatt.WriteDescriptor(clientCharacteristiConfigDescriptor);

                gatt.Disconnect();

                var data = characteristic.GetValue();
                var reading = GlucoseReading.ParseRawCharacteristicData(data);

                // fill in source
                reading.Source = gatt.Device.Name;

                dataService.PersistReading(reading);

                if ( reading.ErrorCode == GlucoseReading.ReadingErrorCode.NoError )
                {
                    dataService.LastValidReading = DateTimeOffset.UtcNow;
                    EvaluateReading(reading);
                }

                var dataIntent = new Intent(DT1WatchDogService.IntentIncommingData);
                service.SendBroadcast(dataIntent);
            }
        }

        private void EvaluateReading( GlucoseReading reading )
        {
            if ( reading.GlucoseLatest < DT1WatchDogService.NonConditionalHypoglycemiaThresholdmgdl )
            {
                service.TriggerAlert(DT1WatchDogService.AlertType.Hypoglycemia, reading.GlucoseLatest.ToString() + "mgdl", false);
                return;
            }

            if ( reading.GlucoseLatest < DT1WatchDogService.ConditionalHypoglycemiaThresholdmgdl )
            {
                service.TriggerAlert(DT1WatchDogService.AlertType.Hypoglycemia, reading.GlucoseLatest.ToString() + "mgdl", true);
                return;
            }

            if ( reading.GetPredictedGlucoseValue(DT1WatchDogService.PredictionInterval) < DT1WatchDogService.ConditionalHypoglycemiaThresholdmgdl )
            {
                service.TriggerAlert(DT1WatchDogService.AlertType.PredictedHypoglycemia, reading.GlucoseLatest.ToString() + "mgdl", true);
                return;
            }

            if ( reading.GlucoseLatest > DT1WatchDogService.HyperglycemiaThresholdmgdl )
            {
                service.TriggerAlert(DT1WatchDogService.AlertType.Hyperglycemia, reading.GlucoseLatest.ToString() + "mgdl", true);
                return;
            }

            if ( reading.GetPredictedGlucoseValue(DT1WatchDogService.PredictionInterval) > DT1WatchDogService.HyperglycemiaThresholdmgdl )
            {
                service.TriggerAlert(DT1WatchDogService.AlertType.PredictedHyperglycemia, reading.GlucoseLatest.ToString() + "mgdl", true);
                return;
            }

            if ( reading.PerCentCharge < DT1WatchDogService.LowPowerThresholdPerCent )
            {
                service.TriggerAlert(DT1WatchDogService.AlertType.LowBattery, reading.PerCentCharge + "%", false);
                return;
            }

            service.TriggerValidInRangeReading();
        }

        public override void OnConnectionStateChange( BluetoothGatt gatt, GattStatus status, ProfileState newState )
        {
            base.OnConnectionStateChange(gatt, status, newState);

            if ( newState == ProfileState.Disconnected )
            {
                log.Debug( "Watchdog disconnected." );
                dataService.DT1HardwareConnectionStatus = BleConnectionStatus.Disconnected;
            }

            if ( newState == ProfileState.Connected )
            {
                log.Debug( "Watchdog connected.");
                dataService.DT1HardwareConnectionStatus = BleConnectionStatus.Connected;

                while ( !gatt.DiscoverServices() )
                {
                    Thread.Yield();
                }
            }
        }

        private DT1WatchDogService service;

        private ILog log;
        private IDT1WatchdogDataService dataService;
    }
}