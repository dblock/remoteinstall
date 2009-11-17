using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Microsoft.Win32;

namespace NUnitDemo
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void CheckWhetherSampleMsiIsInstalled()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{89DD6045-A45B-4ED4-9C06-E93316D52A1D}");
            Assert.IsNotNull(key);
            object version = key.GetValue("Version");
            Console.WriteLine("Version: {0}", version);
            Assert.IsNotNull(version);
        }

        [Test]
        public void ThisTestAlwaysFails()
        {
            Assert.Fail("This test always fails.");
        }

        [Test]
        protected void ThisTestNeverRuns()
        {
        }
    }
}
