// BM019SPI.h

#ifndef _BM019SPI_h
#define _BM019SPI_h

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif
#include <ArduinoSTL.h>
#include <SPI.h>

class BM019SPI
{
public:
	static inline bool IsValidInventoty(std::vector<uint8_t> inventory) { return inventory.size() > 0; }

public:
	BM019SPI(uint8_t slaveSelectPinIn, uint8_t interruptPinIn, uint8_t powerPin01, uint8_t powerPin02);

	String QueryId(void);
	bool SetMaxGain(void);

	bool SetFieldOff(void);
	bool ProtocolSelectISO15693(void);
	std::vector<uint8_t> Inventory(void);

	bool ReadBlocks(uint8_t startIndex, uint8_t blockCount, std::vector<uint8_t>& dataInOut, uint8_t numRetries);

	void WakeUp(void);
	void Hibernate(void);
	void Reset(void);
	void PowerDown(void);

private:
	void SendCommand(const std::vector<uint8_t>& commandIn);
	std::vector<uint8_t> RunCommand(const std::vector<uint8_t>& commandIn);

private:
	void PollForDataReady();


private:
	uint8_t slaveSelectPin;
	uint8_t interruptPin;
	uint8_t powerPin01;
	uint8_t powerPin02;

	SPISettings settings;
	byte receiveBuffer[40];
};

#endif

