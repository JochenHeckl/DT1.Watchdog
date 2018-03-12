// DT1Core.h

#ifndef _DT1CORE_h
#define _DT1CORE_h

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include <ArduinoSTL.h>
#include <Adafruit_BluefruitLE_SPI.h>
#include <Adafruit_BLEGatt.h>

#include "BM019SPI.h"
#include "ScanData.h"

class DT1CoreClass
{
public:
	enum ScanResult
	{
		Success = 0,
		SetProtocolError = 1,
		NoTagInRange = 2,
		ReadBlocksError = 4
	};

private:
	struct BatteryStatus
	{
		float voltagemV;
		uint8_t chargePerCent;
	};

public:
	DT1CoreClass( BM019SPI& bm019In, Adafruit_BluefruitLE_SPI& bleIn );

	template<class ValueType>
	inline void Log(ValueType message)
	{
		Log(String(message));
	}

	inline void Log(String message)
	{
#if defined _VMDEBUG

		Serial.println(message.c_str());

#endif // _VMDEBUG
	}

	void StartSerial(void);
	void SetupBLE(void);
	void SetupBLEGatt(void);

	void UpdateService( ScanResult scanResult );
	
	void StopSPI(void);
	void RestartSPI(void);

	void BlinkNewLoop();
	void BlinkOneSecond(int count);
	
	ScanResult ScanBM019(void);

	void PerformLowPowerBreak(int32_t seconds);

private:
	BatteryStatus GetBatteryStatus( void );


private:
	BM019SPI& bm019;
	Adafruit_BluefruitLE_SPI& ble;

	Adafruit_BLEGatt gatt;
	ScanData scanData;

	uint8_t serviceId;
	int32_t dataCharacteristicIndex;
};

extern DT1CoreClass DT1Core;

#endif

