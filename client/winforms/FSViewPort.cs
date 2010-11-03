using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.Xml;
using System.Xml.XPath;
using System.Reflection;

namespace FishnSpots
{
	public class FSViewPort : DockContent,FSSerializable
	{
		protected FSEngine fsEngine;
		public virtual FSEngine engine
		{
			get
			{
				return fsEngine;
			}
			set
			{
				fsEngine = value;
			}
		}

		public FSViewPort()
		{
		}

		#region FSSerializablePropertyAttribute is supported by the FSSerializable interface methods
		public virtual string SerializeToXml()
		{
			return (FishnSpots.FSSerializableObject.ObjectToXml(this));
		}

		public virtual object DeserializeFromXml(string xmlIn)
		{
			return (FishnSpots.FSSerializableObject.XmlToObject(xmlIn));
		}

		public bool ConfigureFromSerializedXml(string xmlIn)
		{
			return (FishnSpots.FSSerializableObject.ConfigureFromSerializedXml(this, xmlIn));
		}
		#endregion FSSerializablePropertyAttribute is supported by the FSSerializable interface methods

		/// <summary>
		/// Pass-through to TabText, this gives us a serializable TabName synonym for TabText
		/// </summary>
		[FSSerializableProperty(PropertyType.StringType)]
		public string TabName
		{
			get
			{
				return (this.TabText);
			}
			set
			{
				this.TabText = value;
			}
		}

		/// <summary>
		/// Provides the serialized configuration information for this viewport
		/// when saving the layout.  This should be something that can be parsed
		/// by SetFromPersistString() which will be called when this viewport is
		/// deserialized.
		/// </summary>
		/// <returns>A string representing the serialized layout of this
		/// viewport.</returns>
		protected override string GetPersistString()
		{
			return(this.SerializeToXml());
		}

		/// <summary>
		/// Configure this viewport based on the information previously serialized
		/// using the GetPersisString method.
		/// </summary>
		/// <param name="persistString">String created with a previous call to
		/// GetPersistString() or compatible xml.</param>
		/// <returns>True if the string could be deserialized into a valid
		/// configuration, false otherwise.  If this function returns false,
		/// resetToDefault is called.</returns>
		public bool SetFromPersistString(string persistString)
		{
			return (this.ConfigureFromSerializedXml(persistString));
		}

		/// <summary>
		/// Reset this viewport to its default state, throwing out all configuration
		/// information stored in memory and resorting to a clean slate without
		/// loading any configuration data not compiled in.
		/// </summary>
		public void resetToDefault()
		{
		}

		public object ParsePropertyType(string propType, string propVal)
		{
			switch((PropertyType)Enum.Parse(typeof(PropertyType), propType)) {
				case PropertyType.BoolType:
					return(bool.Parse(propVal));
				case PropertyType.DecimalType:
					return (decimal.Parse(propVal));
				case PropertyType.DoubleType:
					return (double.Parse(propVal));
				case PropertyType.IntegerType:
					return (int.Parse(propVal));
				case PropertyType.StringType:
					return (propVal);
				case PropertyType.TimestampType:
					return (DateTime.Parse(propVal));
				default:
					throw new ArgumentOutOfRangeException("PropertyType", (PropertyType)Enum.Parse(typeof(PropertyType), propType), "Unexpected property type: " + propVal);
			}
		}
	}
}
