using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using System.Reflection;
using RemoteInstall;
using System.Text.RegularExpressions;

namespace RemoteInstallUnitTests
{
    [TestFixture]
    public class ConfigManagerUnitTests
    {
        /// <summary>
        /// Check that ${hostenv.*} and ${guestenv.*} aren't resolved by the ConfigManager.
        /// </summary>
        [Test]
        public void GuestAndHostEnvTests()
        {
            Stream configStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "RemoteInstallUnitTests.Samples.SampleMinimumCopy.config");
            
            string configFileName = Path.GetTempFileName();            
            using (StreamReader everythingReader = new StreamReader(configStream))
            {
                File.WriteAllText(configFileName, everythingReader.ReadToEnd());
            }

            ConfigManager configManager = new ConfigManager(configFileName, new NameValueCollection());
            Assert.AreEqual(1, configManager.Configuration.CopyFiles.Count);
            
            /*
                <copyfiles destpath="windows\${hostenv.PROCESSOR_ARCHITECTURE}\${env.PROCESSOR_ARCHITECTURE}\systemfiles">
                  <copyfile file="${guestenv.ProgramFiles(x86)}\system.ini" />
                </copyfiles>
             */

            CopyFileConfig copyFileConfig = configManager.Configuration.CopyFiles[0];
            Console.WriteLine("{0}: {1} => {2}", copyFileConfig.Name, copyFileConfig.File, copyFileConfig.DestinationPath);
            string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            Assert.AreEqual(string.Format("system dot ini ({0})", pa), copyFileConfig.Name);
            Assert.AreEqual(@"${guestenv.ProgramFiles(x86)}\system.ini", copyFileConfig.File);
            Assert.AreEqual(string.Format(@"windows\{0}\{1}\systemfiles", 
                "${hostenv.PROCESSOR_ARCHITECTURE}", pa), copyFileConfig.DestinationPath);
            File.Delete(configFileName);
        }

        [Test]
        public void VarRegexTests()
        {
            Assert.AreEqual(1, Regex.Matches("${var.NAME}", ConfigManager.VarRegex).Count);
            Assert.AreEqual(1, Regex.Matches("${var.Program-Files}", ConfigManager.VarRegex).Count);
            Assert.AreEqual(1, Regex.Matches("${var.ProgramFiles(x86)}", ConfigManager.VarRegex).Count);
            Assert.AreEqual("ProgramFiles(x86)", Regex.Matches("${var.ProgramFiles(x86)}", ConfigManager.VarRegex)[0].Groups["name"].Value);
        }
    }
}
