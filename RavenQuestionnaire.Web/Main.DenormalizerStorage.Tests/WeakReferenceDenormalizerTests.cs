// -----------------------------------------------------------------------
// <copyright file="WeakReferenceDenormalizerTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Moq;
using NUnit.Framework;

namespace Main.DenormalizerStorage.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class WeakReferenceDenormalizerTests
    {
        [Test]
        public void SmokeTest()
        {
            Mock<IPersistentStorage<object>> storageMock=new Mock<IPersistentStorage<object>>();
            WeakReferenceDenormalizer<object> target = new WeakReferenceDenormalizer<object>(storageMock.Object);
            var key = Guid.NewGuid();
            var objectToStore = new object();
            target.Store(objectToStore, key);
            storageMock.Verify(x => x.Store(objectToStore, key), Times.Once());

            storageMock.Setup(x => x.GetByGuid(key)).Returns(objectToStore);

            var result = target.GetByGuid(key);
            Assert.IsTrue(objectToStore == result);

            storageMock.Verify(x => x.GetByGuid(key), Times.Once());
            result = target.GetByGuid(key);
            
            //still once!
            storageMock.Verify(x => x.GetByGuid(key), Times.Once());

            result = objectToStore = null;
            //befoure collect once
            storageMock.Verify(x => x.Store(It.IsAny<object>(), key), Times.Once());
            GC.Collect();
            //after collect twice
            storageMock.Verify(x => x.Store(It.IsAny<object>(), key), Times.Exactly(2));
        }
        [Test]
        public void Store_WhenObjectWasChangedAndGCCollectiongId_ObjectWillDumpTheLatestVersion()
        {
            Mock<IPersistentStorage<TestObjectDump>> storageMock = new Mock<IPersistentStorage<TestObjectDump>>();
            WeakReferenceDenormalizer<TestObjectDump> target = new WeakReferenceDenormalizer<TestObjectDump>(storageMock.Object);
            var key = Guid.NewGuid();
            var objectToStore = new TestObjectDump("test", Guid.NewGuid());

            target.Store(objectToStore, key);

            storageMock.Setup(x => x.GetByGuid(key)).Returns(new TestObjectDump("test", Guid.NewGuid()));
            var result = target.GetByGuid(key);
            result.Name = "hello world";

           // objectToStore = null;
            GC.Collect();
            storageMock.Verify(x => x.Store(It.Is<TestObjectDump>(o => o.Name == "hello world"), key), Times.Once());
        }
    }

    public class TestObjectDump
    {
        public TestObjectDump(string name, Guid key)
        {
            Name = name;
            Key = key;
        }

        public string Name { get; set; }
        public Guid Key { get; private set; }
    }
}
