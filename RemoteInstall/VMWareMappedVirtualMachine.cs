using System;
using System.Collections.Generic;
using System.Text;
using Vestris.VMWareLib;
using System.IO;
using Vestris.VMWareLib.Tools.Windows;
using System.Text.RegularExpressions;
using Interop.VixCOM;

namespace RemoteInstall
{
    /// <summary>
    /// A VMWare virtual machine with a mapped drive.
    /// </summary>
    public class VMWareMappedVirtualMachine : IDisposable
    {
        private string _name = null;
        private VMWareVirtualMachine _vm = null;
        private CopyMethod _copyMethod = CopyMethod.undefined;
        private string _username;
        private string _password;
        private bool _simulationOnly = false;
        private Dictionary<string, string> _guestEnvironmentVariables = null;
        private Dictionary<string, string> _resolvedGuestEnvironmentVariables = new Dictionary<string, string>();
        private Dictionary<string, string> _resolvedHostEnvironmentVariables = new Dictionary<string, string>();

        public VMWareMappedVirtualMachine(
            string name,
            VMWareVirtualMachine vm,
            string username,
            string password,
            CopyMethod copyMethod,
            bool simulationOnly)
        {
            _name = name;
            _vm = vm;
            _copyMethod = copyMethod;
            _username = username;
            _password = password;
            _simulationOnly = simulationOnly;
        }

        #region passthrough calls

        public void PowerOn()
        {
            ConsoleOutput.WriteLine(" Powering on 'Remote:{0}'", _name);
            if (!_simulationOnly)
            {
                _vm.PowerOn();
            }
        }

        public void WaitForToolsInGuest()
        {
            ConsoleOutput.WriteLine(" Waiting for 'Remote:{0}'", _name);
            if (!_simulationOnly)
            {
                _vm.WaitForToolsInGuest();
            }
        }

        public void PowerOff()
        {
            ConsoleOutput.WriteLine(" Powering off 'Remote:{0}'", _name);
            if (!_simulationOnly)
            {
                _vm.PowerOff();
            }
        }

        public void LoginInGuest(string username, string password, GuestLoginType logintype)
        {
            int loginOptions = 0;
            switch (logintype)
            {
                case GuestLoginType.interactive:
                    ConsoleOutput.WriteLine(" Interactively logging on to 'Remote:{0}' as '{1}'", _name, username);
                    loginOptions = Constants.VIX_LOGIN_IN_GUEST_REQUIRE_INTERACTIVE_ENVIRONMENT;
                    break;
                default:
                    ConsoleOutput.WriteLine(" Logging on to 'Remote:{0}' as '{1}'", _name, username);
                    break;
            }

            if (!_simulationOnly)
            {
                _vm.LoginInGuest(username, password, loginOptions);
            }
        }

        public VMWareRootSnapshotCollection Snapshots
        {
            get
            {
                return _simulationOnly ? null : _vm.Snapshots;
            }
        }

        public VMWareVirtualMachine.VariableIndexer GuestVariables
        {
            get
            {
                return _simulationOnly ? null : _vm.GuestVariables;
            }
        }

        public bool FileExistsInGuest(string guestPathName)
        {
            return _simulationOnly ? false : _vm.FileExistsInGuest(guestPathName);
        }

        public bool DirectoryExistsInGuest(string guestPathName)
        {
            return _simulationOnly ? false : _vm.DirectoryExistsInGuest(guestPathName);
        }

        public void CreateDirectoryInGuest(string guestPathName)
        {
            ConsoleOutput.WriteLine(" Creating directory 'Remote:{0}'", guestPathName);
            if (!_simulationOnly)
            {
                _vm.CreateDirectoryInGuest(guestPathName);
            }
        }

        public void ShutdownGuest()
        {
            ConsoleOutput.WriteLine(" Shutting down 'Remote:{0}'", _name);
            if (!_simulationOnly)
            {
                _vm.ShutdownGuest();
            }
        }

