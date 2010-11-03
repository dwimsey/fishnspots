using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

namespace FishnSpots
{
	public interface FSSerializable
	{
		/// <summary>
		/// Generates an XML document string which can be used to regenerate a like object in the future using
		/// DeserializedFromXml or ConfigureFromSerializedXml.
		/// </summary>
		/// <returns>A string of XML which can be used to recreate the object.</returns>
		string SerializeToXml();

		object DeserializeFromXml(string xmlIn);
		bool ConfigureFromSerializedXml(string xmlIn);
	}

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
	public class FSSerializablePropertyAttribute : Attribute
	{
		public readonly PropertyType AttributeType;
		public FSSerializablePropertyAttribute(PropertyType AttributeType)
		{
			this.AttributeType = AttributeType;
		}
	}

	public class FSSerializableObject : FSSerializable
	{
		public string SerializeToXml()
		{
			return(FishnSpots.FSSerializableObject.ObjectToXml(this));
		}

		public object DeserializeFromXml(string xmlIn)
		{
			object[] constructor_args = null;
			return (FishnSpots.FSSerializableObject.XmlToObject(xmlIn, constructor_args));
		}

		public bool ConfigureFromSerializedXml(string xmlIn)
		{
			return (FishnSpots.FSSerializableObject.ConfigureFromSerializedXml(this, xmlIn));
		}

		public static string ObjectToXml(object objIn)
		{


			XmlDocument xDoc = new XmlDocument();
			XmlElement rootNode = xDoc.CreateElement("Settings");
			// Get the object type for the object we're serializing
			Type objectType = objIn.GetType();

			rootNode.SetAttribute("class", objectType.ToString());

			XmlElement propsContainer = xDoc.CreateElement("Properties");

			XmlElement propNode;
			FSSerializablePropertyAttribute attributeInfo = null;
			// Get the list of members for this object
			MemberInfo[] members = objectType.GetMembers();

			foreach(MemberInfo m in members) {
				// we only car about fields and properties, skip others
				if(m.MemberType != MemberTypes.Field && m.MemberType != MemberTypes.Property) {
					continue;
				}
				attributeInfo = null;
				// check to see if this member has a FSSerializableProperty attributes attached
				foreach(Attribute a in m.GetCustomAttributes(typeof(FSSerializablePropertyAttribute), true)) {
					if(a.GetType() == typeof(FSSerializablePropertyAttribute)) {
						attributeInfo = (FSSerializablePropertyAttribute)a;
						break;
					}
				}
				if(attributeInfo == null) {
					continue;
				}
				switch(attributeInfo.AttributeType) {
					case PropertyType.BoolType:
					case PropertyType.DecimalType:
					case PropertyType.DoubleType:
					case PropertyType.IntegerType:
					case PropertyType.StringType:
						propNode = xDoc.CreateElement(m.Name);
						//propNode.SetAttribute("PropType", attributeInfo.AttributeType.ToString());
						propNode.AppendChild(xDoc.CreateTextNode(objectType.GetProperty(m.Name).GetValue(objIn, null).ToString()));
						propsContainer.AppendChild(propNode);
						break;

					case PropertyType.TimestampType:
						propNode = xDoc.CreateElement("ViewPortProperty");
						propNode.SetAttribute("PropName", m.Name);
						propNode.SetAttribute("PropType", attributeInfo.AttributeType.ToString());
						propNode.AppendChild(xDoc.CreateTextNode(objectType.GetProperty(m.Name).GetValue(objIn, null).ToString()));
						propsContainer.AppendChild(propNode);
						break;
					default:
						throw new ArgumentOutOfRangeException("AttributeType", attributeInfo.AttributeType, "Unexpected attribute objectType specified: " + attributeInfo.AttributeType.ToString());
				}


			}
			rootNode.AppendChild(propsContainer);

			xDoc.AppendChild(rootNode);
			return (xDoc.OuterXml.ToString());
		}

		public static object XmlToObject(string xmlIn)
		{
			return XmlToObject(xmlIn, null);
		}

		public static object XmlToObject(string xmlIn, params object[] constructor_args)
		{
			object newObject = null;
			Type newObjectType = null;
			XPathDocument xp = new XPathDocument(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlIn)));
			XPathNavigator n = xp.CreateNavigator();

