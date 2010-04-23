using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace RemoteInstall
{
    public class ExeInstallerConfig : InstallerConfig
    {
        public ExeInstallerConfig(string destinationPath, CopyMethod copyMethod)
            : base(destinationPath, copyMethod)
        {

        }

        /// <summary>
        /// Log file, if generated.
        /// </summary>
        [ConfigurationProperty("logFile", IsRequired = false)]
        public virtual string LogFile
        {
            get
            {
                return Rewrite((string)this["logFile"]);
            }
            set
            {
                this["logFile"] = value;
            }
        }

        /// <summary>
        /// Install arguments.
        /// </summary>
        [ConfigurationProperty("installArgs", IsRequired = false)]
        public virtual string InstallArgs
        {
            get
            {
                return Rewrite((string) this["installArgs"]);
            }
            set
            {
                this["installArgs"] = value;
            }
        }

        /// <summary>
        /// Uninstall arguments.
        /// </summary>
        [ConfigurationProperty("uninstallArgs", IsRequired = false)]
        public virtual string UnInstallArgs
        {
            get
            {
                return Rewrite((string) this["uninstallArgs"]);
            }
            set
            {
                this["uninstallArgs"] = value;
            }
        }

        public override VirtualMachineDeployment CreateDeployment(VMWareMappedVirtualMachine vm)
        {
            return new VirtualMachineExeDeployment(vm, this);
        }

        public override InstallerType Type
        {
            get
            {
                return InstallerType.exe;
            }
            set
            {
                if (value != InstallerType.exe)
                {
                    throw new ArgumentException(string.Format(
                        "Invalid installer type: {0}", value));
                }
            }
        }

        [ConfigurationProperty("exitcodes", IsDefaultCollection = false, IsRequired = false)]
        [ConfigurationCollection(typeof(ExitCodesConfig), AddItemName = "exitcode")]
        public ExitCodesConfig ExitCodes
        {
            get
            {
                return (ExitCodesConfig) this["exitcodes"];
            }
            set
            {
                this["exitcodes"] = value;
            }
        }
    }
}
