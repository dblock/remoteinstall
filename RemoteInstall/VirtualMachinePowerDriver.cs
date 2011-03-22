using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Vestris.VMWareLib;
using Interop.VixCOM;

namespace RemoteInstall
{
    public class VirtualMachinePowerDriver : IDisposable
    {
        private VirtualMachineConfig _vmConfig;
        private bool _simulationOnly;
        private VMWareVirtualHost _host;
        private VMWareMappedVirtualMachine _vm;
        private SnapshotConfig _snapshotConfig;
        private VirtualMachinesPowerDriver _dependenciesPowerDriver;

        public VirtualMachinePowerDriver(
            VirtualMachineConfig virtualMachineConfig,
            SnapshotConfig snapshotConfig,
            bool simulationOnly)
        {
            _vmConfig = virtualMachineConfig;
            _snapshotConfig = snapshotConfig;
            _simulationOnly = simulationOnly;

            ConsoleOutput.WriteLine("Loading '{0}'", _vmConfig.Name);
        }

        public VMWareMappedVirtualMachine Vm
        {
            get
            {
                return _vm;
            }
        }

        public VirtualMachineConfig VmConfig
        {
            get
            {
                return _vmConfig;
            }
        }

        public SnapshotConfig SnapshotConfig
        {
            get
            {
                return _snapshotConfig;
            }
        }

        public VirtualMachinePowerResults PowerResults
        {
            get
            {
                return _dependenciesPowerDriver != null
                    ? _dependenciesPowerDriver.PowerResults
                    : new VirtualMachinePowerResults();
            }
        }

        public void ConnectToHost()
        {
            ConsoleOutput.WriteLine("Connecting to '{0}' ({1})",
                string.IsNullOrEmpty(_vmConfig.Host) ? "localhost" : _vmConfig.Host,
                _vmConfig.Type);

            if (!_simulationOnly)
            {
                _host = new VMWareVirtualHost();
                switch (_vmConfig.Type)
                {
                    case VirtualMachineType.Workstation:
                        _host.ConnectToVMWareWorkstation();
                        break;
                    case VirtualMachineType.ESX:
                        _host.ConnectToVMWareVIServer(_vmConfig.Host, _vmConfig.Username, _vmConfig.Password);
                        break;
                }
            }
        }

        public void DisconnectFromHost()
        {
            ConsoleOutput.WriteLine("Disconnecting from '{0}' ({1})",
                string.IsNullOrEmpty(_vmConfig.Host) ? "localhost" : _vmConfig.Host,
                _vmConfig.Type);

            if (!_simulationOnly && _host != null)
            {
                _host.Disconnect();
                _host = null;
            }
        }

        public void CloseVirtualMachine()
        {
            ConsoleOutput.WriteLine("Closing '{0}' ({1})", _vmConfig.File, _vmConfig.Type);

            if (!_simulationOnly)
            {
                if (_vm != null)
                {
                    _vm.Dispose();
                    _vm = null;
                }

                // bug: http://communities.vmware.com/message/1144091
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public void PowerOnDependencies()
        {
            if (_snapshotConfig.VirtualMachines.Count == 0)
            {
                _dependenciesPowerDriver = null;
                return;
            }

            ConsoleOutput.WriteLine("Powering on {0} dependenc{1}",
                _snapshotConfig.VirtualMachines.Count,
                _snapshotConfig.VirtualMachines.Count == 1 ? "y" : "ies");

            _dependenciesPowerDriver = new VirtualMachinesPowerDriver(
                _snapshotConfig.VirtualMachines,
                _simulationOnly);

            _dependenciesPowerDriver.PowerOn();
        }

        public void PowerOffDependencies()
        {
            if (_dependenciesPowerDriver == null)
                return;

            ConsoleOutput.WriteLine("Powering off {0} dependenc{1}",
                _snapshotConfig.VirtualMachines.Count,
                _snapshotConfig.VirtualMachines.Count == 1 ? "y" : "ies");

            _dependenciesPowerDriver.PowerOff();
            _dependenciesPowerDriver.Dispose();
            _dependenciesPowerDriver = null;
        }

        public void PrepareSnapshot()
        {
            ConsoleOutput.WriteLine("Preparing '{0}:{1}'", _vmConfig.Name, _snapshotConfig.Name);

            if (!_snapshotConfig.IsCurrentSnapshot)
            {
                ConsoleOutput.WriteLine("Restoring snapshot '{0}:{1}'", _vmConfig.Name, _snapshotConfig.Name);

                if (!_simulationOnly)
                {
                    VMWareSnapshot snapshot = _vm.Snapshots.FindSnapshot(_snapshotConfig.Name);
                    if (snapshot == null) snapshot = _vm.Snapshots.GetNamedSnapshot(_snapshotConfig.Name);
                    if (snapshot == null) throw new Exception(string.Format("Missing snapshot: {0}",
                        _snapshotConfig.Name));
                    snapshot.RevertToSnapshot(Constants.VIX_VMPOWEROP_SUPPRESS_SNAPSHOT_POWERON);
                }
            }

            PowerOn();
        }

        public void PowerOn()
        {
            // power on the virtual machine if it's powered off
            _vm.PowerOn();
            _vm.WaitForToolsInGuest();

            int powerDelay = _vmConfig.PowerDelay;
            if (powerDelay > 0)
            {
                ConsoleOutput.WriteLine("Waiting for {0} seconds for '{1}:{2}'",
                    powerDelay, _vmConfig.Name, _snapshotConfig.Name);

                if (!_simulationOnly)
                {
                    Thread.Sleep(powerDelay * 1000);
                }
            }
        }

        public void LoginToGuest()
        {
            _vm.LoginInGuest(_snapshotConfig.Username, _snapshotConfig.Password, 
                _snapshotConfig.LoginType);
        }

        public void PowerOff()
        {
            if ((!_snapshotConfig.IsCurrentSnapshot) && (_snapshotConfig.PowerOff))
            {
                _vm.PowerOff();
            }
        }

        public void ShutdownGuest()
        {
            _vm.ShutdownGuest();
        }

        public void MapVirtualMachine(CopyMethod copyMethod)
        {
            ConsoleOutput.WriteLine("Opening '{0}' ({1})", _vmConfig.File, _vmConfig.Type);

            _vm = new VMWareMappedVirtualMachine(
                _vmConfig.Name,
                _simulationOnly ? null : _host.Open(_vmConfig.File),
                _snapshotConfig.Username,
                _snapshotConfig.Password,
                copyMethod,
                _simulationOnly);
        }

        /// <summary>
        /// Throw an exception if any of the dependencies failed to power on.
        /// </summary>
        public void ThrowOnFailure()
        {
            if (_dependenciesPowerDriver != null)
            {
                _dependenciesPowerDriver.ThrowOnFailure();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_dependenciesPowerDriver != null)
            {
                _dependenciesPowerDriver.Dispose();
                _dependenciesPowerDriver = null;
            }

            if (_vm != null)
            {
                _vm.Dispose();
                _vm = null;
            }
        }

        #endregion
    }
}
