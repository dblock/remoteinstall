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

            VMWareVirtualMachine.Process process = _vm.RunProgramInGuest(
                _config.DestinationPath, string.Format("/q /log /LogFile \"{0}\" {1}",
                    logfile, fullArgs));
            
            if (process.ExitCode != 0)
            {
                throw new Exception(string.Format("Installation failed, return code: {0}", 
                    process.ExitCode));
            }
        }

    }
}
