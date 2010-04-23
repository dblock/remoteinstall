using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Text.RegularExpressions;

namespace RemoteInstall
{
    /// <summary>
    /// A global tasks configuration section with collections that belong to most nodes.
    /// </summary>
    public abstract class GlobalTasksConfigurationElement : ConfigurationElement
    {
        public static string VarRegex = @"\@\{(?<var>[\w_]*)[\.\:](?<name>[\w_\.\-\(\)]*)\}";

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

        public EventHandler<ReflectionResolverEventArgs> OnRewrite;

        /// <summary>
        /// Resolve a value via reflection.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Rewrite(string value)
        {
            return Regex.Replace(value, VarRegex, new MatchEvaluator(Rewrite),
                RegexOptions.IgnoreCase);
        }

        private string Rewrite(Match m)
        {
            string var = m.Groups["var"].Value;
            string name = m.Groups["name"].Value;

            ReflectionResolverEventArgs args = new ReflectionResolverEventArgs(
                var, name);

            if (OnRewrite != null)
            {
                OnRewrite(this, args);                
            }

            if (! args.Rewritten)
            {
                throw new Exception(string.Format("Unsupported variable or missing handler: @({0}.{1})",
                    var, name));
            }

            return args.Result;
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
