// 
// 
// 

#include "BM019SPI.h"


BM019SPI::BM019SPI(uint8_t slaveSelectPinIn, uint8_t interruptPinIn, uint8_t powerPin01In, uint8_t powerPin02In) :
	slaveSelectPin(slaveSelectPinIn),
	interruptPin(interruptPinIn),
	powerPin01(powerPin01In),
	powerPin02(powerPin02In),
	settings(57600, MSBFIRST, SPI_MODE0)
{
	pinMode(slaveSelectPin, OUTPUT);
	digitalWrite(slaveSelectPin, HIGH);

	pinMode(interruptPin, OUTPUT);
	digitalWrite(interruptPin, HIGH);

	pinMode(powerPin01, OUTPUT);
	digitalWrite(powerPin01, HIGH);

	pinMode(powerPin02, OUTPUT);
	digitalWrite(powerPin02, HIGH);
}

void BM019SPI::WakeUp(void)
{
	digitalWrite(powerPin01, HIGH);
	digitalWrite(powerPin02, HIGH);

	// send a wake up pulse to put the BM019 into SPI mode 
	delay(10);
	digitalWrite(interruptPin, LOW);
	delayMicroseconds(100);
	digitalWrite(interruptPin, HIGH);
	delay(10);

	delay(1000);
}

void BM019SPI::Hibernate(void)
{
	delay(100);
	std::vector<uint8_t> command
	{
		0x00,	// SPI control byte to send command to CR95HF

		0x07,	// Idle command
		0x0E,	// length of data to follow

		0x08,	// waku up by IRQ_IN

		0x04,	// Enter Hibernate
		0x00,

		0x04,	// Wake up from Hibernate
		0x00,

		0x18,	// Leave whatever ... always 0x1800
		0x00,

		0x00,	// Waku up period 0x0
		0x00,	// HFO stab delay

		0x00,	// DAC stab delay
		0x00,	// DAC Data
		0x00,
		0x00,	// Swing Count
		0x00,	// Max Sleep
	};

	SPI.beginTransaction(settings);

	SendCommand(command);

	SPI.endTransaction();
	delay(100);
}

void BM019SPI::PowerDown(void)
{
	digitalWrite(powerPin01, LOW);
	digitalWrite(powerPin02, LOW);

	digitalWrite(slaveSelectPin, HIGH);
	digitalWrite(interruptPin, LOW);

	delay(100);
}

String BM019SPI::QueryId(void)
{
	SPI.beginTransaction(settings);

	// step 1 send the command
	digitalWrite(slaveSelectPin, LOW);
	SPI.transfer(0);  // SPI control byte to send command to CR95HF
	SPI.transfer(1);  // IDN command
	SPI.transfer(0);  // length of data that follows is 0
	digitalWrite(slaveSelectPin, HIGH);
	delay(1);

	PollForDataReady();

	// step 3, read the data
	digitalWrite(slaveSelectPin, LOW);

	SPI.transfer(0x02);   // SPI control byte for read         

	receiveBuffer[0] = SPI.transfer(0);  // response code
	receiveBuffer[1] = SPI.transfer(0);  // length of data

	for (int i = 0; i < receiveBuffer[1]; i++)
	{
		receiveBuffer[i + 2] = SPI.transfer(0);  // data
	}

	digitalWrite(slaveSelectPin, HIGH);
	delay(1);

	String result;
	result.reserve(receiveBuffer[1]);

	for (int idIdx = 2; idIdx < receiveBuffer[1]; idIdx++)
	{
		result += receiveBuffer[idIdx];
	}

	SPI.endTransaction();

	// Serial.print(result);

	return result;
}

bool BM019SPI::SetMaxGain(void)
{
	SPI.beginTransaction(settings);

	// step 1 send the command
	digitalWrite(slaveSelectPin, LOW);
	SPI.transfer(0x00);  // SPI control byte to send command to CR95HF
	SPI.transfer(0x09);  // Write Register
	SPI.transfer(0x04);  // length of data that follows is 0

	SPI.transfer(0x68);  // Register address
	SPI.transfer(0x00);  // do not inc address
	SPI.transfer(0x01);  // register index for modulation and gain

	SPI.transfer(0x10);  // modulation 10% gain 34 dB

	digitalWrite(slaveSelectPin, HIGH);
	delay(1);

	PollForDataReady();

	// step 3, read the data
	digitalWrite(slaveSelectPin, LOW);

	SPI.transfer(0x02);   // SPI control byte for read         

	receiveBuffer[0] = SPI.transfer(0);  // response code
	receiveBuffer[1] = SPI.transfer(0);  // length of data

	digitalWrite(slaveSelectPin, HIGH);
	delay(1);

	SPI.endTransaction();

	return (receiveBuffer[0] == 0x0) && (receiveBuffer[1] == 0x0);
}


std::vector<uint8_t> BM019SPI::Inventory(void)
{
	std::vector<uint8_t> command
	{
		0x00,	// SPI control byte to send command to CR95HF
		0x04,	// Send Receive CR95HF command
		0x03,	// length of data that follows is 0
		0x26,	// request Flags byte
		0x01,	// Inventory Command for ISO/IEC 15693
		0x00	// mask length for inventory command
	};

	auto result = RunCommand(command);
	delay(100);

	if (result[0] != 0x80)
	{
		// no tag in range
		return std::vector<uint8_t>();
	}

	return std::vector<uint8_t>(result.begin() + 2, result.end());
}

