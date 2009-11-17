using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.IO;

namespace RemoteInstall
{
    public enum TaskType
    {
        command,
        snapshot,
        virtualmachine
    };

    public class TaskConfigProxy : ConfigurationElement
    {
        private SequenceWhen _when = SequenceWhen.afterall;
        private TaskConfigInstance _config = null;

        public TaskConfigProxy(SequenceWhen when)
        {
            _when = when;
        }

        public TaskConfigProxy(TaskConfigInstance instance)
        {
            _when = instance.ExecuteCommandWhen;
            _config = instance;
        }

        public SequenceWhen SequenceWhen
        {
            get
            {
                return _when;
            }
            set
            {
                _when = value;
            }
        }

        public TaskConfigInstance Instance
        {
            get
            {
                return _config;
            }
        }

        protected override void DeserializeElement(System.Xml.XmlReader reader, bool serializeCollectionKey)
        {
            string taskType = reader.GetAttribute("type");
            if (string.IsNullOrEmpty(taskType)) taskType = TaskType.command.ToString();
            TaskType type = (TaskType)Enum.Parse(typeof(TaskType), taskType);
            switch (type)
            {
                case TaskType.snapshot:
                    _config = new SnapshotTaskConfig(_when);
                    break;
                case TaskType.virtualmachine:
                    _config = new VirtualMachineTaskConfig(_when);
                    break;
                case TaskType.command:
                default:
                    _config = new CommandTaskConfig(_when);
                    break;
            }

            _config.ProxyDeserializeElement(reader, serializeCollectionKey);
        }
    }

    /// <summary>
    /// Configuration for something to execute
    /// </summary>
    public abstract class TaskConfigInstance : ConfigurationElement
    {
        public TaskConfigInstance(SequenceWhen when)
        {
            ExecuteCommandWhen = when;
        }

        /// <summary>
        /// Execute label
        /// </summary>
        [ConfigurationProperty("name")]
        public virtual string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        public void ProxyDeserializeElement(System.Xml.XmlReader reader, bool serializeCollectionKey)
        {
            base.DeserializeElement(reader, serializeCollectionKey);
        }

        /// <summary>
        /// Choose when to collect files
        /// </summary>
        [ConfigurationProperty("when", DefaultValue = SequenceWhen.afterall)]
        public SequenceWhen ExecuteCommandWhen
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

        [ConfigurationProperty("type", IsRequired = false)]
        public abstract TaskType Type { get; set; }
    }
}
