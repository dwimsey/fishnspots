using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace FishnSpots
{
	public partial class SensorListViewPort : FSViewPort
	{
		public SensorListViewPort()
		{
			InitializeComponent();
		}

		/// <summary>
		/// FSEngine this view port uses
		/// </summary>
		public FSEngine engine
		{
			get
			{
				return fsEngine;
			}
			set
			{
				if(this.fsEngine != null) {
					// clear any existing hooks
					foreach(KeyValuePair<string, SensorValue> kvp in fsEngine.Sensors) {
						kvp.Value.OnValueUpdated -= sv_OnValueUpdated;
					}

					// clear the item list
					this.sensorListView.Items.Clear();
				}

				this.fsEngine = value;
				string[] rowItems;
				// now add our sensors to the list
				SensorValue sv;
				foreach(KeyValuePair<string,SensorValue> kvp in fsEngine.Sensors) {
					sv = kvp.Value;

					rowItems = new string[4];
					rowItems[0] = kvp.Key;
					rowItems[1] = sv.m_SensorType.ToString();
					if(sv.Value != null) {
						rowItems[2] = sv.Value.ToString();
					} else {
						rowItems[2] = "(NULL)";
					}
					rowItems[3] = "M";
					//lvi = new ListViewItem(rowItems);
					this.sensorListView.Items.Add(new ListViewItem(rowItems));
					sv.OnValueUpdated += sv_OnValueUpdated;
				}
			}
		}

		private IAsyncResult r = null;
		void sv_OnValueUpdated(SensorValue sender, object oldValue)
		{
			try {
				if(this.InvokeRequired) {
					// this makes sure we don't pile up, we'll wait for the last one to complete, since we're
					// just updating the display
					if(r != null) {
						if(!r.IsCompleted) {
							int p_GUIWaitTime = (int)(fsEngine.prefs.GUI.AsyncWaitTimeout/1000);
							if(!r.AsyncWaitHandle.WaitOne((int)(fsEngine.prefs.GUI.AsyncWaitTimeout/1000))) {
								// previous invoke did not complete in time
								// skip updating the gui until it does!
							}
						}
					}
					r = this.BeginInvoke(new SensorValue.SensorValueUpdated(this.sv_OnValueUpdated), sender, oldValue);
					//this.Invoke(new SensorValue.SensorValueUpdated(this.PositionUpdated), sender, oldValue);
					return;
				}
			} catch(Exception ex) {
				throw ex;
			}

			int i = 0;
			ListViewItem lvi = null;
			foreach(ListViewItem li in this.sensorListView.Items) {
				if(!string.IsNullOrEmpty(li.Text)) {
					if(li.Text.Equals(sender.Name)) {
						lvi = li;
					}
				}
			}
			if(lvi == null) {
				// couldn't find any list item named for this sensor
				i++;
			}
			if(lvi.SubItems.Count == 4) {
				try {
					if(sender.Value != null) {
						lvi.SubItems[2].Text = sender.Value.ToString();
					} else {
						lvi.SubItems[2].Text = "(NULL)";
					}
				} catch(Exception ex) {
					i++;
				}
			} else {
				i++;
			}
		}
	}
}
