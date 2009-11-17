using System;
using System.Collections.Generic;
using System.Configuration;

namespace RemoteInstall
{
    /// <summary>
    /// Section to include VM's (vmx files) you want the installers to be tested on
    /// </summary>
    public class VirtualMachinesConfig : GlobalTasksConfigurationElementCollection
    {
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

        public VirtualMachinesConfig()
        {

        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new VirtualMachineConfig(CopyMethod);
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((VirtualMachineConfig)element).File;
        }

        public void Add(VirtualMachineConfig value)
        {
            BaseAdd(value);
        }

        public VirtualMachineConfig this[int index]
        {
            get
            {
                return (VirtualMachineConfig)BaseGet(index);
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
