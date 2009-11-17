using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using RemoteInstall;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Collections.Specialized;

namespace RemoteInstallUnitTests
{
    [TestFixture]
    public class ConfigUnitTests
    {
        [Test]
        public void TimeoutsConfigurationTest()
        {
            Configuration exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            RemoteInstallConfig remoteInstallConfig = new RemoteInstallConfig();
            remoteInstallConfig.Timeouts.ResetToDefaultValues();
            exeConfig.Sections.Add("RemoteInstallConfig", remoteInstallConfig);
            string configFileName = Path.GetTempFileName();
            exeConfig.SaveAs(configFileName);
            Console.WriteLine(File.ReadAllText(configFileName));
            File.Delete(configFileName);
        }

        [Test]
        public void EverythingConfigurationTest()
        {
            Stream everythingConfigStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "RemoteInstallUnitTests.Samples.Everything.config");
            string configFileName = Path.GetTempFileName();
            
            using (StreamReader everythingReader = new StreamReader(everythingConfigStream))
            {
                File.WriteAllText(configFileName, everythingReader.ReadToEnd());
            }

            ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
            configMap.ExeConfigFilename = configFileName;
            Configuration targetConfig = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            Console.WriteLine(File.ReadAllText(configFileName));
            File.Delete(configFileName);
        }

        [Test]
        public void EverythingSimulationTest()
        {
            Stream everythingConfigStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "RemoteInstallUnitTests.Samples.Everything.config");

            string configFileName = Path.GetTempFileName();

            using (StreamReader everythingConfigReader = new StreamReader(everythingConfigStream))
            {
                File.WriteAllText(configFileName, everythingConfigReader.ReadToEnd());
            }

            Stream everythingXmlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "RemoteInstallUnitTests.Samples.EverythingTask.xml");

            using (StreamReader everythingXmlReader = new StreamReader(everythingXmlStream))
            {
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(configFileName), "EverythingTask.xml"),
                    everythingXmlReader.ReadToEnd());
            }

            string outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(outputDir);

            NameValueCollection vars = new NameValueCollection();
            vars["root"] = @"..\..\..";

            Driver driver = new Driver(
                outputDir,
                true,
                configFileName,
                vars,
                1);

            // save results
            Results results = new Results();
            results.AddRange(driver.Run());
            string xmlFileName = Path.Combine(outputDir, "Results.xml");
            new ResultCollectionXmlWriter().Write(results, xmlFileName);
            // make sure results is a valid xml document with a number of results
            XmlDocument xmlResults = new XmlDocument();
            xmlResults.Load(xmlFileName);
            Assert.AreEqual(1, xmlResults.SelectNodes("/remoteinstallresultsgroups").Count);
            // reload the xml results
            Results resultsCopy = new Results();
            resultsCopy.Load(xmlResults);
            Assert.AreEqual(1, resultsCopy.GetXml().SelectNodes("/remoteinstallresultsgroups").Count);
            Directory.Delete(outputDir, true);
            File.Delete(configFileName);
        }
    }
}
