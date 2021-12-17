using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.ClassificationsTests
{
    [TestFixture]
    public class ClassificationStorageTests
    {
        [Test]
        public async Task When_updating_categories()
        {
            var storage = Create.ClassificationsAccessor(
                Entity.Classification.Group(Id.g1, title: "Z"),
                Entity.Classification.Classification(Id.g2, "A", userId: Id.gA, parent: Id.g1));

            var classificationStorage = Create.ClassificationStorage(storage);

            await classificationStorage.UpdateCategories(Id.g2, new []
                {
                    new Category{ Id = Id.g3, Parent = Id.g2, Title = "Yes", Value = 1 }, 
                    new Category{ Id = Id.g4, Parent = Id.g2, Title = "No", Value = 2 }, 
                    new Category{ Id = Id.g5, Parent = Id.g2, Title = "Don't know", Value = 3 }, 
                }, userId:Id.gA, isAdmin: false);

            var category1 = storage.ClassificationEntities.Find(Id.g3);
            Assert.That(category1, Is.Not.Null);
            Assert.That(category1.ClassificationId, Is.EqualTo(Id.g2));
            Assert.That(category1.UserId, Is.EqualTo(Id.gA));
            Assert.That(category1.Title, Is.EqualTo("Yes"));
            Assert.That(category1.Value, Is.EqualTo(1));
            Assert.That(category1.Index, Is.EqualTo(0));
            Assert.That(category1.Parent, Is.EqualTo(Id.g2));

            var category2 = storage.ClassificationEntities.Find(Id.g4);
            Assert.That(category2, Is.Not.Null);
            Assert.That(category2.ClassificationId, Is.EqualTo(Id.g2));
            Assert.That(category2.UserId, Is.EqualTo(Id.gA));
            Assert.That(category2.Title, Is.EqualTo("No"));
            Assert.That(category2.Value, Is.EqualTo(2));
            Assert.That(category2.Index, Is.EqualTo(1));
            Assert.That(category2.Parent, Is.EqualTo(Id.g2));

            var category3 = storage.ClassificationEntities.Find(Id.g5);
            Assert.That(category3, Is.Not.Null);
            Assert.That(category3.ClassificationId, Is.EqualTo(Id.g2));
            Assert.That(category3.UserId, Is.EqualTo(Id.gA));
            Assert.That(category3.Title, Is.EqualTo("Don't know"));
            Assert.That(category3.Value, Is.EqualTo(3));
            Assert.That(category3.Index, Is.EqualTo(2));
            Assert.That(category3.Parent, Is.EqualTo(Id.g2));
        }


        [Test]
        public async Task When_creating_classification()
        {
            var storage = Create.ClassificationsAccessor(Entity.Classification.Group(Id.g1, title: "Z"));

            var classificationStorage = Create.ClassificationStorage(
                 storage);

            await classificationStorage.CreateClassification(new Classification
            (
                id : Id.g2,
                title : "New classification",
                parent : Id.g1,
                categoriesCount:1,
                group:new ClassificationGroup()
            ), userId:Id.gA);

            Assert.That(storage.ClassificationEntities.Find(Id.g2).Type, Is.EqualTo(ClassificationEntityType.Classification));
            Assert.That(storage.ClassificationEntities.Find(Id.g2).Title, Is.EqualTo( "New classification"));
            Assert.That(storage.ClassificationEntities.Find(Id.g2).UserId, Is.EqualTo(Id.gA));
            Assert.That(storage.ClassificationEntities.Find(Id.g2).ClassificationId, Is.EqualTo(Id.g2));
            Assert.That(storage.ClassificationEntities.Find(Id.g2).Parent, Is.EqualTo(Id.g1));
        }

        [Test]
        public void When_non_admin_is_updating_public_classification()
        {
            var storage = Create.ClassificationsAccessor(
                Entity.Classification.Group(Id.g1, title: "Z"),
                Entity.Classification.Classification(Id.g2, "A", userId: null, parent: Id.g1));

            var classificationStorage = Create.ClassificationStorage(
                 storage);

            Assert.ThrowsAsync<ClassificationException>(async () => await classificationStorage.UpdateClassification(
                new Classification(id : Id.g2, title:"", new ClassificationGroup(), 0),userId:Id.gA, isAdmin: false));
        }

        [Test]
        public void When_non_admin_is_updating_others_classification()
        {
            var storage = Create.ClassificationsAccessor(
                Entity.Classification.Group(Id.g1, title: "Z"),
                Entity.Classification.Classification(Id.g2, "A", userId: Id.gB, parent: Id.g1));

            var classificationStorage = Create.ClassificationStorage(
                 storage);

            Assert.ThrowsAsync<ClassificationException>(async () => await classificationStorage.UpdateClassification(new Classification
            (
                id : Id.g2,
                title:"Classification A",
                group:new ClassificationGroup(), 
                categoriesCount: 1
                
            ), userId:Id.gA, isAdmin: false));
        }

        [Test]
        public async Task When_admin_is_updating_public_classification()
        {
            var group = Entity.Classification.Group(Id.g1, title: "Z");

            var storage = Create.ClassificationsAccessor(
                group,
                Entity.Classification.Classification(Id.g2, "A", userId: null, parent: Id.g1));

            var classificationStorage = Create.ClassificationStorage( storage);

            await classificationStorage.UpdateClassification(new Classification
            (
                id : Id.g2,
                title : "New classification",
                parent : Id.g1,
                categoriesCount:1,
                group: new ClassificationGroup()
                
                ), userId:Id.gA, isAdmin: true);

            Assert.That(storage.ClassificationEntities.Find(Id.g2).Type, Is.EqualTo(ClassificationEntityType.Classification));
            Assert.That(storage.ClassificationEntities.Find(Id.g2).Title, Is.EqualTo( "New classification"));
            Assert.That(storage.ClassificationEntities.Find(Id.g2).UserId, Is.Null);
            Assert.That(storage.ClassificationEntities.Find(Id.g2).ClassificationId, Is.EqualTo(Id.g2));
            Assert.That(storage.ClassificationEntities.Find(Id.g2).Parent, Is.EqualTo(Id.g1));
        }

        [Test]
        public async Task When_creating_classification_groups()
        {
            var storage = Create.ClassificationsAccessor();

            var classificationStorage = Create.ClassificationStorage( storage);

            await classificationStorage.CreateClassificationGroup(new ClassificationGroup
            {
                Id = Id.g1,
                Title = "New group"
            }, isAdmin: true);

            Assert.That(storage.ClassificationEntities.Find(Id.g1).Type, Is.EqualTo(ClassificationEntityType.Group));
            Assert.That(storage.ClassificationEntities.Find(Id.g1).Title, Is.EqualTo("New group"));
            Assert.That(storage.ClassificationEntities.Find(Id.g1).UserId, Is.Null);
        }

        
        [Test]
        public async Task When_deleting_classification()
        {
            var storage = Create.ClassificationsAccessor(
                Entity.Classification.Group(id: Id.g1, title: "group A"),
                Entity.Classification.Classification(id: Id.g2, title: $"B", parent: Id.g1),
                Entity.Classification.Classification(id: Id.g3, title: $"C", parent: Id.g1),
                Entity.Classification.Category(title: $"B", parent: Id.g2),
                Entity.Classification.Category(title: $"B", userId: Id.gA, parent: Id.g2),
                Entity.Classification.Category(title: $"B", userId: Id.gA, parent: Id.g2),
                Entity.Classification.Category(title: $"B", userId: Id.gA, parent: Id.g2),
                Entity.Classification.Category(id: Id.g4, title: $"C", userId: Id.gB, parent: Id.g3)
            );

            var classificationStorage = Create.ClassificationStorage(
                 storage);

            await classificationStorage.DeleteClassificationAsync(Id.g2, userId:Id.gA, isAdmin: true);

            CollectionAssert.AreEqual(new []{ Id.g1, Id.g3, Id.g4}, storage.ClassificationEntities.Select(x => x.Id).ToArray());
        }

        [Test]
        public void When_non_admin_is_deleting_classification()
        {
            var storage = Create.ClassificationsAccessor(
                Entity.Classification.Group(Id.g1, title: "Z"),
                Entity.Classification.Classification(Id.g2, "A", userId: null, parent: Id.g1));

            var classificationStorage = Create.ClassificationStorage(
                 storage);

            Assert.ThrowsAsync<ClassificationException>(async () => await classificationStorage.DeleteClassificationAsync(Id.g2, userId:Id.gA, isAdmin: false));
        }


        [Test]
        public void When_non_admin_is_creating_classification_group()
        {
            var classificationStorage = Create.ClassificationStorage(
                 Create.ClassificationsAccessor());

            Assert.ThrowsAsync<ClassificationException>(async () => await classificationStorage.CreateClassificationGroup(new ClassificationGroup(), isAdmin: false));
        }

        [Test]
        public async Task When_admin_updating_classification_group()
        {
            var storage = Create.ClassificationsAccessor(Entity.Classification.Group(Id.g1, title: "Z"));

            var classificationStorage = Create.ClassificationStorage(
                 storage);

            await classificationStorage.UpdateClassificationGroup(new ClassificationGroup
            {
                Id = Id.g1,
                Title = "New group"
            }, isAdmin: true);

            Assert.That(storage.ClassificationEntities.Count(), Is.EqualTo(1));
            Assert.That(storage.ClassificationEntities.Find(Id.g1).Type, Is.EqualTo(ClassificationEntityType.Group));
            Assert.That(storage.ClassificationEntities.Find(Id.g1).Title, Is.EqualTo("New group"));
            Assert.That(storage.ClassificationEntities.Find(Id.g1).UserId, Is.Null);
        }

        [Test]
        public void When_non_admin_is_updating_classification_group()
        {
            var classificationStorage = Create.ClassificationStorage(
                 Create.ClassificationsAccessor());

            Assert.ThrowsAsync<ClassificationException>(async () => await classificationStorage.UpdateClassificationGroup(new ClassificationGroup
            {
                Id = Id.g1,
                Title = "New group"
            }, isAdmin: false));
        }

        [Test]
        public async Task When_deleting_classification_group()
        {
            var storage = Create.ClassificationsAccessor(
                Entity.Classification.Group(id: Id.g1, title: "group A"),
                Entity.Classification.Classification(id: Id.g2, title: $"B", parent: Id.g1),
                Entity.Classification.Classification(id: Id.g3, title: $"C", parent: Id.g1),
                Entity.Classification.Category(title: $"B", parent: Id.g2),
                Entity.Classification.Category(title: $"B", userId: Id.gA, parent: Id.g2),
                Entity.Classification.Category(title: $"B", userId: Id.gA, parent: Id.g2),
                Entity.Classification.Category(title: $"B", userId: Id.gA, parent: Id.g2),
                Entity.Classification.Category(title: $"C", userId: Id.gB, parent: Id.g3)
                );

            var classificationStorage = Create.ClassificationStorage(
                 storage);

            await classificationStorage.DeleteClassificationGroup(Id.g1, true);

            Assert.That(storage.ClassificationEntities.Count(), Is.EqualTo(0));
        }

        [Test]
        public void When_non_admin_is_deleting_classification_group()
        {
            var classificationStorage = Create.ClassificationStorage(
                 Create.ClassificationsAccessor());

            Assert.ThrowsAsync<ClassificationException>(async () => await classificationStorage.DeleteClassificationGroup(Id.g1, isAdmin: false));
        }

        [Test]
        public async Task When_getting_classification_groups_for_user()
        {
            var storage = Create.ClassificationsAccessor(
                Entity.Classification.Group(id: Id.g2, title: "Z"),
                Entity.Classification.Group(title: "G"),
                Entity.Classification.Group(title: "K"),
                Entity.Classification.Group(title: "A"),
                Entity.Classification.Group(title: "B", userId: Id.gA),
                Entity.Classification.Group(id: Id.g1, title: "C", userId: Id.gB),
                Entity.Classification.Classification(parent: Id.g2),
                Entity.Classification.Classification(parent: Id.g2),
                Entity.Classification.Category()
            );

            var classificationStorage = Create.ClassificationStorage(
                 storage);

            var groups = (await classificationStorage.GetClassificationGroups(userId:Id.gA)).ToList();

            Assert.That(groups.Count, Is.EqualTo(5));
            CollectionAssert.IsOrdered(groups.Select(x => x.Title));
            CollectionAssert.DoesNotContain(groups.Select(x => x.Id), Id.g1);
            CollectionAssert.AllItemsAreInstancesOfType(groups,  typeof(ClassificationGroup));
            Assert.That(groups.Last().Count, Is.EqualTo(2));

        }

        [Test]
        public async Task When_getting_categories_for_user()
        {
            var entities = new List<ClassificationEntity>()
            {
                Entity.Classification.Group(id: Id.g1, title: "A"),
                Entity.Classification.Classification(id: Id.g2, title: "B", userId: Id.gA, parent: Id.g1)
            };
            var orders = Enumerable.Range(1, Constants.MaxLongRosterRowCount + 10).OrderBy(x => Guid.NewGuid()).ToArray();
            entities.AddRange(
                Enumerable
                    .Range(1, Constants.MaxLongRosterRowCount + 10)
                    .Select(i => Entity.Classification.Category(title: $"c {i}", parent: Id.g2, index: orders[i-1], value: i)));

            var storage = Create.ClassificationsAccessor(entities.ToArray());

            var classificationStorage = Create.ClassificationStorage(
                 storage);

            var categories = (await classificationStorage.GetCategories(Id.g2)).ToList();

            Assert.That(categories.Count, Is.EqualTo(Constants.MaxLongRosterRowCount));
            CollectionAssert.IsOrdered(categories.Select(x => x.Order));
            Assert.That(categories.Max(x => x.Order), Is.EqualTo(Constants.MaxLongRosterRowCount));
            Assert.That(categories.Min(x => x.Order), Is.EqualTo(1));
            CollectionAssert.AllItemsAreInstancesOfType(categories,  typeof(Category));
        }

        [Test]
        public async Task When_searching_for_classifications_for_user_in_all_groups()
        {
            var query = "aaa";

            var entities = new List<ClassificationEntity>()
            {
                Entity.Classification.Group(id: Id.g1, title: "group A"),
                Entity.Classification.Group(id: Id.g9, title: "group B"),
                Entity.Classification.Classification(id: Id.g2, title: $"B{query}", parent: Id.g1),
                Entity.Classification.Classification(id: Id.g3, title: $"{query} C", parent: Id.g1),
                Entity.Classification.Classification(id: Id.g4, title: $"D", parent: Id.g9),
                Entity.Classification.Classification(id: Id.g5, title: $"E", userId: Id.gB,  parent: Id.g1),
                Entity.Classification.Classification(id: Id.g6, title: $"F", userId: Id.gA, parent: Id.g1),
                Entity.Classification.Classification(id: Id.g7, title: $"G", userId: Id.gA, parent: Id.g1),
                Entity.Classification.Category(title: $"B{query} {query}", parent: Id.g2),
                Entity.Classification.Category(title: $"{query}B", userId: Id.gA, parent: Id.g2),
                Entity.Classification.Category(title: $"B{query}", userId: Id.gA, parent: Id.g2),
                Entity.Classification.Category(title: $"B", userId: Id.gA, parent: Id.g2),
                Entity.Classification.Category(title: $"B1", userId: Id.gA, parent: Id.g2),
                Entity.Classification.Category(title: $"C", userId: Id.gA, parent: Id.g3),
                Entity.Classification.Category(title: $"C2", userId: Id.gA, parent: Id.g3),
                Entity.Classification.Category(title: $"D {query}", userId: Id.gA, parent: Id.g4),
                Entity.Classification.Category(title: $"{query} D", userId: Id.gA, parent: Id.g4),
                Entity.Classification.Category(title: $"{query} E", userId: Id.gB, parent: Id.g5),
                Entity.Classification.Category(title: $"FF", userId: Id.gA, parent: Id.g6),
                Entity.Classification.Category(title: $"FFF", userId: Id.gA, parent: Id.g6),
            };

            var storage = Create.ClassificationsAccessor(entities.ToArray());

            var classificationStorage = Create.ClassificationStorage(
                 storage);

            var searchResult= await classificationStorage.SearchAsync(query, null, false, userId:Id.gA);

            Assert.That(searchResult.Classifications.Count, Is.EqualTo(3));
            Assert.That(searchResult.Total, Is.EqualTo(3));
            CollectionAssert.AreEqual(searchResult.Classifications.Select(x => x.Id).ToArray(), new []{ Id.g2, Id.g3, Id.g4 });

            Assert.That(searchResult.Classifications[0].CategoriesCount, Is.EqualTo(5));
            Assert.That(searchResult.Classifications[1].CategoriesCount, Is.EqualTo(2));
            Assert.That(searchResult.Classifications[2].CategoriesCount, Is.EqualTo(2));

            Assert.That(searchResult.Classifications[0].Group.Id, Is.EqualTo(Id.g1));
            Assert.That(searchResult.Classifications[1].Group.Id, Is.EqualTo(Id.g1));
            Assert.That(searchResult.Classifications[2].Group.Id, Is.EqualTo(Id.g9));
        }

        [Test]
        public async Task When_searching_for_classifications_for_user_in_selected_groups()
        {
            var query = "aaa";

            var entities = new List<ClassificationEntity>()
            {
                Entity.Classification.Group(id: Id.g1, title: "group A"),
                Entity.Classification.Group(id: Id.g9, title: "group B"),
                Entity.Classification.Classification(id: Id.g2, title: $"B{query}", parent: Id.g1),
                Entity.Classification.Classification(id: Id.g3, title: $"{query} C", parent: Id.g1),
                Entity.Classification.Classification(id: Id.g4, title: $"D", parent: Id.g9),
                Entity.Classification.Classification(id: Id.g5, title: $"E", userId: Id.gB,  parent: Id.g1),
                Entity.Classification.Classification(id: Id.g6, title: $"F", userId: Id.gA, parent: Id.g1),
                Entity.Classification.Classification(id: Id.g7, title: $"G", userId: Id.gA, parent: Id.g1),
                Entity.Classification.Category(title: $"B{query} {query}", parent: Id.g2),
                Entity.Classification.Category(title: $"{query}B", userId: Id.gA, parent: Id.g2),
                Entity.Classification.Category(title: $"B{query}", userId: Id.gA, parent: Id.g2),
                Entity.Classification.Category(title: $"B", userId: Id.gA, parent: Id.g2),
                Entity.Classification.Category(title: $"B1", userId: Id.gA, parent: Id.g2),
                Entity.Classification.Category(title: $"C", userId: Id.gA, parent: Id.g3),
                Entity.Classification.Category(title: $"C2", userId: Id.gA, parent: Id.g3),
                Entity.Classification.Category(title: $"D {query}", userId: Id.gA, parent: Id.g4),
                Entity.Classification.Category(title: $"{query} D", userId: Id.gA, parent: Id.g4),
                Entity.Classification.Category(title: $"{query} E", userId: Id.gB, parent: Id.g5),
                Entity.Classification.Category(title: $"FF", userId: Id.gA, parent: Id.g6),
                Entity.Classification.Category(title: $"FFF", userId: Id.gA, parent: Id.g6),
            };

            var storage = Create.ClassificationsAccessor(entities.ToArray());

            var classificationStorage = Create.ClassificationStorage(
                 storage);

            var searchResult= await classificationStorage.SearchAsync(query, Id.g1, false, userId:Id.gA);

            Assert.That(searchResult.Classifications.Count, Is.EqualTo(2));
            Assert.That(searchResult.Total, Is.EqualTo(2));
            CollectionAssert.AreEqual(searchResult.Classifications.Select(x => x.Id).ToArray(), new []{ Id.g2, Id.g3 });

            Assert.That(searchResult.Classifications[0].CategoriesCount, Is.EqualTo(5));
            Assert.That(searchResult.Classifications[1].CategoriesCount, Is.EqualTo(2));

            Assert.That(searchResult.Classifications[0].Group.Id, Is.EqualTo(Id.g1));
            Assert.That(searchResult.Classifications[1].Group.Id, Is.EqualTo(Id.g1));
        }

        
        [Test]
        public async Task When_searching_for_private_classifications_for_user()
        {
            var query = "aaa";

            var entities = new List<ClassificationEntity>()
            {
                Entity.Classification.Group(id: Id.g1, title: "group A"),
                Entity.Classification.Group(id: Id.g9, title: "group B"),
                Entity.Classification.Classification(id: Id.g2, title: $"B{query}", parent: Id.g1),
                Entity.Classification.Classification(id: Id.g3, title: $"{query} C", userId: Id.gA, parent: Id.g1),
                Entity.Classification.Classification(id: Id.g4, title: $"D", parent: Id.g9),
                Entity.Classification.Classification(id: Id.g5, title: $"E", userId: Id.gB,  parent: Id.g1),
                Entity.Classification.Classification(id: Id.g6, title: $"F", userId: Id.gA, parent: Id.g1),
                Entity.Classification.Classification(id: Id.g7, title: $"G", userId: Id.gA, parent: Id.g1),
                Entity.Classification.Category(title: $"B{query} {query}", parent: Id.g2),
                Entity.Classification.Category(title: $"{query}B", userId: Id.gA, parent: Id.g2),
                Entity.Classification.Category(title: $"B{query}", userId: Id.gA, parent: Id.g2),
                Entity.Classification.Category(title: $"B", userId: Id.gA, parent: Id.g2),
                Entity.Classification.Category(title: $"B1", userId: Id.gA, parent: Id.g2),
                Entity.Classification.Category(title: $"C", userId: Id.gA, parent: Id.g3),
                Entity.Classification.Category(title: $"C2", userId: Id.gA, parent: Id.g3),
                Entity.Classification.Category(title: $"D {query}", userId: Id.gA, parent: Id.g4),
                Entity.Classification.Category(title: $"{query} D", userId: Id.gA, parent: Id.g4),
                Entity.Classification.Category(title: $"{query} E", userId: Id.gB, parent: Id.g5),
                Entity.Classification.Category(title: $"FF", userId: Id.gA, parent: Id.g6),
                Entity.Classification.Category(title: $"FFF", userId: Id.gA, parent: Id.g6),
            };

            var storage = Create.ClassificationsAccessor(entities.ToArray());

            var classificationStorage = Create.ClassificationStorage(
                 storage);

            var searchResult= await classificationStorage.SearchAsync(query, Id.g1, privateOnly: true, userId: Id.gA);

            Assert.That(searchResult.Classifications.Count, Is.EqualTo(1));
            Assert.That(searchResult.Total, Is.EqualTo(1));
            CollectionAssert.AreEqual(searchResult.Classifications.Select(x => x.Id).ToArray(), new []{ Id.g3 });

            Assert.That(searchResult.Classifications[0].CategoriesCount, Is.EqualTo(2));
            Assert.That(searchResult.Classifications[0].Group.Id, Is.EqualTo(Id.g1));
        }
    }
}
