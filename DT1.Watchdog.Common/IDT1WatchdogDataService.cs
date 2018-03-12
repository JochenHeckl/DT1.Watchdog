using System;
using System.Collections.Generic;
using System.Text;

namespace DT1.Watchdog.Common
{
    public enum AlertNotification
    {
        SMSAlert,
        AudioAlert
    }

    public enum BleConnectionStatus
    {
        Connected,
        Disconnected
    }

    public interface IDT1WatchdogDataService
    {
        string MostRecentDT1DeviceDeviceAddress { get; set; }
        DateTimeOffset LastValidReading { get; set; }
        BleConnectionStatus DT1HardwareConnectionStatus { get; set; }
        string WatchdogDeviceName { get; set; }

        void PersistReading(GlucoseReading reading);
        string GetDevicePresentString(string deviceName);
    }
}
