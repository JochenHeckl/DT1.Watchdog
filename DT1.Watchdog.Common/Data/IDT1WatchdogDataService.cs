using System;
using System.Collections.Generic;
using System.Text;

namespace DT1.Watchdog.Common.Data
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

        void PersistReading(GlucoseReading reading);
    }
}
