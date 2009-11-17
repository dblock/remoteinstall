using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace RemoteInstall
{
    /// <summary>
    /// A copied file.
    /// </summary>
    public class CopyFileResult : XmlResult
    {
        private string _file;
        private string _name;
        private string _destfilename;
        private string _lasterror;
        private string _data;
        private bool _success = true;
        private bool _includeInResults = true;

        /// <summary>
        /// A copied file.
        /// </summary>
        public CopyFileResult()
        {

        }

        /// <summary>
        /// Name of the copied file.
        /// </summary>
        [XmlResultNode]
        public string File
        {
            get { return _file; }
            set { _file = value; }
        }

        /// <summary>
        /// File contents/data.
        /// </summary>
        [XmlResultNode]
        public string Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// Include the file in results.
        /// </summary>
        [XmlResultNode]
        public bool IncludeInResults
        {
            get { return _includeInResults; }
            set { _includeInResults = value; }
        }

        /// <summary>
        /// Humanly readable name of the copied file.
        /// </summary>
        [XmlResultNode]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Destination file name.
        /// </summary>
        [XmlResultNode]
        public string DestFilename
        {
            get { return _destfilename; }
            set { _destfilename = value; }
        }

        /// <summary>
        /// Last error.
        /// </summary>
        [XmlResultNode]
        public string LastError
        {
            get { return _lasterror; }
            set { _lasterror = value; }
        }

        /// <summary>
        /// Name of the container node in the results xml.
        /// </summary>
        public override string ParentNode
        {
            get { return "copyfiles"; }
        }

        /// <summary>
        /// Name of this node in the results xml.
        /// </summary>
        public override string ThisNode
        {
            get { return "copyfile"; }
        }

        /// <summary>
        /// Whether the copy operation was successful.
        /// </summary>
        [XmlResultNode(LowerCase = true, NodeType = XmlNodeType.Attribute)]
        public override bool Success
        {
            get { return _success; }
            set { _success = value; }
        }
    }
}
