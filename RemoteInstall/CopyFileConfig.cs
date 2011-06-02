using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace RemoteInstall
{
    /// <summary>
    /// Configuration element that defines a single file to copy from/to the target
    /// guest operating system.
    /// </summary>
    public class CopyFileConfig : ConfigurationElement
    {
        public CopyFileConfig()
        {

        }

        /// <summary>
        /// A configuration to copy a file.
        /// </summary>
        /// <param name="destinationPath">destination path</param>
        /// <param name="destination">destination host</param>
        /// <param name="when">when to copy</param>
        public CopyFileConfig(
            string destinationPath, 
            CopyDestination destination, 
            SequenceWhen when)
        {
            DestinationPath = destinationPath;
            CopyWhen = when;
        }

        /// <summary>
        /// File to copy
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
                ResolvePath();
            }
        }

        /// <summary>
        /// Check if the file exists before copying; when false and file doesn't exist, an error is reported
        /// </summary>
        [ConfigurationProperty("checkIfExists", DefaultValue = false)]
        public bool CheckIfExists
        {
            get
            {
                return (bool)this["checkIfExists"];
            }
            set
            {
                this["checkIfExists"] = value;
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
        /// File label/name in the results
        /// </summary>
        [ConfigurationProperty("name", IsRequired = false)]
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
        /// Destination subfolder of the resulting filename
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
                ResolvePath();
            }
        }

        /// <summary>
        /// Optional XSL transform to apply to the file.
        /// </summary>
        [ConfigurationProperty("xslt")]
        public string XslTransform
        {
            get
            {
                return (string)this["xslt"];
            }
            set
            {
                this["xslt"] = value;
            }
        }

        /// <summary>
        /// Include the file data as is in results.
        /// </summary>
        [ConfigurationProperty("includeDataInResults", DefaultValue = false)]
        public bool IncludeDataInResults
        {
            get
            {
                return (bool)this["includeDataInResults"];
            }
            set
            {
                this["includeDataInResults"] = value;
            }
        }

        /// <summary>
        /// Include the file in the results set.
        /// </summary>
        [ConfigurationProperty("includeInResults", DefaultValue = true)]
        public bool IncludeInResults
        {
            get
            {
                return (bool)this["includeInResults"];
            }
            set
            {
                this["includeInResults"] = value;
            }
        }

        /// <summary>
        /// Pattern to exclude files.
        /// </summary>
        [ConfigurationProperty("exclude", IsRequired = false)]
        public string Exclude
        {
            get
            {
                return (string)this["exclude"];
            }
            set
            {
                this["exclude"] = value;
            }
        }

        /// <summary>
        /// Resolve paths, file and svn revision
        /// </summary>
        private void ResolvePath()
        {
            if (Destination == CopyDestination.toVirtualMachine)
            {
                this["file"] = FileTools.ResolveFilePath((string)this["file"]);
            }
        }

        protected override void PostDeserialize()
        {
            ResolvePath();
            base.PostDeserialize();
        }
    }
}
