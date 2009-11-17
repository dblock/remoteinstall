using System;
using System.Collections.Generic;
using System.Configuration;

namespace RemoteInstall
{
    public enum InstallersSequence
    {
        clean, // alternate install/uninstall restoring the snapshot every time
        fifo, // first in, first out (install 1, install 2, uninstall 1, uninstall 2
        lifo, // last in, first out (install 1, install 2, uninstall 2, uninstall 1
        alternate, // alternate install/uninstall
        install, // install only
        uninstall, // uninstall only
    }

    /// <summary>
    /// Section to include installers (msi files) you want tested
    /// </summary>
    public class InstallersConfig : GlobalTasksConfigurationElementCollection
    {
        public InstallersConfig()
        {

        }

        /// <summary>
        /// Default copy method for all installers.
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
        /// Path where the installers are copied to on the VM's
        /// </summary>
        [ConfigurationProperty("destpath", DefaultValue = @"C:\", IsRequired = false)]
        public string DestinationPath
        {
            get
            {
                return (string)this["destpath"];
            }
            set
            {
                this["destpath"] = value;
            }
        }

        [ConfigurationProperty("sequence", DefaultValue = InstallersSequence.clean)]
        public InstallersSequence Sequence
        {
            get
            {
                return (InstallersSequence)this["sequence"];
            }
            set
            {
                this["sequence"] = value;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new InstallerConfigProxy(DestinationPath, CopyMethod);
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((InstallerConfigProxy)element).Instance;
        }

        public InstallerConfig this[int index]
        {
            get
            {
                return ((InstallerConfigProxy)BaseGet(index)).Instance;
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

        public bool EnsureOne()
        {
            if (Count == 0)
            {
                BaseAdd(new NoopInstallerConfigProxy(DestinationPath, CopyMethod));
                return true;
            }

            return false;
        }
    }
}
