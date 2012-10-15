// -----------------------------------------------------------------------
// <copyright file="WeakReferenceDenormalizerTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Runtime.Caching;
using System.Threading;
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
    public class PersistentDenormalizerTests
    {
        [Test]
        public void SmokeTest()
        {
            Mock<IPersistentStorage> storageMock=new Mock<IPersistentStorage>();
            var cache = new MemoryCache("WeakReferenceDenormalizer");
            PersistentDenormalizer<object> target = new PersistentDenormalizer<object>(cache, storageMock.Object, new List<Guid>());
            var key = Guid.NewGuid();
            var objectToStore = new object();
            target.Store(objectToStore, key);
            storageMock.Verify(x => x.Store(objectToStore, key), Times.Once());

            storageMock.Setup(x => x.GetByGuid<object>(key)).Returns(objectToStore);

            var result = target.GetByGuid(key);
            Assert.IsTrue(objectToStore == result);

            storageMock.Verify(x => x.GetByGuid<object>(key), Times.Once());
            result = target.GetByGuid(key);
            
            //still once!
            storageMock.Verify(x => x.GetByGuid<object>(key), Times.Once());

        //    result = objectToStore = null;
            //befoure collect once
            storageMock.Verify(x => x.Store(It.IsAny<object>(), key), Times.Once());
            cache.Remove(key.ToString());
           
            //after collect twice
            storageMock.Verify(x => x.Store(It.IsAny<object>(), key), Times.Exactly(2));
        }
        [Test]
        public void Store_WhenObjectWasChangedAndGCCollectiongId_ObjectWillDumpTheLatestVersion()
        {
            Mock<IPersistentStorage> storageMock = new Mock<IPersistentStorage>();
            var cache = new MemoryCache("WeakReferenceDenormalizer");
            PersistentDenormalizer<TestObjectDump> target = new PersistentDenormalizer<TestObjectDump>(cache, storageMock.Object, new List<Guid>());
            var key = Guid.NewGuid();
            var objectToStore = new TestObjectDump("test", key);

            target.Store(objectToStore, key);
            storageMock.Setup(x => x.GetByGuid<TestObjectDump>(key)).Returns(objectToStore);
            var result = target.GetByGuid(key);
            result.Name = "hello world";

           // objectToStore = null;
            cache.Remove(key.ToString());
            storageMock.Verify(x => x.Store(It.Is<TestObjectDump>(o => o.Name == "hello world"), key), Times.Exactly(2));
        }
        [Test]
        public void Remove_WhenCachedObjectIsAllover_ObjectIsAbswentInAllStoreges()
        {
            var key = Guid.NewGuid();
            var objectToStore = new TestObjectDump("test", key);
            Mock<IPersistentStorage> storageMock = new Mock<IPersistentStorage>();
            storageMock.Setup(x => x.GetByGuid<TestObjectDump>(key)).Returns(objectToStore);
            var cache = new MemoryCache("WeakReferenceDenormalizer");
            var bag = new List<Guid>();
            PersistentDenormalizer<TestObjectDump> target = new PersistentDenormalizer<TestObjectDump>(cache, storageMock.Object, bag);
           

            bag.Add(key);
           
            var result = target.GetByGuid(key);

            target.Remove(key);
            Thread.Sleep(1000);
            Assert.IsTrue(bag.Count==0);
            Assert.IsTrue(cache[key.ToString()] == null);

            storageMock.Verify(x => x.Store<TestObjectDump>(objectToStore, key), Times.Never());
            storageMock.Verify(x => x.Remove<TestObjectDump>(key), Times.Once());
            storageMock.Verify(x => x.GetByGuid<TestObjectDump>(key), Times.Once());
          /*  Assert.IsTrue(storageStub.StoreCount == 0);
            Assert.IsTrue(storageStub.DeleteCount == 1);
            Assert.IsTrue(storageStub.GetCount == 1);*/

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
