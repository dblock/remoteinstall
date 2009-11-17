using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Vestris.VMWareLib;
using System.Reflection;

namespace RemoteInstall
{
    /// <summary>
    /// Contains all the default timeout data for RemoteInstall to be used if timeout data is not provided in the config file. All time-outs are in seconds
    /// </summary>
    public abstract class VirtualMachineTimeoutDefaults
    {
        public const int defaultBaseTimeout = 60;
        public const int defaultConnectionTimeout = defaultBaseTimeout;
        public const int defaultOpenVMTimeout = defaultBaseTimeout;
        public const int defaultRevertToSnapshotTimeout = defaultBaseTimeout;
        /// <summary>
        /// the operational time to bring the power to/from the vm, not to boot it
        /// </summary>
        public const int defaultPowerOnTimeout = defaultBaseTimeout;
        public const int defaultPowerOffTimeout = defaultBaseTimeout;
        public const int defaultWaitForToolsTimeout = 5 * defaultBaseTimeout;
        /// <summary>
        /// the time to actually boot the machine
        /// </summary>
        public const int defaultLoginTimeout = defaultBaseTimeout;
        /// <summary>
        /// copy is very slow, see http://communities.vmware.com/thread/184489
        /// </summary>
        public const int defaultCopyFileTimeout = defaultBaseTimeout * 20;
        public const int defaultRunProgramTimeout = defaultBaseTimeout * 20;
        public const int defaultFileExistsTimeout = defaultBaseTimeout;
        public const int defaultLogoutTimeout = defaultBaseTimeout;
        public const int defaultListDirectoryTimeout = defaultBaseTimeout;
    }

    /// <summary>
    /// Configuration template class for VM timeouts
    /// </summary>
    public class VirtualMachineTimeoutConfig : ConfigurationElement
    {
        /// <summary>
        /// Default constuctor
        /// </summary>
        public VirtualMachineTimeoutConfig()
        {

        }

        [ConfigurationProperty("connection", DefaultValue = VirtualMachineTimeoutDefaults.defaultConnectionTimeout)]
        public int ConnectionTimeout 
        { 
            get { return (int) this["connection"]; }
            set { this["connection"] = value; }
        }

        [ConfigurationProperty("openVM", DefaultValue = VirtualMachineTimeoutDefaults.defaultOpenVMTimeout)]
        public int OpenVMTimeout
        {
            get { return (int)this["openVM"]; }
            set { this["openVM"] = value; }
        }

        // for backwards compatibility, last build to have this is 1.0.3412.0
        [ConfigurationProperty("openFile", DefaultValue = VirtualMachineTimeoutDefaults.defaultOpenVMTimeout)]
        public int OpenFileTimeout
        {
            get { return OpenVMTimeout; }
            set { OpenVMTimeout = value; }
        }

        [ConfigurationProperty("revertToSnapshot", DefaultValue = VirtualMachineTimeoutDefaults.defaultRevertToSnapshotTimeout)]
        public int RevertToSnapshotTimeout
        {
            get { return (int)this["revertToSnapshot"]; }
            set { this["revertToSnapshot"] = value; }
        }

        [ConfigurationProperty("powerOn", DefaultValue = VirtualMachineTimeoutDefaults.defaultPowerOnTimeout)]
        public int PowerOnTimeout
        {
            get { return (int)this["powerOn"]; }
            set { this["powerOn"] = value; }
        }

        [ConfigurationProperty("powerOff", DefaultValue = VirtualMachineTimeoutDefaults.defaultPowerOffTimeout)]
        public int PowerOffTimeout
        {
            get { return (int)this["powerOff"]; }
            set { this["powerOff"] = value; }
        }

        [ConfigurationProperty("waitForTools", DefaultValue = VirtualMachineTimeoutDefaults.defaultWaitForToolsTimeout)]
        public int WaitForToolsTimeout
        {
            get { return (int)this["waitForTools"]; }
            set { this["waitForTools"] = value; }
        }

        [ConfigurationProperty("login", DefaultValue = VirtualMachineTimeoutDefaults.defaultLoginTimeout)]
        public int LoginTimeout
        {
            get { return (int)this["login"]; }
            set { this["login"] = value; }
        }

        [ConfigurationProperty("logout", DefaultValue = VirtualMachineTimeoutDefaults.defaultLogoutTimeout)]
        public int LogoutTimeout
        {
            get { return (int)this["logout"]; }
            set { this["logout"] = value; }
        }

        [ConfigurationProperty("copyFile", DefaultValue = VirtualMachineTimeoutDefaults.defaultCopyFileTimeout)]
        public int CopyFileTimeout
        {
            get { return (int)this["copyFile"]; }
            set { this["copyFile"] = value; }
        }

        [ConfigurationProperty("runProgram", DefaultValue = VirtualMachineTimeoutDefaults.defaultRunProgramTimeout)]
        public int RunProgramTimeout
        {
            get { return (int)this["runProgram"]; }
            set { this["runProgram"] = value; }
        }

        [ConfigurationProperty("fileExists", DefaultValue = VirtualMachineTimeoutDefaults.defaultFileExistsTimeout)]
        public int FileExistsTimeout
        {
            get { return (int)this["fileExists"]; }
            set { this["fileExists"] = value; }
        }

        [ConfigurationProperty("listDirectory", DefaultValue = VirtualMachineTimeoutDefaults.defaultListDirectoryTimeout)]
        public int ListDirectoryTimeout
        {
            get { return (int)this["listDirectory"]; }
            set { this["listDirectory"] = value; }
        }

        public VMWareTimeouts GetVMWareTimeouts()
        {
            VMWareTimeouts timeouts = new VMWareTimeouts();
            timeouts.ConnectTimeout = ConnectionTimeout;
            timeouts.CopyFileTimeout = CopyFileTimeout;
            timeouts.RunProgramTimeout = RunProgramTimeout;
            // timeouts.DeleteTimeout = 
            timeouts.FileExistsTimeout = FileExistsTimeout;
            timeouts.ListDirectoryTimeout = ListDirectoryTimeout;
            timeouts.LoginTimeout = LoginTimeout;
            timeouts.LogoutTimeout = LogoutTimeout;
            timeouts.OpenVMTimeout = OpenVMTimeout;
            timeouts.PowerOffTimeout = PowerOffTimeout;
            timeouts.PowerOnTimeout = PowerOnTimeout;
            timeouts.RevertToSnapshotTimeout = RevertToSnapshotTimeout;
            timeouts.WaitForToolsTimeout = WaitForToolsTimeout;
            return timeouts;
        }

        /// <summary>
        /// Reset to default values.
        /// </summary>
        public void ResetToDefaultValues()
        {
            PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (PropertyInfo property in properties)
            {
                object[] customAttributes = property.GetCustomAttributes(typeof(ConfigurationPropertyAttribute), true);
                if (customAttributes.Length == 0)
                    continue;

                if (customAttributes.Length != 1)
                {
                    throw new Exception(string.Format("Invalid number of ConfigurationProperty attributes on property '{0}'",
                        property.Name));
                }

                ConfigurationPropertyAttribute configurationProperty = customAttributes[0] as ConfigurationPropertyAttribute;
                property.SetValue(this, configurationProperty.DefaultValue, null);
            }
        }
    }
}
