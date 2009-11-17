using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;
using System.IO;

namespace RemoteInstall
{
    /// <summary>
    /// A collection of RemoteInstall results
    /// </summary>
    public class Results : XmlResult,
        IEnumerable<ResultsGroup>,
        ICollection<ResultsGroup>,
        IList<ResultsGroup>
    {
        private List<ResultsGroup> _groups = new List<ResultsGroup>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public Results()
        {

        }

        /// <summary>
        /// Loads in previous results data from a file
        /// </summary>
        /// <param name="fileName"></param>
        public void Load(string fileName)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(fileName);
            Load(xml);
        }

        /// <summary>
        /// Appends (loads) results from a previously created xml document.
        /// </summary>
        /// <param name="xml">Xml document.</param>
        public void Load(XmlDocument xml)
        {
            base.ReadFromXml(xml.SelectSingleNode("/remoteinstallresultsgroups"));
        }

        /// <summary>
        /// Add a collection of group results.
        /// </summary>
        /// <param name="range">Results range.</param>
        public void AddRange(IEnumerable<ResultsGroup> range)
        {
            _groups.AddRange(range);
        }

        /// <summary>
        /// Whether all results yielded success.
        /// </summary>
        [XmlResultNode(LowerCase = true, NodeType = XmlNodeType.Attribute)]
        public override bool Success
        {
            get
            {
                foreach (ResultsGroup group in _groups)
                {
                    if (!group.Success)
                        return false;
                }

                return true;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override string ThisNode
        {
            get { return "remoteinstallresultsgroups"; }
        }

        public override string ParentNode
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Returns results in an xml format.
        /// </summary>
        /// <returns>xml document with all results</returns>
        public XmlDocument GetXml()
        {
            XmlDocument xml = new XmlDocument();
            xml.AppendChild(xml.CreateXmlDeclaration("1.0", "UTF-8", null));
            base.WriteToXml(xml);
            return xml;
        }

        public override void ReadPropertyFromXml(XmlNode childNode)
        {
            switch (childNode.LocalName)
            {
                case "remoteinstallresultsgroup":
                    ResultsGroup group = new ResultsGroup();
                    group.ReadFromXml(childNode);
                    _groups.Add(group);
                    break;
                default:
                    base.ReadPropertyFromXml(childNode);
                    break;
            }
        }

        public override void WritePropertiesToXml(XmlNode targetNode)
        {
            foreach (ResultsGroup group in _groups)
            {
                group.WriteToXml(targetNode);
            }

            base.WritePropertiesToXml(targetNode);
        }

        #region IEnumerable<RemoteInstallResultsGroup> Members

        public IEnumerator<ResultsGroup> GetEnumerator()
        {
            return _groups.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _groups.GetEnumerator();
        }

        #endregion

        #region ICollection<RemoteInstallResultsGroup> Members

        public void Add(ResultsGroup item)
        {
            _groups.Add(item);
        }

        public void Clear()
        {
            _groups.Clear();
        }

        public bool Contains(ResultsGroup item)
        {
            return _groups.Contains(item);
        }

        public void CopyTo(ResultsGroup[] array, int arrayIndex)
        {
            _groups.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _groups.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(ResultsGroup item)
        {
            return _groups.Remove(item);
        }

        #endregion

        #region IList<RemoteInstallResultsGroup> Members

        public int IndexOf(ResultsGroup item)
        {
            return _groups.IndexOf(item);
        }

        public void Insert(int index, ResultsGroup item)
        {
            _groups.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _groups.RemoveAt(index);
        }

        public ResultsGroup this[int index]
        {
            get
            {
                return _groups[index];
            }
            set
            {
                _groups[index] = value;
            }
        }

        #endregion
    }
}
