using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace RemoteInstall
{
    public class ResultsCollection : XmlResult
        , IEnumerable<Result>
        , ICollection<Result>
        , IList<Result>
    {
        private List<Result> _results = new List<Result>();

        public override string ParentNode
        {
            get { return "remoteinstallresultsgroup"; }
        }

        public override string ThisNode
        {
            get { return "remoteinstallresults"; }
        }

        /// <summary>
        /// Whether all results yielded success.
        /// </summary>
        [XmlResultNode(LowerCase = true, NodeType = XmlNodeType.Attribute)]
        public override bool Success
        {
            get
            {
                foreach (Result result in _results)
                {
                    if (!result.Success)
                        return false;
                }

                return true;
            }
            set
            {
                throw new InvalidOperationException();
            }
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

        public override void WritePropertiesToXml(XmlNode targetNode)
        {
            foreach (Result result in _results)
            {
                result.WriteToXml(targetNode);
            }

            base.WritePropertiesToXml(targetNode);
        }

        public override void ReadPropertyFromXml(XmlNode childNode)
        {
            switch (childNode.LocalName)
            {
                case "remoteinstallresult":
                    Result result = new Result();
                    result.ReadFromXml(childNode);
                    Add(result);
                    break;
                default:
                    base.ReadPropertyFromXml(childNode);
                    break;
            }
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
