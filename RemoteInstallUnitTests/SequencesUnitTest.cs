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
    public class SequencesUnitTests
    {
        [Test]
        public void SimulateAllMsiSequencesTest()
        {
            Stream sequencesConfigStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "RemoteInstallUnitTests.Samples.MsiSequences.config");

            string configFileName = Path.GetTempFileName();
            using (StreamReader sequencesConfigReader = new StreamReader(sequencesConfigStream))
            {
                File.WriteAllText(configFileName, sequencesConfigReader.ReadToEnd());
            }

            string outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(outputDir);

            foreach (InstallersSequence sequence in Enum.GetValues(typeof(InstallersSequence)))
            {
                Console.WriteLine("Sequence: {0}", sequence);
                NameValueCollection vars = new NameValueCollection();
                vars["root"] = @"..\..\..";
                vars["sequence"] = sequence.ToString();

                Driver driver = new Driver(
                    outputDir,
                    true,
                    configFileName,
                    vars,
                    1);

                Results results = new Results();
                results.AddRange(driver.Run());

                switch (sequence)
                {
                    case InstallersSequence.clean:
                        // a clean sequence is like 2 separate, clean installations
                        Assert.AreEqual(2, results.Count);
                        Assert.AreEqual(1, results[0].Count);
                        Assert.AreEqual(1, results[1].Count);
                        break;
                    case InstallersSequence.alternate:
                    case InstallersSequence.install:
                    case InstallersSequence.uninstall:
                        // two installers alternating with install+uninstall in the same run 
                        // or just install or uninstall
                        Assert.AreEqual(1, results.Count);
                        Assert.AreEqual(2, results[0].Count);
                        break;
                    default:
                        // two installers split into install+uninstall in a sequence of 4
                        Assert.AreEqual(1, results.Count);
                        Assert.AreEqual(4, results[0].Count);
                        break;
                }
            }

            Directory.Delete(outputDir, true);
            File.Delete(configFileName);
        }

        [Test]
        public void SimulateAllDniSequencesTest()
        {
            Stream sequencesConfigStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "RemoteInstallUnitTests.Samples.DniSequences.config");

            string configFileName = Path.GetTempFileName();
            using (StreamReader sequencesConfigReader = new StreamReader(sequencesConfigStream))
            {
                File.WriteAllText(configFileName, sequencesConfigReader.ReadToEnd());
            }

            string outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(outputDir);

            foreach (InstallersSequence sequence in Enum.GetValues(typeof(InstallersSequence)))
            {
                Console.WriteLine("Sequence: {0}", sequence);
                NameValueCollection vars = new NameValueCollection();
                vars["root"] = @"..\..\..";
                vars["sequence"] = sequence.ToString();

                Driver driver = new Driver(
                    outputDir,
                    true,
                    configFileName,
                    vars,
                    1);

                Results results = new Results();
                results.AddRange(driver.Run());

                switch (sequence)
                {
                    case InstallersSequence.clean:
                        // a clean sequence is like 2 separate, clean installations
                        Assert.AreEqual(2, results.Count);
                        Assert.AreEqual(1, results[0].Count);
                        Assert.AreEqual(1, results[1].Count);
                        break;
                    case InstallersSequence.alternate:
                    case InstallersSequence.install:
                        // two installers alternating with install+uninstall in the same run 
                        // or just install or uninstall
                        Assert.AreEqual(1, results.Count);
                        Assert.AreEqual(2, results[0].Count);
                        break;
                    case InstallersSequence.uninstall:
                        Assert.AreEqual(1, results.Count);
                        Assert.AreEqual(2, results[0].Count);
                        break;
                    default:
                        // two installers split into install+uninstall in a sequence of 4                        
                        Assert.AreEqual(1, results.Count);
                        Assert.AreEqual(4, results[0].Count);
                        break;
                }
            }

            Directory.Delete(outputDir, true);
            File.Delete(configFileName);
        }
    }
}
