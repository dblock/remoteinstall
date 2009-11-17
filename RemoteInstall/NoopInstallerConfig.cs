using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteInstall
{
    public class NoopInstallerConfig : InstallerConfig
    {
        public NoopInstallerConfig(string destinationPath, CopyMethod copyMethod)
            : base(destinationPath, copyMethod)
        {
            File = string.Empty;
        }

        public override VirtualMachineDeployment CreateDeployment(VMWareMappedVirtualMachine vm)
        {
            return new VirtualMachineNoopDeployment(vm, this);
        }

        public override InstallerType Type
        {
            get
            {
                return InstallerType.noop;
            }
            set
            {
                if (value != InstallerType.noop)
                {
                    throw new ArgumentException(string.Format(
                        "Invalid installer type: {0}", value));
                }
            }
        }
    }
}
