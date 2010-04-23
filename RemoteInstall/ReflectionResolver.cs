using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace RemoteInstall
{
    public class ReflectionResolverEventArgs : EventArgs
    {
        private string _variableName;
        private string _variableType;
        private string _result;
        private bool _rewritten = false;

        public string Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
                _rewritten = true;
            }
        }

        public string VariableName
        {
            get
            {
                return _variableName;
            }
            set
            {
                _variableName = value;
            }
        }

        public string VariableType
        {
            get
            {
                return _variableType;
            }
            set
            {
                _variableType = value;
            }
        }

        public bool Rewritten
        {
            get
            {
                return _rewritten;
            }
        }

        public ReflectionResolverEventArgs(string variableType, string variableName)
        {
            _variableName = variableName;
            _variableType = variableType;
        }
    }

    public class ReflectionResolver
    {
        private object[] _objects;

        public ReflectionResolver(object[] objects)
        {
            _objects = objects;
        }

        public bool TryResolve(string objectName, string propertyName, out String value)
        {
            foreach (object o in _objects)
            {
                if (o.GetType().Name.ToLower() == objectName.ToLower())
                {
                    PropertyInfo[] properties = o.GetType().GetProperties();
                    foreach (PropertyInfo pi in properties)
                    {
                        if (pi.Name.ToLower() == propertyName.ToLower())
                        {
                            value = pi.GetValue(o, null).ToString();
                            return true;
                        }
                    }
                }
            }

            value = null;
            return false;
        }
    }
}
