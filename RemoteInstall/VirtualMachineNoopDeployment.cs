using System;
using System.Collections.Generic;
using Vestris.VMWareLib;

namespace RemoteInstall
{
    /// <summary>
    /// A noop deployment package
    /// </summary>
    public class VirtualMachineNoopDeployment : VirtualMachineDeployment
    {        
        private VMWareMappedVirtualMachine _vm = null;
        private NoopInstallerConfig _config = null;

        /// <summary>
        /// VirtualMachine host to connect to
        /// </summary>
        public VMWareMappedVirtualMachine VirtualMachineHost
        {
            get
            {
                return _vm;
            }
            set
            {
                _vm = value;
            }
        }

        public VirtualMachineNoopDeployment(VMWareMappedVirtualMachine vm, NoopInstallerConfig config)
        {
            _vm = vm;
            _config = config;
        }

        /// <summary>
        /// Install
        /// </summary>
        /// <returns>the path to the log file</returns>
        public void Install(out string logfile)
        {
            logfile = string.Empty;
        }

        /// <summary>
        /// UnInstall
        /// </summary>
        /// <returns>the path to the log file</returns>
        public void UnInstall(out string logfile)
        {
            logfile = string.Empty;
        }
    }
}