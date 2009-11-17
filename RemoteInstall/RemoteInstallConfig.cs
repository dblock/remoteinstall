using System;
using System.Configuration;
using System.IO;

namespace RemoteInstall
{
    /// <summary>
    /// General remote installer configuration section.
    /// </summary>
    public class RemoteInstallConfig : ConfigurationSection
    {
        public RemoteInstallConfig()
        {

        }

        /// <summary>
        /// Section to define various timeouts for RemoteInstall
        /// </summary>
        [ConfigurationProperty("timeouts", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(VirtualMachineTimeoutConfig), AddItemName = "timeouts")]
        public VirtualMachineTimeoutConfig Timeouts
        {
            get
            {
                return (VirtualMachineTimeoutConfig)this["timeouts"];
            }
            set
            {
                this["timeouts"] = value;
            }
        }

        /// <summary>
        /// Section to include installers (msi files) you want tested
        /// </summary>
        [ConfigurationProperty("installers", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(InstallersConfig), AddItemName = "installer")]
        public InstallersConfig Installers
        {
            get
            {
                return (InstallersConfig)this["installers"];
            }
            set
            {
                this["installers"] = value;
            }
        }

        /// <summary>
        /// Section to include VM's (vmx files) you want the installers to be tested on
        /// </summary>
        [ConfigurationProperty("virtualmachines", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(VirtualMachinesConfig), AddItemName = "virtualmachine")]
        public VirtualMachinesConfig VirtualMachines
        {
            get
            {
                return (VirtualMachinesConfig)this["virtualmachines"];
            }
            set
            {
                this["virtualmachines"] = value;
            }
        }

        [ConfigurationProperty("copyfiles", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(CopyFilesConfig), AddItemName = "copyfile")]
        public CopyFilesConfig CopyFiles
        {
            get
            {
                return (CopyFilesConfig)this["copyfiles"];
            }
            set
            {
                this["copyfiles"] = value;
            }
        }

        [ConfigurationProperty("tasks", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(TasksConfig), AddItemName = "task")]
        public TasksConfig Tasks
        {
            get
            {
                return (TasksConfig)this["tasks"];
            }
            set
            {
                this["tasks"] = value;
            }
        }
    }
}