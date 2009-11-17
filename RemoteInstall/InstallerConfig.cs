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
        msi
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
            ResolveFilePath();
            ResolveSvnRevision();
        }

        /// <summary>
        /// Resolve file path
        /// </summary>
        private void ResolveFilePath()
        {
            _file = (string)this["file"];
            int pos = _file.IndexOf('*');
            while (pos != -1)
            {
                int front = _file.Substring(0, pos).LastIndexOf('\\') + 1;
                int back = _file.IndexOf('\\', pos);
                string lastDir = null;
                if (back > 0)
                {
                    lastDir = FindLatest(_file.Substring(0, front), _file.Substring(front, back - front), false);
                    _file = lastDir + _file.Substring(back);
                }
                else
                {
                    lastDir = FindLatest(_file.Substring(0, front), _file.Substring(front), true);
                    _file = lastDir;
                }
                pos = _file.IndexOf('*');
            }
        }

        /// <summary>
        /// Resolve current svn revision
        /// </summary>
        private void ResolveSvnRevision()
        {
            string file = (string)this["file"];
            int pos = file.IndexOf('*');
            int front = 0;
            int back = 0;

            if (pos < 0)
            {
                int lastSeparator = file.LastIndexOf('\\');
                if (lastSeparator < 0)
                {
                    _svnrevision = "unknown";
                    return;
                }

                int beforeLastSeparator = file.Substring(0, lastSeparator - 1).LastIndexOf('\\');
                front = beforeLastSeparator + 1;
                back = lastSeparator;
            }
            else
            {
                front = file.Substring(0, pos).LastIndexOf('\\') + 1;
                back = file.IndexOf('\\', pos);
            }

            string[] subdirs = null;
            string path = file.Substring(0, front);
            if (string.IsNullOrEmpty(path))
            {
                _svnrevision = "unknown";
                return;
            }

            if (back > 0)
            {
                subdirs = Directory.GetDirectories(path, file.Substring(front, back - front));
            }
            else
            {
                subdirs = Directory.GetFiles(path, file.Substring(front));
            }

            if (subdirs.Length == 0)
            {
                throw new DirectoryNotFoundException("Could not find directory that met the search pattern criteria in directory: " + file + ".");
            }

            _svnrevision = subdirs[subdirs.Length - 1].Substring(
                subdirs[subdirs.Length - 1].LastIndexOf('\\') + 1);
        }

        /// <summary>
        /// Installer file
        /// </summary>
        [ConfigurationProperty("file", IsRequired = true)]
        public string File
        {
            get
            {
                return _file;
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
        /// Finds the latest version of an installer
        /// </summary>
        private static string FindLatest(string path, string dirpattern, Boolean searchfiles)
        {
            // todo: find all files or directories and sort by last modified date

            string[] subdirs = null;

            try
            {
                if (searchfiles)
                {
                    subdirs = Directory.GetFiles(path, dirpattern);
                }
                else
                {
                    subdirs = Directory.GetDirectories(path, dirpattern);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not find a file or directory that met the search pattern {0} in {1}: {2}",
                    dirpattern, path, ex.Message), ex);
            }

            if (subdirs.Length == 0)
            {
                throw new DirectoryNotFoundException(string.Format("Could not find directory or file that met the search pattern {0} in {1}",
                    dirpattern, path));
            }

            return subdirs[subdirs.Length - 1];
        }

        /// <summary>
        /// Additional installer args
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
        /// Installer label
        /// </summary>
        [ConfigurationProperty("name")]
        public string Name
        {
            get
            {
                string result = (string)this["name"];
                return string.IsNullOrEmpty(result) ? Path.GetFileName(File) : result;
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
                return _destinationPath + File.Substring(File.LastIndexOf('\\') + 1);
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
