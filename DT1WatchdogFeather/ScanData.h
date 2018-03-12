#pragma once

#pragma pack(push, 1)

struct ScanData
{
public:
	struct Reading
	{
		uint16_t GlucoseReadingMgOverTenDl;
		int8_t Unknown01;
		int8_t Unknown02;
		uint16_t TemperatureDegCentOverTen;
	};

	uint8_t Header01[26];
	int8_t WrapAreoundOffsetShortTerm;
	int8_t WrapAreoundOffsetLongTerm;
	Reading ShortTermReadings[16];
	Reading LongTermReadings[32];
	uint32_t ElapsedMinutes;
};

#pragma pack(pop)