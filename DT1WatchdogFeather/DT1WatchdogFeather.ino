#include <Adafruit_BluefruitLE_SPI.h>
#include "BluefruitConfig.h"

#include "BM019SPI.h"
#include "DT1Core.h"

BM019SPI bm019(6, 5, 10, 11);
Adafruit_BluefruitLE_SPI ble(BLUEFRUIT_SPI_CS, BLUEFRUIT_SPI_IRQ, BLUEFRUIT_SPI_RST);
DT1CoreClass dt1Core(bm019, ble);

void setup()
{
	dt1Core.StartSerial();
	dt1Core.SetupBLE();
	dt1Core.SetupBLEGatt();
}

// the loop function runs over and over again until power down or reset
void loop()
{
	if (ble.isConnected())
	{
		auto scanResult = dt1Core.ScanBM019();
		
		dt1Core.UpdateService(scanResult);
		dt1Core.PerformLowPowerBreak(4);

		if (ble.isConnected())
		{
			dt1Core.UpdateService(scanResult);
			dt1Core.PerformLowPowerBreak(4);
		}

		ble.disconnect();
	}

	// dt1Core.StopSPI();

	dt1Core.PerformLowPowerBreak(8);

	// dt1Core.RestartSPI();
}
