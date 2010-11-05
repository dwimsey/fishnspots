using System;
using System.Collections.Generic;
using System.Text;
using NMEAParser;
using NMEAParser.SentenceHandlers;

namespace FishnSpots
{
	class NMEADevice : FSDevice
	{
		SensorValue SensorEnabled;

		// For GPRMC
		SensorValue SensorFixTime;
		SensorValue SensorAltitude;
		SensorValue SensorLatitude;
		SensorValue SensorLongitude;
		SensorValue SensorGPSFix;
		SensorValue SensorCourse;
		SensorValue SensorSpeed;

		// For GPGGA
		SensorValue SensorDGPSAge;
		SensorValue SensorDGPSStationId;
		SensorValue SensorFixQuality;
		SensorValue SensorHeightOfGeoid;
		SensorValue SensorSatellitesTracked;

		// Misc
		SensorValue SensorPDOP;
		SensorValue SensorHDOP;
		SensorValue SensorVDOP;

		// Depth Sounder sensors
		SensorValue SensorDepth;
		SensorValue SensorTransducerDepthFeet;
		SensorValue SensorTransducerDepthMeters;
		SensorValue SensorTransducerDepthFathoms;
		SensorValue SensorMeanWaterTemp;

		private string p_DeviceName;
		public string Name
		{
			get
			{
				return (p_DeviceName);
			}
			set
			{
				p_DeviceName = Name;
			}
		}

		public const string DevIDPrefix = "nmea";
		private readonly FSEngine fsEngine;
		internal NMEADevice(FSEngine fsEngine)
		{
			this.fsEngine = fsEngine;
			string DevId =  DevIDPrefix + "0";
			// 2048 should be enough for anything this app ever sees
			int i;
			for(i = 0; i < 2048; i++) {
				DevId = DevIDPrefix + i.ToString();
				if(!fsEngine.Sensors.HasSensor(DevId + "/enabled")) {
					break;
				}
			}
			if(i == 2048) {
				throw new Exception("Gave up finding device name for NMEA device at 2048 tries.");
			}
			SensorEnabled = fsEngine.Sensors.CreateSensorValue(DevId + "/enabled", SensorValue.SensorType.Bool, false);

			// Used by all sentences which provide a fix time
			SensorFixTime = fsEngine.Sensors.CreateSensorValue(DevId + "/FixTime", SensorValue.SensorType.TimeStamp, DateTime.MinValue);

			SensorAltitude = fsEngine.Sensors.CreateSensorValue(DevId + "/Altitude", SensorValue.SensorType.Double, 0.0);

			// Used by GPRMC
			SensorLatitude = fsEngine.Sensors.CreateSensorValue(DevId + "/Latitude", SensorValue.SensorType.Double, 35.7987669706413);
			SensorLongitude = fsEngine.Sensors.CreateSensorValue(DevId + "/Longitude", SensorValue.SensorType.Double, -79.0180706977844);
			SensorGPSFix = fsEngine.Sensors.CreateSensorValue(DevId + "/GPSFix", SensorValue.SensorType.Bool, false);
			SensorCourse = fsEngine.Sensors.CreateSensorValue(DevId + "/Course", SensorValue.SensorType.Double, 0.0);
			SensorSpeed = fsEngine.Sensors.CreateSensorValue(DevId + "/Speed", SensorValue.SensorType.Double, 0.0);

			// Used by GPGGA
			SensorDGPSAge = fsEngine.Sensors.CreateSensorValue(DevId + "/DGPSAge", SensorValue.SensorType.Double, 0.0);
			SensorFixQuality = fsEngine.Sensors.CreateSensorValue(DevId + "/FixQuality", SensorValue.SensorType.Integer, 1);
			SensorDGPSStationId = fsEngine.Sensors.CreateSensorValue(DevId + "/DGPSStationId", SensorValue.SensorType.String, "");
			SensorHeightOfGeoid = fsEngine.Sensors.CreateSensorValue(DevId + "/HeightOfGeoid", SensorValue.SensorType.Double, 0.0);
			SensorSatellitesTracked = fsEngine.Sensors.CreateSensorValue(DevId + "/SatellitesTracked", SensorValue.SensorType.Integer, 0);

			// Used by several sentence types
			SensorPDOP = fsEngine.Sensors.CreateSensorValue(DevId + "/PDOP", SensorValue.SensorType.Double, double.MinValue);
			SensorHDOP = fsEngine.Sensors.CreateSensorValue(DevId + "/HDOP", SensorValue.SensorType.Double, double.MinValue);
			SensorVDOP = fsEngine.Sensors.CreateSensorValue(DevId + "/VDOP", SensorValue.SensorType.Double, double.MinValue);

			// Used by depth sounders
			// SDDPT
			SensorDepth = fsEngine.Sensors.CreateSensorValue(DevId + "/Depth", SensorValue.SensorType.Double, 0.0);
			// SDMTW
			SensorMeanWaterTemp = fsEngine.Sensors.CreateSensorValue(DevId + "/MeanWaterTemp", SensorValue.SensorType.Double, 0.0);
			// SDDBT
			SensorTransducerDepthFeet = fsEngine.Sensors.CreateSensorValue(DevId + "/DepthFromTransducerFeet", SensorValue.SensorType.Double, 0.0);
			SensorTransducerDepthMeters = fsEngine.Sensors.CreateSensorValue(DevId + "/DepthFromTransducerMeters", SensorValue.SensorType.Double, 0.0);
			SensorTransducerDepthFathoms = fsEngine.Sensors.CreateSensorValue(DevId + "/DepthFromTransducerFathoms", SensorValue.SensorType.Double, 0.0);

			p_NMEAParser = new NMEAParser.NMEAParser();
			p_NMEAParser.Sentences["GPRMC"].OnSentenceRecieved += NMEAParser_OnGPRMCSentenceRecieved;
			p_NMEAParser.Sentences["GPGGA"].OnSentenceRecieved += NMEADevice_OnGPGGASentenceRecieved;
			p_NMEAParser.Sentences["GPGSA"].OnSentenceRecieved += NMEADevice_OnGPGSASentenceRecieved;


			p_NMEAParser.Sentences["SDDPT"].OnSentenceRecieved += NMEADevice_OnSDDPTSentenceRecieved;
			p_NMEAParser.Sentences["SDDBT"].OnSentenceRecieved += NMEADevice_OnSDDBTSentenceRecieved;
			p_NMEAParser.Sentences["SDMTW"].OnSentenceRecieved += NMEADevice_OnSDMTWSentenceRecieved;
		}

