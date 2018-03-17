using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DT1.Watchdog.Common
{
    public interface IBleDeviceService
    {
		event Action DeviceDetected;
		event Action<bool> DeviceConnectionStateChanged;

		bool IsScanningForDevice { get; }
		bool IsDeviceDetected { get; }
		bool IsDeviceConnected { get; }
		string DeviceName { get; }

		void ScanForDevice();
        Task ScanReadingsAsync();
    }
}
