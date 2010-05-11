using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteInstall
{
    /// <summary>
    /// Invalid configuration.
    /// </summary>
    public class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException(string message)
            : base(message)
        {

        }

        public InvalidConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }
    }
}
