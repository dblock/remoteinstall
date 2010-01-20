using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Collections.Specialized;

namespace RemoteInstall
{
    /// <summary>
    /// A configuration file manager that expands variables.
    ///  ${env.name}: value of an environment variable
    ///  ${folder.type}: special folder, one of Environment.SpecialFolder
    ///  ${var.name}: a variable specified as name=value on the command line
    ///  ${file:name}: a file to include
    /// </summary>
    public class ConfigManager
    {        
        RemoteInstallConfig _config;
        NameValueCollection _variables;
        string _configFilename;

        public ConfigManager(string filename, NameValueCollection variables)
        {
            _variables = variables;
            Load(filename);
        }

        private void Load(string filename)
        {
            _configFilename = filename;
            string tempFilename = string.Empty;
            try
            {
                tempFilename = Path.GetTempFileName();
                File.WriteAllText(tempFilename, EvaluateFileContents(filename), Encoding.UTF8);
                _config = GetConfiguration(tempFilename);

                if (_config == null)
                {
                    throw new FileLoadException(string.Format("Error loading config file: {0}", filename));
                }

                // insert a noop installer when it's missing from the configuration
                _config.Installers.EnsureOne();
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempFilename) && File.Exists(tempFilename))
                {
                    File.Delete(tempFilename);
                }
            }
        }

        /// <summary>
        /// Configuration data
        /// </summary>
        public RemoteInstallConfig Configuration
        {
            get
            {
                return _config;
            }
        }

        private string Rewrite(Match m)
        {
            string var = m.Groups["var"].Value;
            string name = m.Groups["name"].Value;
            switch (var)
            {
                case "env":
                    return Environment.GetEnvironmentVariable(name);
                case "var":
                    string value = _variables[name];
                    if (value == null) throw new Exception(string.Format("Missing variable: {0}", name));
                    return value;
                case "folder":
                    return Environment.GetFolderPath((Environment.SpecialFolder)Enum.Parse(
                        typeof(Environment.SpecialFolder), name));
                case "file":
                    return EvaluateFileContents(
                        Path.Combine(Path.GetDirectoryName(_configFilename), 
                        name));
                case "guestenv":
                case "hostenv":
                    return "${" + string.Format("{0}.{1}", var, name) + "}";
                default:
                    throw new Exception(string.Format("Unsupported variable: $({0}.{1})",
                        var, name));
            }
        }

        private string EvaluateFileContents(string filename)
        {
            ConsoleOutput.WriteLine("Loading {0} ...", filename);
            using (StreamReader reader = new StreamReader(filename))
            {
                return Regex.Replace(reader.ReadToEnd(),
                    @"\$\{(?<var>[\w_]*)[\.\:](?<name>[\w_\.-]*)\}",
                    new MatchEvaluator(Rewrite),
                    RegexOptions.IgnoreCase);
            }
        }

        private RemoteInstallConfig GetConfiguration(string filename)
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = filename;
            Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(
                map, ConfigurationUserLevel.None);

            return (RemoteInstallConfig)configuration.Sections[typeof(RemoteInstallConfig).Name];
        }
    }
}
