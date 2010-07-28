using System;
using System.Collections.Generic;
using System.Configuration;

namespace RemoteInstall
{
    /// <summary>
    /// Types of supported virtual machines
    /// </summary>
    public enum VirtualMachineType
    {
        Workstation,
        ESX
    };

    /// <summary>
    /// Virtual Machine configuration
    /// </summary>
    public class VirtualMachineConfig : GlobalTasksConfigurationElement
    {
        public VirtualMachineConfig()
            : this(CopyMethod.undefined)
        {

        }

        public VirtualMachineConfig(CopyMethod copyMethod)
        {
            CopyMethod = copyMethod;
        }

        /// <summary>
        /// Virtual machine type
        /// </summary>
        [ConfigurationProperty("type", IsRequired = true, DefaultValue = VirtualMachineType.Workstation)]
        public VirtualMachineType Type
        {
            get
            {
                return (VirtualMachineType)this["type"];
            }
            set
            {
                this["type"] = value;
            }
        }

        /// <summary>
        /// VirtualMachine vmx storage file
        /// </summary>
        [ConfigurationProperty("file", IsRequired = true)]
        public string File
        {
            get
            {
                return (string)this["file"];
            }
            set
            {
                this["file"] = value;
            }
        }

        /// <summary>
        /// Host which the VM lives on
        /// </summary>
        [ConfigurationProperty("host", IsRequired = false)]
        public string Host
        {
            get
            {
                return (string)this["host"];
            }
            set
            {
                this["host"] = value;
            }
        }

        /// <summary>
        /// Username used to login to the virtual machine host.
        /// </summary>
        [ConfigurationProperty("username", IsRequired = false)]
        public string Username
        {
            get
            {
                return (string)this["username"];
            }
            set
            {
                this["username"] = value;
            }
        }

        /// <summary>
        /// Password used to login to the virtual machine host.
        /// </summary>
        [ConfigurationProperty("password", IsRequired = false)]
        public string Password
        {
            get
            {
                return (string)this["password"];
            }
            set
            {
                this["password"] = value;
            }
        }

        /// <summary>
        /// User friendly string to used to title the VM
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        /// <summary>
        /// A delay after power-on to enable services to startup.
        /// </summary>
        [ConfigurationProperty("powerDelay")]
        public int PowerDelay
        {
            get
            {
                return (int)this["powerDelay"];
            }
            set
            {
                this["powerDelay"] = value;
            }
        }

        /// <summary>
        /// Copy method to/from Virtual Machine.
        /// </summary>
        [ConfigurationProperty("copymethod", DefaultValue = CopyMethod.undefined)]
        public CopyMethod CopyMethod
        {
            get
            {
                return (CopyMethod)this["copymethod"];
            }
            set
            {
                this["copymethod"] = value;
            }
        }

        /// <summary>
        /// Collection of snapshots of this VM that will be used to run installers on
        /// </summary>
        [ConfigurationProperty("snapshots", IsDefaultCollection = false, IsRequired = false)]
        [ConfigurationCollection(typeof(SnapshotsConfig), AddItemName = "snapshot")]
        public SnapshotsConfig Snapshots
        {
            get
            {
                return (SnapshotsConfig)this["snapshots"];
            }
            set
            {
                this["snapshots"] = value;
            }
        }

        /// <summary>
        /// Returns true if there's an overlap between two configurations.
        /// </summary>
        /// <param name="config">Virtual machine configuration.</param>
        /// <returns>True if there's an overlap.</returns>
        public bool Overlaps(VirtualMachineConfig config)
        {
            // same virtual machine
            if (Host == config.Host && File == config.File)
                return true;

            // each snapshot in this vm overlapping the configuration
            foreach (SnapshotConfig snapshot in Snapshots)
            {
                if (snapshot.Overlaps(config))
                    return true;
            }

            // each snapshot in the configuration overlapping this
            foreach (SnapshotConfig snapshot in config.Snapshots)
            {
                if (snapshot.Overlaps(this))
                    return true;
            }

            return false;
        }
    }
}
