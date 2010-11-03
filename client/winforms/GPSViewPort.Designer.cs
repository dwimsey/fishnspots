namespace FishnSpots
{
	partial class GPSViewPort
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.MainMap = new GMap.NET.WindowsForms.GMapControl();
			this.SuspendLayout();
			// 
			// MainMap
			// 
			this.MainMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MainMap.CanDragMap = true;
			this.MainMap.GrayScaleMode = false;
			this.MainMap.LevelsKeepInMemmory = 10;
			this.MainMap.Location = new System.Drawing.Point(0, 0);
			this.MainMap.MapType = GMap.NET.MapType.OpenStreetMap;
			this.MainMap.MarkersEnabled = true;
			this.MainMap.MaxZoom = 18;
			this.MainMap.MinZoom = 6;
			this.MainMap.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.ViewCenter;
			this.MainMap.Name = "MainMap";
			this.MainMap.PolygonsEnabled = true;
			this.MainMap.RetryLoadTile = 0;
			this.MainMap.RoutesEnabled = true;
			this.MainMap.ShowTileGridLines = false;
			this.MainMap.Size = new System.Drawing.Size(310, 175);
			this.MainMap.TabIndex = 4;
			this.MainMap.Zoom = 18;
			// 
			// GPSViewPort
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(302, 144);
			this.Controls.Add(this.MainMap);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "GPSViewPort";
			this.ResumeLayout(false);

		}

		#endregion

		private GMap.NET.WindowsForms.GMapControl MainMap;

	}
}
