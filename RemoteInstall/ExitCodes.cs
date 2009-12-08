using System;
using System.Configuration;

namespace RemoteInstall
{
    class ExitCodeResults
    {
        public bool Reboot = false;
        public bool Success = true;
        public bool Failure = false;
    }

    public enum ExitCodeResult
    {
        reboot,
        success,
        failure
    }

    /// <summary>
    /// Exit code configuration.
    /// </summary>
    public class ExitCodesConfig : ConfigurationElementCollection
    {
        public ExitCodesConfig()
        {

        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ExitCodeConfig();
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((ExitCodeConfig)element).Value;
        }

        public void Add(ExitCodeConfig value)
        {
            BaseAdd(value);
        }

        public ExitCodeConfig this[int index]
        {
            get
            {
                return (ExitCodeConfig)BaseGet(index);
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

        /// <summary>
        /// Check an exit code for failure, success or reboot.
        /// </summary>
        /// <param name="exitCode">exit code</param>
        public void Check(int exitCode)
        {
            foreach (ExitCodeConfig exitCodeConfig in this)
            {
                if (exitCodeConfig.IsValue(exitCode))
                {
                    switch (exitCodeConfig.Result)
                    {
                        case ExitCodeResult.failure:
                            throw new Exception(string.Format("Execution failed (defined in exitcodes), return code: {0}",
                                exitCode));
                        case ExitCodeResult.reboot:
                            ConsoleOutput.WriteLine(string.Format("Execution succeeded and requires reboot (defined in exitcodes), return code: {0}",
                                exitCode));
                            return;
                        case ExitCodeResult.success:
                            ConsoleOutput.WriteLine(string.Format("Execution succeeded (defined in exitcodes), return code: {0}",
                                exitCode));
                            return;
                    }
                }
            }

            ConsoleOutput.WriteLine(string.Format("Warning: exitcodes fails to define action, ignored return code: {0}",
                exitCode));
        }

        public bool Contains(int exitCode, ExitCodeResult result)
        {
            foreach (ExitCodeConfig exitCodeConfig in this)
            {
                if (exitCodeConfig.IsValue(exitCode))
                {
                    if (exitCodeConfig.Result == result)
                    {
                        return true;
                    }

                    break;
                }
            }

            return false;
        }
    }

    public class ExitCodeConfig : ConfigurationElement
    {
        public ExitCodeConfig()
        {

        }

        [ConfigurationProperty("value", IsRequired = false)]
        public string Value
        {
            get
            {
                return (string)this["value"];
            }
            set
            {
                this["value"] = value;
            }
        }

        public bool IsValue(int exitCode)
        {
            int result = 0;
            if (int.TryParse(Value, out result))
            {
                return (result == exitCode);
            }
            else if (string.IsNullOrEmpty(Value))
            {
                return true;
            }
            else
            {
                throw new Exception(string.Format("Invalid exitcode value: {0}",
                    Value));
            }
        }

        [ConfigurationProperty("result", IsRequired = true)]
        public ExitCodeResult Result
        {
            get
            {
                return (ExitCodeResult)this["result"];
            }
            set
            {
                this["result"] = value;
            }
        }
    }
}
