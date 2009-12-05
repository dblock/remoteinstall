using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

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

        /// <summary>
        /// Additional installer args
        /// </summary>
        [ConfigurationProperty("installargs", IsRequired = false)]
        public string InstallArgs
        {
            get
            {
                return (string)this["installargs"];
            }
            set
            {
                this["installargs"] = value;
            }
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
