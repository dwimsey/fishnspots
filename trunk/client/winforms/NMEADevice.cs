using System;
using System.Collections.Generic;
using System.Text;
using NMEAParser;

namespace FishnSpots
{
	class NMEADevice : FSDevice
	{
		SensorValue SensorEnabled;
		SensorValue SensorFixTime;
		SensorValue SensorAltitude;
		SensorValue SensorDepth;
		SensorValue SensorLatitude;
		SensorValue SensorLongitude;
		SensorValue SensorGPSFix;
		SensorValue SensorCourse;
		SensorValue SensorSpeed;

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

			SensorAltitude = fsEngine.Sensors.CreateSensorValue(DevId + "/Altitude", SensorValue.SensorType.Double, 35.7987669706413);
			SensorDepth = fsEngine.Sensors.CreateSensorValue(DevId + "/Depth", SensorValue.SensorType.Double, 35.7987669706413);

			// Used by GPRMC
			SensorLatitude = fsEngine.Sensors.CreateSensorValue(DevId + "/Latitude", SensorValue.SensorType.Double, 35.7987669706413);
			SensorLongitude = fsEngine.Sensors.CreateSensorValue(DevId + "/Longitude", SensorValue.SensorType.Double, -79.0180706977844);
			SensorGPSFix = fsEngine.Sensors.CreateSensorValue(DevId + "/GPSFix", SensorValue.SensorType.Bool, false);
			SensorCourse = fsEngine.Sensors.CreateSensorValue(DevId + "/Course", SensorValue.SensorType.Double, 0.0);
			SensorSpeed = fsEngine.Sensors.CreateSensorValue(DevId + "/Speed", SensorValue.SensorType.Double, 0.0);

			p_NMEAParser = new NMEAParser.NMEAParser();
			p_NMEAParser.OnNewGPRMC += NMEAParser_NewGPRMC;
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

		void NMEAParser_NewGPRMC(NMEAParser.GPRMC Data)
		{
			SensorLatitude.Value = Data.Latitude;
			SensorLongitude.Value = Data.Longitude;
			SensorGPSFix.Value = Data.GPSFix;
			SensorCourse.Value = Data.Course;
			SensorSpeed.Value = Data.Speed;
			SensorFixTime.Value = Data.FixTimeStamp;
		}
	}
}
