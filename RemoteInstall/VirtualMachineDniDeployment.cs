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
        private enum DniAction
        {
            Install,
            UnInstall
        };

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
        /// Install a DNI package.
        /// </summary>
        public void Install(out string logfile)
        {
            string args = _config.InstallArgs;
            foreach (ComponentConfig component in _config.components)
            {
                args += _config.Rewrite(" /ComponentArgs \"" + component.Description + "\":\"" + component.Args + "\"");
            }

            DniExec(_config.DestinationPath, args, DniAction.UnInstall, out logfile);
        }

        /// <summary>
        /// Uninstall a DNI package.
        /// </summary>
        public void UnInstall(out string logfile)
        {
            DniExec(_config.DestinationPath, "/x", DniAction.Install, out logfile);
        }

        /// <summary>
        /// Execute dotNetInstaller.
        /// </summary>
        /// <param name="dniPath"></param>
        /// <param name="dniArgs"></param>
        /// <param name="action"></param>
        /// <param name="logfile"></param>
        private void DniExec(string dniPath, string dniArgs, DniAction action, out string logfile)
        {
            logfile = string.Format("{0}{1}.log", _config.DestinationPath, action);

            _process = _vm.RunProgramInGuest(
                _config.DestinationPath, string.Format("/q /log /LogFile \"{0}\" {1}",
                    logfile, dniArgs));

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
