using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteInstall
{
    public class VirtualMachinePowerResults : List<VirtualMachinePowerResult>
    {
        public VirtualMachinePowerResults()
        {

        }

        /// <summary>
        /// Throw an exception if any of the dependencies failed to power on.
        /// </summary>
        public void ThrowOnFailure()
        {
            StringBuilder powerFailures = new StringBuilder();
            foreach (VirtualMachinePowerResult powerResult in this)
            {
                if (!powerResult.Success)
                {
                    powerFailures.AppendLine(string.Format("Virtual machine '{0}', snapshot '{1}' failed to power on: {2}", 
                        powerResult.Name, powerResult.Snapshot, powerResult.LastError));
                }
            }

            if (powerFailures.Length > 0)
            {
                throw new Exception(powerFailures.ToString());
            }
        }
    }
}
