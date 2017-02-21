using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.GenericSubdomains.Utils.ObjectPool
{
    public class ObjectPoolsTests
    {
        private class PoolTarget
        {
            public long Created = 0;
            public long Disposed = 0;

            public PoolTarget()
            {
                
            }

            public void IncCreated()
            {
                this.Created++;
            }

            public void Clear()
            {
                this.Disposed++;
            }
        }

        [Test]
        public void ObjectPoolCreateNewItems()
        {
            var pool = new ObjectPool<PoolTarget>(() => new PoolTarget());

            var target = pool.GetObject();
            target.IncCreated();

            Assert.That(target.Created, Is.EqualTo(1));
        }


        [Test]
        public void ObjectPoolRunDisposeActionOnItemReturn()
        {
            var pool = new ObjectPool<PoolTarget>(() => new PoolTarget(), p => p.Clear());

            var target = pool.GetObject();
            target.IncCreated();
            pool.PutObject(target);

            Assert.That(target.Created, Is.EqualTo(1));
        }

        [Test]
        public void PoolReuseObjects()
        {
            var pool = new ObjectPool<PoolTarget>(() => new PoolTarget(), p => p.Clear());

            var item1 = pool.GetObject();
            item1.IncCreated();

            pool.PutObject(item1);

            var item2 = pool.GetObject();
            item2.IncCreated();

            Assert.That(item1, Is.EqualTo(item2));
            Assert.That(item1.Created, Is.EqualTo(2));
        }

        [Test]
        public void PoolDisposePatternDisposeProperly()
        {
            var pool = new ObjectPool<PoolTarget>(() => new PoolTarget(), p => p.Clear());

            for (int i = 0; i < 100; i++)
            {
                using (var target = pool.Object)
                {
                    target.Value.IncCreated();
                }
            }

            var poolitem = pool.GetObject();
            Assert.That(poolitem.Created, Is.EqualTo(100));
            Assert.That(poolitem.Disposed, Is.EqualTo(100));
        }
    }
}
