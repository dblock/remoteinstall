using System;
using System.Collections.Generic;
using System.Text;
using Vestris.VMWareLib;

namespace RemoteInstall
{
    public interface VirtualMachineDeployment
    {
        /// <summary>
        /// Install a package
        /// </summary>
        /// <returns>the path to the log file</returns>
        void Install(out string logfile);

        /// <summary>
        /// UnInstall a package
        /// </summary>
        /// <returns>the path to the log file</returns>
        void UnInstall(out string logfile);
    }
}
