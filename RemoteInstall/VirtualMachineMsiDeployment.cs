using System;
using System.Collections.Generic;
using Vestris.VMWareLib;

namespace RemoteInstall
{
    /// <summary>
    /// A deployment package for Windows Installer MSIs
    /// </summary>
    public class VirtualMachineMsiDeployment : VirtualMachineDeployment
    {
        private enum MsiAction
        {
            Install,
            UnInstall
        };

        /// <summary>
        /// Convert an msi action to string.
        /// </summary>
        /// <param name="action">an msi action</param>
        /// <returns>one of i, x</returns>
        private string MsiActionToString(MsiAction action)
        {
            switch (action)
            {
                case MsiAction.Install:
                    return "i";
                case MsiAction.UnInstall:
                    return "x";
                default:
                    throw new ArithmeticException();
            }
        }

        private VMWareMappedVirtualMachine _vm = null;
        private MsiInstallerConfig _config = null;

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

        public VirtualMachineMsiDeployment(VMWareMappedVirtualMachine vm, MsiInstallerConfig config)
        {
            _vm = vm;
            _config = config;
        }

        /// <summary>
        /// Install an MSI
        /// </summary>
        /// <returns>the path to the log file</returns>
        public void Install(out string logfile)
        {
            MsiExec(_config.DestinationPath, _config.InstallArgs, MsiAction.Install, out logfile);
        }

        /// <summary>
        /// UnInstall an MSI
        /// </summary>
        /// <returns>the path to the log file</returns>
        public void UnInstall(out string logfile)
        {
            MsiExec(_config.DestinationPath, string.Empty, MsiAction.UnInstall, out logfile);
        }

        /// <summary>
        /// Install an MSI
        /// </summary>
        /// <param name="action">msi action</param>
        /// <param name="msiPath">path to the msi</param>
        /// <param name="msiArgs">additional msi parameters</param>
        /// <param name="logfile">resulting log file</param>
        private void MsiExec(string msiPath, string msiArgs, MsiAction action, out string logfile)
        {
            string msiAction = MsiActionToString(action);
            logfile = string.Format("{0}{1}.log", msiPath, msiAction);

            VMWareVirtualMachine.Process msiexecProcess = this.VirtualMachineHost.RunProgramInGuest(
                "msiexec.exe", string.Format("/qn /{0} \"{1}\" /l*v \"{2}\" {3}",
                    msiAction, msiPath, logfile, msiArgs));
            
            if (msiexecProcess.ExitCode != 0)
            {
                throw new Exception(string.Format("{0} failed, return code: {1}",
                    action, msiexecProcess.ExitCode));
            }
        }
    }
}