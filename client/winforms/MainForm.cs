using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FishnSpots
{
    public partial class MainForm : Form
    {
		private readonly FSEngine fsEngine;

		public MainForm()
        {
			fsEngine = FSEngine.GetSingletonInstance();
            InitializeComponent();
			this.depthStrip1.engine = fsEngine;
			SetStatus("Initialized.");
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

		internal void SetStatus(String statusMsg)
		{
			statusBarMapModeLabel.Text = "Status: " + statusMsg;
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
