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

        /// <summary>
        /// Determine whether a reboot is required.
        /// </summary>
        /// <returns>true if reboot is required</returns>
        bool IsRebootRequired();
    }
}
