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

		private DateTime p_LastUpdated = DateTime.MinValue;
		public DateTime LastUpdated
		{
			get
			{
				return (p_LastUpdated);
			}
		}

		private DateTime p_LastModified = DateTime.MinValue;
		public DateTime LastModified
		{
			get
			{
				return (p_LastUpdated);
			}
		}

		private object m_SensorValue = null;
		public object Value
		{
			get
			{
				return m_SensorValue;
			}
			set
			{
				bool hasChanged = false;
				DateTime oldUpdateTime = DateTime.MinValue;
				p_LastUpdated = DateTime.UtcNow;
				object oldValue = m_SensorValue;
				if(oldValue != value) {
					if(oldValue == null || value == null) {
						hasChanged = true;
						oldUpdateTime = p_LastModified;
						p_LastModified = p_LastUpdated;
					} else {
						if(!oldValue.Equals(value)) {
							hasChanged = true;
							oldUpdateTime = p_LastModified;
							p_LastModified = p_LastUpdated;
						}
					}
				}
				m_SensorValue = value;
				if(OnValueUpdated != null) {
					OnValueUpdated(this, oldValue);
				}
				if(hasChanged) {
					if(OnValueChanged != null) {
						OnValueChanged(this, oldValue, p_LastUpdated-oldUpdateTime);
					}
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

		public delegate void SensorValueChanged(SensorValue sender, object oldValue, TimeSpan timeSinceLastChange);
		public event SensorValueChanged OnValueChanged;
	}
}
