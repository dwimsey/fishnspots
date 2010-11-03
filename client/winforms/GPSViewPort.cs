using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.CacheProviders;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;

namespace FishnSpots
{
	internal partial class GPSViewPort : FSViewPort
	{
		private SensorValue SensorFixTime;
		private SensorValue SensorLatitude;
		private SensorValue SensorLongitude;
		private SensorValue SensorAltitude;
		private SensorValue SensorCourse;
		private SensorValue SensorHeading;
		private SensorValue SensorSpeed;

		enum MapTrackingMode
		{
			Manual,
			Centered,
			OffsetNorth,
			OffsetCourse,
		}

		private MapTrackingMode p_TrackingMode = MapTrackingMode.Centered;
		private double p_TrackingOffsetDistance = 0.0;
		/// <summary>
		/// Direction of offset, relative to North on the map or
		/// </summary>
		private double p_TrackingOffsetBearing = 0.0;

		private GMapMarker center;
		private GMapMarker myObject;

		// layers
		private GMapOverlay top;
//		private GMapOverlay objects;
//		private GMapOverlay routes;
//		private GMapOverlay tracks;
//		private GMapOverlay waypoints;

		public const string DevId = "nmea0";

		/// <summary>
		/// FSEngine this view port uses
		/// </summary>
		public override FSEngine engine
		{
			get
			{
				return fsEngine;
			}
			set
			{
				if(this.SensorLongitude != null) {
					this.SensorLongitude.OnValueUpdated -= PositionUpdated;
				}
				this.SensorFixTime = null;
				this.SensorLatitude = null;
				this.SensorLongitude = null;
				this.SensorAltitude = null;
				this.SensorCourse = null;
				this.SensorHeading = null;
				this.SensorSpeed = null;

				this.fsEngine = value;
				if(this.fsEngine != null) {
					this.MainMap.Manager.ImageCacheSecond = fsEngine.GetImageCache();
					if(this.fsEngine.Sensors.HasSensor(DevId + "/enabled")) {
						this.SensorFixTime = this.fsEngine.Sensors[DevId + "/FixTime"];
						this.SensorLatitude = this.fsEngine.Sensors[DevId + "/Latitude"];
						this.SensorLongitude = this.fsEngine.Sensors[DevId + "/Longitude"];
						this.SensorAltitude = this.fsEngine.Sensors[DevId + "/Altitude"];
						this.SensorCourse = this.fsEngine.Sensors[DevId + "/Course"];
						this.SensorHeading = this.fsEngine.Sensors[DevId + "/Course"];
						this.SensorSpeed = this.fsEngine.Sensors[DevId + "/Speed"];
						this.SensorFixTime.OnValueUpdated += PositionUpdated;		// this lets us only process updates when the fix itself is updated from the gps
						double lat = (double)SensorLatitude.Value;
						double lon = (double)SensorLongitude.Value;
						double alt = (double)SensorAltitude.Value;
						MainMap.CurrentPosition = new PointLatLng(lat, lon);
						center.Position = MainMap.CurrentPosition;
					}
				} else {
					this.MainMap.Manager.ImageCacheSecond = null;
				}
			}
		}

		private IAsyncResult r = null;
		public void PositionUpdated(SensorValue sender, object oldValue)
		{
			try {
				if(this.InvokeRequired) {
					// this makes sure we don't pile up, we'll wait for the last one to complete
					if(r != null) {
						if(!r.IsCompleted) {
							int p_GUIWaitTime = (int)(fsEngine.prefs.GUI.AsyncWaitTimeout/1000);
							if(!r.AsyncWaitHandle.WaitOne((int)(fsEngine.prefs.GUI.AsyncWaitTimeout/1000))) {
								// previous invoke did not complete in time
								// skip updating the gui until it does!
							}
						}
					}
					r = this.BeginInvoke(new SensorValue.SensorValueUpdated(this.PositionUpdated), sender, oldValue);
					//this.Invoke(new SensorValue.SensorValueUpdated(this.PositionUpdated), sender, oldValue);
					return;
				}
			} catch(Exception ex) {
				throw ex;
			}

			try {
			double lat = (double)SensorLatitude.Value;
			double lon = (double)SensorLongitude.Value;
			double alt = (double)SensorAltitude.Value;
			myObject.Position = new PointLatLng(lat, lon);
			switch(this.p_TrackingMode) {
				case MapTrackingMode.Manual:
					break;
				case MapTrackingMode.Centered:
					MainMap.CurrentPosition = new PointLatLng(lat, lon);
					break;
				case MapTrackingMode.OffsetNorth:
				case MapTrackingMode.OffsetCourse:
					{
						double offset_dir = p_TrackingOffsetBearing;
						if(this.p_TrackingMode == MapTrackingMode.OffsetCourse) {
							offset_dir += (double)SensorHeading.Value;
							if(offset_dir >= 360.0) {
								offset_dir = offset_dir % 360.0;
							}
						}
						GPSMath.MoveCoords(ref lat, ref lon, offset_dir, p_TrackingOffsetDistance);
						MainMap.CurrentPosition = new PointLatLng(lat, lon);
					}
					break;
			}
			} catch(Exception ex) {
				throw ex;
			}

		}

