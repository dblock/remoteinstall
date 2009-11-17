using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace RemoteInstall
{
    /// <summary>
    /// Install result.
    /// </summary>
    public enum InstallResult
    {
        True,
        False,
        NotRun
    };

    /// <summary>
    /// Details of an RemoteInstall, tell which VM/snapshot was tested on and what installer was tested
    /// </summary>
    public class Result : XmlResult
    {
        private string _installerName;
        private string _installerVersion;

        private string _installlogfile;
        private string _uninstalllogfile;

        private InstallResult _successfulInstall = InstallResult.NotRun;
        private InstallResult _successfulUnInstall = InstallResult.NotRun;

        private string _lastError;
        private TimeSpan _duration;

        private List<XmlResult> _results = new List<XmlResult>();

        public Result()
        {

        }

        public Result(string installerName, string installerVersion)
        {
            _installerName = installerName;
            _installerVersion = installerVersion;
        }

        /// <summary>
        /// Write to targetNode.OwnerDocument.
        /// </summary>
        /// <param name="targetNode"></param>
        public override void WriteToXml(XmlNode targetNode)
        {
            XmlNode resultNode = targetNode.OwnerDocument.CreateElement("remoteinstallresult");
            targetNode.AppendChild(resultNode);

            WritePropertiesToXml(resultNode);

            Dictionary<string, XmlNode> parentNodes = new Dictionary<string, XmlNode>();
            foreach (XmlResult xmlresult in _results)
            {
                XmlNode parentNode = null;
                if (!parentNodes.TryGetValue(xmlresult.ParentNode, out parentNode))
                {
                    parentNode = targetNode.OwnerDocument.CreateElement(xmlresult.ParentNode);
                    resultNode.AppendChild(parentNode);
                    parentNodes[xmlresult.ParentNode] = parentNode;
                }

                xmlresult.WriteToXml(parentNode);
            }
        }

        private void ReadAdditionalSequenceResultsFromXml<T>(XmlNode sourceNode)
            where T : XmlResult, new()
        {
            foreach (XmlNode additionalSequenceResultNode in sourceNode.ChildNodes)
            {
                T additionalSequenceResult = new T();
                additionalSequenceResult.ReadFromXml(additionalSequenceResultNode);
                Results.Add(additionalSequenceResult);
            }
        }

        public override void ReadFromXml(XmlNode sourceNode)
        {
            foreach (XmlNode childNode in sourceNode)
            {
                switch (childNode.LocalName)
                {
                    case "copyfiles":
                        ReadAdditionalSequenceResultsFromXml<CopyFileResult>(childNode);
                        break;
                    case "tasks":
                        ReadAdditionalSequenceResultsFromXml<TaskResult>(childNode);
                        break;
                    case "dependencies":
                        ReadAdditionalSequenceResultsFromXml<VirtualMachinePowerResult>(childNode);
                        break;
                    default:
                        ReadPropertyFromXml(childNode);
                        break;
                }
            }
        }

        /// <summary>
        /// Amount of time the RemoteInstall took
        /// </summary>
        [XmlResultNode]
        public TimeSpan Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        [XmlResultNode]
        public string DurationString
        {
            get
            {
                string result = Duration.ToString();
                int pos = result.LastIndexOf('.');
                if (pos >= 0) result = result.Remove(pos);
                return result;
            }
        }

        /// <summary>
        /// The name of the installer used
        /// </summary>
        [XmlResultNode]
        public string InstallerName
        {
            get { return _installerName; }
            set { _installerName = value; }
        }

        /// <summary>
        /// Whether the installation was successful
        /// </summary>
        [XmlResultNode]
        public InstallResult SuccessfulInstall
        {
            get { return _successfulInstall; }
            set { _successfulInstall = value; }
        }

        /// <summary>
        /// Whether the uninstallation was successful
        /// </summary>
        [XmlResultNode]
        public InstallResult SuccessfulUnInstall
        {
            get { return _successfulUnInstall; }
            set { _successfulUnInstall = value; }
        }

        /// <summary>
        /// The path of the install log file copied to the host computer
        /// </summary>
        [XmlResultNode]
        public string InstallLogfile
        {
            get { return _installlogfile; }
            set { _installlogfile = value; }
        }

        /// <summary>
        /// The path of the uninstall log file copied to the host computer
        /// </summary>
        [XmlResultNode]
        public string UnInstallLogfile
        {
            get { return _uninstalllogfile; }
            set { _uninstalllogfile = value; }
        }

        /// <summary>
        /// Version of the installer
        /// </summary>
        [XmlResultNode]
        public string InstallerVersion
        {
            get { return _installerVersion; }
            set { _installerVersion = value; }
        }

        /// <summary>
        /// Whether both install, uninstall and tasks succeeded.
        /// </summary>
        [XmlResultNode(LowerCase = true, NodeType = XmlNodeType.Attribute)]
        public override bool Success
        {
            get
            {
                if (SuccessfulInstall == InstallResult.False)
                    return false;

                if (SuccessfulUnInstall == InstallResult.False)
                    return false;

                foreach (XmlResult result in _results)
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
        /// The last error that occured during installation
        /// </summary>
        [XmlResultNode]
        public string LastError
        {
            get { return _lastError; }
            set { _lastError = value; }
        }

        private List<XmlResult> Results
        {
            get { return _results; }
            set { _results = value; }
        }

        public void AddRange(IEnumerable<XmlResult> value)
        {
            _results.AddRange(value);
        }

        public void Add(XmlResult value)
        {
            _results.Add(value);
        }

        /// <summary>
        /// Name of the container node in the results targetNode.OwnerDocument.
        /// </summary>
        public override string ParentNode
        {
            get { return "results"; }
        }

        /// <summary>
        /// Name of this node in the results targetNode.OwnerDocument.
        /// </summary>
        public override string ThisNode
        {
            get { return "result"; }
        }
    }
}
