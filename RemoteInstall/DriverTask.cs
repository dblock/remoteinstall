using System;
using System.Collections.Generic;
using System.Text;
using Vestris.VMWareLib;
using System.Threading;

namespace RemoteInstall.DriverTasks
{
    public class DriverTaskInstance : IDisposable
    {
        private InstallersConfig _installersConfig;
        private VirtualMachineConfig _vmConfig;
        private VirtualMachinePowerDriver _vmPowerDriver;
        private string _logpath;
        private RemoteInstallConfig _config;
        private bool _snapshotRestored = false;
        private bool _simulationOnly = false;
        private List<Result> _results = new List<Result>();

        public class DriverTaskInstanceOptions
        {
            public bool Install = true;
            public bool Uninstall = true;
            public bool PowerOff = true;
        }

        /// <summary>
        /// Install, uninstall and dependent power results.
        /// </summary>
        public List<Result> Results
        {
            get
            {
                return _results;
            }
        }

        public DriverTaskInstance(
            RemoteInstallConfig config,
            string logpath,
            bool simulationOnly,
            VirtualMachineConfig vmConfig,
            InstallersConfig installersConfig,
            SnapshotConfig snapshotConfig)
        {
            _config = config;
            _vmConfig = vmConfig;
            _simulationOnly = simulationOnly;
            _logpath = logpath;
            _vmPowerDriver = new VirtualMachinePowerDriver(vmConfig, snapshotConfig, simulationOnly);
            _installersConfig = installersConfig;
        }

        public void CopyInstaller(InstallerConfig installerConfig)
        {
            if (!string.IsNullOrEmpty(installerConfig.File))
            {
                ConsoleOutput.WriteLine("Copying '{0}' to '{1}:{2}'",
                    installerConfig.Name,
                    _vmPowerDriver.VmConfig.Name,
                    _vmPowerDriver.SnapshotConfig.Name);

                if (!_simulationOnly)
                {
                    _vmPowerDriver.Vm.CopyFileFromHostToGuest(installerConfig.File, installerConfig.DestinationPath);
                }
            }
            else
            {
                ConsoleOutput.WriteLine("Skipped copying installer to '{0}:{1}'",
                    _vmPowerDriver.VmConfig.Name,
                    _vmPowerDriver.SnapshotConfig.Name);
            }
        }

        public bool InstallUninstall(
            InstallerConfig installerConfig,
            DriverTaskInstanceOptions options)
        {
            try
            {
                _results.Clear();

                _vmPowerDriver.PowerOnDependencies();
                _vmPowerDriver.ThrowOnFailure();

                _vmPowerDriver.ConnectToHost();

                CopyMethod copyMethod = _vmConfig.CopyMethod;
                if (copyMethod == CopyMethod.undefined) copyMethod = installerConfig.CopyMethod;
                _vmPowerDriver.MapVirtualMachine(copyMethod);

                if (!_snapshotRestored)
                {
                    _vmPowerDriver.PrepareSnapshot();
                    _snapshotRestored = true;
                }

                _vmPowerDriver.LoginToGuest();

                CopyInstaller(installerConfig);

                Instance installInstance = new Instance(
                    _logpath, _simulationOnly, _config, _vmPowerDriver.Vm, _vmPowerDriver.VmConfig, installerConfig, _vmPowerDriver.SnapshotConfig);

                Instance.InstanceOptions instanceOptions = new Instance.InstanceOptions();
                instanceOptions.Install = options.Install;
                instanceOptions.Uninstall = options.Uninstall;
                Result remoteInstallResult = installInstance.InstallUninstall(instanceOptions);

                foreach (VirtualMachinePowerResult powerResult in _vmPowerDriver.PowerResults)
                {
                    remoteInstallResult.Add(powerResult);
                }

                _results.Add(remoteInstallResult);

                if (options.PowerOff)
                {
                    _vmPowerDriver.PowerOff();
                }
                else if (remoteInstallResult.RebootRequired)
                {
                    if (installerConfig.RebootIfRequired)
                    {
                        ConsoleOutput.WriteLine("Shutting down '{0}:{1}'", _vmPowerDriver.VmConfig.Name,
                            _vmPowerDriver.SnapshotConfig.Name);
                        _vmPowerDriver.ShutdownGuest();
                        ConsoleOutput.WriteLine("Powering on '{0}:{1}'", _vmPowerDriver.VmConfig.Name,
                            _vmPowerDriver.SnapshotConfig.Name);
                        _vmPowerDriver.PowerOn();
                    }
                    else
                    {
                        ConsoleOutput.WriteLine("Skipping reboot of '{0}:{1}'", _vmPowerDriver.VmConfig.Name,
                            _vmPowerDriver.SnapshotConfig.Name);
                    }
                }

                return remoteInstallResult.Success;
            }
            catch (Exception ex)
            {
                ConsoleOutput.WriteLine(ex);
                Result result = new Result(installerConfig.Name, installerConfig.SvnRevision);
                result.LastError = ex.Message;
                result.SuccessfulInstall = result.SuccessfulUnInstall = InstallResult.False;
                _results.Add(result);
                return false;
            }
            finally
            {
                _vmPowerDriver.CloseVirtualMachine();
                _vmPowerDriver.DisconnectFromHost();
                _vmPowerDriver.PowerOffDependencies();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_vmPowerDriver != null)
            {
                _vmPowerDriver.Dispose();
                _vmPowerDriver = null;
            }
        }

