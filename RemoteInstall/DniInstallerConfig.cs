using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace RemoteInstall
{
    public class ComponentsConfig : ConfigurationElementCollection
    {
        public ComponentsConfig()
        {

        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ComponentConfig();
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((ComponentConfig)element).Name;
        }

        public void Add(ComponentConfig value)
        {
            BaseAdd(value);
        }

        public ComponentConfig this[int index]
        {
            get
            {
                return (ComponentConfig)BaseGet(index);
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

    public class ComponentConfig : ConfigurationElement
    {
        public ComponentConfig()
        {

        }

        [ConfigurationProperty("name", IsRequired = true)]
        public String Name
        {
            get
            {
                return (String)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("description", IsRequired = true)]
        public String Description
        {
            get
            {
                return (String)this["description"];
            }
            set
            {
                this["description"] = value;
            }
        }

        [ConfigurationProperty("args", IsRequired = false)]
        public String Args
        {
            get
            {
                return (String)this["args"];
            }
            set
            {
                this["args"] = value;
            }
        }
    }

    public class DniInstallerConfig : InstallerConfig
    {
        public DniInstallerConfig(string destinationPath, CopyMethod copyMethod)
            : base(destinationPath, copyMethod)
        {

        }

        public override VirtualMachineDeployment CreateDeployment(VMWareMappedVirtualMachine vm)
        {
            return new VirtualMachineDniDeployment(vm, this);
        }

        [ConfigurationProperty("components", IsDefaultCollection = false, IsRequired = false)]
        [ConfigurationCollection(typeof(ComponentsConfig), AddItemName = "component")]
        public ComponentsConfig components
        {
            get
            {
                return (ComponentsConfig)this["components"];
            }
            set
            {
                this["components"] = value;
            }
        }

        /// <summary>
        /// Additional installer args
        /// </summary>
        [ConfigurationProperty("installargs", IsRequired = false)]
        public string InstallArgs
        {
            get
            {
                return Rewrite((string)this["installargs"]);
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
                return InstallerType.dni;
            }
            set
            {
                if (value != InstallerType.dni)
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
                return (ExitCodesConfig)this["exitcodes"];
            }
            set
            {
                this["exitcodes"] = value;
            }
        }
    }
}
