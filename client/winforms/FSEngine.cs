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

			Devices.Add(new NMEADevice(this));
			Devices[0].SetParameterValue("Mode", "Serial");
			Devices[0].SetParameterValue("ConnectionString", "AUTO");
			Devices[0].SetParameterValue("Make", "Generic");
			Devices[0].SetParameterValue("Model", "8N1@4800");
			Devices[0].EnableDevice();
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