bool BM019SPI::ReadBlocks(uint8_t startIndex, uint8_t blockCount, std::vector<uint8_t>& dataInOut, uint8_t numRetries)
{
	dataInOut.clear();
	dataInOut.reserve(blockCount * 8);

	SPI.beginTransaction(settings);

	for (auto blockOffetIndex = 0; blockOffetIndex < blockCount; ++blockOffetIndex)
	{
		auto triesLeft = numRetries;
		bool blockSuccess = false;

		auto responseCode = 0x80;
		auto responseSize = 0;

		while (triesLeft)
		{
			--triesLeft;

			// step 1 send the command
			digitalWrite(slaveSelectPin, LOW);
			SPI.transfer(0x00);  // SPI control byte to send command to CR95HF
			SPI.transfer(0x04);  // Send Receive CR95HF command
			SPI.transfer(0x03);  // length of data that follows
			SPI.transfer(0x02);  // request Flags byte
			SPI.transfer(0x20);  // Read Single Block command for ISO/IEC 15693
			SPI.transfer(startIndex + blockOffetIndex);  // memory block address
			digitalWrite(slaveSelectPin, HIGH);
			delay(1);

			PollForDataReady();

			digitalWrite(slaveSelectPin, LOW);

			SPI.transfer(0x02);   // SPI control byte for read         

			responseCode = SPI.transfer(0);
			responseSize = SPI.transfer(0); // length of data

			//Serial.print("Block: ");
			//Serial.print(startIndex + blockOffetIndex);

			//Serial.print(", Response Code: ");
			//Serial.print(responseCode, HEX);

			//Serial.print(", Size: ");
			//Serial.println( responseSize );

			if (responseCode == 0x80)
			{
				// no error
				// first byte is 0 appended by wrapper
				// last 3 bytes are CRC and CRC check

				// drop byte 0
				SPI.transfer(0);

				for (int dataIdx = 1; dataIdx < responseSize - 3; ++dataIdx)
				{
					dataInOut.push_back(SPI.transfer(0));
				}

				// drop CRC
				SPI.transfer(0);
				SPI.transfer(0);

				// test for CRC valid
				auto cRCCheck = SPI.transfer(0);
				if (cRCCheck == 0x0)
				{
					blockSuccess = true;
					triesLeft = 0;
				}
				//else
				//{
				//	Serial.print("CRC error: ");
				//	Serial.println(cRCCheck, HEX);
				//}
			}
			else
			{
				for (int dataIdx = 1; dataIdx < responseSize; ++dataIdx)
				{
					SPI.transfer(0);
				}
			}

			digitalWrite(slaveSelectPin, HIGH);
			delay(1);

			if (blockSuccess)
			{
				break;
			}
		}

		if (!blockSuccess)
		{
			dataInOut.clear();
			dataInOut.push_back(responseCode);
			break;
		}
	}

	SPI.endTransaction();
	delay(100);

	return dataInOut.size() == blockCount * 8;
}

void BM019SPI::Reset(void)
{
	std::vector<uint8_t> command
	{
		0x01,	// SPI control byte to reset CR95HF
	};

	SPI.beginTransaction(settings);

	SendCommand(command);

	SPI.endTransaction();
	delay(100);
}

bool BM019SPI::SetFieldOff(void)
{
	std::vector<uint8_t> command
	{
		0x00,	// SPI control byte to send command to CR95HF
		0x02,	// Set protocol command
		0x02,	// length of data to follow
		0x00,	// field off
		0x00	// Wait for SOF, 10% modulation, append CRC
	};

	auto result = RunCommand(command);
	delay(100);

	return (result[0] == 0) && (result[1] == 0);
}

bool BM019SPI::ProtocolSelectISO15693(void)
{
	std::vector<uint8_t> command
	{
		0x00,	// SPI control byte to send command to CR95HF
		0x02,	// Set protocol command
		0x02,	// length of data to follow
		0x01,	// code for ISO/IEC 15693
		0x0D	// Wait for SOF, 10% modulation, append CRC
	};

	auto result = RunCommand(command);
	delay(100);

	return (result[0] == 0) && (result[1] == 0);
}

void BM019SPI::SendCommand(const std::vector<uint8_t>& commandIn)
{
	digitalWrite(slaveSelectPin, LOW);

	for (auto byte : commandIn)
	{
		SPI.transfer(byte);
	}

	digitalWrite(slaveSelectPin, HIGH);
	delay(1);
}

std::vector<uint8_t> BM019SPI::RunCommand(const std::vector<uint8_t>& commandIn)
{
	SPI.beginTransaction(settings);

	SendCommand(commandIn);

	PollForDataReady();

	// step 3, read the result
	digitalWrite(slaveSelectPin, LOW);

	SPI.transfer(0x02);   // SPI control byte for read         

	std::vector<uint8_t> resultData{ 0, 0 };

	resultData[0] = SPI.transfer(0);  // response code
	resultData[1] = SPI.transfer(0);  // length of data

	resultData.resize(resultData[1] + 2, 0);

	for (auto resultIdx = 0; resultIdx < resultData[1]; resultIdx++)
	{
		resultData[resultIdx + 2] = SPI.transfer(0);  // data
	}

	digitalWrite(slaveSelectPin, HIGH);
	delay(1);

	SPI.endTransaction();

	return resultData;
}

void BM019SPI::PollForDataReady()
{
	uint8_t buffer = 0;

	digitalWrite(slaveSelectPin, LOW);
	while (buffer != 8)
	{
		buffer = SPI.transfer(0x03);  // Write 3 until
		buffer = buffer & 0x08;  // bit 3 is set
	}
	digitalWrite(slaveSelectPin, HIGH);
	delay(1);
}
