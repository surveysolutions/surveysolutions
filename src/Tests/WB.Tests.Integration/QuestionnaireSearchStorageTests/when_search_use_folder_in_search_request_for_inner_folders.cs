using System;
using System.Linq;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Integration.QuestionnaireSearchStorageTests
{
    [TestOf(typeof(QuestionnaireSearchStorage))]
    internal class when_search_use_folder_in_search_request_for_inner_folders : QuestionnaireSearchStorageContext
    {
        [Test]
        public void should_found_elements_because_folder_with_data_down_in_folder_hierarchy()
        {
            var questionId = Guid.NewGuid();
            var questionnaireId = Guid.NewGuid();
            var questionnaireTitle = "q-title";

            var parentFolderId = Guid.NewGuid();
            var folderId = Guid.NewGuid();
            var folderTitle = "it is folder";

            RunActionInScope(sl =>
            {
                var foldersRepository = sl.GetInstance<IPlainStorageAccessor<QuestionnaireListViewFolder>>();
                foldersRepository.Store(CreateQuestionnaireListViewFolder(parentFolderId, "parent folder", null, 1), parentFolderId);
                foldersRepository.Store(CreateQuestionnaireListViewFolder(folderId, folderTitle, parentFolderId, 2, path: $"\\{parentFolderId}\\"), folderId);

                var questionnaireListViewItemRepository = sl.GetInstance<IPlainStorageAccessor<QuestionnaireListViewItem>>();
                questionnaireListViewItemRepository.Store(CreateQuestionnaireListViewItem(questionnaireId, questionnaireTitle, folderId: folderId), questionnaireId.FormatGuid());

                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                searchStorage.AddOrUpdateEntity(questionnaireId, new TextQuestion("test car dog question text") { PublicKey = questionId });
            });

            RunActionInScope(sl =>
            {
                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                var searchResult = searchStorage.Search(new SearchInput() { Query = "cars dogs", FolderId = parentFolderId });

                Assert.That(searchResult.Items.Count, Is.EqualTo(1));
                Assert.That(searchResult.Items.Single().FolderName, Is.EqualTo(folderTitle));
            });
        }
    }
}
