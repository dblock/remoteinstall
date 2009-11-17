using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
namespace RemoteInstall
{
    /// <summary>
    /// A task result.
    /// </summary>
    public class TaskResult : XmlResult
    {
        private string _name;
        private string _lasterror;
        private string _cmdline;
        private bool _success = true;

        /// <summary>
        /// An execute result.
        /// </summary>
        public TaskResult()
        {

        }

        /// <summary>
        /// Command line.
        /// </summary>
        [XmlResultNode]
        public string CmdLine
        {
            get { return _cmdline; }
            set { _cmdline = value; }
        }

        /// <summary>
        /// Humanly readable name of the execute.
        /// </summary>
        [XmlResultNode]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
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
            get { return "tasks"; }
        }

        /// <summary>
        /// Name of this node in the results xml.
        /// </summary>
        public override string ThisNode
        {
            get { return "task"; }
        }

        /// <summary>
        /// Success of this task.
        /// </summary>
        [XmlResultNode(LowerCase = true, NodeType = XmlNodeType.Attribute)]
        public override bool Success
        {
            get { return _success;  }
            set { _success = value; }
        }
    }
}
