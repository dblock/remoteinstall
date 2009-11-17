using System;
using System.Collections.Generic;
using System.Configuration;

namespace RemoteInstall
{
    /// <summary>
    /// A collection of commands to execute
    /// </summary>
    public class TasksConfig : ConfigurationElementCollection
    {
        public TasksConfig()
        {

        }

        /// <summary>
        /// Choose when to execute
        /// </summary>
        [ConfigurationProperty("when", DefaultValue = SequenceWhen.afterall)]
        public SequenceWhen SequenceWhen
        {
            get
            {
                return (SequenceWhen) this["when"];
            }
            set
            {
                this["when"] = value;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TaskConfigProxy(SequenceWhen);
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            TaskConfigProxy proxy = (TaskConfigProxy)element;
            if (proxy.Instance != null) return proxy.Instance;
            return proxy.GetHashCode();
        }

        public void Add(TaskConfigInstance value)
        {
            BaseAdd(new TaskConfigProxy(value));
        }

        public TaskConfigProxy this[int index]
        {
            get
            {
                return (TaskConfigProxy)BaseGet(index);
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
