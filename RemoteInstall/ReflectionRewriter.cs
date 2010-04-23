using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Text.RegularExpressions;

namespace RemoteInstall
{
    /// <summary>
    /// A global tasks configuration section with collections that belong to most nodes.
    /// </summary>
    public class ReflectionRewriter
    {
        public EventHandler<ReflectionResolverEventArgs> OnRewrite;
        public static string VarRegex = @"\@\{(?<var>[\w_]*)[\.\:](?<name>[\w_\.\-\(\)]*)\}";

        /// <summary>
        /// Resolve a value via reflection.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Rewrite(string value)
        {
            return Regex.Replace(value, VarRegex, new MatchEvaluator(Rewrite),
                RegexOptions.IgnoreCase);
        }

        private string Rewrite(Match m)
        {
            string var = m.Groups["var"].Value;
            string name = m.Groups["name"].Value;

            ReflectionResolverEventArgs args = new ReflectionResolverEventArgs(
                var, name);

            if (OnRewrite != null)
            {
                OnRewrite(this, args);
            }

            if (!args.Rewritten)
            {
                throw new Exception(string.Format("Unsupported variable or missing handler: @({0}.{1})",
                    var, name));
            }

            return args.Result;
        }

    }   
}
