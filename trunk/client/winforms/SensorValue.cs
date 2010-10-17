using System;
using System.Collections.Generic;
using System.Text;

namespace FishnSpots
{
	public class SensorValue
	{
		public enum SensorType {
			Bool,
			Integer,
			Float,
			Double,
			String,
			TimeStamp,
			Raw
		}
		public readonly string Name;
		public readonly SensorType m_SensorType;

		private object m_SensorValue = null;
		public object Value
		{
			get
			{
				return m_SensorValue;
			}
			set
			{
				object oldValue = m_SensorValue;
				m_SensorValue = value;
				if(OnValueUpdated != null) {
					OnValueUpdated(this, oldValue);
				}
			}
		}

		internal SensorValue(string SensorName, SensorType SensorType, object SensorInitialValue)
		{
			this.Name = SensorName;
			this.m_SensorType = SensorType;
			m_SensorValue = SensorInitialValue;
		}

		// override object.GetHashCode
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public delegate void SensorValueUpdated(SensorValue sender, object oldValue);
		public event SensorValueUpdated OnValueUpdated;
	}
}
