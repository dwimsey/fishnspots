using System;
using System.Collections.Generic;
using System.Text;

namespace FishnSpots
{
	public class FSPrefs
	{
		private String tileCacheDir = ".";
		private String dataDir = ".";
		private FSEngine fsEngine;
		private Double lastZoomLevel = 14.0;

		internal FSPrefs(FSEngine fsEngine)
		{
			this.fsEngine = fsEngine;
			this.GUI = new GUIPrefs(this);
		}

		public String TileCacheDir
		{
			get
			{
				return (tileCacheDir);
			}
		}

		public String DataDir
		{
			get
			{
				return (dataDir);
			}
		}

		public Double LastZoomLevel
		{
			get
			{
				return (lastZoomLevel);
			}
		}

		public readonly GUIPrefs GUI;

		public class GUIPrefs
		{
			/// <summary>
			/// Amount of time (in seconds) to wait for pending GUI updates to complete before skiping a GUI
			/// update.  This value is cumulative so the maximum time per thread is determined
			/// by multiplying the total number of outstanding Async method invocations by this
			/// value.
			/// </summary>
			public double AsyncWaitTimeout = 0.1;

			/// <summary>
			/// Reference to main preferences class instance.
			/// </summary>
			private readonly FSPrefs pPrefs;

			/// <summary>
			/// Internal constructor for GUI preferences.
			/// </summary>
			/// <param name="parentPrefs">Reference to main preferences class instance.</param>
			internal GUIPrefs(FSPrefs parentPrefs)
			{
				this.pPrefs = parentPrefs;
			}
		}
	}
}
