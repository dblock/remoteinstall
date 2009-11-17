using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace RemoteInstall
{
    public enum VirtualMachineCommand
    {
        none,
        poweroff,
        poweron,
        waitfortoolsinguest,
        shutdownguest
    }

    /// <summary>
    /// Configuration element that defines a single command to execute on the target test host.
    /// </summary>
    public class VirtualMachineTaskConfig : TaskConfigInstance
    {
        public VirtualMachineTaskConfig()
            : this(SequenceWhen.afterall)
        {

        }

        public VirtualMachineTaskConfig(SequenceWhen when)
            : base(when)
        {
            Command = VirtualMachineCommand.none;
        }

        public override TaskType Type
        {
            get
            {
                return TaskType.virtualmachine;
            }
            set
            {
                if (value != TaskType.virtualmachine)
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
        public VirtualMachineCommand Command
        {
            get
            {
                return (VirtualMachineCommand)this["command"];
            }
            set
            {
                this["command"] = value;
            }
        }
    }
}
