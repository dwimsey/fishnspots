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

		[FSSerializableProperty("Map rotation offset from course.  Using 0.0 causes up on the map to point towards the current course, values of -359 to 359 will track the course with that offset added to it.  Use NegativeInfinity to use up is north")]
		private double p_MapRotationOffset = double.NegativeInfinity;
		public double MapRotationOffset
		{
			get
			{
				return (this.p_MapRotationOffset);
			}
			set
			{
				this.p_MapRotationOffset = value;
			}
		}

		private GMapMarker markerMapCenter;
		private GMapMarker markerMyPosition;

		// layers
		private GMapOverlay top;

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
						MainMap.Position = new PointLatLng(lat, lon);
						markerMapCenter.Position = MainMap.Position;
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
				markerMyPosition.Position = new PointLatLng(lat, lon);

				if(this.p_MapRotationOffset != double.NegativeInfinity) {
					double brg = (double)SensorCourse.Value;
					double doff = this.p_MapRotationOffset;
					double mapbrg = brg + doff;
					mapbrg %= 360.0;
					if(mapbrg < 0.0) {
						mapbrg += 360;
					}
					MainMap.Bearing = (float)mapbrg;
				} else {
					if(MainMap.Bearing != 0.0f) {
						MainMap.Bearing = 0.0f;
					}
				}
				switch(this.p_TrackingMode) {
					case MapTrackingMode.Manual:
						break;
					case MapTrackingMode.Centered:
						MainMap.Position = new PointLatLng(lat, lon);
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
							MainMap.Position = new PointLatLng(lat, lon);
						}
						break;
				}
			} catch(Exception ex) {
				throw ex;
			}

		}

		public static void DownloadMapImages(GMap.NET.PureImageCache cache, double startLat, double startLon, double endLat, double endLon, int minZoom, int maxZoom, GMap.NET.MapType[] mapTypes)
		{
			PureProjection prj = null;
			List<GMap.NET.Point> tileArea = null;

			GMaps.Instance.Mode = AccessMode.ServerAndCache;
			if(cache!=null) {
				GMaps.Instance.ImageCacheSecond = cache;//fsEngine.GetImageCache();
			}
			//GMaps.Instance.ImageProxy = new WindowsFormsImageProxy();

			RectLatLng area = RectLatLng.FromLTRB(startLon, startLat, endLon, endLat);
			if(area.IsEmpty) {
				return;
			}
			foreach(GMap.NET.MapType mType in mapTypes) {
				prj = null;
				GMaps.Instance.AdjustProjection(mType, ref prj, out maxZoom);
				GMap.NET.MapType[] types = GMaps.Instance.GetAllLayersOfType(mType);

				for(int cZoom = minZoom; cZoom <= maxZoom; cZoom++) {
					tileArea = prj.GetAreaTileList(area, cZoom, 0);
					Console.WriteLine("Zoom: " + cZoom);
					Console.WriteLine("Type: " + mType.ToString());
					Console.WriteLine("Area: " + area);


					// current area
					GMap.NET.Point topLeftPx = prj.FromLatLngToPixel(area.LocationTopLeft, cZoom);
					GMap.NET.Point rightButtomPx = prj.FromLatLngToPixel(area.Bottom, area.Right, cZoom);
					GMap.NET.Point pxDelta = new GMap.NET.Point(rightButtomPx.X - topLeftPx.X, rightButtomPx.Y - topLeftPx.Y);



					// get tiles & combine into one
					foreach(GMap.NET.Point p in tileArea) {
						Console.WriteLine("Downloading[" + p + "]: " + (tileArea.IndexOf(p) + 1).ToString() + " of " + tileArea.Count);
						foreach(GMap.NET.MapType tp in types) {
							Exception ex;
							object objectTile = GMaps.Instance.GetImageFrom(tp, p, cZoom, out ex);
							if(objectTile == null) {
								continue;
							}

							// if we want to do something with the image, now is the time to do so
						}
					}
				}
			}
		}

		public GPSViewPort()
		{
			InitializeComponent();
			MainMap.OnCurrentPositionChanged += MainMap_OnCurrentPositionChanged;
			MainMap.OnMapZoomChanged += MainMap_OnMapZoomChanged;
			MainMap.OnMapTypeChanged += MainMap_OnMapTypeChanged;

			top = new GMapOverlay("top");
			MainMap.Overlays.Add(top);

			markerMapCenter = new GMapMarkerCross(MainMap.Position);
			top.Markers.Add(markerMapCenter);

			markerMyPosition = new GMapMarkerCross(MainMap.Position);
			top.Markers.Add(markerMyPosition);
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
		[FSSerializableProperty("Zoom level of the map display")]
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
		[FSSerializableProperty("Map tileset source type")]
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
			markerMapCenter.Position = point;
		}

		// MapZoomChanged
		void MainMap_OnMapZoomChanged()
		{
			markerMapCenter.Position = MainMap.Position;
		}

		void MainMap_OnMapTypeChanged(MapType type)
		{
		}
	}
}
