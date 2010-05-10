using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteInstall.DriverTasks
{
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
                    VirtualMachineConfig vmconfig = _vmConfig;

                    InstallerConfig.OnRewrite = new EventHandler<ReflectionResolverEventArgs>(
                        delegate(object sender, ReflectionResolverEventArgs args)
                        {
                            object[] objs = { snapshotConfig, _vmConfig, _installersConfig };
                            ReflectionResolver resolver = new ReflectionResolver(objs);
                            string result = null;
                            if (resolver.TryResolve(args.VariableType + "Config", args.VariableName, out result))
                            {
                                args.Result = result;
                            }
                        });

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
}
