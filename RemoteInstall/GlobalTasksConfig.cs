using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace RemoteInstall
{
    /// <summary>
    /// A global tasks configuration section with collections that belong to most nodes.
    /// </summary>
    public abstract class GlobalTasksConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("copyfiles", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(CopyFilesConfig), AddItemName = "copyfile")]
        public CopyFilesConfig CopyFiles
        {
            get { return (CopyFilesConfig)this["copyfiles"]; }
            set { this["copyfiles"] = value; }
        }

        [ConfigurationProperty("tasks", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(TasksConfig), AddItemName = "task")]
        public TasksConfig Tasks
        {
            get { return (TasksConfig)this["tasks"]; }
            set { this["tasks"] = value; }
        }
    }

    public abstract class GlobalTasksConfigurationElementCollection : ConfigurationElementCollection
    {
        [ConfigurationProperty("copyfiles", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(CopyFilesConfig), AddItemName = "copyfile")]
        public CopyFilesConfig CopyFiles
        {
            get { return (CopyFilesConfig)this["copyfiles"]; }
            set { this["copyfiles"] = value; }
        }

        [ConfigurationProperty("tasks", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(TasksConfig), AddItemName = "task")]
        public TasksConfig Tasks
        {
            get { return (TasksConfig)this["tasks"]; }
            set { this["tasks"] = value; }
        }
    }
}