        public VMWareVirtualMachine.Process RunProgramInGuest(string path, string parameters, int options)
        {
            ConsoleOutput.WriteLine(" Executing 'Remote:{0} {1}'", path, parameters);
            return _simulationOnly ? new VMWareVirtualMachine.Process(null)
                : _vm.RunProgramInGuest(path, parameters, options, VMWareInterop.Timeouts.RunProgramTimeout);
        }

        #endregion

        private void CopyFiles(string sourcePath, string destinationPath)
        {
            if (_simulationOnly)
                return;

            if (File.Exists(sourcePath))
            {
                // this is a file, target is a directory or a file
                if (Directory.Exists(destinationPath))
                {
                    // target is a directory
                    string resolvedDestinationPath = Path.Combine(destinationPath, Path.GetFileName(sourcePath));
                    ConsoleOutput.WriteLine(" Copy '{0}' => '{1}'", sourcePath, resolvedDestinationPath);
                    File.Copy(sourcePath, resolvedDestinationPath, true);
                }
                else
                {
                    ConsoleOutput.WriteLine(" Copy '{0}' => '{1}'", sourcePath, destinationPath);
                    File.Copy(sourcePath, destinationPath, true);
                }
            }
            else
            {
                if (!Directory.Exists(destinationPath))
                {
                    ConsoleOutput.WriteLine(" MkDir '{0}'", destinationPath);
                    Directory.CreateDirectory(destinationPath);
                }

                string[] systementries = Directory.GetFileSystemEntries(sourcePath);

                foreach (string systementry in systementries)
                {
                    if (Directory.Exists(systementry))
                    {
                        CopyFiles(systementry, Path.Combine(destinationPath, Path.GetFileName(systementry)));
                    }
                    else
                    {
                        string resolvedDestinationPath = Path.Combine(destinationPath, Path.GetFileName(systementry));
                        ConsoleOutput.WriteLine(" Copy '{0}' => '{1}'", systementry, resolvedDestinationPath);
                        File.Copy(systementry, resolvedDestinationPath, true);
                    }
                }
            }
        }

        public void CopyFileFromHostToGuest(string hostPath, string guestPath)
        {
            switch (_copyMethod)
            {
                case CopyMethod.network:
                    string guestRootPath = Path.GetPathRoot(guestPath);
                    MappedNetworkDriveInfo mappedNetworkDriveInfo = new MappedNetworkDriveInfo(guestRootPath);
                    mappedNetworkDriveInfo.Username = _username;
                    mappedNetworkDriveInfo.Password = _password;
                    mappedNetworkDriveInfo.Auto = false;
                    ConsoleOutput.WriteLine(" Mapping 'Remote:{0}' as '{1}'", mappedNetworkDriveInfo.RemotePath, _username);
                    if (!_simulationOnly)
                    {
                        using (MappedNetworkDrive mappedNetworkDrive = new MappedNetworkDrive(_vm, mappedNetworkDriveInfo))
                        {
                            string guestNetworkPath = mappedNetworkDrive.GuestPathToNetworkPath(guestPath);
                            string guestNetworkRootPath = mappedNetworkDrive.GuestPathToNetworkPath(guestRootPath);
                            ConsoleOutput.WriteLine(" Resolving 'Remote:{0}'", guestNetworkRootPath);
                            mappedNetworkDrive.MapNetworkDrive();
                            ConsoleOutput.WriteLine(" Copying '{0}' => 'Remote:{1}'", hostPath, guestNetworkPath);
                            CopyFiles(hostPath, guestNetworkPath);
                        }
                    }
                    break;
                case CopyMethod.vmware:
                default:
                    ConsoleOutput.WriteLine(" '{0}' => Remote:'{1}'", hostPath, guestPath);
                    if (!_simulationOnly)
                    {
                        _vm.CopyFileFromHostToGuest(hostPath, guestPath);
                    }
                    break;
            }
        }

