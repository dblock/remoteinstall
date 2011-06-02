using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace RemoteInstall
{
    public enum SequenceWhen
    {
        beforeinstall,
        beforeuninstall,
        beforeall,
        aftersuccessfulinstall,
        aftersuccessfuluninstall,
        afterall,
        afterfailedinstalluninstall
    }

    public interface ISequenceDriver
    {
        List<XmlResult> ExecuteSequence(SequenceWhen when);
    }

    public abstract class SequenceDriver<T> : ISequenceDriver
        where T : ConfigurationElementCollection
    {
        protected List<T> _configs = new List<T>();
        protected Instance _installInstance;

        public SequenceDriver(Instance instance)
        {
            _installInstance = instance;
        }

        public void Add(T config)
        {
            _configs.Add(config);
        }

        public abstract List<XmlResult> ExecuteSequence(SequenceWhen when); 
    }

    public class SequenceDrivers : List<ISequenceDriver>
    {
        public List<XmlResult> ExecuteSequence(SequenceWhen when)
        {
            List<XmlResult> results = new List<XmlResult>();
            foreach (ISequenceDriver driver in this)
            {
                results.AddRange(driver.ExecuteSequence(when));
            }
            return results;
        }
    }
}
