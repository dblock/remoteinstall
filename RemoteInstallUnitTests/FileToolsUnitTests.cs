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
    public class FileToolsUnitTests
    {
        [Test]
        public void ResolveFilePathTest()
        {
            string dir1 = null;
            string dir2 = null;
            string prefix = Guid.NewGuid().ToString() + '-';
            const string fileName = "testFile";

            try
            {
                string tempPath = Path.GetTempPath() + prefix;
                dir1 = tempPath + "dir001" + '\\';
                Directory.CreateDirectory(dir1);
                File.CreateText(dir1 + fileName).Close();
                dir2 = tempPath + "dir002" + '\\';
                Directory.CreateDirectory(dir2);
                File.CreateText(dir2 + fileName).Close();

                string latestFile = FileTools.ResolveFilePath(tempPath + "*\\" + fileName);

                Assert.AreEqual(dir2 + fileName, latestFile);
            }
            finally
            {
                Directory.Delete(dir1, true);
                Directory.Delete(dir2, true);
            }
        }

        [Test]
        public void ResolveSvnRevisionTest()
        {
            string dir1 = null;
            string dir2 = null;
            string prefix = Guid.NewGuid().ToString() + '-';
            const string fileName = "testFile";

            try
            {
                string tempPath = Path.GetTempPath() + prefix;
                dir1 = tempPath + "dir001" + '\\';
                Directory.CreateDirectory(dir1);
                File.CreateText(dir1 + fileName).Close();
                dir2 = tempPath + "dir002" + '\\';
                Directory.CreateDirectory(dir2);
                File.CreateText(dir2 + fileName).Close();

                string latestSvnRevision = FileTools.ResolveSvnRevision(tempPath + "*\\" + fileName);

                Assert.AreEqual(prefix + "dir002", latestSvnRevision);
            }
            finally
            {
                Directory.Delete(dir1, true);
                Directory.Delete(dir2, true);
            }
        }

    }
}
