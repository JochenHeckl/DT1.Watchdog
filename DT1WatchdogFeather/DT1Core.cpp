#include "DT1Core.h"

#include <LowPower.h>
#include "BluefruitConfig.h"

struct SleepInterval
{
	period_t interval;
	int16_t deltaMilliSec;
};



SleepInterval FindSleepInterval(int32_t milliseconds)
{
	std::vector<SleepInterval> sleepIntervals
	{
		{ SLEEP_15MS, 15 },
		{ SLEEP_30MS, 30 },
		{ SLEEP_60MS, 60 },
		{ SLEEP_120MS, 120 },
		{ SLEEP_250MS, 250 },
		{ SLEEP_500MS, 500 },
		{ SLEEP_1S, 1000 },
		{ SLEEP_2S, 2000 },
		{ SLEEP_4S, 4000 },
		{ SLEEP_8S, 8000 }
	};

	for (auto interval : sleepIntervals)
	{
		if (milliseconds <= interval.deltaMilliSec)
		{
			return interval;
		}
	}

	return sleepIntervals.back();
}

DT1CoreClass::DT1CoreClass(BM019SPI& bm019In, Adafruit_BluefruitLE_SPI& bleIn) :
	bm019(bm019In),
	ble(bleIn),
	gatt(bleIn)
{
}

void DT1CoreClass::StartSerial()
{
#if defined _VMDEBUG

	const unsigned long SerialBaudRate = 115200;

	Serial.begin(SerialBaudRate);
	delay(2000);

#endif // _VMDEBUG
}

void DT1CoreClass::SetupBLE(void)
{
	while (!ble.begin(VERBOSE_MODE, true))
	{
		DT1Core.Log("Couldn't find Bluefruit, make sure it's in CoMmanD mode & check wiring?");
	}

	/* Perform a factory reset to make sure everything is in a known state */
	DT1Core.Log("Performing a factory reset: ");

	if (!ble.factoryReset())
	{
		DT1Core.Log("Couldn't factory reset");
	}

	// turn off mode LED to not waste power
	ble.sendCommandCheckOK("AT+HWMODELED=0");
	ble.sendCommandCheckOK("AT+GAPDEVNAME=DT1Watchdog:0001");

	ble.sendCommandCheckOK("AT+GAPINTERVALS");
	// ble.sendCommandCheckOK("AT+GAPINTERVALS=100,200,500,,4000");
}

void DT1CoreClass::SetupBLEGatt(void)
{
	gatt.clear();

	serviceId = gatt.addService(static_cast<uint16_t>(0xffe0));

	dataCharacteristicIndex =
		gatt.addCharacteristic(static_cast<uint16_t>(0xffe1), 0x10, 2, 14, BLEDataType_t::BLE_DATATYPE_BYTEARRAY,
			"0xFF[B]:ErrorCode[B]:TableIdx[B]:Glucose01[W]:Glucose02[W]:Glucose03[W]:MinutesRemaining[W]:Voltage[W]:Charge%[B] || 0xFF[B]:ErrorCode[B]:Voltage[W]:Charge%[B]");

	ble.sendCommandCheckOK("AT+GAPSETADVDATA=02-01-06-03-02-e0-ff");

	ble.sendCommandCheckOK("AT+BLEKEYBOARDEN=off");
	ble.sendCommandCheckOK("AT+BLEHIDGAMEPADEN=off");
	ble.sendCommandCheckOK("AT+BLEMIDIEN=off");

	ble.reset();
	delay(100);
}

void DT1CoreClass::UpdateService(ScanResult scanResult)
{
	auto batteryStatus = GetBatteryStatus();

	std::vector<uint8_t> encodedData;

	if (scanResult == ScanResult::Success)
	{
		uint8_t shortTermIndex = scanData.WrapAreoundOffsetShortTerm;
		auto lastIndex = (16 + shortTermIndex - 1) % 16;
		Log(String("LastIndex: ") + lastIndex);

		auto lasButOneIndex = (16 + shortTermIndex - 2) % 16;
		Log(String("Last but one Index: ") + lasButOneIndex);

		auto lasButTwoIndex = (16 + shortTermIndex - 3) % 16;
		Log(String("Last but two Index: ") + lasButTwoIndex);

		uint16_t last = scanData.ShortTermReadings[lastIndex].GlucoseReadingMgOverTenDl;
		uint16_t lastButOne = scanData.ShortTermReadings[lasButOneIndex].GlucoseReadingMgOverTenDl;
		uint16_t lastButTwo = scanData.ShortTermReadings[lasButTwoIndex].GlucoseReadingMgOverTenDl;

		Log(String("Glucose values: ") + shortTermIndex + ": " + last + ", " + lastButOne + ", " + lastButTwo);

		//encodedData.push_back(0xff);
		//encodedData.push_back(scanResult);

		encodedData.push_back(shortTermIndex);
		encodedData.push_back(static_cast<uint8_t>(last & 0xff)); encodedData.push_back(static_cast<uint8_t>((last >> 8) & 0xff));
		encodedData.push_back(static_cast<uint8_t>(lastButOne & 0xff)); encodedData.push_back(static_cast<uint8_t>((lastButOne >> 8) & 0xff));
		encodedData.push_back(static_cast<uint8_t>(lastButTwo & 0xff)); encodedData.push_back(static_cast<uint8_t>((lastButTwo >> 8) & 0xff));
		encodedData.push_back(static_cast<uint8_t>(scanData.ElapsedMinutes & 0xff)); encodedData.push_back(static_cast<uint8_t>((scanData.ElapsedMinutes >> 8) & 0xff));
	}
	else
	{
		encodedData.push_back(0xff);
		encodedData.push_back(scanResult);
	}

	auto voltage = static_cast<uint16_t>(batteryStatus.voltagemV);
	encodedData.push_back(static_cast<uint8_t>(voltage & 0xff)); encodedData.push_back(static_cast<uint8_t>((voltage >> 8) & 0xff));
	encodedData.push_back(batteryStatus.chargePerCent);

	if (gatt.setChar(dataCharacteristicIndex, &encodedData[0], encodedData.size()))
	{
		BlinkOneSecond(5);
	}
	else
	{
		BlinkOneSecond(1);
	}
}

