using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Tests.Entities
{
    [TestFixture]
    public class CollectionEntityTest
    {
        [Test]
        public void ChangeNameOfCollectionTest()
        {
            CollectionDocument innerDocument = new CollectionDocument();
            innerDocument.Name = "some name";
            Collection collection = new Collection(innerDocument);
            collection.UpdateText("new name");
            Assert.AreEqual(innerDocument.Name, "new name");
        }

        [Test]
        public void GetAllItemsFromCollection()
        {
            CollectionDocument innerDocument = new CollectionDocument();
            innerDocument.Items.Add(new CollectionItem() { Key = "USA", Value = "United States" });
            innerDocument.Items.Add(new CollectionItem() { Key = "Ukraine", Value = "Ukraine" });
            Collection collection = new Collection(innerDocument);
            Assert.AreEqual(collection.GetAllItems().Count, 2);
        }
        
        [Test]
        public void AddItemToCollection()
        {
            CollectionDocument innerDocument = new CollectionDocument();
            Collection collection = new Collection(innerDocument);
            collection.AddNewItems("Canada", "Canada");
            Assert.AreEqual(innerDocument.Items.Count, 1);
        }

        [Test]
        public void FindItemFromCollection()
        {
            CollectionDocument innerDocument = new CollectionDocument();
            Collection collection = new Collection(innerDocument);
            collection.AddNewItems("Canada", "Canada");
            collection.AddNewItems("USA", "United States");
            collection.AddNewItems("Ukraine", "Ukraine");
            CollectionItem result = collection.FindCollectionItem("Ukraine");
            Assert.AreEqual(result.Key, "Ukraine");
        }
    }
}
