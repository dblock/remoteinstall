using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace RemoteInstall
{
    /// <summary>
    /// A group of remote installer results.
    /// </summary>
    public class ResultsGroup : XmlResult,
        IEnumerable<Result>,
        ICollection<Result>,
        IList<Result>
    {
        private ResultsCollection _results = new ResultsCollection();
        private string _vm;
        private string _snapshot;
        private string _description;

        /// <summary>
        /// The name of the VM used
        /// </summary>
        [XmlResultNode]
        public string Vm
        {
            get { return _vm; }
            set { _vm = value; }
        }

        /// <summary>
        /// The name of the snapshot used
        /// </summary>
        [XmlResultNode]
        public string Snapshot
        {
            get { return _snapshot; }
            set { _snapshot = value; }
        }

        /// <summary>
        /// The description of the snapshot used
        /// </summary>
        [XmlResultNode]
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public ResultsGroup()
        {
        }

        public ResultsGroup(string vm, string snapshot, string description)
        {
            _vm = vm;
            _snapshot = snapshot;
            _description = description;
        }

        public override string ParentNode
        {
            get { return "remoteinstallresultsgroups"; }
        }

        public override string ThisNode
        {
            get { return "remoteinstallresultsgroup"; }
        }

        /// <summary>
        /// Whether all results yielded success.
        /// </summary>
        [XmlResultNode(LowerCase = true, NodeType = XmlNodeType.Attribute)]
        public override bool Success
        {
            get { return _results.Success; }
            set { _results.Success = value; }
        }

        /// <summary>
        /// Add a collection of results.
        /// </summary>
        /// <param name="results">enumerable results set</param>
        public void AddRange(IEnumerable<Result> results)
        {
            _results.AddRange(results);
        }

        /// <summary>
        /// Add a single result to the results collection.
        /// </summary>
        /// <param name="result">single result</param>
        public void Add(Result result)
        {
            _results.Add(result);
        }

        public override void ReadPropertyFromXml(XmlNode childNode)
        {
            switch (childNode.LocalName)
            {
                case "remoteinstallresults":
                    _results.ReadFromXml(childNode);
                    break;
                default:
                    base.ReadPropertyFromXml(childNode);
                    break;
            }
        }

        public override void WritePropertiesToXml(XmlNode targetNode)
        {
            _results.WriteToXml(targetNode);
            base.WritePropertiesToXml(targetNode);
        }

        #region IEnumerable<RemoteInstallResult> Members

        public IEnumerator<Result> GetEnumerator()
        {
            return _results.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _results.GetEnumerator();
        }

        #endregion

        #region ICollection<RemoteInstallResult> Members

        public void Clear()
        {
            _results.Clear();
        }

        public bool Contains(Result item)
        {
            return _results.Contains(item);
        }

        public void CopyTo(Result[] array, int arrayIndex)
        {
            _results.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _results.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Result item)
        {
            return _results.Remove(item);
        }

        #endregion

        #region IList<RemoteInstallResult> Members

        public int IndexOf(Result item)
        {
            return _results.IndexOf(item);
        }

        public void Insert(int index, Result item)
        {
            _results.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _results.RemoveAt(index);
        }

        public Result this[int index]
        {
            get
            {
                return _results[index];
            }
            set
            {
                _results[index] = value;
            }
        }

        #endregion
    }
}
