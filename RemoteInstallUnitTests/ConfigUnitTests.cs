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
                "RemoteInstallUnitTests.TestConfigs.Everything.config");
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
                "RemoteInstallUnitTests.TestConfigs.Everything.config");

            string configFileName = Path.GetTempFileName();

            using (StreamReader everythingConfigReader = new StreamReader(everythingConfigStream))
            {
                File.WriteAllText(configFileName, everythingConfigReader.ReadToEnd());
            }

            Stream everythingXmlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "RemoteInstallUnitTests.TestConfigs.EverythingTask.xml");

            using (StreamReader everythingXmlReader = new StreamReader(everythingXmlStream))
            {
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(configFileName), "EverythingTask.xml"),
                    everythingXmlReader.ReadToEnd());
            }

            string outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(outputDir);

            NameValueCollection vars = new NameValueCollection();
            vars["root"] = @"..\..\..\..";

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

        [Test]
        public void NothingToDoTest()
        {
            Stream configStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "RemoteInstallUnitTests.TestConfigs.NothingToDo.config");
            string configFileName = Path.GetTempFileName();

            using (StreamReader sr = new StreamReader(configStream))
            {
                File.WriteAllText(configFileName, sr.ReadToEnd());
            }

            string outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(outputDir);

            try
            {
                Driver driver = new Driver(outputDir, true, configFileName, null, 0);
                Results results = new Results();
                results.AddRange(driver.Run());
                Assert.Fail("Expected InvalidConfigurationException");
            }
            catch (InvalidConfigurationException ex)
            {
                Console.WriteLine("Expected exception: {0}", ex.Message);
            }
            finally
            {
                Directory.Delete(outputDir, true);
                File.Delete(configFileName);
            }
        }

        [Test]
        public void SnapshotsWithParametersTest()
        {
            Stream configStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "RemoteInstallUnitTests.TestConfigs.SnapshotWithParameters.config");
            string configFileName = Path.GetTempFileName();

            using (StreamReader sr = new StreamReader(configStream))
            {
                File.WriteAllText(configFileName, sr.ReadToEnd());
            }

            string outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(outputDir);

            try
            {
                Driver driver = new Driver(outputDir, true, configFileName, null, 0);
                Results results = new Results();
                results.AddRange(driver.Run());
                // four individual results
                Assert.AreEqual(4, results.Count);
                foreach (ResultsGroup group in results)
                {
                    Console.WriteLine("{0}: {1}", group.Vm, group.Snapshot);
                    foreach(Result result in group)
                    {
                        // installer name is defined as name="@{snapshot.installargs}" 
                        // and each installargs is vm.snapshot
                        Assert.AreEqual(string.Format("{0}.{1}", group.Vm, group.Snapshot), 
                            result.InstallerName);
                    }
                }
            }
            finally
            {
                Directory.Delete(outputDir, true);
                File.Delete(configFileName);
            }
        }

        [Test]
        public void LatestDirConfigTest()
        {
            string dir1 = null;
            string dir2 = null;
            string prefix = Guid.NewGuid().ToString() + '-';
            string configFileName = null;
            string tempMsi = null;
            const string fileName = "testFile";

            try
            {
                // Create temp directories and files
                string tempPath = Path.GetTempPath() + prefix;
                dir1 = tempPath + "dir001" + '\\';
                Directory.CreateDirectory(dir1);
                File.CreateText(dir1 + fileName).Close();
                dir2 = tempPath + "dir002" + '\\';
                Directory.CreateDirectory(dir2);
                File.CreateText(dir2 + fileName).Close();
                tempMsi = Path.GetTempFileName();

                // Save config to disk
                Stream configStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "RemoteInstallUnitTests.TestConfigs.LatestDir.config");
                configFileName = Path.GetTempFileName();
                using (StreamReader sr = new StreamReader(configStream))
                {
                    File.WriteAllText(configFileName, sr.ReadToEnd());
                }

                // Check config
                NameValueCollection vars = new NameValueCollection();
                vars["temp"] = tempPath;
                vars["msi"] = tempMsi;
                ConfigManager config = new ConfigManager(configFileName, vars);
                Assert.AreEqual(config.Configuration.CopyFiles.Count, 1);
                Assert.AreEqual(config.Configuration.CopyFiles[0].File, dir2 + fileName);
            }
            finally
            {
                File.Delete(configFileName);
                File.Delete(tempMsi);
                Directory.Delete(dir1, true);
                Directory.Delete(dir2, true);
            }
        }
    }
}
