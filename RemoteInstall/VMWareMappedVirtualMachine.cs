using System;
using System.Collections.Generic;
using System.Text;
using Vestris.VMWareLib;
using System.IO;
using Vestris.VMWareLib.Tools.Windows;

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

        public void LoginInGuest(string username, string password)
        {
            ConsoleOutput.WriteLine(" Logging on to 'Remote:{0}' as '{1}'", _name, username);
            if (!_simulationOnly)
            {
                _vm.LoginInGuest(username, password);
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

        public VMWareVirtualMachine.Process RunProgramInGuest(string path, string parameters)
        {
            ConsoleOutput.WriteLine(" Executing 'Remote:{0} {1}'", path, parameters);
            return _simulationOnly ? new VMWareVirtualMachine.Process(null)
                : _vm.RunProgramInGuest(path, parameters);
        }

        public VMWareVirtualMachine.Process DetachProgramInGuest(string path, string parameters)
        {
            ConsoleOutput.WriteLine(" Detaching 'Remote:{0} {1}'", path, parameters);
            return _simulationOnly ? new VMWareVirtualMachine.Process(null)
                : _vm.DetachProgramInGuest(path, parameters);
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
                    File.Copy(sourcePath, Path.Combine(destinationPath, Path.GetFileName(sourcePath)), true);
                }
                else
                {
                    File.Copy(sourcePath, destinationPath, true);
                }
            }
            else
            {
                if (!Directory.Exists(destinationPath))
                {
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
                        File.Copy(systementry, Path.Combine(destinationPath, Path.GetFileName(systementry)), true);
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
                    if (! _simulationOnly)
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
                    if (! _simulationOnly)
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
                    if (! _simulationOnly)
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
                    if (! _simulationOnly)
                    {
                        _vm.CopyFileFromGuestToHost(guestPath, hostPath);
                    }
                    break;
            }
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
