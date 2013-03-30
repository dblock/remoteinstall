##  Next Release

### Features

* Added support for interactive virtual machine logon with a `logintype` option. Can be `normal` (default) or `interactive` and be specified at `snapshots` and `snapshot` nodes.
* Added `activatewindow` to `command` tasks. For Windows guest operating systems, when running a program with a graphical user interface, will ensure that the application's window is visible and not minimized on the guest's screen.
* Added support for `*` in `copyfile` tag when copying files to the virtual machine, used to substitute portions of a directory for the last entry in the directory listing.
* 13185: Added `includeMemory`, support to create snapshots which include the full state of the running virtual machine, including memory.
* Added support for reverting to snapshots with a powered on state.
* Added support for values `beforeinstall` and `beforeuninstall` in `when` attribute.
* Added support for attribute `exclude` in `copyfile` tag. You can use a regular expression to exclude a file or folder based on its path.

### Interface Changes

* For `command` tasks using `waitforcompletion="false"`, add `activatewindow="true"` as the default behavior has changed.

### Bugs

* Corrected unit test XSL transforms (`UnitTests.xsl` and `UnitTestDetails.xsl`) to include junit suite-level errors.
* 12943: Not powering off the vm if `powerOff="false"` and using a saved snapshot.

### Misc

* 12999: Updated RemoteInstaller.exe to automatically delete the results xml file to resolve unexpected ccnet build summary email results behavior.
* 12931: Updated RemoteInstaller.exe to output results xml file on exception and errors written to stderr.

## 1.2 (7/17/2010)

### Features

* Added `exe` installer type that installs any executable bootstrappers.
* Added support for user-defined exit codes that control success, failure and reboot in dni and exe installers.
* Added support for reboots in-between installers.
* Added `rebootRequired` option to installers to force reboot.
* Added `rebootIfRequred` option to installers to suppress reboot.
* Added `${guestenv.VARIABLE}` and `${hostenv.VARIABLE}` into `copyfile name`, `source` and `destination` nodes.
* Added support for dotNetInstaller 1.8 uninstall. If your bootstrapper supports uninstall, remove `uninstall="false"` from the configuration to test uninstall.
* Added inheritance of snapshot and virtual machine parameters to installers. You can now have a single configuration file for installers accepting different arguments for different snapshots.

### Misc

* Project upgraded to Visual Studio 2008.
* Added 3rd party component license information into the documentation.
* Added an icon to RemoteInstaller.exe.

### Bugs

* Bug: ERROR: Operation is not valid due to the current state of the object, assigning failure result after a task.
* Bug: NUnit or JUnit test errors embedded within results are formatted in the same red color as failures.
* Bug: snapshot `powerOff` option has no effect in `lifo` and `fifo` sequences.
* Bug: doesn't run on 64-bit Windows.

## 1.1 (11/17/2009)

* Initial open-source release.

