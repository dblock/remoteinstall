using System;
using System.Collections.Generic;
using System.Text;
using Vestris.VMWareLib;

namespace RemoteInstall
{
    /// <summary>
    /// A deployment driver for an executable installer.
    /// </summary>
    class VirtualMachineExeDeployment : VirtualMachineDeployment
    {
        private VMWareMappedVirtualMachine _vm = null;
        private ExeInstallerConfig _config = null;
        private VMWareVirtualMachine.Process _process = null;

        /// <summary>
        /// VirtualMachine host to connect to.
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

        /// <summary>
        /// A virtual machine EXE deployment package.
        /// </summary>
        /// <param name="vm">target virtual machine</param>
        /// <param name="config">installation configuration</param>
        public VirtualMachineExeDeployment(VMWareMappedVirtualMachine vm, ExeInstallerConfig config)
        {
            _vm = vm;
            _config = config;
        }

        /// <summary>
        /// Uninstall an EXE package.
        /// </summary>
        public void UnInstall(out string logfile)
        {
            Run(out logfile, _config.UnInstallArgs);
        }

        /// <summary>
        /// Install a DNI package.
        /// </summary>
        public void Install(out string logfile)
        {
            Run(out logfile, _config.InstallArgs);
        }

        private void Run(out string logfile, string args)
        {
            logfile = _config.LogFile;

            _process = _vm.RunProgramInGuest(
                _config.DestinationPath, args, 0);

            if (_config.ExitCodes.Count > 0)
            {
                _config.ExitCodes.Check(_process.ExitCode); 
            }
            else if (_process.ExitCode != 0)
            {
                throw new Exception(string.Format("Execution failed, return code: {0}",
                    _process.ExitCode));
            }
        }

        /// <summary>
        /// Is a reboot required?
        /// </summary>
        /// <returns>true if a reboot was required</returns>
        public bool IsRebootRequired()
        {
            if (_config.RebootRequired)
                return true;

            if (_process == null)
                return false;

            if (_config.ExitCodes.Contains(_process.ExitCode, ExitCodeResult.reboot))
            {
                ConsoleOutput.WriteLine(string.Format("Execution requires reboot (defined in exitcodes), return code: {0}",
                    _process.ExitCode));
                return true;
            }

            return false;
        }
    }
}