        public void CopyFileFromGuestToHost(string guestPath, string hostPath)
        {
            switch (_copyMethod)
            {
                case CopyMethod.network:
                    string guestRootPath = Path.GetPathRoot(guestPath);
                    MappedNetworkDriveInfo mappedNetworkDriveInfo = new MappedNetworkDriveInfo(guestRootPath);
                    mappedNetworkDriveInfo.Username = _username;
                    mappedNetworkDriveInfo.Password = _password;
                    mappedNetworkDriveInfo.Auto = false;
                    ConsoleOutput.WriteLine(" Mapping 'Remote:{0}' as '{1}'", mappedNetworkDriveInfo.RemotePath, _username);
                    if (!_simulationOnly)
                    {
                        using (MappedNetworkDrive mappedNetworkDrive = new MappedNetworkDrive(_vm, mappedNetworkDriveInfo))
                        {
                            string guestNetworkPath = mappedNetworkDrive.GuestPathToNetworkPath(guestPath);
                            string guestNetworkRootPath = mappedNetworkDrive.GuestPathToNetworkPath(guestRootPath);
                            ConsoleOutput.WriteLine(" Resolving 'Remote:{0}'", guestNetworkRootPath);
                            mappedNetworkDrive.MapNetworkDrive();
                            ConsoleOutput.WriteLine(" Copying 'Remote:{0}' => '{1}'", guestNetworkPath, hostPath);
                            CopyFiles(guestNetworkPath, hostPath);
                        }
                    }
                    break;
                case CopyMethod.vmware:
                default:
                    ConsoleOutput.WriteLine(" 'Remote:{0}' => '{1}'", guestPath, hostPath);
                    if (!_simulationOnly)
                    {
                        _vm.CopyFileFromGuestToHost(guestPath, hostPath);
                    }
                    break;
            }
        }

        private string Rewrite(Match m)
        {
            string var = m.Groups["var"].Value;
            string name = m.Groups["name"].Value;
            switch (var)
            {
                case "hostenv":
                    string hostenvValue = string.Empty;
                    if (!_resolvedHostEnvironmentVariables.TryGetValue(name, out hostenvValue))
                    {
                        hostenvValue = Environment.GetEnvironmentVariable(name);
                        ConsoleOutput.WriteLine(" Resolved 'Local:%{0}%' => '{1}'", name, hostenvValue);
                        _resolvedHostEnvironmentVariables.Add(name, hostenvValue);
                    }
                    return hostenvValue;
                case "guestenv":
                    string guestenvValue = string.Empty;
                    if (!_resolvedGuestEnvironmentVariables.TryGetValue(name, out guestenvValue))
                    {
                        GetGuestEnvironmentVariable(name, out guestenvValue);
                        ConsoleOutput.WriteLine(" Resolved 'Remote:%{0}%' => '{1}'", name, guestenvValue);
                        _resolvedGuestEnvironmentVariables.Add(name, guestenvValue);
                    }
                    return guestenvValue;
                default:
                    throw new Exception(string.Format("Unsupported variable: $({0}.{1})",
                        var, name));
            }
        }

        public bool GetGuestEnvironmentVariable(string name, out string result)
        {
            if (_simulationOnly)
            {
                result = name;
                return true;
            }

            if (_guestEnvironmentVariables == null)
            {
                Shell guestShell = new Shell(_vm);
                _guestEnvironmentVariables = guestShell.GetEnvironmentVariables();
            }

            return _guestEnvironmentVariables.TryGetValue(name, out result);
        }

        public string Rewrite(string value)
        {
            return Regex.Replace(value, ConfigManager.VarRegex, new MatchEvaluator(Rewrite),
                RegexOptions.IgnoreCase);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_vm != null)
            {
                _vm.Dispose();
            }
        }

        #endregion
    }
}
