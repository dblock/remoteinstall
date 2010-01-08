using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Collections.Specialized;
using Vestris.VMWareLib;
using System.Text.RegularExpressions;
using RemoteInstall.DriverTasks;
using Amib.Threading;

namespace RemoteInstall
{
    /// <summary>
    /// Does the actual RemoteInstall.  Opens a VM, copies an installer over and installs/uninstalls it
    /// </summary>
    public class Driver
    {
        private RemoteInstallConfig _config = null;
        private string _logpath = Environment.CurrentDirectory;
        private bool _simulationOnly = false;
        private int _pipelineCount = -1;

        /// <summary>
        /// The path to the log file
        /// </summary>
        public string LogPath
        {
            get { return _logpath; }
            set { _logpath = value; }
        }

        /// <summary>
        /// RemoteInstall configuration
        /// </summary>
        public RemoteInstallConfig Config
        {
            get
            {
                return _config;
            }
        }

        /// <summary>
        /// Create a new remote installer driver, logging to logpath with configuration stored in 
        /// filename and additional command-line variables.
        /// </summary>
        public Driver(string logpath, bool simulationOnly, string filename, NameValueCollection variables, int pipelineCount)
        {
            ConfigManager configManager = new ConfigManager(filename, variables);
            _config = configManager.Configuration;
            _simulationOnly = simulationOnly;
            VMWareInterop.Timeouts = _config.Timeouts.GetVMWareTimeouts();
            _logpath = logpath;
            _pipelineCount = pipelineCount;
        }

        /// <summary>
        /// Runs each installer on every VM in specified by config file
        /// </summary>
        /// <returns>List of all the results from the RemoteInstalls</returns>
        public List<ResultsGroup> Run()
        {           
            STPStartInfo poolStartInfo = new STPStartInfo();
            List<IWorkItemResult<IList<IList<ResultsGroup>>>> poolResults = new List<IWorkItemResult<IList<IList<ResultsGroup>>>>();

            // build parallelizable tasks
            ParallelizableRemoteInstallDriverTaskCollections ptasks = new ParallelizableRemoteInstallDriverTaskCollections();

            // build the sequence that is going to be run
            foreach (VirtualMachineConfig vm in _config.VirtualMachines)
            {
                DriverTaskCollection tasks = new DriverTaskCollection();

                if (vm.Snapshots.Count == 0)
                {
                    throw new Exception(string.Format("Missing snapshots in {0}. Define a 'current' snapshot with <snapshot name='*' />.",
                        vm.Name));
                }

                switch (_config.Installers.Sequence)
                {
                    case InstallersSequence.alternate:
                        tasks.Add(new DriverTasks.DriverTask_Alternate(_config, _logpath, _simulationOnly, vm, _config.Installers, true, true));
                        break;
                    case InstallersSequence.install:
                        tasks.Add(new DriverTasks.DriverTask_Alternate(_config, _logpath, _simulationOnly, vm, _config.Installers, true, false));
                        break;
                    case InstallersSequence.uninstall:
                        tasks.Add(new DriverTasks.DriverTask_Alternate(_config, _logpath, _simulationOnly, vm, _config.Installers, false, true));
                        break;
                    case InstallersSequence.fifo:
                        tasks.Add(new DriverTasks.DriverTask_Fifo(_config, _logpath, _simulationOnly, vm, _config.Installers));
                        break;
                    case InstallersSequence.lifo:
                        tasks.Add(new DriverTasks.DriverTask_Lifo(_config, _logpath, _simulationOnly, vm, _config.Installers));
                        break;
                    case InstallersSequence.clean:
                    default:
                        tasks.Add(new DriverTasks.DriverTask_Clean(_config, _logpath, _simulationOnly, vm, _config.Installers));
                        break;
                }

                ptasks.Add(tasks);
            }

            // the number of threads in the pipeline is either user-defined
            // or the number of virtual machines (default)
            if (_pipelineCount > 0)
            {
                poolStartInfo.MaxWorkerThreads = _pipelineCount;
            }
            else
            {
                poolStartInfo.MaxWorkerThreads = ptasks.Count;
            }

            ConsoleOutput.WriteLine(string.Format("Starting {0} parallel installation(s) ({1} max) ...",
                poolStartInfo.MaxWorkerThreads, ptasks.Count));

            SmartThreadPool pool = new SmartThreadPool(poolStartInfo);

            ConsoleOutput.ShowThreadID = (poolStartInfo.MaxWorkerThreads > 1);            

            foreach (DriverTaskCollections tasks in ptasks)
            {
                poolResults.Add(pool.QueueWorkItem<DriverTaskCollections, IList<IList<ResultsGroup>>>(
                    RunTasks, tasks));
            }

            // wait for the pool to finish all the work
            SmartThreadPool.WaitAll(poolResults.ToArray());

            // collect results
            List<ResultsGroup> groups = new List<ResultsGroup>();
            foreach (IWorkItemResult<IList<IList<ResultsGroup>>> poolResult in poolResults)
            {
                foreach (IList<ResultsGroup> poolResultItem in poolResult.GetResult())
                {
                    groups.AddRange(poolResultItem);
                }
            }

            ConsoleOutput.WriteLine("Finished installation(s) with {0} result(s)", 
                groups.Count);

            return groups;
        }

        private IList<IList<ResultsGroup>> RunTasks(object args)
        {
            DriverTaskCollections coll = (DriverTaskCollections) args;
            List<IList<ResultsGroup>> results = new List<IList<ResultsGroup>>();
            foreach (DriverTaskCollection item in coll)
            {
                List<ResultsGroup> itemResults = new List<ResultsGroup>();
                foreach (DriverTask task in item)
                {
                    itemResults.AddRange(task.Run());
                }
                results.Add(itemResults);
            }
            return results;
        }
   }
}