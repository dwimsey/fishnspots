namespace FishnSpots
{
	partial class SensorListViewPort
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
			this.sensorListView = new System.Windows.Forms.ListView();
			this.colSensorName = new System.Windows.Forms.ColumnHeader();
			this.colSensorType = new System.Windows.Forms.ColumnHeader();
			this.colSensorValue = new System.Windows.Forms.ColumnHeader();
			this.colSensorScale = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// sensorListView
			// 
			this.sensorListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.sensorListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.sensorListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colSensorName,
            this.colSensorType,
            this.colSensorValue,
            this.colSensorScale});
			this.sensorListView.GridLines = true;
			this.sensorListView.Location = new System.Drawing.Point(0, 0);
			this.sensorListView.Name = "sensorListView";
			this.sensorListView.Size = new System.Drawing.Size(507, 322);
			this.sensorListView.TabIndex = 0;
			this.sensorListView.UseCompatibleStateImageBehavior = false;
			this.sensorListView.View = System.Windows.Forms.View.Details;
			// 
			// colSensorName
			// 
			this.colSensorName.Text = "Name";
			this.colSensorName.Width = 164;
			// 
			// colSensorType
			// 
			this.colSensorType.Text = "Type";
			this.colSensorType.Width = 74;
			// 
			// colSensorValue
			// 
			this.colSensorValue.Text = "Value";
			this.colSensorValue.Width = 188;
			// 
			// colSensorScale
			// 
			this.colSensorScale.Text = "Scale";
			// 
			// SensorListViewPort
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.sensorListView);
			this.Name = "SensorListViewPort";
			this.Size = new System.Drawing.Size(507, 322);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView sensorListView;
		private System.Windows.Forms.ColumnHeader colSensorName;
		private System.Windows.Forms.ColumnHeader colSensorValue;
		private System.Windows.Forms.ColumnHeader colSensorType;
		private System.Windows.Forms.ColumnHeader colSensorScale;

	}
}
