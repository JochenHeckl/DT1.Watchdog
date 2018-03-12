using System;
using System.Linq;

namespace DT1.Watchdog.Common
{
    public class GlucoseReading
    {
        public enum GlucoseTrend
        {
            TowardsTarget,
            AwayFromTarget,
        }

        [Flags]
        public enum ReadingErrorCode
        {
            NoError = 0,
            SetProtocolError = 1,
            NoTagInRange = 2,
            ReadBlocksError = 4,
            TransmissionError = 8
        }

        public static GlucoseReading ParseRawCharacteristicData(byte[] data, DateTimeOffset date)
        {
            if (data[0] == 0xff)
            {
                if (data[1] != (byte)ReadingErrorCode.NoError)
                {
                    return new GlucoseReading()
                    {
                        ErrorCode = (ReadingErrorCode)data[1],
                        Voltage = BitConverter.ToUInt16(data, 2),
                        PerCentCharge = data[4],
                        ScanTime = date.UtcDateTime
                    };
                }

                if (data.Length == 14)
                {
                    return new GlucoseReading()
                    {
                        ErrorCode = (ReadingErrorCode)data[1],
                        RollingTableIndex = data[2],
                        GlucoseLatest = Convert.ToDouble(BitConverter.ToUInt16(data, 3)) * 0.1,
                        GlucoseLatestButOne = Convert.ToDouble(BitConverter.ToUInt16(data, 5)) * 0.1,
                        GlucoseLatestButTwo = Convert.ToDouble(BitConverter.ToUInt16(data, 7)) * 0.1,
                        ElapsedMinutes = BitConverter.ToUInt16(data, 9),
                        Voltage = BitConverter.ToUInt16(data, 11),
                        PerCentCharge = data[13],
                        ScanTime = date.UtcDateTime
                    };
                }
            }

            return new GlucoseReading()
            {
                ErrorCode = ReadingErrorCode.TransmissionError,
                ScanTime = date.UtcDateTime
            };
        }

        public static GlucoseReading ParseRawCharacteristicData( byte[] data )
        {
            return ParseRawCharacteristicData(data, DateTimeOffset.Now);
        }
        
        public GlucoseTrend GetTrend( int targetGlucose )
        {
            var latestDeltaTarget = Math.Abs(targetGlucose - GlucoseLatest);
            var latestButOneDeltaTarget = Math.Abs(targetGlucose - GlucoseLatestButOne);

            return latestButOneDeltaTarget >= latestDeltaTarget ? GlucoseTrend.TowardsTarget : GlucoseTrend.AwayFromTarget;
        }

        public int GetPredictedGlucoseValue( TimeSpan deltaTime )
        {
            //// radings have 1 minute intervals
            
            var deltaPerMin = Convert.ToDouble( GlucoseLatest - GlucoseLatestButTwo ) / 2.0;
            return Convert.ToInt32( GlucoseLatest + deltaPerMin * deltaTime.TotalMinutes );
        }

        public string Source { get; set; }
        public int RollingTableIndex { get; set; }
        public double GlucoseLatest { get; set; }
        public double GlucoseLatestButOne { get; set; }
        public double GlucoseLatestButTwo { get; set; }
        public uint ElapsedMinutes { get; set; }
        public int Voltage { get; set; }
        public int PerCentCharge { get; set; }
        public DateTimeOffset ScanTime { get; set; }
        public ReadingErrorCode ErrorCode { get; set; }
    }
}