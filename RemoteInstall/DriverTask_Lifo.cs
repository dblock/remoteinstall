﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteInstall.DriverTasks
{
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

                if (snapshotConfig.PowerOff)
                {
                    try
                    {
                        driverTaskInstance.PowerOff();
                    }
                    catch (Exception ex)
                    {
                        ConsoleOutput.WriteLine("Error powering off '{0}:{1}'", _vmConfig.Name, snapshotConfig.Name);
                        ConsoleOutput.WriteLine(ex);
                    }
                }
            }
            return results;
        }
    }
}
