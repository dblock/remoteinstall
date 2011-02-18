using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using Interop.VixCOM;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.Specialized;
using CommandLine;
using RemoteInstall;

namespace RemoteInstaller
{
    class RemoteInstaller
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static int Main(string[] args)
        {
            ConsoleOutput.WriteLine("RemoteInstaller {0}: Product Test Driver", Assembly.GetExecutingAssembly().GetName().Version);
            ConsoleOutput.WriteLine();

            InstallerArgs iArgs = new InstallerArgs();
            if (!Parser.ParseArgumentsWithUsage(args, iArgs))
                return -1;

            Results results = new Results();

            try
            {
                iArgs.Parse();

                if (iArgs.verboseOutput)
                {
                    ConsoleOutput.ShowExceptionStack = true;
                    ConsoleOutput.WriteLine("Parsed command line arguments: ");
                    foreach (string arg in args)
                    {
                        ConsoleOutput.WriteLine(" {0}", arg);
                    }
                }

                Driver driver = new Driver(
                    iArgs.outputDir,
                    iArgs.simulationOnly,
                    iArgs.configFile,
                    iArgs.VariablesCollection,
                    iArgs.pipelineCount);

                ConsoleOutput.WriteLine("Results will be written to '{0}'", Path.GetFullPath(iArgs.outputDir));
                Directory.CreateDirectory(iArgs.outputDir);

                string outputXmlFile = Path.Combine(iArgs.outputDir, iArgs.outputXml);
                if (iArgs.appendOutput &&
                    !string.IsNullOrEmpty(iArgs.outputXml) &&
                    File.Exists(outputXmlFile))
                {
                    ConsoleOutput.WriteLine("Loading '{0}'", outputXmlFile);
                    results.Load(outputXmlFile);
                }
                else if (!string.IsNullOrEmpty(iArgs.outputXml) &&
                    File.Exists(outputXmlFile))
                {
                    File.Delete(outputXmlFile);
                }

                results.AddRange(driver.Run());

                return results.Success ? 0 : -1;
            }
            catch (Exception ex)
            {
                ConsoleOutput.WriteLine(ex);
                return -2;
            }
            finally
            {
                if (!string.IsNullOrEmpty(iArgs.outputXml))
                {
                    string xmlFileName = Path.Combine(iArgs.outputDir, iArgs.outputXml);
                    ConsoleOutput.WriteLine("Writing {0}", xmlFileName);
                    new ResultCollectionXmlWriter().Write(results, xmlFileName);
                }

                if (!string.IsNullOrEmpty(iArgs.outputHtml))
                {
                    string htmlFileName = Path.Combine(iArgs.outputDir, iArgs.outputHtml);
                    ConsoleOutput.WriteLine("Writing {0}", htmlFileName);
                    new ResultCollectionHtmlWriter().Write(results, htmlFileName);
                }
            }
        }
    }
}