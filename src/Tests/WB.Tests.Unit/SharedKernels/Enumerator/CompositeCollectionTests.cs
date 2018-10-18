using System;
using System.Collections.Specialized;
using System.ComponentModel;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator
{
    [TestOf(typeof (CompositeCollection<>))]
    public class CompositeCollectionTests
    {
        [Test]
        public void when_clearing_child_collection_should_raise_items_removed_with_offset_and_items()
        {
            var items = Create.Entity.CompositeCollection<string>();

            items.Add("zero");
            var childCollection = new CompositeCollection<string> {"one", "two"};
            items.AddCollection(childCollection);
            items.Add("three");

            NotifyCollectionChangedEventArgs collectionChangedArgs = null;
            items.CollectionChanged += (sender, args) =>
            {
                if (collectionChangedArgs != null) throw new Exception("Only one event expected");
                collectionChangedArgs = args;
            };

            childCollection.Clear();

            Assert.That(collectionChangedArgs, Is.Not.Null);
            Assert.That(collectionChangedArgs.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(collectionChangedArgs.OldStartingIndex, Is.EqualTo(1));
            Assert.That(collectionChangedArgs.OldItems, Is.EquivalentTo(new[] { "one", "two" }));
        }

        [Test]
        public void should_be_able_to_get_element_from_second_collection_by_index()
        {
            
            var items = Create.Entity.CompositeCollection<string>();

            items.Add("zero");
            var childCollection = new CompositeCollection<string> {"one", "two"};
            items.AddCollection(childCollection);

            // Act
            var result = items[2];

            // Assert
            Assert.That(result, Is.EqualTo("two"));
            Assert.That(items.Contains((object)"two"));
        }

        [Test]
        public void when_clearing_child_collection_should_raise_count_property_changed()
        {
            var items = Create.Entity.CompositeCollection<string>();

            items.Add("zero");
            var childCollection = new CompositeCollection<string> {"one", "two"};
            items.AddCollection(childCollection);
            items.Add("three");

            PropertyChangedEventArgs propertyChangedEventArgs = null;
            items.PropertyChanged += (sender, args) =>
            {
                if (propertyChangedEventArgs != null) throw new Exception("Only one event expected");
                propertyChangedEventArgs = args;
            };

            childCollection.Clear();

            Assert.That(propertyChangedEventArgs, Is.Not.Null);
            Assert.That(propertyChangedEventArgs.PropertyName, Is.EqualTo("Count"));
        }

        [Test]
        public void when_notifing_item_changed()
        {
            var items = Create.Entity.CompositeCollection<string>();

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

        [Test]
        public void when_adding_child_collection_should_raise_collection_changed_with_new_item_in_args()
        {
            var items = Create.Entity.CompositeCollection<string>();

            items.Add("zero");
            var childCollection = new CompositeCollection<string> { "one", "two" };
            items.AddCollection(childCollection);
            items.Add("four");

            NotifyCollectionChangedEventArgs collectionChangedArgs = null;
            items.CollectionChanged += (sender, args) => collectionChangedArgs = args;

            // act
            childCollection.AddCollection(new CovariantObservableCollection<string> { "three" });

            // assert
            Assert.That(collectionChangedArgs.NewItems, Is.EquivalentTo(new[] { "three" }));
            Assert.That(collectionChangedArgs.NewStartingIndex, Is.EqualTo(3));
        }

        [Test]
        public void when_clearing_child_collection_should_update_count_of_parent()
        {
            var items = Create.Entity.CompositeCollection<string>();

            var options = new CovariantObservableCollection<string> { "item" };
            items.AddCollection(options);

            var options1 = new CovariantObservableCollection<string> {"item1"};
            items.AddCollection(options1);

            // act
            options.Clear();

            // assert
            Assert.That(items.Count, Is.EqualTo(1));
            
        }
    }
}