		public GPSViewPort()
		{
			InitializeComponent();
			MainMap.OnCurrentPositionChanged += MainMap_OnCurrentPositionChanged;
			MainMap.OnMapZoomChanged += MainMap_OnMapZoomChanged;
			MainMap.OnMapTypeChanged += MainMap_OnMapTypeChanged;

			top = new GMapOverlay(MainMap, "top");
			MainMap.Overlays.Add(top);

			center = new GMapMarkerCross(MainMap.CurrentPosition);
			myObject = new GMapMarkerCross(MainMap.CurrentPosition);
			top.Markers.Add(center);

			/*
						routes = new GMapOverlay(MainMap, "routes");
						MainMap.Overlays.Add(routes);

						tracks = new GMapOverlay(MainMap, "tracks");
						MainMap.Overlays.Add(tracks);

						objects = new GMapOverlay(MainMap, "objects");
						MainMap.Overlays.Add(objects);

						waypoints = new GMapOverlay(MainMap, "waypoints");
						MainMap.Overlays.Add(waypoints);


						routes.Routes.CollectionChanged += new GMap.NET.ObjectModel.NotifyCollectionChangedEventHandler(Routes_CollectionChanged);
						objects.Markers.CollectionChanged += new GMap.NET.ObjectModel.NotifyCollectionChangedEventHandler(Markers_CollectionChanged);


						List<PointLatLng> aList = new List<PointLatLng>();
						aList.Add(new PointLatLng(35.79876, -79.01707));
						aList.Add(new PointLatLng(35.79876, -79.01807));
						aList.Add(new PointLatLng(35.79576, -79.01807));
						aList.Add(new PointLatLng(35.79576, -79.01707));
						GMapPolygon gp = new GMapPolygon(aList, "Default Track");
						tracks.Polygons.Add(gp);
			*/
		}

		/// <summary>
		/// min zoom
		/// </summary>      
		[Category("GSPViewPort")]
		[Description("Map zoom level")]
		[FSSerializableProperty(PropertyType.DoubleType)]
		public double Zoom
		{
			get
			{
				return MainMap.Zoom;
			}
			set
			{
				if(value < 1) {
					MainMap.Zoom = 1;
				} else {
					MainMap.Zoom = value;
				}
			}
		}

		/// <summary>
		/// min zoom
		/// </summary>      
		[Category("GSPViewPort")]
		[Description("Map Tileset Type")]
		[FSSerializableProperty(PropertyType.StringType)]
		public string MapType
		{
			get
			{
				return MainMap.MapType.ToString();
			}
			set
			{
				MainMap.MapType = (MapType)Enum.Parse(typeof(MapType), value);
			}
		}

		void Markers_CollectionChanged(object sender, GMap.NET.ObjectModel.NotifyCollectionChangedEventArgs e)
		{
			//textBoxMarkerCount.Text = objects.Markers.Count.ToString();
		}

		void Routes_CollectionChanged(object sender, GMap.NET.ObjectModel.NotifyCollectionChangedEventArgs e)
		{
			//textBoxrouteCount.Text = routes.Routes.Count.ToString();
		}

		// current point changed
		void MainMap_OnCurrentPositionChanged(PointLatLng point)
		{
			center.Position = point;
		}

		// MapZoomChanged
		void MainMap_OnMapZoomChanged()
		{
			center.Position = MainMap.CurrentPosition;
		}

		void MainMap_OnMapTypeChanged(MapType type)
		{
		}
	}
}