		public void SetParameterValue(string ParameterName, object ParameterValue)
		{
			switch(ParameterName) {
				case "ConnectionString":
					p_NMEAParser.ConnectionString = ParameterValue.ToString();
					break;
				case "Make":
					p_NMEAParser.DeviceMake = ParameterValue.ToString();
					break;
				case "Model":
					p_NMEAParser.DeviceModel = ParameterValue.ToString();
					break;
				case "AutoReplayLogfile":
					p_NMEAParser.AutoRestartLogfilePlayback = (bool)ParameterValue;
					break;
				case "LogPlaybackCycleDelay":
					p_NMEAParser.InputLogCyclePeriod = (int)ParameterValue;
					break;
				case "Mode":
					switch(ParameterValue.ToString()) {
						case "Serial":
							p_NMEAParser.Mode = NMEAParser.NMEAParser.ParserMode.Serial;
							break;
						case "LogFile":
							p_NMEAParser.Mode = NMEAParser.NMEAParser.ParserMode.LogFile;
							break;
						case "Generated":
							p_NMEAParser.Mode = NMEAParser.NMEAParser.ParserMode.Generated;
							break;
						default:
							throw new Exception("Unexpected mode specified: " + ParameterValue);
					}
					break;
				default:
					throw new ArgumentException("No such parameter supported: " + ParameterName);
			}
		}

		public object GetParameterValue(string ParameterName)
		{
			switch(ParameterName) {
				case "ConnectionString":
					return (p_NMEAParser.ConnectionString);
				case "Make":
					return(p_NMEAParser.DeviceMake);
				case "Model":
					return(p_NMEAParser.DeviceModel);
				case "Mode":
					return (p_NMEAParser.Mode.ToString());
				case "AutoReplayLogfile":
					return (p_NMEAParser.AutoRestartLogfilePlayback);
				case "LogPlaybackCycleDelay":
					return (p_NMEAParser.InputLogCyclePeriod);
				default:
					throw new ArgumentException("No such parameter supported: " + ParameterName);
			}
		}

		private NMEAParser.NMEAParser p_NMEAParser;
		public void EnableDevice()
		{
			DisableDevice();
			p_NMEAParser.Connect();
			SensorEnabled.Value = true;
		}

		public void DisableDevice()
		{
			p_NMEAParser.Disconnect();
		}