        #endregion
    }

    public abstract class DriverTask
    {
        protected bool _simulationOnly;
        protected string _logpath;
        protected RemoteInstallConfig _config;
        protected VirtualMachineConfig _vmConfig;
        protected InstallersConfig _installersConfig;

        public DriverTask(
            RemoteInstallConfig config,
            string logpath,
            bool simulationOnly,
            VirtualMachineConfig vmConfig,
            InstallersConfig installersConfig)
        {
            _config = config;
            _simulationOnly = simulationOnly;
            _logpath = logpath;
            _vmConfig = vmConfig;
            _installersConfig = installersConfig;
        }

        public abstract List<ResultsGroup> Run();

        /// <summary>
        /// Returns true if there's an overlap in virtual machine or dependencies.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <returns>True if there's an overlap.</returns>
        public bool Overlaps(DriverTask task)
        {
            return _vmConfig.Overlaps(task._vmConfig);
        }
    }

    /// <summary>
    /// Clean: install/uninstall on a clean snapshot every time
    /// </summary>
    public class DriverTask_Clean : DriverTask
    {
        public DriverTask_Clean(RemoteInstallConfig config, string logpath, bool simulationOnly, VirtualMachineConfig vmConfig, InstallersConfig installersConfig)
            : base(config, logpath, simulationOnly, vmConfig, installersConfig)
        {

        }

        public override List<ResultsGroup> Run()
        {
            List<ResultsGroup> results = new List<ResultsGroup>();
            foreach (InstallerConfigProxy installerConfigProxy in _installersConfig)
            {
                InstallerConfig installerConfig = installerConfigProxy.Instance;
                foreach (SnapshotConfig snapshotConfig in _vmConfig.Snapshots)
                {
                    ResultsGroup group = new ResultsGroup(
                        _vmConfig.Name, snapshotConfig.Name, snapshotConfig.Description);
                    results.Add(group);

                    DriverTaskInstance driverTaskInstance = new DriverTaskInstance(
                        _config,
                        _logpath,
                        _simulationOnly,
                        _vmConfig,
                        _installersConfig,
                        snapshotConfig);

                    DriverTaskInstance.DriverTaskInstanceOptions options = new DriverTaskInstance.DriverTaskInstanceOptions();
                    options.Install = installerConfig.Install;
                    options.Uninstall = installerConfig.UnInstall;
                    options.PowerOff = true;
                    driverTaskInstance.InstallUninstall(installerConfig, options);
                    group.AddRange(driverTaskInstance.Results);
                }
            }
            return results;
        }
    }

    /// <summary>
    /// Alternate: install/uninstall (with options) on the same snapshot
    /// </summary>
    public class DriverTask_Alternate : DriverTask
    {
        private bool _install = true;
        private bool _uninstall = true;

