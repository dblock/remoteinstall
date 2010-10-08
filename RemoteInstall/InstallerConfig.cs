using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.IO;

namespace RemoteInstall
{
    /// <summary>
    /// Copy method to work around VMWare copy performance issues.
    /// </summary>
    public enum CopyMethod
    {
        undefined, // undefined
        vmware, // use VMWare VIX API
        network // attempt to map network drive
    };

    public enum InstallerType
    {
        noop,
        dni,
        msi,
        exe
    }

    public class InstallerConfigProxy : GlobalTasksConfigurationElement
    {
        private CopyMethod _copyMethod = CopyMethod.undefined;
        private string _destinationPath = null;
        protected InstallerConfig _config = null;

        public InstallerConfigProxy(string destinationPath, CopyMethod copyMethod)
        {
            _destinationPath = destinationPath;
            _copyMethod = copyMethod;
        }

        public InstallerConfig Instance
        {
            get
            {
                return _config;
            }
        }

        protected override void DeserializeElement(System.Xml.XmlReader reader, bool serializeCollectionKey)
        {
            string installerType = reader.GetAttribute("type");
            if (string.IsNullOrEmpty(installerType)) installerType = InstallerType.msi.ToString();
            InstallerType type = (InstallerType)Enum.Parse(typeof(InstallerType), installerType);
            switch (type)
            {
                case InstallerType.noop:
                    _config = new NoopInstallerConfig(_destinationPath, _copyMethod);
                    break;
                case InstallerType.dni:
                    _config = new DniInstallerConfig(_destinationPath, _copyMethod);
                    break;
                case InstallerType.exe:
                    _config = new ExeInstallerConfig(_destinationPath, _copyMethod);
                    break;
                case InstallerType.msi:
                default:
                    _config = new MsiInstallerConfig(_destinationPath, _copyMethod);
                    break;
            }

            _config.ProxyDeserializeElement(reader, serializeCollectionKey);
        }
    }

    public class NoopInstallerConfigProxy : InstallerConfigProxy
    {
        public NoopInstallerConfigProxy(string destinationPath, CopyMethod copyMethod)
            : base(destinationPath, copyMethod)
        {
            _config = new NoopInstallerConfig(destinationPath, copyMethod);
        }
    }

    /// <summary>
    /// Configuration for an MSI installer
    /// </summary>
    public abstract class InstallerConfig : GlobalTasksConfigurationElement
    {
        private string _destinationPath = string.Empty;
        private string _file = string.Empty;
        private string _svnrevision = string.Empty;

        public InstallerConfig(string destinationPath, CopyMethod copyMethod)
        {
            _destinationPath = destinationPath;
            CopyMethod = copyMethod;
        }

        protected override void PostDeserialize()
        {
            ResolvePaths();
            base.PostDeserialize();
        }

        /// <summary>
        /// Resolve paths, file and svn revision
        /// </summary>
        private void ResolvePaths()
        {
            _file = FileTools.ResolveFilePath((string)this["file"]);
            _svnrevision = FileTools.ResolveSvnRevision((string)this["file"]);
        }

        /// <summary>
        /// Installer file
        /// </summary>
        [ConfigurationProperty("file", IsRequired = true)]
        public string File
        {
            get
            {
                return Rewrite(_file);
            }
            set
            {
                this["file"] = value;
                ResolvePaths();
            }
        }

        /// <summary>
        /// This installer supports install.
        /// </summary>
        [ConfigurationProperty("install", IsRequired = false, DefaultValue = true)]
        public virtual bool Install
        {
            get
            {
                return (bool) this["install"];
            }
            set
            {
                this["install"] = value;
            }
        }

        /// <summary>
        /// This installer supports install.
        /// </summary>
        [ConfigurationProperty("uninstall", IsRequired = false, DefaultValue = true)]
        public virtual bool UnInstall
        {
            get
            {
                return (bool) this["uninstall"];
            }
            set
            {
                this["uninstall"] = value;
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
        /// Reboot if required.
        /// </summary>
        [ConfigurationProperty("rebootIfRequired", IsRequired = false, DefaultValue = true)]
        public virtual bool RebootIfRequired
        {
            get
            {
                return (bool)this["rebootIfRequired"];
            }
            set
            {
                this["rebootIfRequired"] = value;
            }
        }

        /// <summary>
        /// Reboot if required.
        /// </summary>
        [ConfigurationProperty("rebootRequired", IsRequired = false, DefaultValue = false)]
        public virtual bool RebootRequired
        {
            get
            {
                return (bool)this["rebootRequired"];
            }
            set
            {
                this["rebootRequired"] = value;
            }
        }

        /// <summary>
        /// Returns the SVN revision number of the installer
        /// </summary>
        public string SvnRevision
        {
            get
            {
                return _svnrevision;
            }
        }

        /// <summary>
        /// Installer label
        /// </summary>
        [ConfigurationProperty("name")]
        public string Name
        {
            get
            {
                string result = (string)this["name"];
                result = string.IsNullOrEmpty(result) ? Path.GetFileName(File) : result;
                return Rewrite(result);
            }
            set
            {
                this["name"] = value;
            }
        }

        /// <summary>
        /// Full destination path including the name of the installer
        /// </summary>
        public string DestinationPath
        {
            get
            {                
                return Rewrite(_destinationPath + File.Substring(File.LastIndexOf('\\') + 1));
            }
        }

        public abstract VirtualMachineDeployment CreateDeployment(VMWareMappedVirtualMachine vm);

        public void ProxyDeserializeElement(System.Xml.XmlReader reader, bool serializeCollectionKey)
        {
            base.DeserializeElement(reader, serializeCollectionKey);

            if (! Install && ! UnInstall)
            {
                throw new Exception(string.Format("Installer configuration '{0}' must support either install, uninstall or both.",
                    Name));
            }
        }

        [ConfigurationProperty("type", IsRequired = false)]
        public abstract InstallerType Type { get; set; }
    }
}
