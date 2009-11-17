using System;
using System.Collections.Generic;
using System.Text;
using Vestris.VMWareLib;

namespace RemoteInstall
{
    /// <summary>
    /// A virtual machine power/snapshot driver that starts multiple virtual machines with their snapshots.
    /// </summary>
    public class VirtualMachinesPowerDriver : IDisposable
    {
        private VirtualMachinesConfig _virtualMachinesConfig;
        private List<VirtualMachinePowerDriver> _virtualMachinePowerDrivers = new List<VirtualMachinePowerDriver>();
        private VirtualMachinePowerResults _powerResults = new VirtualMachinePowerResults();
        private bool _simulationOnly;

        /// <summary>
        /// A virtual machine power/snapshot driver.
        /// </summary>
        /// <param name="virtualMachinesConfig"></param>
        /// <param name="simulationOnly"></param>
        public VirtualMachinesPowerDriver(VirtualMachinesConfig virtualMachinesConfig, bool simulationOnly)
        {
            _virtualMachinesConfig = virtualMachinesConfig;
            _simulationOnly = simulationOnly;
        }

        /// <summary>
        /// Power operation results.
        /// </summary>
        public VirtualMachinePowerResults PowerResults
        {
            get
            {
                return _powerResults;
            }
        }

        public void PowerOn()
        {
            foreach (VirtualMachineConfig virtualMachineConfig in _virtualMachinesConfig)
            {
                _powerResults.Add(PowerOn(virtualMachineConfig));
            }
        }

        private VirtualMachinePowerResult PowerOn(VirtualMachineConfig virtualMachineConfig)
        {
            VirtualMachinePowerResult powerResult = new VirtualMachinePowerResult();
            powerResult.Name = virtualMachineConfig.Name;
            try
            {
                if (virtualMachineConfig.Snapshots.Count != 1)
                {
                    throw new Exception(string.Format("Invalid number of snapshots in '{0}'",
                        virtualMachineConfig.Name));
                }

                VirtualMachinePowerDriver powerDriver = new VirtualMachinePowerDriver(
                    virtualMachineConfig,
                    virtualMachineConfig.Snapshots[0],
                    _simulationOnly);
                
                _virtualMachinePowerDrivers.Add(powerDriver);
                
                powerDriver.ConnectToHost();
                powerDriver.MapVirtualMachine(virtualMachineConfig.CopyMethod);
                powerDriver.PrepareSnapshot();
                powerResult.Success = true;
            }
            catch (Exception ex)
            {
                ConsoleOutput.WriteLine(ex);
                powerResult.LastError = ex.Message;
                powerResult.Success = false;
            }

            return powerResult;
        }

        public void PowerOff()
        {
            foreach (VirtualMachinePowerDriver virtualMachinePowerDriver in _virtualMachinePowerDrivers)
            {
                try
                {
                    virtualMachinePowerDriver.PowerOff();
                    virtualMachinePowerDriver.CloseVirtualMachine();
                    virtualMachinePowerDriver.DisconnectFromHost();
                }
                catch (Exception ex)
                {
                    ConsoleOutput.WriteLine("ERROR: Failed to power off and close '{0}:{1}'", 
                        virtualMachinePowerDriver.VmConfig.Name, virtualMachinePowerDriver.SnapshotConfig.Name);
                    ConsoleOutput.WriteLine(ex);
                }

                virtualMachinePowerDriver.Dispose();
            }
            
            _virtualMachinePowerDrivers.Clear();
        }

        /// <summary>
        /// Throw an exception if any of the dependencies failed to power on.
        /// </summary>
        public void ThrowOnFailure()
        {
            _powerResults.ThrowOnFailure();
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (VirtualMachinePowerDriver virtualMachinePowerDriver in _virtualMachinePowerDrivers)
            {
                virtualMachinePowerDriver.Dispose();
            }

            _virtualMachinePowerDrivers.Clear();
        }

        #endregion
    }
}
