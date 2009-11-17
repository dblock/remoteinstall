using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using RemoteInstall;

namespace RemoteInstallUnitTests
{
    [TestFixture]
    public class ConfigOverlapUnitTests
    {
        [Test]
        public void NoOverlapTest()
        {
            VirtualMachineConfig config1 = new VirtualMachineConfig();
            config1.File = Guid.NewGuid().ToString();
            config1.Host = Guid.NewGuid().ToString();

            VirtualMachineConfig config2 = new VirtualMachineConfig();
            config2.File = Guid.NewGuid().ToString();
            config2.Host = Guid.NewGuid().ToString();

            // overlap self
            Assert.IsTrue(config1.Overlaps(config1));
            Assert.IsTrue(config2.Overlaps(config2));
            // overlap other
            Assert.IsFalse(config1.Overlaps(config2));
            Assert.IsFalse(config2.Overlaps(config1));
        }

        [Test]
        public void NoOverlapFileTest()
        {
            VirtualMachineConfig config1 = new VirtualMachineConfig();
            config1.File = Guid.NewGuid().ToString();
            config1.Host = Guid.NewGuid().ToString();

            VirtualMachineConfig config2 = new VirtualMachineConfig();
            config2.File = config1.File;
            config2.Host = Guid.NewGuid().ToString();

            // overlap self
            Assert.IsTrue(config1.Overlaps(config1));
            Assert.IsTrue(config2.Overlaps(config2));
            // overlap other
            Assert.IsFalse(config1.Overlaps(config2));
            Assert.IsFalse(config2.Overlaps(config1));
        }

        [Test]
        public void NoOverlapHostTest()
        {
            VirtualMachineConfig config1 = new VirtualMachineConfig();
            config1.File = Guid.NewGuid().ToString();
            config1.Host = Guid.NewGuid().ToString();

            VirtualMachineConfig config2 = new VirtualMachineConfig();
            config2.File = Guid.NewGuid().ToString();
            config2.Host = config1.Host;

            // overlap self
            Assert.IsTrue(config1.Overlaps(config1));
            Assert.IsTrue(config2.Overlaps(config2));
            // overlap other
            Assert.IsFalse(config1.Overlaps(config2));
            Assert.IsFalse(config2.Overlaps(config1));
        }

        [Test]
        public void OneOverlapTest()
        {
            VirtualMachineConfig config1 = new VirtualMachineConfig();
            config1.File = Guid.NewGuid().ToString();
            config1.Host = Guid.NewGuid().ToString();

            VirtualMachineConfig config2 = new VirtualMachineConfig();
            config2.File = config1.File;
            config2.Host = config2.File;

            Assert.IsFalse(config1.Overlaps(config2));
            Assert.IsFalse(config2.Overlaps(config1));
        }

        [Test]
        public void OneChildNoOverlapTest()
        {
            VirtualMachineConfig config1 = new VirtualMachineConfig();
            config1.File = Guid.NewGuid().ToString();
            config1.Host = Guid.NewGuid().ToString();

            VirtualMachineConfig config2 = new VirtualMachineConfig();
            config2.File = Guid.NewGuid().ToString();
            config2.Host = Guid.NewGuid().ToString();

            SnapshotConfig snapshot1 = new SnapshotConfig();
            config2.Snapshots.Add(snapshot1);
            SnapshotsConfig snapshots1 = new SnapshotsConfig();
            snapshots1.Add(snapshot1);

            Assert.IsFalse(config1.Overlaps(config2));
            Assert.IsFalse(config2.Overlaps(config1));
        }

        [Test]
        public void OneChildOverlapTest()
        {
            VirtualMachineConfig config1 = new VirtualMachineConfig();
            config1.File = Guid.NewGuid().ToString();
            config1.Host = Guid.NewGuid().ToString();

            VirtualMachineConfig config2 = new VirtualMachineConfig();
            config2.File = Guid.NewGuid().ToString();
            config2.Host = Guid.NewGuid().ToString();

            SnapshotConfig snapshot1 = new SnapshotConfig();
            snapshot1.VirtualMachines.Add(config1);
            SnapshotsConfig snapshots1 = new SnapshotsConfig();
            snapshots1.Add(snapshot1);
            config2.Snapshots.Add(snapshot1);

            Assert.IsTrue(config1.Overlaps(config2));
            Assert.IsTrue(config2.Overlaps(config1));
        }

        [Test]
        public void TwoChildrenOverlapTest()
        {
            VirtualMachineConfig config1 = new VirtualMachineConfig();
            config1.File = Guid.NewGuid().ToString();
            config1.Host = Guid.NewGuid().ToString();

            VirtualMachineConfig config2 = new VirtualMachineConfig();
            config2.File = Guid.NewGuid().ToString();
            config2.Host = Guid.NewGuid().ToString();

            // a snapshot child with a virtual machine
            SnapshotConfig snapshot0 = new SnapshotConfig();
            SnapshotsConfig snapshots0 = new SnapshotsConfig();
            snapshots0.Add(snapshot0);
            VirtualMachineConfig config3 = new VirtualMachineConfig();
            config3.File = Guid.NewGuid().ToString();
            config3.Host = Guid.NewGuid().ToString();
            snapshot0.VirtualMachines.Add(config3);
            config2.Snapshots.Add(snapshot0);
            
            SnapshotConfig snapshot1 = new SnapshotConfig();
            SnapshotsConfig snapshots1 = new SnapshotsConfig();
            snapshots1.Add(snapshot1);
            config3.Snapshots.Add(snapshot1);

            Assert.IsFalse(config1.Overlaps(config2));
            Assert.IsFalse(config2.Overlaps(config1));

            snapshot1.VirtualMachines.Add(config1);
            Assert.IsTrue(config1.Overlaps(config2));
            Assert.IsTrue(config2.Overlaps(config1));
        }
    }
}
