using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using CommandLine;
using System.IO;

namespace RemoteInstall
{
    public class InstallerArgs
    {
        [Argument(ArgumentType.Required, HelpText = "Configuration File", LongName = "config", ShortName = "c")]
        public string configFile;
        [Argument(ArgumentType.AtMostOnce, HelpText = "Output Directory", LongName = "outputDir", ShortName = "o")]
        public string outputDir = Environment.CurrentDirectory;
        [Argument(ArgumentType.AtMostOnce, HelpText = "XML Output File (cannot include path)", LongName = "outputXml", ShortName = "x")]
        public string outputXml = string.Empty;
        [Argument(ArgumentType.AtMostOnce, HelpText = "HTML Output File (cannot include path)", LongName = "outputHtml", ShortName = "h")]
        public string outputHtml = string.Empty;
        [Argument(ArgumentType.AtMostOnce, HelpText = "Append (vs. overwrite) output file", ShortName = "a", DefaultValue = false)]
        public bool appendOutput;
        [Argument(ArgumentType.AtMostOnce, HelpText = "Simulate VMWare operations", ShortName = "s", DefaultValue = false)]
        public bool simulationOnly;
        [Argument(ArgumentType.AtMostOnce, HelpText = "Cause additional verbosity", ShortName = "v", DefaultValue = false)]
        public bool verboseOutput;
        [Argument(ArgumentType.AtMostOnce, HelpText = "Maximum number of parallel pipelines", ShortName = "p", DefaultValue = -1)]
        public int pipelineCount = -1;
        
        [DefaultArgument(ArgumentType.MultipleUnique, HelpText = "Additional Variables")]
        public string[] variables = { };

        public NameValueCollection VariablesCollection
        {
            get
            {
                NameValueCollection variablesCollection = new NameValueCollection();
                foreach (string variable in variables)
                {
                    string[] variablePair = variable.Split("=".ToCharArray(), 2);
                    if (variablePair.Length != 2)
                    {
                        throw new Exception(string.Format("Invalid variable: {0}", variable));
                    }
                    variablesCollection.Add(variablePair[0], variablePair[1]);
                }
                return variablesCollection;
            }
        }

        public void Parse()
        {
            if (outputHtml.IndexOf(Path.DirectorySeparatorChar) >= 0)
            {
                throw new ArgumentException("outputHtml cannot contain a path", "outputHtml");
            }

            if (outputXml.IndexOf(Path.DirectorySeparatorChar) >= 0)
            {
                throw new ArgumentException("outputXml cannot contain a path", "outputXml");
            }
        }
    }
}
