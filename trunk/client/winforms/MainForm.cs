using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.Reflection;
using System.IO;
namespace FishnSpots
{
    public partial class MainForm : Form
    {
		private readonly FSEngine fsEngine;

		public MainForm()
        {
			fsEngine = FSEngine.GetSingletonInstance();
            InitializeComponent();

			m_deserializeDockContent = new DeserializeDockContent(XmlToIDockContent);
        }

		private IDockContent XmlToIDockContent(string xmlIn)
		{
			FSViewPort vpInfo = (FSViewPort)FishnSpots.FSSerializableObject.XmlToObject(xmlIn);
			vpInfo.engine = fsEngine;
			return (vpInfo);
		}

		private DeserializeDockContent m_deserializeDockContent;

		private void MainForm_Load(object sender, System.EventArgs e)
		{
			string configFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "default.layout");
			try{
				if(fsEngine.prefs.GUI.SaveLayoutOnExit && File.Exists(configFile)) {
					mainPanel.LoadFromXml(configFile, m_deserializeDockContent);
				} else {
					this.resetFormLayoutToDefault();
				}
			} catch {
				this.resetFormLayoutToDefault();
			}
		}

		private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			string configFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "default.layout");
			if(fsEngine.prefs.GUI.SaveLayoutOnExit) {
				mainPanel.SaveAsXml(configFile);
			}
			//if(File.Exists(configFile)) { File.Delete(configFile); }
		}
		
		private void tsComboMapMode_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			/*
			if (checkServer) {
				SetStatus("Checking server status: " + serverHost + " ...");
				try {
					System.Net.IPHostEntry eIp = System.Net.Dns.GetHostEntry(serverHost);
					SetStatus("Server online.");
					MainMap.Manager.Mode = AccessMode.ServerAndCache;
				} catch (Exception ex)
				{
					MainMap.Manager.Mode = AccessMode.CacheOnly;
					SetStatus("Server unavailable, using cache only: " + ex.Message);
					MessageBox.Show("No internet connection avaible, going to CacheOnly mode.", "GMap.NET - Demo.WindowsForms", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
			*/
		}


		private void newToolStripButton_Click(object sender, EventArgs e)
		{
			GPSViewPort v = new GPSViewPort();
			v.engine = fsEngine;
			v.TabText = "Map";
			v.Text = "Map";
			v.MapType = GMap.NET.MapType.OpenStreetOsm.ToString();
			v.Show(this.mainPanel, DockState.Document);
		}

		private void resetFormLayoutToDefault()
		{
			CloseAllDocuments();
			this.mainPanel.Dock = DockStyle.Fill;

			bool reloadComplete = false;
			Assembly assembly = Assembly.GetAssembly(typeof(MainForm));
			Stream xmlStream = assembly.GetManifestResourceStream("FishnSpots.Resources.DefaultLayout.xml2");
			if(xmlStream != null) {
				try {
					mainPanel.LoadFromXml(xmlStream, m_deserializeDockContent);
					reloadComplete = true;
				} catch {
				} finally {
					xmlStream.Close();
				}
			}

			if(!reloadComplete) {
				SensorListViewPort slVp;
				GPSViewPort mapVp;
				slVp = new SensorListViewPort();
				slVp.engine = fsEngine;
				slVp.TabText = "Sensor Values";
				slVp.Text = "Sensor Values";
				slVp.Show(this.mainPanel, DockState.DockBottomAutoHide);

				mapVp = new GPSViewPort();
				mapVp.engine = fsEngine;
				mapVp.TabText = "Google";
				mapVp.Text = "Map";
				mapVp.Zoom = 15.0;
				mapVp.MapType = GMap.NET.MapType.BingSatellite.ToString();
				mapVp.Show(this.mainPanel, DockState.Document);

				mapVp = new GPSViewPort();
				mapVp.engine = fsEngine;
				mapVp.TabText = "OSM";
				mapVp.Text = "Map";
				mapVp.Zoom = 15.0;
				mapVp.MapType = GMap.NET.MapType.OpenStreetMap.ToString();
				mapVp.Show(this.mainPanel, DockState.Document);

			}
		}

		private void CloseAllDocuments()
		{
			if(mainPanel.DocumentStyle == DocumentStyle.SystemMdi) {
				foreach(Form form in MdiChildren)
					form.Close();
			} else {
				for(int index = mainPanel.Contents.Count - 1; index >= 0; index--) {
					if(mainPanel.Contents[index] is IDockContent) {
						IDockContent content = (IDockContent)mainPanel.Contents[index];
						content.DockHandler.Close();
					}
				}
			}
		}

		private void sensorListToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SensorListViewPort slVp;
			slVp = new SensorListViewPort();
			slVp.engine = fsEngine;
			slVp.ViewportName = "Sensor Values";
			slVp.Text = "Sensor Values";
			slVp.Show(this.mainPanel, DockState.DockBottomAutoHide);
		}

		private void movingMapToolStripMenuItem_Click(object sender, EventArgs e)
		{
			GPSViewPort mapVp;
			mapVp = new GPSViewPort();
			mapVp.engine = fsEngine;
			mapVp.ViewportName = "Google";
			mapVp.Text = "Map";
			mapVp.Zoom = 15.0;
			mapVp.MapType = GMap.NET.MapType.BingSatellite.ToString();
			mapVp.Show(this.mainPanel, DockState.Document);
		}

		private void chartOverlayToolStripMenuItem_Click(object sender, EventArgs e)
		{
			GLViewPortBase chartVp;
			chartVp = new GLViewPortBase();
			chartVp.engine = fsEngine;
			chartVp.ViewportName = "GLControl";
			chartVp.Show(this.mainPanel, DockState.Document);
		}

		private void compassToolStripMenuItem_Click(object sender, EventArgs e)
		{
			GLCompassViewPort compassVp;
			compassVp = new GLCompassViewPort();
			compassVp.engine = fsEngine;
			compassVp.ViewportName = "Compass";
			compassVp.Show(this.mainPanel, DockState.Document);
		}
	}
/*
	public class GMapMarkerRect : GMapMarker
	{
		public Pen Pen;

		public GMapMarkerGoogleGreen InnerMarker;

		public GMapMarkerRect(PointLatLng p)
			: base(p)
		{
			Pen = new Pen(Brushes.Blue, 5);

			// do not forget set Size of the marker
			// if so, you shall have no event on it ;}
			Size = new System.Drawing.Size(111, 111);
			Offset = new System.Drawing.Point(-Size.Width / 2, -Size.Height / 2);
		}

		public override void OnRender(Graphics g)
		{
			g.DrawRectangle(Pen, new System.Drawing.Rectangle(LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height));
		}
	}
*/
}
