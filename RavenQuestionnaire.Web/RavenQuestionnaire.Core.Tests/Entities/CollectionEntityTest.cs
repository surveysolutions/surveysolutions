using NUnit.Framework;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Documents;

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
    }
}
