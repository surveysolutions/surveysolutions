using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Synchronization.Core.SynchronizationFlow;

namespace Synchronization.Core.Tests
{
    [TestFixture]
    public class SyncChainDirectorTests
    {
        [Test]
        public void AddSynchronizer_RootIsEmpty_SynchronizerAddedAsRoot()
        {
            SyncChainDirector target = new SyncChainDirector();

            Mock<ISynchronizer> synchronizer1=new Mock<ISynchronizer>();

            target.AddSynchronizer(synchronizer1.Object);

            Mock<ISynchronizer> synchronizer2 = new Mock<ISynchronizer>();

            target.AddSynchronizer(synchronizer2.Object);

            synchronizer1.Verify(x => x.SetNext(synchronizer2.Object), Times.Once());
        }
        [Test]
        public void Push_2SynchronizersFirstIsSuccess_SecondWasntCalled_()
        {
            SyncChainDirector target = new SyncChainDirector();

            Mock<ISynchronizer> synchronizer1 = new Mock<ISynchronizer>();

            target.AddSynchronizer(synchronizer1.Object);

            Mock<ISynchronizer> synchronizer2 = new Mock<ISynchronizer>();

            target.AddSynchronizer(synchronizer2.Object);

            target.Push();

            synchronizer1.Verify(x=>x.Push(), Times.Once());
            synchronizer2.Verify(x => x.Push(), Times.Exactly(0));
        }
        [Test]
        public void Pull_2SynchronizersFirstIsSuccess_SecondWasntCalled_()
        {
            SyncChainDirector target = new SyncChainDirector();

            Mock<ISynchronizer> synchronizer1 = new Mock<ISynchronizer>();

            target.AddSynchronizer(synchronizer1.Object);

            Mock<ISynchronizer> synchronizer2 = new Mock<ISynchronizer>();

            target.AddSynchronizer(synchronizer2.Object);

            target.Pull();

            synchronizer1.Verify(x => x.Pull(), Times.Once());
            synchronizer2.Verify(x => x.Pull(), Times.Exactly(0));
        }
    }
}
