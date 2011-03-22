using System;
using System.Collections.Generic;
using System.Text;
using Interop.VixCOM;
using Vestris.VMWareLib;

namespace RemoteInstall
{
    public class ExecuteDriver : SequenceDriver<TasksConfig>
    {
        public ExecuteDriver(Instance remoteInstallInstance)
            : base(remoteInstallInstance)
        {

        }

        /// <summary>
        /// Execute during additional sequence.
        /// </summary>
        public override List<XmlResult> ExecuteSequence(SequenceWhen when)
        {
            List<XmlResult> results = new List<XmlResult>();
            foreach (TasksConfig executes in _configs)
            {
                foreach (TaskConfigProxy proxy in executes)
                {
                    TaskConfigInstance execute = proxy.Instance;
                    if (execute.ExecuteCommandWhen == when)
                    {
                        if (execute is CommandTaskConfig)
                        {
                            results.Add(Execute((CommandTaskConfig)execute));
                        }
                        else if (execute is SnapshotTaskConfig)
                        {
                            results.Add(Execute((SnapshotTaskConfig)execute));
                        }
                        else if (execute is VirtualMachineTaskConfig)
                        {
                            results.Add(Execute((VirtualMachineTaskConfig)execute));
                        }
                        else
                        {
                            throw new NotImplementedException(string.Format("Unsupported task type: {0}",
                                execute.GetType().Name));
                        }
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Execute a file from a remote vm.
        /// </summary>
        private TaskResult Execute(CommandTaskConfig executeConfig)
        {
            TaskResult copyFileResult = new TaskResult();
            copyFileResult.CmdLine = executeConfig.CmdLine;
            copyFileResult.Name = executeConfig.Name;
            try
            {
                ConsoleOutput.WriteLine("{0}{1} 'Remote:{2}'",
                    executeConfig.WaitForCompletion ? "Executing" : "Detaching",
                    executeConfig.ActivateWindow ? " and activating" : "",
                    executeConfig.CmdLine);

                if (!_installInstance.SimulationOnly)
                {
                    int options = 0;
                    if (! executeConfig.WaitForCompletion)
                        options |= Constants.VIX_RUNPROGRAM_RETURN_IMMEDIATELY;
                    if (executeConfig.ActivateWindow)
                        options |= Constants.VIX_RUNPROGRAM_ACTIVATE_WINDOW;

                    VMWareVirtualMachine.Process process = _installInstance.VirtualMachine.RunProgramInGuest(
                        executeConfig.Command, executeConfig.CommandLineArgs, options);

                    string exitCodeAction = "success";
                    if (executeConfig.IgnoreExitCode) exitCodeAction = "ignored";
                    else if (process.ExitCode != executeConfig.ExitCode) exitCodeAction = "error";
                    ConsoleOutput.WriteLine("Finished 'Remote:{0}', exit code = {1} ({2})",
                        executeConfig.CmdLine, process.ExitCode, exitCodeAction);

                    if (!executeConfig.IgnoreExitCode && executeConfig.WaitForCompletion)
                    {
                        if (process.ExitCode != executeConfig.ExitCode)
                        {
                            throw new Exception(string.Format("Execute failed, return code: {0}",
                                process.ExitCode));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                copyFileResult.LastError = ex.Message;
                copyFileResult.Success = false;
                ConsoleOutput.WriteLine(ex);
            }

            return copyFileResult;
        }

        /// <summary>
        /// Execute a file from a remote vm.
        /// </summary>
        private TaskResult Execute(VirtualMachineTaskConfig vmConfig)
        {
            TaskResult vmResult = new TaskResult();
            vmResult.CmdLine = string.Format("VirtualMachine: {0} '{1}'", vmConfig.Command, vmConfig.Name);
            vmResult.Name = vmConfig.Name;
            try
            {
                ConsoleOutput.WriteLine(vmResult.CmdLine);

                if (!_installInstance.SimulationOnly)
                {
                    switch (vmConfig.Command)
                    {
                        case VirtualMachineCommand.poweroff:
                            _installInstance.VirtualMachine.PowerOff();
                            break;
                        case VirtualMachineCommand.poweron:
                            _installInstance.VirtualMachine.PowerOn();
                            break;
                        case VirtualMachineCommand.waitfortoolsinguest:
                            _installInstance.VirtualMachine.WaitForToolsInGuest();
                            break;
                        case VirtualMachineCommand.shutdownguest:
                            _installInstance.VirtualMachine.ShutdownGuest();
                            break;
                        default:
                            throw new Exception(string.Format("Unsupported command '{0}'",
                                vmConfig.Command));
                    }

                    ConsoleOutput.WriteLine(string.Format("Finished {0}", vmResult.CmdLine));
                }
            }
            catch (Exception ex)
            {
                vmResult.LastError = ex.Message;
                vmResult.Success = false;
                ConsoleOutput.WriteLine(ex);
            }

            return vmResult;
        }

        /// <summary>
        /// Execute a file from a remote vm.
        /// </summary>
        private TaskResult Execute(SnapshotTaskConfig snapshotConfig)
        {
            TaskResult snapshotTaskResult = new TaskResult();
            snapshotTaskResult.CmdLine = string.Format("Snapshot: {0} '{1}'", snapshotConfig.Command, snapshotConfig.Name);
            snapshotTaskResult.Name = snapshotConfig.Name;
            try
            {
                ConsoleOutput.WriteLine(snapshotTaskResult.CmdLine);

                if (!_installInstance.SimulationOnly)
                {
                    switch (snapshotConfig.Command)
                    {
                        case SnapshotCommand.create:
                            _installInstance.VirtualMachine.Snapshots.CreateSnapshot(
                                snapshotConfig.Name, snapshotConfig.Description, 
                                snapshotConfig.IncludeMemory ? Constants.VIX_SNAPSHOT_INCLUDE_MEMORY : 0, 
                                VMWareInterop.Timeouts.CreateSnapshotTimeout);
                            break;
                        case SnapshotCommand.remove:
                            _installInstance.VirtualMachine.Snapshots.RemoveSnapshot(
                                snapshotConfig.Name);
                            break;
                        case SnapshotCommand.removeifexists:
                            
                            VMWareSnapshot snapshot = _installInstance.VirtualMachine.Snapshots.FindSnapshotByName(
                                snapshotConfig.Name);

                            if (snapshot != null)
                            {
                                ConsoleOutput.WriteLine("Removing {0}", snapshotTaskResult.CmdLine);
                                snapshot.RemoveSnapshot();
                            }
                            else
                            {
                                ConsoleOutput.WriteLine("No {0}", snapshotTaskResult.CmdLine);
                            }

                            break;

                        case SnapshotCommand.revert:
                            _installInstance.VirtualMachine.Snapshots.FindSnapshotByName(
                                snapshotConfig.Name).RevertToSnapshot(Constants.VIX_VMPOWEROP_SUPPRESS_SNAPSHOT_POWERON);
                            break;
                        default:
                            throw new Exception(string.Format("Unsupported command '{0}'",
                                snapshotConfig.Command));
                    }

                    ConsoleOutput.WriteLine(string.Format("Finished {0}", snapshotTaskResult.CmdLine));
                }
            }
            catch (Exception ex)
            {
                snapshotTaskResult.LastError = ex.Message;
                snapshotTaskResult.Success = false;
                ConsoleOutput.WriteLine(ex);
            }

            return snapshotTaskResult;
        }
    }
}
