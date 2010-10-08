using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace RemoteInstall
{
    /// <summary>
    /// Contains file-related actions
    /// </summary>
    public class FileTools
    {
        /// <summary>
        /// Resolve file path
        /// </summary>
        public static string ResolveFilePath(string file)
        {
            int pos = file.IndexOf('*');
            while (pos != -1)
            {
                int front = file.Substring(0, pos).LastIndexOf('\\') + 1;
                int back = file.IndexOf('\\', pos);
                string lastDir = null;
                if (back > 0)
                {
                    lastDir = FindLatest(file.Substring(0, front), file.Substring(front, back - front), false);
                    file = lastDir + file.Substring(back);
                }
                else
                {
                    lastDir = FindLatest(file.Substring(0, front), file.Substring(front), true);
                    file = lastDir;
                }
                pos = file.IndexOf('*');
            }
            return file;
        }

        /// <summary>
        /// Resolve current svn revision
        /// </summary>
        public static string ResolveSvnRevision(string file)
        {
            int pos = file.IndexOf('*');
            int front = 0;
            int back = 0;

            if (pos < 0)
            {
                int lastSeparator = file.LastIndexOf('\\');
                if (lastSeparator < 0)
                {
                    return "unknown";
                }

                int beforeLastSeparator = file.Substring(0, lastSeparator - 1).LastIndexOf('\\');
                front = beforeLastSeparator + 1;
                back = lastSeparator;
            }
            else
            {
                front = file.Substring(0, pos).LastIndexOf('\\') + 1;
                back = file.IndexOf('\\', pos);
            }

            string[] subdirs = null;
            string path = file.Substring(0, front);
            if (string.IsNullOrEmpty(path))
            {
                return "unknown";
            }

            if (back > 0)
            {
                subdirs = Directory.GetDirectories(path, file.Substring(front, back - front));
            }
            else
            {
                subdirs = Directory.GetFiles(path, file.Substring(front));
            }

            if (subdirs.Length == 0)
            {
                throw new DirectoryNotFoundException("Could not find directory that met the search pattern criteria in directory: " + file + ".");
            }

            return subdirs[subdirs.Length - 1].Substring(
                subdirs[subdirs.Length - 1].LastIndexOf('\\') + 1);
        }

        /// <summary>
        /// Finds the latest version of an installer
        /// </summary>
        private static string FindLatest(string path, string dirpattern, Boolean searchfiles)
        {
            // todo: find all files or directories and sort by last modified date

            string[] subdirs = null;

            try
            {
                if (searchfiles)
                {
                    subdirs = Directory.GetFiles(path, dirpattern);
                }
                else
                {
                    subdirs = Directory.GetDirectories(path, dirpattern);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not find a file or directory that met the search pattern {0} in {1}: {2}",
                    dirpattern, path, ex.Message), ex);
            }

            if (subdirs.Length == 0)
            {
                throw new DirectoryNotFoundException(string.Format("Could not find directory or file that met the search pattern {0} in {1}",
                    dirpattern, path));
            }

            return subdirs[subdirs.Length - 1];
        }
    }
}
