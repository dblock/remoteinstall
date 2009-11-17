using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace RemoteInstall
{
    /// <summary>
    /// A powered dependent virtual machine/snapshot.
    /// </summary>
    public class VirtualMachinePowerResult : XmlResult
    {
        private string _name;
        private string _snapshot;
        private string _lasterror;
        private bool _success = false; 

        /// <summary>
        /// A copied file.
        /// </summary>
        public VirtualMachinePowerResult()
        {

        }

        /// <summary>
        /// Name of the virtual machine.
        /// </summary>
        [XmlResultNode]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Name of the snapshot.
        /// </summary>
        [XmlResultNode]
        public string Snapshot
        {
            get { return _snapshot; }
            set { _snapshot = value; }
        }

        /// <summary>
        /// Successful power operation.
        /// </summary>
        [XmlResultNode(LowerCase = true, NodeType = XmlNodeType.Attribute)]
        public override bool Success
        {
            get { return _success; }
            set { _success = value; }
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
            get { return "dependencies"; }
        }

        /// <summary>
        /// Name of this node in the results xml.
        /// </summary>
        public override string ThisNode
        {
            get { return "virtualmachine"; }
        }
    }
}
