using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DT1.Watchdog.Common
{
    public interface IBleDeviceService
    {
		event Action<string> DeviceDetected;

		bool IsScanningForDevice { get; }
		bool IsDeviceDetected { get; }
		bool IsDeviceConnected { get; }

        void ScanForDevice();
        Task<string> ScanReadings();
    }
}
