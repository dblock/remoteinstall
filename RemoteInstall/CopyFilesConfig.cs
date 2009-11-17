using System;
using System.Collections.Generic;
using System.Configuration;

namespace RemoteInstall
{
    public enum CopyDestination
    {
        toVirtualMachine,
        toTestClient
    }

    /// <summary>
    /// A collection of files to copy from/to the target guest host.
    /// </summary>
    public class CopyFilesConfig : ConfigurationElementCollection
    {
        public CopyFilesConfig()
        {

        }

        /// <summary>
        /// Default destination path for files to copy.
        /// </summary>
        [ConfigurationProperty("destpath", IsRequired = false)]
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

        /// <summary>
        /// Choose when to copy files
        /// </summary>
        [ConfigurationProperty("when", DefaultValue = SequenceWhen.afterall)]
        public SequenceWhen CopyWhen
        {
            get
            {
                return (SequenceWhen)this["when"];
            }
            set
            {
                this["when"] = value;
            }
        }

        /// <summary>
        /// Default direction for files to copy.
        /// </summary>
        [ConfigurationProperty("destination", DefaultValue = CopyDestination.toTestClient)]
        public CopyDestination Destination
        {
            get
            {
                return (CopyDestination)this["destination"];
            }
            set
            {
                this["destination"] = value;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CopyFileConfig(DestinationPath, Destination, CopyWhen);
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((CopyFileConfig)element).Name;
        }

        public void Add(CopyFileConfig value)
        {
            BaseAdd(value);
        }

        public CopyFileConfig this[int index]
        {
            get
            {
                return (CopyFileConfig)BaseGet(index);
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
