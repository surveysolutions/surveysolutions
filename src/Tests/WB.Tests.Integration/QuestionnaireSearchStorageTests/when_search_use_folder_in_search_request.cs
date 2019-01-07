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
    internal class when_search_use_folder_in_search_request : QuestionnaireSearchStorageContext
    {
        [Test]
        public void should_dont_found_elements_because_folder_with_data_upper_in_folder_hierarchy()
        {
            var questionId = Guid.NewGuid();
            var questionnaireId = Guid.NewGuid();
            var questionnaireTitle = "q-title";

            var parentFolderId = Guid.NewGuid();
            var folderId = Guid.NewGuid();

            RunActionInScope(sl =>
            {
                var foldersRepository = sl.GetInstance<IPlainStorageAccessor<QuestionnaireListViewFolder>>();
                foldersRepository.Store(CreateQuestionnaireListViewFolder(parentFolderId, "parent folder", null, 1), parentFolderId);
                foldersRepository.Store(CreateQuestionnaireListViewFolder(folderId, "it is folder", parentFolderId, 2), folderId);

                var questionnaireListViewItemRepository = sl.GetInstance<IPlainStorageAccessor<QuestionnaireListViewItem>>();
                questionnaireListViewItemRepository.Store(CreateQuestionnaireListViewItem(questionnaireId, questionnaireTitle, folderId: parentFolderId), questionnaireId.FormatGuid());

                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                searchStorage.AddOrUpdateEntity(questionnaireId, new TextQuestion("test car dog question text") { PublicKey = questionId });
            });

            RunActionInScope(sl =>
            {
                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                var searchResult = searchStorage.Search(new SearchInput() { Query = "cars dogs", FolderId = folderId });

                Assert.That(searchResult.Items.Count, Is.EqualTo(0));
            });
        }
    }
}
