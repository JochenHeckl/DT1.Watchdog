using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DT1.Watchdog.Common
{
    public interface IBleScanService
    {
        string BleDeviceName { get; }

        event Action<string> DeviceDetected;

        void ScanForDevice();
        Task<string> ScanReadings();
    }
}
