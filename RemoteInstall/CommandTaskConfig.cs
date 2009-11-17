using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace RemoteInstall
{
    /// <summary>
    /// Configuration element that defines a single command to execute on the target test host.
    /// </summary>
    public class CommandTaskConfig : TaskConfigInstance
    {
        public CommandTaskConfig()
            : base(SequenceWhen.afterall)
        {

        }

        public CommandTaskConfig(SequenceWhen when)
            : base(when)
        {

        }

        public override TaskType Type
        {
            get
            {
                return TaskType.command;
            }
            set
            {
                if (value != TaskType.command)
                {
                    throw new ArgumentException(string.Format(
                        "Invalid command type: {0}", value));
                }
            }
        }

        /// <summary>
        /// Command line (path to executable).
        /// </summary>
        [ConfigurationProperty("cmd", IsRequired = true)]
        public string Command
        {
            get
            {
                return (string)this["cmd"];
            }
            set
            {
                this["cmd"] = value;
            }
        }

        /// <summary>
        /// Command line arguments.
        /// </summary>
        [ConfigurationProperty("cmdargs", IsRequired = false)]
        public string CommandLineArgs
        {
            get
            {
                return (string)this["cmdargs"];
            }
            set
            {
                this["cmdargs"] = value;
            }
        }

        /// <summary>
        /// Expected return code.
        /// </summary>
        [ConfigurationProperty("exitcode", DefaultValue = 0)]
        public int ExitCode
        {
            get
            {
                return (int)this["exitcode"];
            }
            set
            {
                this["exitcode"] = value;
            }
        }

        /// <summary>
        /// Ignore exit code.
        /// </summary>
        [ConfigurationProperty("ignoreexitcode", DefaultValue = false)]
        public bool IgnoreExitCode
        {
            get
            {
                return (bool)this["ignoreexitcode"];
            }
            set
            {
                this["ignoreexitcode"] = value;
            }
        }

        /// <summary>
        /// Wait for process completion vs. detach.
        /// </summary>
        [ConfigurationProperty("waitforcompletion", DefaultValue = true)]
        public bool WaitForCompletion
        {
            get
            {
                return (bool)this["waitforcompletion"];
            }
            set
            {
                this["waitforcompletion"] = value;
            }
        }

        public string CmdLine
        {
            get
            {
                string result = Command;
                if (!string.IsNullOrEmpty(CommandLineArgs))
                {
                    result += " ";
                    result += CommandLineArgs;
                }
                return result;
            }
        }

        public override string Name
        {
            get
            {
                string result = base.Name;
                return string.IsNullOrEmpty(result) ? Path.GetFileName(Command) : result;
            }
            set
            {
                base.Name = value;
            }
        }
    }
}