void DT1CoreClass::StopSPI(void)
{
	SPI.end();
	delay(10);

	// pull down MOSI, SCK to avoid wasing power on the bm019 LED
	pinMode(15, OUTPUT);
	digitalWrite(15, LOW);

	pinMode(16, OUTPUT);
	digitalWrite(16, LOW);

	delay(10);
}

void DT1CoreClass::RestartSPI(void)
{
	SPI.begin();
}

void DT1CoreClass::BlinkNewLoop()
{
	pinMode(OUTPUT, 13);
	digitalWrite(13, HIGH);
	delay(1000);
	digitalWrite(13, LOW);
	delay(500);
	digitalWrite(13, HIGH);
	delay(500);
	digitalWrite(13, LOW);
	delay(300);
	digitalWrite(13, HIGH);
	delay(300);
	digitalWrite(13, LOW);
}

void DT1CoreClass::BlinkOneSecond(int count)
{
	pinMode(OUTPUT, 13);

	for (int i = 0; i < count; ++i)
	{
		digitalWrite(13, HIGH);
		delay(500 / count);
		digitalWrite(13, LOW);
		delay(500 / count);
	}
}

DT1CoreClass::ScanResult DT1CoreClass::ScanBM019(void)
{
	Log("Scanning...");

	auto scanResult = ScanResult::Success;

	bm019.WakeUp();

	if (!bm019.ProtocolSelectISO15693())
	{
		scanResult = ScanResult::SetProtocolError;
	}
	else
	{
		Log("Protocol set to ISO15693.");

		auto tagInventory = bm019.Inventory();

		if (!bm019.IsValidInventoty(tagInventory))
		{
			Log(String("No Tag in range."));
			scanResult = ScanResult::NoTagInRange;
		}
		else
		{
			Log(String("Tag in range."));

			std::vector<uint8_t> dataBlocks;

			if (!bm019.ReadBlocks(0, 40, dataBlocks, 5))
			{
				return ScanResult::ReadBlocksError;
			}
			else
			{

				Log(String("data size: ") + dataBlocks.size());

				//for (auto&& byte : dataBlocks)
				//{
				//	Serial.print(byte);
				//	Serial.print(", ");
				//}

				//Serial.println();

				scanData = *(reinterpret_cast<const ScanData*>(&dataBlocks[0]));
			}
		}
	}

	while (!bm019.SetFieldOff());
	bm019.Hibernate();

	return scanResult;
}

void DT1CoreClass::PerformLowPowerBreak(int32_t seconds)
{
	int32_t powerDownMS = seconds * 1000;
	DT1Core.Log(String("Sleeping for a total of ") + powerDownMS + " milliSeconds.");

	delay(10);

	while (powerDownMS > 0)
	{
		// manually turn off ADC for the entire loop
		//	ADCSRA &= ~(1 << ADEN);

		auto interval = FindSleepInterval(powerDownMS);

		DT1Core.Log(String("Sleeping for ") + interval.deltaMilliSec + " milliSeconds.");
		
#if defined _DEBUG
		// powering down will break the serial connection
		// so to be able to debug we go with a simple delay here
		delay(interval.interval);
#else	
		LowPower.powerDown(interval.interval, ADC_OFF, BOD_ON);
#endif
		powerDownMS -= interval.deltaMilliSec;

		// ADCSRA |= (1 << ADEN);
	}

	delay(10);

	DT1Core.Log("Power up!");
}

DT1CoreClass::BatteryStatus DT1CoreClass::GetBatteryStatus(void)
{
	// copied from bluefruit documentation
	BatteryStatus batteryStatus;

	const auto VBATPIN = A9;

	batteryStatus.voltagemV = analogRead(VBATPIN);
	batteryStatus.voltagemV *= 2.0f; // we divided by 2, so multiply back
	batteryStatus.voltagemV *= 3.3f; // Multiply by 3.3V, our reference voltage
	batteryStatus.voltagemV *= 1000.0f / 1024.0f; // convert to voltage (milli volts)

	const auto maxVoltage = 4200.0f;
	const auto minVoltage = 3500.0f;
	const auto voltageRange = maxVoltage - minVoltage;

	auto normVoltage = std::min(1.0f, std::max(0.0f, (batteryStatus.voltagemV - minVoltage) / voltageRange));
	batteryStatus.chargePerCent = static_cast<int>(normVoltage * 100.0f);

	return batteryStatus;
}

