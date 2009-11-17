using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;

namespace RemoteInstall
{
    /// <summary>
    /// An attribute that identifies nodes that can be read/written to/from the xml file.
    /// </summary>
    public class XmlResultNode : Attribute
    {
        private XmlNodeType _nodetype = XmlNodeType.Element;
        private bool _lowercase = false;
        private bool _writeempty = true;

        /// <summary>
        /// Type of xml node to write.
        /// </summary>
        public XmlNodeType NodeType
        {
            get
            {
                return _nodetype;
            }
            set
            {
                _nodetype = value;
            }
        }

        /// <summary>
        /// Lower-case attribute or node name.
        /// </summary>
        public bool LowerCase
        {
            get
            {
                return _lowercase;
            }
            set
            {
                _lowercase = value;
            }
        }

        /// <summary>
        /// Write an empty value.
        /// </summary>
        public bool WriteEmpty
        {
            get
            {
                return _writeempty;
            }
            set
            {
                _writeempty = value;
            }
        }
    }

    /// <summary>
    /// An XML result.
    /// </summary>
    public abstract class XmlResult
    {
        /// <summary>
        /// Success or failure of the operation at this level.
        /// </summary>
        public abstract bool Success { get; set; }

        /// <summary>
        /// Write properties to an xml document.
        /// </summary>
        /// <param name="targetNode"></param>
        public virtual void WritePropertiesToXml(XmlNode targetNode)
        {
            PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (PropertyInfo property in properties)
            {
                object[] customAttributes = property.GetCustomAttributes(typeof(XmlResultNode), true);
                if (customAttributes.Length == 0)
                    continue;

                if (customAttributes.Length != 1)
                {
                    throw new Exception(string.Format("Invalid number of XmlResultNode attributes on property '{0}'",
                        property.Name));
                }

                object propertyValue = GetType().InvokeMember(property.Name, BindingFlags.GetProperty, null, this, null);
                string propertyStringValue = propertyValue == null ? null : propertyValue.ToString();

                string propertyName = property.Name;

                XmlResultNode xmlAttribute = (XmlResultNode)customAttributes[0];

                // skip empty values
                if (!xmlAttribute.WriteEmpty && string.IsNullOrEmpty(propertyStringValue))
                    continue;

                // lowercase property name
                if (xmlAttribute.LowerCase) propertyName = propertyName.ToLowerInvariant();

                switch (xmlAttribute.NodeType)
                {
                    case XmlNodeType.Element:
                        XmlNode node = targetNode.OwnerDocument.CreateElement(propertyName);
                        node.AppendChild(targetNode.OwnerDocument.CreateTextNode(propertyStringValue));
                        targetNode.AppendChild(node);
                        break;
                    case XmlNodeType.Attribute:
                        XmlAttribute attribute = targetNode.OwnerDocument.CreateAttribute(propertyName);
                        attribute.Value = propertyStringValue;
                        targetNode.Attributes.Append(attribute);
                        break;
                    default:
                        throw new Exception(string.Format("Unsupported XmlResultNode attribute type {0} on property '{1}'",
                            xmlAttribute.NodeType, property.Name));
                }
            }
        }

        public virtual void ReadPropertyFromXml(XmlNode childNode)
        {
            PropertyInfo property = GetType().GetProperty(childNode.LocalName);

            if (property == null)
            {
                throw new Exception(string.Format(
                    "Invalid property: {0}", childNode.LocalName));
            }

            if (property.CanWrite)
            {
                object value = childNode.InnerText;
                if (property.PropertyType == typeof(DateTime))
                    value = DateTime.Parse((string)value);
                if (property.PropertyType == typeof(TimeSpan))
                    value = TimeSpan.Parse((string)value);
                else if (property.PropertyType == typeof(int))
                    value = int.Parse((string)value);
                else if (property.PropertyType == typeof(bool))
                    value = bool.Parse((string)value);
                else if (property.PropertyType == typeof(InstallResult))
                    value = Enum.Parse(typeof(InstallResult), (string)value);
                property.SetValue(this, value, null);
            }
        }

        public virtual void ReadPropertiesFromXml(XmlNode sourceNode)
        {
            foreach (XmlNode childNode in sourceNode.ChildNodes)
            {
                ReadPropertyFromXml(childNode);
            }
        }

        /// <summary>
        /// Write to an XML document.
        /// </summary>
        /// <param name="targetNode"></param>
        public virtual void WriteToXml(XmlNode targetNode)
        {
            XmlNode node = targetNode is XmlDocument
                ? (targetNode as XmlDocument).CreateElement(ThisNode)
                : targetNode.OwnerDocument.CreateElement(ThisNode);
            targetNode.AppendChild(node);
            WritePropertiesToXml(node);
        }

        /// <summary>
        /// Read from an XML document.
        /// </summary>
        public virtual void ReadFromXml(XmlNode sourceNode)
        {
            ReadPropertiesFromXml(sourceNode);
        }

        public abstract string ThisNode { get; }
        public abstract string ParentNode { get; }
    }
}
