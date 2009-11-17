using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Reflection;

namespace RemoteInstall
{
    /// <summary>
    /// interface for RemoteInstall results writers, if another output format is created it should derive this class
    /// </summary>
    public interface ResultCollectionWriter
    {
        /// <summary>
        /// Writes the results to a file specified
        /// </summary>
        /// <param name="results"></param>
        /// <param name="fileName"></param>
        void Write(Results results, string fileName);
    }

    /// <summary>
    /// Abstract class that should be derived from by a results writer class if an XSL file is needed, a default XSL file is provided
    /// </summary>
    public abstract class XslDependentResultsWriter
    {
        /// <summary>
        /// xsl file represented as a stream
        /// </summary>
        private Stream _xslStream;

        /// <summary>
        /// Sets the XSL stream to the default file provided
        /// </summary>
        public XslDependentResultsWriter()
        {
            _xslStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "RemoteInstall.Xsl.Results.xsl");
        }

        public Stream XslStream
        {
            get
            {
                return _xslStream;
            }
        }

        /// <summary>
        /// Returns an XmlDocument format of the XSL stream
        /// </summary>
        public XmlDocument Xsl
        {
            get
            {
                StreamReader xslStream = new StreamReader(XslStream);
                XmlDocument xslDocument = new XmlDocument();
                xslDocument.LoadXml(xslStream.ReadToEnd());
                return xslDocument;
            }
        }
    }

    public class ResultCollectionXmlWriter : XslDependentResultsWriter, ResultCollectionWriter
    {
        /// <summary>
        /// Writes RemoteInstall output to an XML file, also generates an XSL file in the same directory to use for XSLT
        /// </summary>
        public void Write(Results results, string fileName)
        {
            XmlDocument xml = results.GetXml();
            string xslFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + ".xsl");
            XmlProcessingInstruction pi = xml.CreateProcessingInstruction("xml-stylesheet", string.Format("type=\"text/xsl\" href=\"{0}\"", Path.GetFileName(xslFileName)));
            xml.InsertAfter(pi, xml.FirstChild);            
            xml.Save(fileName);
            Xsl.Save(xslFileName);
        }
    }

    public class ResultCollectionHtmlWriter : XslDependentResultsWriter, ResultCollectionWriter
    {
        /// <summary>
        /// Writes RemoteInstall output to an HTML file, uses formatting from an XSL file
        /// </summary>
        public void Write(Results results, string fileName)
        {
            XPathDocument xmlPathDocument = new XPathDocument(new XmlNodeReader(results.GetXml()));
            XslCompiledTransform xslTransform = new XslCompiledTransform();
            xslTransform.Load(XmlReader.Create(XslStream));
            XmlTextWriter writer = new XmlTextWriter(fileName, System.Text.Encoding.UTF8);
            xslTransform.Transform(xmlPathDocument, null, writer);
            writer.Close();
        }
    }
}
