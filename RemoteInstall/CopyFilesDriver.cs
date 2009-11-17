using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Reflection;

namespace RemoteInstall
{
    public class CopyFilesDriver : SequenceDriver<CopyFilesConfig>
    {
        public CopyFilesDriver(Instance remoteInstallInstance)
            : base(remoteInstallInstance)
        {

        }

        /// <summary>
        /// Copy files
        /// </summary>
        public override List<XmlResult> ExecuteSequence(SequenceWhen when)
        {
            List<XmlResult> results = new List<XmlResult>();
            foreach (CopyFilesConfig copyFiles in _configs)
            {
                foreach (CopyFileConfig copyFile in copyFiles)
                {
                    if (copyFile.CopyWhen == when)
                    {
                        results.Add(CopyFile(copyFile));
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Copy a file from a remote vm.
        /// </summary>
        private CopyFileResult CopyFile(CopyFileConfig copyFileConfig)
        {
            CopyFileResult copyFileResult = new CopyFileResult();
            copyFileResult.File = copyFileConfig.File;
            copyFileResult.Name = copyFileConfig.Name;
            copyFileResult.IncludeInResults = copyFileConfig.IncludeInResults;
            copyFileResult.Success = true;
            try
            {
                switch (copyFileConfig.Destination)
                {
                    case CopyDestination.toTestClient:
                        CopyToTestClient(copyFileConfig, copyFileResult);
                        break;
                    case CopyDestination.toVirtualMachine:
                        CopyToVirtualMachine(copyFileConfig);
                        break;
                }
            }
            catch (Exception ex)
            {
                copyFileResult.Success = false;
                copyFileResult.LastError = ex.Message;
                ConsoleOutput.WriteLine(ex);
            }

            return copyFileResult;
        }

        private void CopyToVirtualMachine(CopyFileConfig copyFileConfig)
        {
            string destinationPath = copyFileConfig.DestinationPath;
            string destinationFileName = Path.Combine(destinationPath, Path.GetFileName(copyFileConfig.Name));
            string sourceFilePath = Path.Combine(Environment.CurrentDirectory, copyFileConfig.File);

            bool createDirectory = false;
            bool copyFile = false;

            if (!_installInstance.SimulationOnly)
            {
                if (!copyFileConfig.CheckIfExists || (File.Exists(sourceFilePath)
                    || Directory.Exists(sourceFilePath)))
                {
                    if (!_installInstance.VirtualMachine.DirectoryExistsInGuest(destinationPath))
                    {
                        createDirectory = true;
                    }

                    copyFile = true;
                }
            }
            else
            {
                copyFile = true;
                createDirectory = true;
            }

            if (createDirectory)
            {
                _installInstance.VirtualMachine.CreateDirectoryInGuest(destinationPath);
            }

            if (copyFile)
            {
                ConsoleOutput.WriteLine("Copying 'Local:{0}' to 'Remote:{1}'",
                    sourceFilePath, destinationFileName);

                _installInstance.VirtualMachine.CopyFileFromHostToGuest(
                    sourceFilePath, destinationFileName);
            }
            else
            {
                ConsoleOutput.WriteLine("Skipping 'Local:{0}'", sourceFilePath);
            }
        }

        private void CopyToTestClient(CopyFileConfig copyFileConfig, CopyFileResult copyFileResult)
        {
            string destinationPath = Path.Combine(_installInstance.LogPath, copyFileConfig.DestinationPath);
            string destinationFileName = Path.Combine(destinationPath, Path.GetFileName(copyFileConfig.Name));

            bool createDirectory = false;
            bool copyFile = false;

            if (!_installInstance.SimulationOnly)
            {
                if (!copyFileConfig.CheckIfExists || (_installInstance.VirtualMachine.FileExistsInGuest(copyFileConfig.File)
                    || _installInstance.VirtualMachine.DirectoryExistsInGuest(copyFileConfig.File)))
                {
                    if (!Directory.Exists(destinationPath))
                    {
                        createDirectory = true;
                    }

                    copyFile = true;
                }
            }
            else
            {
                copyFile = true;
                createDirectory = true;
            }

            if (createDirectory)
            {
                ConsoleOutput.WriteLine("Creating 'Local:{0}'", destinationPath);
                Directory.CreateDirectory(destinationPath);
            }

            if (copyFile)
            {
                ConsoleOutput.WriteLine("Copying 'Remote:{0}' to 'Local:{1}'", copyFileConfig.File, destinationFileName);
                _installInstance.VirtualMachine.CopyFileFromGuestToHost(copyFileConfig.File, destinationFileName);
                // local destination only exists when file was successfuly copied
                string destinationShortFileName = Path.Combine(_installInstance.ShortLogPath, copyFileConfig.DestinationPath);
                destinationShortFileName = Path.Combine(destinationShortFileName, Path.GetFileName(copyFileConfig.Name));
                copyFileResult.DestFilename = destinationShortFileName;

                if (!_installInstance.SimulationOnly)
                {
                    string transformedData = File.ReadAllText(destinationFileName);
                    if (!string.IsNullOrEmpty(copyFileConfig.XslTransform))
                    {
                        // transform data
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.LoadXml(transformedData);
                        XPathDocument xmlPathDocument = new XPathDocument(new XmlNodeReader(xmlDocument));
                        XslCompiledTransform xslTransform = new XslCompiledTransform();
                        xslTransform.Load(XmlReader.Create(new StreamReader(copyFileConfig.XslTransform)));
                        StringWriter writer = new StringWriter();
                        xslTransform.Transform(xmlPathDocument, null, writer);
                        writer.Close();
                        transformedData = writer.ToString();
                        File.WriteAllText(destinationFileName, transformedData);
                    }

                    // include contents in results, as is
                    if (copyFileConfig.IncludeDataInResults)
                    {
                        copyFileResult.Data = transformedData;
                    }
                }
            }
        }
    }
}
