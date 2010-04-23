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

                if (remoteInstallResult.RebootRequired)
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
                    else if (options.PowerOff)
                    {
                        _vmPowerDriver.PowerOff();
                    }
                    else
                    {
                        ConsoleOutput.WriteLine("Skipping reboot of '{0}:{1}'", _vmPowerDriver.VmConfig.Name,
                            _vmPowerDriver.SnapshotConfig.Name);
                    }
                }
                else if (options.PowerOff)
                {
                    _vmPowerDriver.PowerOff();
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

        public void PowerOff()
        {
            try
            {
                _vmPowerDriver.ConnectToHost();
                _vmPowerDriver.PowerOff();
            }
            finally
            {
                _vmPowerDriver.CloseVirtualMachine();
                _vmPowerDriver.DisconnectFromHost();
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
}
