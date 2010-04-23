using System;
using System.Collections.Generic;
using System.Configuration;

namespace RemoteInstall
{
    /// <summary>
    /// Configuration of a snapshot
    /// </summary>
    public class SnapshotConfig : GlobalTasksConfigurationElement
    {
        /// <summary>
        /// Used to identify the current snapshot of a VM
        /// </summary>
        public static string CurrentSnapshotName = "*";

        public SnapshotConfig()
            : this(string.Empty, string.Empty, false)
        {

        }

        public SnapshotConfig(string username, string password, bool poweroff)
        {
            Username = username;
            Password = password;
            PowerOff = poweroff;
        }

        /// <summary>
        /// Returns true if the snapshot identifies the current snapshot
        /// </summary>
        public bool IsCurrentSnapshot
        {
            get
            {
                return ((string)this["name"] == SnapshotConfig.CurrentSnapshotName);
            }
        }

        /// <summary>
        /// Name of the snapshot
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                string result = (string)this["name"];
                return result == SnapshotConfig.CurrentSnapshotName
                    ? "Current Snapshot"
                    : result;
            }
            set
            {
                this["name"] = value;
            }
        }

        /// <summary>
        /// Description of the snapshot
        /// </summary>
        [ConfigurationProperty("description", IsRequired = false)]
        public string Description
        {
            get
            {
                return (string)this["description"];
            }
            set
            {
                this["description"] = value;
            }
        }

        /// <summary>
        /// Description of the snapshot
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

        /// <summary>
        /// Username used to login to the snapshot
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
        /// Password used to login to the snapshot
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
        /// Power off this snapshot.
        /// </summary>
        [ConfigurationProperty("powerOff", IsRequired = false)]
        public bool PowerOff
        {
            get
            {
                return (bool)this["powerOff"];
            }
            set
            {
                this["powerOff"] = value;
            }
        }

        /// <summary>
        /// Dependent virtual machine(s) with snapshots you want to be powered for this snapshot.
        /// </summary>
        [ConfigurationProperty("dependencies", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(VirtualMachinesConfig), AddItemName = "virtualmachine")]
        public VirtualMachinesConfig VirtualMachines
        {
            get
            {
                return (VirtualMachinesConfig)this["dependencies"];
            }
            set
            {
                this["dependencies"] = value;
            }
        }

        /// <summary>
        /// Returns true if there's an overlap between two configurations.
        /// </summary>
        /// <param name="config">Virtual machine configuration.</param>
        /// <returns>True if there's an overlap.</returns>
        public bool Overlaps(VirtualMachineConfig config)
        {
            foreach (VirtualMachineConfig virtualMachine in VirtualMachines)
            {
                // the virtual machine overlaps
                if (virtualMachine.Overlaps(config))
                    return true;
            }

            return false;
        }
    }
}
