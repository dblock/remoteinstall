using System;
using System.Collections.Generic;
using System.Text;
using Vestris.VMWareLib;

namespace RemoteInstall
{
    /// <summary>
    /// A deployment driver for DNI (http://www.codeplex.com/dotnetinstaller).
    /// </summary>
    class VirtualMachineDniDeployment : VirtualMachineDeployment
    {
        private VMWareMappedVirtualMachine _vm = null;
        private DniInstallerConfig _config = null;
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
        /// A virtual machine DNI deployment package.
        /// </summary>
        /// <param name="vm">target virtual machine</param>
        /// <param name="config">installation configuration</param>
        public VirtualMachineDniDeployment(VMWareMappedVirtualMachine vm, DniInstallerConfig config)
        {
            _vm = vm;
            _config = config;
        }

        /// <summary>
        /// Uninstall a DNI package.
        /// </summary>
        public void UnInstall(out string logfile)
        {
            logfile = string.Empty;
            throw new NotSupportedException("DotNetInstaller doesn't support uninstall. Set uninstall to false.");
        }

        /// <summary>
        /// Install a DNI package.
        /// </summary>
        public void Install(out string logfile)
        {            
            logfile = string.Format("{0}.log", _config.DestinationPath);
            string fullArgs = _config.InstallArgs;
            foreach (ComponentConfig component in _config.components)
            {
                fullArgs += " /ComponentArgs \"" + component.Description + "\":\"" + component.Args + "\"";
            }

            _process = _vm.RunProgramInGuest(
                _config.DestinationPath, string.Format("/q /log /LogFile \"{0}\" {1}",
                    logfile, fullArgs));

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
