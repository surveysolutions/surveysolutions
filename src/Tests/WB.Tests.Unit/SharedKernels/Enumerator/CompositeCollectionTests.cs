using System.Collections.Specialized;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator
{
    [TestOf(typeof (CompositeCollection<>))]
    public class CompositeCollectionTests
    {
        [Test]
        public void when_clearing_child_collection_should_raise_items_reset_event()
        {
            CompositeCollection<string> items = new CompositeCollection<string>();

            items.Add("zero");
            var childCollection = new CompositeCollection<string> {"one", "two"};
            items.AddCollection(childCollection);
            items.Add("three");

            NotifyCollectionChangedEventArgs collectionChangedArgs = null;
            items.CollectionChanged += (sender, args) => collectionChangedArgs = args;

            childCollection.Clear();

            Assert.That(collectionChangedArgs, Is.Not.Null);
            Assert.That(collectionChangedArgs.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
        }

        [Test]
        public void when_notifing_item_changed()
        {
            CompositeCollection<string> items = new CompositeCollection<string>();

            items.Add("zero");
            items.AddCollection(new CompositeCollection<string> { "one", "two" });
            items.Add("three");

            NotifyCollectionChangedEventArgs collectionChangedArgs = null;
            items.CollectionChanged += (sender, args) => collectionChangedArgs = args;

            items.NotifyItemChanged("one");

            Assert.That(collectionChangedArgs, Is.Not.Null);
            Assert.That(collectionChangedArgs.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));
            Assert.That(collectionChangedArgs.OldStartingIndex, Is.EqualTo(1));
            Assert.That(collectionChangedArgs.NewStartingIndex, Is.EqualTo(1));
        }
    }
}