		private DateTime gpsDate = DateTime.UtcNow;
		void NMEAParser_OnGPRMCSentenceRecieved(NMEAParser.NMEAParser sender, object SentenceObject)
		{
			SensorLatitude.Value = ((GPRMC)SentenceObject).Latitude;
			SensorLongitude.Value = ((GPRMC)SentenceObject).Longitude;
			SensorGPSFix.Value = ((GPRMC)SentenceObject).GPSFix;
			SensorCourse.Value = ((GPRMC)SentenceObject).Course;
			SensorSpeed.Value = ((GPRMC)SentenceObject).Speed;
			SensorFixTime.Value = ((GPRMC)SentenceObject).FixTimeStamp;
			gpsDate = ((GPRMC)SentenceObject).FixTimeStamp.Date;
		}

		void NMEADevice_OnGPGGASentenceRecieved(NMEAParser.NMEAParser sender, object SentenceObject)
		{
			SensorLatitude.Value = ((GPGGA)SentenceObject).Latitude;
			SensorLongitude.Value = ((GPGGA)SentenceObject).Longitude;
			SensorAltitude.Value = ((GPGGA)SentenceObject).Altitude;
			SensorDGPSAge.Value = ((GPGGA)SentenceObject).DGPSAge;
			SensorDGPSStationId.Value = ((GPGGA)SentenceObject).DGSStationId;
			SensorFixQuality.Value = ((GPGGA)SentenceObject).FixQuality;
			DateTime fTime = ((GPGGA)SentenceObject).FixTime;
			SensorFixTime.Value = new DateTime(gpsDate.Year, gpsDate.Month, gpsDate.Day, fTime.Hour, fTime.Minute, fTime.Second, fTime.Millisecond, DateTimeKind.Utc);
			SensorHDOP.Value = ((GPGGA)SentenceObject).HDOP;
			SensorHeightOfGeoid.Value = ((GPGGA)SentenceObject).HeightOfGeoid;
			SensorSatellitesTracked.Value = ((GPGGA)SentenceObject).SatellitesTracked;
		}

		void NMEADevice_OnGPGSASentenceRecieved(NMEAParser.NMEAParser sender, object SentenceObject)
		{
			SensorPDOP.Value = ((GPGSA)SentenceObject).PDOP;
			SensorHDOP.Value = ((GPGSA)SentenceObject).HDOP;
			SensorVDOP.Value = ((GPGSA)SentenceObject).VDOP;
			//o.PRNs
			//o.AutoFixMode;
			//o.FixMode;
		}

		void NMEADevice_OnSDDPTSentenceRecieved(NMEAParser.NMEAParser sender, object SentenceObject)
		{
			SensorDepth.Value = ((SDDPT)SentenceObject).Depth;
			// SensorKeelOffset.Value = ((SDDPT)SentenceObject).KeelOffset;
			// SensorMaxDepth.Value = ((SDDPT)SentenceObject).MaxDepth;
		}

		void NMEADevice_OnSDDBTSentenceRecieved(NMEAParser.NMEAParser sender, object SentenceObject)
		{
			SensorTransducerDepthFeet.Value = ((SDDBT)SentenceObject).Feet;
			SensorTransducerDepthMeters.Value = ((SDDBT)SentenceObject).Meters;
			SensorTransducerDepthFathoms.Value = ((SDDBT)SentenceObject).Fathoms;
		}

		void NMEADevice_OnSDMTWSentenceRecieved(NMEAParser.NMEAParser sender, object SentenceObject)
		{
			switch(((SDMTW)SentenceObject).Scale) {
				case SDMTW.TemperatureScale.Celsius:
					SensorMeanWaterTemp.Value = ((SDMTW)SentenceObject).Temperature;
					break;
				case SDMTW.TemperatureScale.Fahrenheit:
					// F to C
					SensorMeanWaterTemp.Value = ((((((SDMTW)SentenceObject).Temperature) - 32.0) * 5.0) / 9.0);
					// C to F
					// SensorMeanWaterTemp.Value = ((((((SDMTW)SentenceObject).Temperature) / 5.0) * 9.0) + 32);
					break;
				case SDMTW.TemperatureScale.Kelvin:
					// K to C
					SensorMeanWaterTemp.Value = ((((SDMTW)SentenceObject).Temperature) - 272.15);
					// C to K
					// SensorMeanWaterTemp.Value = ((((SDMTW)SentenceObject).Temperature) + 272.15);
					break;
				default:
					// Assume celsius and hope for the best :(
					SensorMeanWaterTemp.Value = ((SDMTW)SentenceObject).Temperature;
					break;
			}
		}
	}
}
