using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.PublicFoldersStorageTests
{
    [TestFixture]
    public class PublicFoldersStorageTests
    {
        [Test]
        public async Task When_getting_all_folders_depth_first()
        {
            var foldersAccessor = Create.InMemoryDbContext();

            var questionnaireListViewFolders = new []
            {
                Create.QuestionnaireListViewFolder(id: Id.g1, title: "A", parent: null),
                Create.QuestionnaireListViewFolder(id: Id.g2, title: "A1", parent: Id.g1),
                Create.QuestionnaireListViewFolder(id: Id.g7, title: "B1_1", parent: Id.g6),
                Create.QuestionnaireListViewFolder(id: Id.g5, title: "B", parent: null),
                Create.QuestionnaireListViewFolder(id: Id.g6, title: "B1", parent: Id.g5),
                Create.QuestionnaireListViewFolder(id: Id.g3, title: "A2", parent: Id.g1),
                Create.QuestionnaireListViewFolder(id: Id.g4, title: "A2_1", parent: Id.g3),
                Create.QuestionnaireListViewFolder(id: Id.g8, title: "B1_1_1", parent: Id.g7)
            };
            foreach (var questionnaireListViewFolder in questionnaireListViewFolders)
            {
                await foldersAccessor.QuestionnaireFolders.AddAsync(questionnaireListViewFolder);
            }

            await foldersAccessor.SaveChangesAsync();

            var storage = Create.PublicFoldersStorage(foldersAccessor);

            var folders = await storage.GetAllFoldersAsync();

            Assert.That(folders.Count, Is.EqualTo(8));
            CollectionAssert.IsOrdered(folders.Select(x => x.Title));
            CollectionAssert.IsOrdered(folders.Select(x => x.PublicId));
        }
    }
}
