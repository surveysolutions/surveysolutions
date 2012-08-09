using System;
using System.Collections;
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
            
            List<ISynchronizer>  synchronizerChain = new List<ISynchronizer>();

            SyncChainDirector target = new SyncChainDirector(synchronizerChain);

            Mock<ISynchronizer> synchronizer1=new Mock<ISynchronizer>();

            target.AddSynchronizer(synchronizer1.Object);

            Assert.AreEqual(synchronizerChain[0], synchronizer1.Object);

            Mock<ISynchronizer> synchronizer2 = new Mock<ISynchronizer>();

            target.AddSynchronizer(synchronizer2.Object);

            Assert.AreEqual(synchronizerChain[0], synchronizer1.Object);
            Assert.AreEqual(synchronizerChain[1], synchronizer2.Object);
        }
        [Test]
        public void Push_2SynchronizersFirstIsSuccess_SecondWasntCalled_()
        {
            IList<Exception> errorList = new List<Exception>();
            SyncChainDirector target = new SyncChainDirector();

            Mock<ISynchronizer> synchronizer1 = new Mock<ISynchronizer>();

            target.AddSynchronizer(synchronizer1.Object);

            Mock<ISynchronizer> synchronizer2 = new Mock<ISynchronizer>();

            target.AddSynchronizer(synchronizer2.Object);

            target.ExecuteAction((s) => s.Push(), errorList);

            synchronizer1.Verify(x=>x.Push(), Times.Once());
            synchronizer2.Verify(x => x.Push(), Times.Exactly(0));
        }
        [Test]
        public void Push_2SynchronizersFirstIThrowException_SecondWasCalled_()
        {
            IList<Exception> errorList = new List<Exception>();
            SyncChainDirector target = new SyncChainDirector();

            Mock<ISynchronizer> synchronizer1 = new Mock<ISynchronizer>();

            SynchronizationException exception = new SynchronizationException();
            synchronizer1.Setup(x => x.Push()).Throws(exception);

            target.AddSynchronizer(synchronizer1.Object);

            Mock<ISynchronizer> synchronizer2 = new Mock<ISynchronizer>();

            target.AddSynchronizer(synchronizer2.Object);

            target.ExecuteAction((s)=> s.Push(),errorList);

            synchronizer1.Verify(x => x.Push(), Times.Once());
            synchronizer2.Verify(x => x.Push(), Times.Exactly(1));
            Assert.AreEqual(errorList[0], exception);
        }
    }
}
