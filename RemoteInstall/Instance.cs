using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Vestris.VMWareLib;

namespace RemoteInstall
{
    public class Instance
    {
        private string _logpath;
        private bool _simulationOnly;
        private RemoteInstallConfig _config;
        private VMWareMappedVirtualMachine _vm;
        private VirtualMachineConfig _vmConfig;
        private InstallerConfig _installerConfig;
        private SnapshotConfig _snapshotConfig;

        public VMWareMappedVirtualMachine VirtualMachine
        {
            get
            {
                return _vm;
            }
        }

        public bool SimulationOnly
        {
            get
            {
                return _simulationOnly;
            }
        }

        public class InstanceOptions
        {
            public bool Install = true;
            public bool Uninstall = true;
        }

        public Instance(
            string logpath,
            bool simulationOnly,
            RemoteInstallConfig config,
            VMWareMappedVirtualMachine vm,
            VirtualMachineConfig vmConfig,
            InstallerConfig installerConfig,
            SnapshotConfig snapshotConfig)
        {
            _logpath = logpath;
            _simulationOnly = simulationOnly;
            _config = config;
            _vm = vm;
            _vmConfig = vmConfig;
            _installerConfig = installerConfig;
            _snapshotConfig = snapshotConfig;
        }

        /// <summary>
        /// Relative log path under destination output directory.
        /// </summary>
        public string ShortLogPath
        {
            get
            {
                List<string> sb = new List<string>();
                
                if (!string.IsNullOrEmpty(_installerConfig.SvnRevision)) 
                    sb.Add(_installerConfig.SvnRevision);
                if (!string.IsNullOrEmpty(_installerConfig.Name))
                    sb.Add(_installerConfig.Name);
                if (!string.IsNullOrEmpty(_vmConfig.Name))
                    sb.Add(_vmConfig.Name);
                if (!string.IsNullOrEmpty(_snapshotConfig.Name))
                    sb.Add(_snapshotConfig.Name);

                return string.Join(@"\", sb.ToArray());
            }
        }

        /// <summary>
        /// Physical destination log path.
        /// </summary>
        public string LogPath
        {
            get
            {
                return Path.Combine(_logpath, ShortLogPath);
            }
        }

        /// <summary>
        /// Install/uninstall MSI
        /// </summary>
        /// <returns></returns>
        public Result InstallUninstall(InstanceOptions options)
        {
            ConsoleOutput.WriteLine("Running 'Remote:{0}', '{1}'", _installerConfig.Name, _installerConfig.DestinationPath);

            // remote logfiles
            string install_logfile = string.Empty;
            string uninstall_logfile = string.Empty;

            // the remote install result
            Result result = new Result(
                _installerConfig.Name,
                _installerConfig.SvnRevision);

            if (!Directory.Exists(LogPath))
            {
                ConsoleOutput.WriteLine("Creating directory '{0}'", LogPath);
                Directory.CreateDirectory(LogPath);
            }

            ConsoleOutput.WriteLine("Saving logs in '{0}'", LogPath);

            SequenceDrivers additionalSequences = new SequenceDrivers();

            ExecuteDriver executeDriver = new ExecuteDriver(this);
            executeDriver.Add(_config.Tasks);
            executeDriver.Add(_config.VirtualMachines.Tasks);
            executeDriver.Add(_vmConfig.Tasks);
            executeDriver.Add(_vmConfig.Snapshots.Tasks);
            executeDriver.Add(_snapshotConfig.Tasks);
            executeDriver.Add(_config.Installers.Tasks);
            executeDriver.Add(_installerConfig.Tasks);
            additionalSequences.Add(executeDriver);

            CopyFilesDriver copyFilesDriver = new CopyFilesDriver(this);
            copyFilesDriver.Add(_config.CopyFiles);
            copyFilesDriver.Add(_config.VirtualMachines.CopyFiles);
            copyFilesDriver.Add(_vmConfig.CopyFiles);
            copyFilesDriver.Add(_vmConfig.Snapshots.CopyFiles);
            copyFilesDriver.Add(_snapshotConfig.CopyFiles);
            copyFilesDriver.Add(_config.Installers.CopyFiles);
            copyFilesDriver.Add(_installerConfig.CopyFiles);
            additionalSequences.Add(copyFilesDriver);

            // execute and copy files before all
            result.AddRange(additionalSequences.ExecuteSequence(
                SequenceWhen.beforeall));

            DateTime start = DateTime.UtcNow;
            try
            {
                // execute the installers
                try
                {
                    VirtualMachineDeployment deploy = null;

                    if (!_simulationOnly)
                    {
                        deploy = _installerConfig.CreateDeployment(_vm);
                    }

                    // install
                    if (options.Install)
                    {
                        // execute and copy files before install
                        result.AddRange(additionalSequences.ExecuteSequence(
                            SequenceWhen.beforeinstall));

                        ConsoleOutput.WriteLine("Installing 'Remote:{0}', '{1}'",
                            _installerConfig.Name, _installerConfig.DestinationPath);

                        result.SuccessfulInstall = InstallResult.False;

                        if (!_simulationOnly)
                        {
                            deploy.Install(out install_logfile);
                        }

                        result.SuccessfulInstall = InstallResult.True;
                        result.AddRange(additionalSequences.ExecuteSequence(
                            SequenceWhen.aftersuccessfulinstall));
                    }

                    // uninstall
                    if (options.Uninstall)
                    {
                        // execute and copy files before uninstall
                        result.AddRange(additionalSequences.ExecuteSequence(
                            SequenceWhen.beforeuninstall));

                        DateTime startUnInstall = DateTime.UtcNow;
                        ConsoleOutput.WriteLine("Uninstalling 'Remote:{0}', '{1}'", _installerConfig.Name, _installerConfig.DestinationPath);

                        result.SuccessfulUnInstall = InstallResult.False;

                        if (!_simulationOnly)
                        {
                            deploy.UnInstall(out uninstall_logfile);
                        }

                        result.SuccessfulUnInstall = InstallResult.True;
                        result.AddRange(additionalSequences.ExecuteSequence(
                            SequenceWhen.aftersuccessfuluninstall));
                    }

                    if (! _simulationOnly && deploy != null && deploy.IsRebootRequired())
                    {
                        ConsoleOutput.WriteLine("Reboot required after 'Remote:{0}'", _installerConfig.Name);
                        result.RebootRequired = true;
                    }
                }
                catch (Exception ex)
                {
                    result.LastError = ex.Message;
                    result.Success = false;
                    ConsoleOutput.WriteLine(ex);
                    result.AddRange(additionalSequences.ExecuteSequence(
                        SequenceWhen.afterfailedinstalluninstall));
                }
            }
            finally
            {
                result.Duration = DateTime.UtcNow.Subtract(start);
                ConsoleOutput.WriteLine("Done after '{0}'", result.Duration);
            }

            result.AddRange(additionalSequences.ExecuteSequence(
                SequenceWhen.afterall));

            // copy the remote logfiles back
            if (!string.IsNullOrEmpty(install_logfile))
            {
                string target_install_logfile = Path.Combine(LogPath, Path.GetFileName(install_logfile));
                ConsoleOutput.WriteLine("Collecting 'Remote:{0}' => '{1}'", install_logfile, target_install_logfile);
                _vm.CopyFileFromGuestToHost(install_logfile, target_install_logfile);
                result.InstallLogfile = Path.Combine(ShortLogPath, Path.GetFileName(install_logfile));
            }

            if (!string.IsNullOrEmpty(uninstall_logfile))
            {
                string target_uninstall_logfile = Path.Combine(LogPath, Path.GetFileName(uninstall_logfile));
                ConsoleOutput.WriteLine("Collecting 'Remote:{0}' => '{1}'", uninstall_logfile, target_uninstall_logfile);
                _vm.CopyFileFromGuestToHost(uninstall_logfile, target_uninstall_logfile);
                result.UnInstallLogfile = Path.Combine(ShortLogPath, Path.GetFileName(uninstall_logfile));
            }

            return result;
        }
    }
}
