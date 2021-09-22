using System.Collections.Generic;
using System.Collections.Specialized;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Utils
{
    [TestOf(typeof(ObservableRangeCollection<>))]
    public class ObservableRangeCollectionTests
    {
        [Test]
        public void AddRangeTest()
        {
            var eventCollectionChangedCount = 0;

            var orc = new ObservableRangeCollection<int>(new List<int> { 0, 1, 2, 3 });
            orc.CollectionChanged += (sender, e) =>
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Add, e.Action);
                eventCollectionChangedCount++;
            };

            orc.AddRange(new List<int> { 4, 5, 6, 7 });

            Assert.AreEqual(8, orc.Count);
            CollectionAssert.AreEqual(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 }, orc);
            Assert.AreEqual(1, eventCollectionChangedCount);
        }

        [Test]
        public void InsertRangeTest()
        {
            var eventCollectionChangedCount = 0;

            var orc = new ObservableRangeCollection<int>(new List<int> { 0, 1, 2, 3 });
            orc.CollectionChanged += (sender, e) =>
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Add, e.Action);
                eventCollectionChangedCount++;
            };

            orc.InsertRange(1, new List<int> { 4, 5, 6 });

            Assert.AreEqual(7, orc.Count);
            CollectionAssert.AreEqual(new List<int> { 0, 4, 5, 6, 1, 2, 3 }, orc);
            Assert.AreEqual(1, eventCollectionChangedCount);
        }

        [Test]
        public void RemoveRangeTest()
        {
            var eventCollectionChangedCount = 0;

            var orc = new ObservableRangeCollection<int>(new List<int> { 0, 1, 2, 3 });
            orc.CollectionChanged += (sender, e) =>
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, e.Action);
                eventCollectionChangedCount++;
            };

            orc.RemoveRange(new List<int> { 1, 3, 6 });

            Assert.AreEqual(2, orc.Count);
            CollectionAssert.AreEqual(new List<int> { 0, 2 }, orc);
            Assert.AreEqual(1, eventCollectionChangedCount);
        }

        [Test]
        public void AddNullRangeTest()
        {
            Assert.That((TestDelegate)(() => new ObservableRangeCollection<int>().AddRange(null)), Throws.ArgumentNullException);
        }

        [Test]
        public void InsertNullRangeTest()
        {
            Assert.That((TestDelegate)(() => new ObservableRangeCollection<int>().InsertRange(0, null)), Throws.ArgumentNullException);
        }

        [Test]
        public void RemoveNullRangeTest()
        {
            Assert.That((TestDelegate)(() => new ObservableRangeCollection<int>().RemoveRange(null)), Throws.ArgumentNullException);
        }
    }
}