			XPathExpression expr;
			XPathNodeIterator iterator;
			XPathNavigator nav2;

			expr = n.Compile("/Settings");
			iterator = n.Select(expr);
			if(iterator.MoveNext()) {
				nav2 = iterator.Current.Clone();
				string className = nav2.GetAttribute("class", "");
				if(!string.IsNullOrEmpty(className)) {
					newObjectType = Type.GetType(className);
					if(constructor_args != null) {
						newObject = Activator.CreateInstance(newObjectType, constructor_args);
					} else {
						newObject = Activator.CreateInstance(newObjectType);
					}
				} else {
					throw new ArgumentOutOfRangeException("class", className, "Unknown class specified, unable to create new object");
				}
			} else {
				return (null);
			}

			expr = n.Compile("/Settings/Properties/*");
			iterator = n.Select(expr);
			string pn;
			string pt;
			string pv;
			MemberInfo[] members = newObject.GetType().GetMembers();

			while(iterator.MoveNext()) {
				nav2 = iterator.Current.Clone();
				// @todo FSViewPortFakeURI
				pn = nav2.Name;
				pt = nav2.GetAttribute("Type", "");
				pv = nav2.Value;
				if(pn == null) {
					// @TODO signal an error or something here
					continue;
				}
				if(pv == null) {
					pv = "";
				}

				members = newObjectType.GetMember(pn);
				foreach(MemberInfo m in members) {
					if(m.MemberType != MemberTypes.Field && m.MemberType != MemberTypes.Property) {
						continue;
					}
					if(m.Name.Equals(pn)) {
						/*if(m.MemberType == MemberTypes.Property) {
							pv = pv;
						} else {
							throw new ArgumentOutOfRangeException("Meaaeta35a");
						}*/
						PropertyInfo pInfo = newObjectType.GetProperty(pn);

						object newValue = FishnSpots.FSSerializableObject.ParsePropertyValue(pInfo.PropertyType, pv);
						newObjectType.GetProperty(pn).SetValue(newObject, newValue, null);
						break;
					}
				}
			}
			return (newObject);
		}

		public static bool ConfigureFromSerializedXml(object objectIn, string xmlIn)
		{
			XPathDocument xp = new XPathDocument(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlIn)));
			XPathNavigator n = xp.CreateNavigator();

			XPathExpression expr;
			XPathNodeIterator iterator;
			XPathNavigator nav2;

			expr = n.Compile("/ViewportSettings/Properties/*");
			iterator = n.Select(expr);
			string pn;
			string pt;
			string pv;
			MemberInfo[] members = objectIn.GetType().GetMembers();

			while(iterator.MoveNext()) {
				nav2 = iterator.Current.Clone();
				// @todo FSViewPortFakeURI
				pn = nav2.GetAttribute("PropName", "");
				pt = nav2.GetAttribute("Type", "");
				pv = nav2.Value;
				if(pn == null || pt == null) {
					// @TODO signal an error or something here
					continue;
				}
				if(pv == null) {
					pv = "";
				}
				if(pn.Equals("TabName")) {
					//objectIn.TabText = pv;
				} else {
					foreach(MemberInfo m in members) {
						if(m.MemberType != MemberTypes.Field && m.MemberType != MemberTypes.Property) {
							continue;
						}
						if(m.Name.Equals(pn)) {
							//objectIn.GetType().GetProperty(pn).SetValue(objectIn, ParsePropertyType(pt, pv), null);
							break;
						}
					}
				}
			}
			return (true);
		}

		public static object ParsePropertyValue(Type objectType, string inValue)
		{
			// there is no need to 'Parse' a string, so just pass back a clone
			if(objectType == typeof(string)) {
				return (inValue.Clone());
			}

			Type[] argList = new Type[1];
			argList[0] = typeof(string);
			object[] args = new object[1];
			args[0] = inValue;

			MethodInfo m = objectType.GetMethod("Parse", argList);
			if(m != null) {
				return(m.Invoke(null, args));
			}

			// couldn't find conversion parser method, explode
			throw new Exception("Could not convert stored property string to type: " + objectType.ToString());
		}
	}
}
