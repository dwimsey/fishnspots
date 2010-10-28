using System;
using System.Collections.Generic;
using System.Text;

namespace FishnSpots
{
	public class SensorManager : System.Collections.IEnumerable
	{
		private readonly System.Collections.Generic.Dictionary<string,SensorValue> SensorList;
		private readonly FSEngine fsEngine;

		/// <summary>
		/// Defines the method signature of methods called when sensors are added.
		/// </summary>
		/// <param name="sender">SensorManager object that added the sensor.</param>
		/// <param name="sensor">SensorValue added to the system.</param>
		public delegate void SensorAdded(object sender, SensorValue sensor);

		/// <summary>
		/// Defines the method signature of the methods called when sensors are deleted.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="sensor"></param>
		public delegate void SensorDeleted(object sender, SensorValue sensor);
		
		/// <summary>
		/// Holds the references to delegates to be called after a new sensor has been added.
		/// </summary>
		public event SensorAdded OnSensorAdded;

		/// <summary>
		/// Holds the references to the delegates to be called after a sensor has been removed from the engine.
		/// </summary>
		public event SensorDeleted OnSensorDeleted;

		/// <summary>
		/// Create a new SensorManager.
		/// </summary>
		/// <param name="fsEngine">Sets the engine to be used by this sensor manager for objects it creates and manages.</param>
		internal SensorManager(FSEngine fsEngine)
		{
			this.SensorList = new System.Collections.Generic.Dictionary<string, SensorValue>();
			this.fsEngine = fsEngine;
		}

		public SensorValue CreateSensorValue(string SensorName, SensorValue.SensorType sensorType, object InitialValue)
		{
			SensorValue v = new SensorValue(SensorName, sensorType, InitialValue);
			this.SensorList.Add(SensorName, v);
			return(v);
		}

		public bool HasSensor(string sensorName)
		{
			return this.SensorList.ContainsKey(sensorName);
		}

		public SensorValue this[string sensorName]    // Indexer declaration
		{
			get
			{
				return this.SensorList[sensorName];
			}
			set
			{
				if(this.SensorList[sensorName] == value) {
					// the same object reference means we really have nothing to do but trigger the delete/add cycle
					return;
				}
				SensorValue oldValue = null;
				if(this.SensorList.ContainsKey(sensorName)) {
					oldValue = this.SensorList[sensorName];
					this.SensorList.Remove(sensorName);
					OnSensorDeleted(this, oldValue);
				}

				if(value != null) {
					OnSensorAdded(this, value);
					this.SensorList.Add(sensorName, value);
				}
			}
		}

		#region IEnumerable support
		//public Dictionary<string, FishnSpots.SensorValue>.Enumerator GetEnumerator()
		public System.Collections.IEnumerator GetEnumerator()
		{
			return(this.SensorList.GetEnumerator());
		}
		#endregion IEnumerable support
	}
}
