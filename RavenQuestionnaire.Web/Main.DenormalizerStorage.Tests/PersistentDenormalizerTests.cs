// -----------------------------------------------------------------------
// <copyright file="WeakReferenceDenormalizerTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Runtime.Caching;
using System.Threading;
using Moq;
using NUnit.Framework;

using WB.Core.Infrastructure;

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
            var target = new PersistentDenormalizer<IView>(cache, storageMock.Object);
            var key = Guid.NewGuid();
            var objectToStore = Mock.Of<IView>();
            target.Store(objectToStore, key);
            storageMock.Verify(x => x.Store(objectToStore, key.ToString()), Times.Never());
            Assert.IsTrue(cache.Contains(key.ToString()));
            cache.Remove(key.ToString());

            storageMock.Setup(x => x.GetByGuid<IView>(key.ToString())).Returns(objectToStore);

            var result = target.GetById(key);
            Assert.IsTrue(objectToStore == result);

            storageMock.Verify(x => x.GetByGuid<IView>(key.ToString()), Times.Once());
            result = target.GetById(key);
            
            //still once!
            storageMock.Verify(x => x.GetByGuid<IView>(key.ToString()), Times.Once());

        //    result = objectToStore = null;
            //befoure collect once
            storageMock.Verify(x => x.Store(It.IsAny<IView>(), key.ToString()), Times.Once());
            cache.Remove(key.ToString());
           
            //after collect twice
            storageMock.Verify(x => x.Store(It.IsAny<IView>(), key.ToString()), Times.Exactly(2));
        }
        [Test]
        public void Store_WhenObjectWasChangedAndExpired_ObjectWillDumpTheLatestVersion()
        {
            Mock<IPersistentStorage> storageMock = new Mock<IPersistentStorage>();
            var cache = new MemoryCache("WeakReferenceDenormalizer");
            PersistentDenormalizer<TestObjectDump> target = new PersistentDenormalizer<TestObjectDump>(cache, storageMock.Object);
            var key = Guid.NewGuid();
            var objectToStore = new TestObjectDump("test", key);

            target.Store(objectToStore, key);
         //   storageMock.Setup(x => x.GetByGuid<TestObjectDump>(key)).Returns(objectToStore);
            var result = target.GetById(key);
            result.Name = "hello world";

           // objectToStore = null;
            cache.Remove(key.ToString());
            storageMock.Verify(x => x.Store(It.Is<TestObjectDump>(o => o.Name == "hello world"), key.ToString()), Times.Exactly(1));
        }
        [Test]
        public void Remove_WhenCachedObjectIsAllover_ObjectIsAbswentInAllStoreges()
        {
            var key = Guid.NewGuid();
            var objectToStore = new TestObjectDump("test", key);
            Mock<IPersistentStorage> storageMock = new Mock<IPersistentStorage>();
            storageMock.Setup(x => x.GetByGuid<TestObjectDump>(key.ToString())).Returns(objectToStore);
            var cache = new MemoryCache("WeakReferenceDenormalizer");
          
            PersistentDenormalizer<TestObjectDump> target = new PersistentDenormalizer<TestObjectDump>(cache, storageMock.Object);
           

            var result = target.GetById(key);

            target.Remove(key);
            Thread.Sleep(1000);
            Assert.IsTrue(cache[key.ToString()] == null);

            storageMock.Verify(x => x.Store<TestObjectDump>(objectToStore, key.ToString()), Times.Once());
            storageMock.Verify(x => x.Remove<TestObjectDump>(key.ToString()), Times.Once());
            storageMock.Verify(x => x.GetByGuid<TestObjectDump>(key.ToString()), Times.Once());
          /*  Assert.IsTrue(storageStub.StoreCount == 0);
            Assert.IsTrue(storageStub.DeleteCount == 1);
            Assert.IsTrue(storageStub.GetCount == 1);*/

        }
        [Test]
        public void Store_WhenObjectAlreadyInMemCache_OldObjectIsReplace()
        {
            var key = Guid.NewGuid();
            var objectToStore = new TestObjectDump("test", key);
            Mock<IPersistentStorage> storageMock = new Mock<IPersistentStorage>();
            var cache = new MemoryCache("WeakReferenceDenormalizer");

            PersistentDenormalizer<TestObjectDump> target = new PersistentDenormalizer<TestObjectDump>(cache, storageMock.Object);

            cache.Add(key.ToString(), objectToStore, new CacheItemPolicy());

            target.Store(new TestObjectDump("hello", key), key);
            Assert.IsTrue((cache[key.ToString()] as TestObjectDump).Name == "hello");
        }
        [Test]
        public void GetByGuid_ObjectExistesOnlyInPersistantStorage_ObjectIsReturnedAndPlacedInMemoryAndBag()
        {
            var key = Guid.NewGuid();
            var objectToStore = new TestObjectDump("test", key);
            Mock<IPersistentStorage> storageMock = new Mock<IPersistentStorage>();
            storageMock.Setup(x => x.GetByGuid<TestObjectDump>(key.ToString())).Returns(objectToStore);
            var cache = new MemoryCache("WeakReferenceDenormalizer");

            PersistentDenormalizer<TestObjectDump> target = new PersistentDenormalizer<TestObjectDump>(cache, storageMock.Object);

            var result = target.GetById(key);
            Assert.AreEqual(result, objectToStore);
        }
    }

    public class TestObjectDump : IView
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
