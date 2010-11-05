using System;
using System.Collections.Generic;
using System.Text;

using GMap.NET;
using GMap.NET.CacheProviders;
using NMEAParser;

namespace FishnSpots
{
	public class FSEngine
	{
		private static FSEngine singletonInstance;
		public static FSEngine GetSingletonInstance()
		{
			if (singletonInstance == null)
			{
				singletonInstance = new FSEngine();
			}
			return(singletonInstance);
		}

		public readonly FSPrefs prefs;
		internal readonly FSDatabase db;
		private PureImageCache imageCache = null;

		public readonly SensorManager Sensors;

		private List<FSDevice> Devices;
		internal FSEngine()
		{
			Devices = new List<FSDevice>(1);
			prefs = new FSPrefs(this);
			db = new FSDatabase(this);
			Sensors = new SensorManager(this);

			NMEADevice nd = null;
			while(true) {
				nd = new NMEADevice(this);
				try {
					nd.SetParameterValue("Mode", "Serial");
					nd.SetParameterValue("ConnectionString", "AUTO");
					nd.SetParameterValue("Make", "Generic");
					nd.SetParameterValue("Model", "8N1@4800");
					nd.EnableDevice();
					Devices.Add(nd);
				} catch {
					// no more serial ports
					break;
				}
			}
			if(Devices.Count == 0) {
				// no real devices found?  Replay the text file
				nd.SetParameterValue("Mode", "LogFile");
				nd.SetParameterValue("ConnectionString", "file://.\\default.nmea0183");
				nd.SetParameterValue("LogPlaybackCycleDelay", 100);
				nd.SetParameterValue("AutoReplayLogfile", true);
				nd.EnableDevice();
			}
		}

		/// <summary>
		/// This method is an internal helper so all GMap.NET windows can share a common
		/// cache object.  This should be thread safe as long as the cache provider (SQLite)
		/// is thread safe.
		/// </summary>
		/// <returns>Cache object for this engine.</returns>
		internal PureImageCache GetImageCache()
		{
			if (imageCache == null) {
				SQLitePureImageCache ic = new SQLitePureImageCache();
				ic.CacheLocation = this.prefs.TileCacheDir;
				imageCache = ic;
			}
			return(imageCache);
		}
	}
}
