using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace RemoteInstall
{
    public enum SnapshotCommand
    {
        none,
        create,
        revert,
        remove,
        removeifexists
    }

    /// <summary>
    /// Configuration element that defines a single command to execute on the target test host.
    /// </summary>
    public class SnapshotTaskConfig : TaskConfigInstance
    {
        public SnapshotTaskConfig()
            : this(SequenceWhen.afterall)
        {
        }

        public SnapshotTaskConfig(SequenceWhen when)
            : base(when)
        {
            Command = SnapshotCommand.none;
        }

        public override TaskType Type
        {
            get
            {
                return TaskType.snapshot;
            }
            set
            {
                if (value != TaskType.snapshot)
                {
                    throw new ArgumentException(string.Format(
                        "Invalid command type: {0}", value));
                }
            }
        }

        /// <summary>
        /// The snapshot command (take, revert, etc.)
        /// </summary>
        [ConfigurationProperty("command", IsRequired = true)]
        public SnapshotCommand Command
        {
            get
            {
                return (SnapshotCommand)this["command"];
            }
            set
            {
                this["command"] = value;
            }
        }

        /// <summary>
        /// Snapshot description.
        /// </summary>
        [ConfigurationProperty("description", IsRequired = false)]
        public string Description
        {
            get
            {
                return (string) this["description"];
            }
            set
            {
                this["description"] = value;
            }
        }
    }
}
