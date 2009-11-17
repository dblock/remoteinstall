using System;
using System.Collections.Generic;
using System.Configuration;

namespace RemoteInstall
{
    /// <summary>
    /// Collection of snapshots of this VM that will be used to run installers on
    /// </summary>
    public class SnapshotsConfig : GlobalTasksConfigurationElementCollection
    {
        public SnapshotsConfig()
        {
            PowerOff = true;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SnapshotConfig(Username, Password, PowerOff);
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            SnapshotConfig snapshotConfig = (SnapshotConfig) element;
            return string.Format("Snapshot: {0} ({1})", snapshotConfig.Name, snapshotConfig.Description);
        }

        /// <summary>
        /// Default username for all snapshots.
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
        /// Default password for all snapshots.
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
        /// Power off snapshots.
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

        public void Add(SnapshotConfig value)
        {
            BaseAdd(value);
        }

        public SnapshotConfig this[int index]
        {
            get
            {
                return (SnapshotConfig)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }
    }
}
