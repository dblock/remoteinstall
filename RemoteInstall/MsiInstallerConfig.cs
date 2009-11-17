using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteInstall
{
    public class MsiInstallerConfig : InstallerConfig
    {
        public MsiInstallerConfig(string destinationPath, CopyMethod copyMethod)
            : base(destinationPath, copyMethod)
        {

        }

        public override VirtualMachineDeployment CreateDeployment(VMWareMappedVirtualMachine vm)
        {
            return new VirtualMachineMsiDeployment(vm, this);
        }

        public override InstallerType Type
        {
            get
            {
                return InstallerType.msi;
            }
            set
            {
                if (value != InstallerType.msi)
                {
                    throw new ArgumentException(string.Format(
                        "Invalid installer type: {0}", value));
                }
            }
        }
    }
}
