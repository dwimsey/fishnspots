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
	public enum PropertyType
	{
		BoolType,
		IntegerType,
		DoubleType,
		DecimalType,
		StringType,
		TimestampType,
	}
	
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false, Inherited=false)]
	public class FSViewPortPropertyAttribute : Attribute
	{
		public readonly PropertyType AttributeType;
		public FSViewPortPropertyAttribute(PropertyType AttributeType)
		{
			this.AttributeType = AttributeType;
		}
	}

	public class FSViewPort : DockContent
	{
		protected FSEngine fsEngine;
		public FSViewPort()
		{
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
			XmlDocument xDoc = new XmlDocument();
			XmlElement rootNode = xDoc.CreateElement("ViewPortProperties");
			rootNode.SetAttribute("class", this.GetType().ToString());

			XmlElement propNode;
			propNode = xDoc.CreateElement("ViewPortProperty");
			propNode.SetAttribute("PropName", "TabName");
			propNode.SetAttribute("PropType", "StringType");
			propNode.AppendChild(xDoc.CreateTextNode(this.TabText));
			rootNode.AppendChild(propNode);





			Type type = this.GetType();
			FSViewPortPropertyAttribute aInfo = null;
			MemberInfo[] members = type.GetMembers();
			foreach(MemberInfo m in members) {
				if(m.MemberType != MemberTypes.Field && m.MemberType != MemberTypes.Property) {
					continue;
				}
				aInfo = null;
				foreach(Attribute a in m.GetCustomAttributes(typeof(FSViewPortPropertyAttribute), true)) {
					if(a.GetType() == typeof(FSViewPortPropertyAttribute)) {
						aInfo = (FSViewPortPropertyAttribute)a;
						break;
					}
				}
				if(aInfo == null) {
					continue;
				}
				switch(aInfo.AttributeType) {
					case PropertyType.BoolType:
					case PropertyType.DecimalType:
					case PropertyType.DoubleType:
					case PropertyType.IntegerType:
					case PropertyType.StringType:
						propNode = xDoc.CreateElement("ViewPortProperty");
						propNode.SetAttribute("PropName", m.Name);
						propNode.SetAttribute("PropType", aInfo.AttributeType.ToString());
						propNode.AppendChild(xDoc.CreateTextNode(type.GetProperty(m.Name).GetValue(this, null).ToString()));
						rootNode.AppendChild(propNode);
						break;

					case PropertyType.TimestampType:
						propNode = xDoc.CreateElement("ViewPortProperty");
						propNode.SetAttribute("PropName", m.Name);
						propNode.SetAttribute("PropType", aInfo.AttributeType.ToString());
						propNode.AppendChild(xDoc.CreateTextNode(type.GetProperty(m.Name).GetValue(this,null).ToString()));
						rootNode.AppendChild(propNode);
						break;
					default:
						throw new ArgumentOutOfRangeException("AttributeType", aInfo.AttributeType, "Unexpected attribute type specified: " + aInfo.AttributeType.ToString());
				}

	
			}
			xDoc.AppendChild(rootNode);
			return (this.GetType().ToString() + ";" + xDoc.OuterXml.ToString());
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
			XPathDocument xp = new XPathDocument(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(persistString)));
			XPathNavigator n = xp.CreateNavigator();

			XPathExpression expr;
			XPathNodeIterator iterator;
			XPathNavigator nav2;

			expr = n.Compile("/ViewPortProperties/ViewPortProperty");
			iterator = n.Select(expr);
			string pn;
			string pt;
			string pv;
			MemberInfo[] members = this.GetType().GetMembers();

			while(iterator.MoveNext()) {
				nav2 = iterator.Current.Clone();
				// @todo FSViewPortFakeURI
				pn = nav2.GetAttribute("PropName", "");
				pt = nav2.GetAttribute("PropType", "");
				pv = nav2.Value;
				if(pn == null || pt == null) {
					// @TODO signal an error or something here
					continue;
				}
				if(pv == null) {
					pv = "";
				}
				if(pn.Equals("TabName")) {
					this.TabText = pv;
				} else {
					foreach(MemberInfo m in members) {
						if(m.MemberType != MemberTypes.Field && m.MemberType != MemberTypes.Property) {
							continue;
						}
						if(m.Name.Equals(pn)) {
							this.GetType().GetProperty(pn).SetValue(this, ParsePropertyType(pt, pv), null);
							break;
						}
					}
				}
			} 
			return (true);
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