        public DriverTask_Alternate(RemoteInstallConfig config, string logpath, bool simulationOnly, VirtualMachineConfig vmConfig, InstallersConfig installersConfig, bool install, bool uninstall)
            : base(config, logpath, simulationOnly, vmConfig, installersConfig)
        {
            _install = install;
            _uninstall = uninstall;
        }

        public override List<ResultsGroup> Run()
        {
            List<ResultsGroup> results = new List<ResultsGroup>();
            foreach (SnapshotConfig snapshotConfig in _vmConfig.Snapshots)
            {
                ResultsGroup group = new ResultsGroup(
                    _vmConfig.Name, snapshotConfig.Name, snapshotConfig.Description);
                results.Add(group);

                DriverTaskInstance driverTaskInstance = new DriverTaskInstance(
                    _config,
                    _logpath,
                    _simulationOnly,
                    _vmConfig,
                    _installersConfig,
                    snapshotConfig);

                foreach (InstallerConfigProxy installerConfigProxy in _installersConfig)
                {
                    InstallerConfig installerConfig = installerConfigProxy.Instance;
                    DriverTaskInstance.DriverTaskInstanceOptions options = new DriverTaskInstance.DriverTaskInstanceOptions();
                    options.Install = _install & installerConfig.Install;
                    options.Uninstall = _uninstall & installerConfig.UnInstall;
                    options.PowerOff = snapshotConfig.PowerOff;

                    if ((options.Install && installerConfig.Install) || (options.Uninstall && installerConfig.UnInstall))
                    {
                        driverTaskInstance.InstallUninstall(installerConfig, options);
                        group.AddRange(driverTaskInstance.Results);
                    }
                    else
                    {
                        ConsoleOutput.WriteLine("Skipping {0} of '{1}' on '{2}:{3}'", ProcedureString, installerConfig.Name,
                            _vmConfig.Name, snapshotConfig.Name);
                    }
                }
            }
            return results;
        }

        public string ProcedureString
        {
            get
            {
                if (_install && _uninstall)
                    return "install and uninstall";
                else if (_install)
                    return "install";
                else if (_uninstall)
                    return "uninstall";
                else
                    throw new InvalidProgramException();
            }
        }
    }

    /// <summary>
    /// Fifo: first in, first out
    /// </summary>
    public class DriverTask_Fifo : DriverTask
    {
        public DriverTask_Fifo(RemoteInstallConfig config, string logpath, bool simulationOnly, VirtualMachineConfig vmConfig, InstallersConfig installersConfig)
            : base(config, logpath, simulationOnly, vmConfig, installersConfig)
        {

        }

        public override List<ResultsGroup> Run()
        {
            List<ResultsGroup> results = new List<ResultsGroup>();
            foreach (SnapshotConfig snapshotConfig in _vmConfig.Snapshots)
            {
                ResultsGroup group = new ResultsGroup(
                    _vmConfig.Name, snapshotConfig.Name, snapshotConfig.Description);
                results.Add(group);

                DriverTaskInstance driverTaskInstance = new DriverTaskInstance(
                    _config,
                    _logpath,
                    _simulationOnly,
                    _vmConfig,
                    _installersConfig,
                    snapshotConfig);

                List<InstallerConfigProxy> uninstallConfigs = new List<InstallerConfigProxy>();

                foreach (InstallerConfigProxy installerConfigProxy in _installersConfig)
                {
                    InstallerConfig installerConfig = installerConfigProxy.Instance;

                    DriverTaskInstance.DriverTaskInstanceOptions installOptions = new DriverTaskInstance.DriverTaskInstanceOptions();
                    installOptions.Install = installerConfig.Install;
                    installOptions.Uninstall = false;
                    installOptions.PowerOff = false;

                    if (installerConfig.Install)
                    {
                        if (driverTaskInstance.InstallUninstall(installerConfig, installOptions))
                        {
                            uninstallConfigs.Add(installerConfigProxy);
                        }
                    }
                    else
                    {
                        ConsoleOutput.WriteLine("Skipping '{0}' on '{1}:{2}'", installerConfig.Name,
                            _vmConfig.Name, snapshotConfig.Name);
                    }

                    group.AddRange(driverTaskInstance.Results);
                }

                foreach (InstallerConfigProxy installerConfigProxy in uninstallConfigs)
                {
                    InstallerConfig installerConfig = installerConfigProxy.Instance;

                    DriverTaskInstance.DriverTaskInstanceOptions uninstallOptions = new DriverTaskInstance.DriverTaskInstanceOptions();
                    uninstallOptions.Install = false;
                    uninstallOptions.Uninstall = installerConfig.UnInstall;
                    uninstallOptions.PowerOff = false;

                    if (installerConfig.UnInstall)
                    {
                        driverTaskInstance.InstallUninstall(installerConfig, uninstallOptions);
                        group.AddRange(driverTaskInstance.Results);
                    }
                    else
                    {
                        ConsoleOutput.WriteLine("Skipping '{0}' on '{1}:{2}'", installerConfig.Name,
                            _vmConfig.Name, snapshotConfig.Name);
                    }
                }
            }
            return results;
        }
    }

