using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteInstall.DriverTasks
{
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
                    installerConfig.OnRewrite = new EventHandler<ReflectionResolverEventArgs>(
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
}