    /// <summary>
    /// Lifo: last in, first out
    /// </summary>
    public class DriverTask_Lifo : DriverTask
    {
        public DriverTask_Lifo(RemoteInstallConfig config, string logpath, bool simulationOnly, VirtualMachineConfig vmConfig, InstallersConfig installersConfig)
            : base(config, logpath, simulationOnly, vmConfig, installersConfig)
        {

        }

        public override List<ResultsGroup> Run()
        {
            List<ResultsGroup> results = new List<ResultsGroup>();
            foreach (SnapshotConfig snapshotConfig in _vmConfig.Snapshots)
            {
                ResultsGroup group = new ResultsGroup(
                    _vmConfig.Name, snapshotConfig.Name, snapshotConfig.Description);
                results.Add(group);

                DriverTaskInstance driverTaskInstance = new DriverTaskInstance(
                    _config,
                    _logpath,
                    _simulationOnly,
                    _vmConfig,
                    _installersConfig,
                    snapshotConfig);

                List<InstallerConfigProxy> uninstallConfigs = new List<InstallerConfigProxy>();

                foreach (InstallerConfigProxy installerConfigProxy in _installersConfig)
                {
                    InstallerConfig installerConfig = installerConfigProxy.Instance;

                    DriverTaskInstance.DriverTaskInstanceOptions installOptions = new DriverTaskInstance.DriverTaskInstanceOptions();
                    installOptions.Install = installerConfig.Install;
                    installOptions.Uninstall = false;
                    installOptions.PowerOff = false;

                    if (installerConfig.Install)
                    {
                        if (driverTaskInstance.InstallUninstall(installerConfig, installOptions))
                        {
                            uninstallConfigs.Insert(0, installerConfigProxy);
                        }

                        group.AddRange(driverTaskInstance.Results);
                    }
                    else
                    {
                        ConsoleOutput.WriteLine("Skipping install of '{0}' on '{1}:{2}'", installerConfig.Name,
                            _vmConfig.Name, snapshotConfig.Name);
                    }
                }

                foreach (InstallerConfigProxy installerConfigProxy in uninstallConfigs)
                {
                    InstallerConfig installerConfig = installerConfigProxy.Instance;

                    DriverTaskInstance.DriverTaskInstanceOptions uninstallOptions = new DriverTaskInstance.DriverTaskInstanceOptions();
                    uninstallOptions.Install = false;
                    uninstallOptions.Uninstall = installerConfig.UnInstall;
                    uninstallOptions.PowerOff = false;

                    if (installerConfig.UnInstall)
                    {
                        driverTaskInstance.InstallUninstall(installerConfig, uninstallOptions);
                        group.AddRange(driverTaskInstance.Results);
                    }
                    else
                    {
                        ConsoleOutput.WriteLine("Skipping uninstall of '{0}' on '{1}:{2}'", installerConfig.Name,
                            _vmConfig.Name, snapshotConfig.Name);
                    }
                }
            }
            return results;
        }
    }
}
