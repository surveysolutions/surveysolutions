using System;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Integration.QuestionnaireSearchStorageTests
{
    [TestOf(typeof(QuestionnaireSearchStorage))]
    internal class when_remove_questionnaire_data_from_search_storage : QuestionnaireSearchStorageContext
    {
        [Test]
        public void should_dont_find_entity_by_text()
        {
            var questionId = Guid.NewGuid();
            var questionnaireId = Guid.NewGuid();
            var questionnaireTitle = "q-title";

            RunActionInScope(sl =>
            {
                var questionnaireListViewItemRepository = sl.GetInstance<IPlainStorageAccessor<QuestionnaireListViewItem>>();
                questionnaireListViewItemRepository.Store(CreateQuestionnaireListViewItem(questionnaireId, questionnaireTitle), questionnaireId.FormatGuid());

                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                searchStorage.AddOrUpdateEntity(questionnaireId, new TextQuestion("question text") { PublicKey = questionId });
            });

            RunActionInScope(sl =>
            {
                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                searchStorage.RemoveAllEntities(questionnaireId);
            });

            RunActionInScope(sl =>
            {
                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                var searchResult = searchStorage.Search(new SearchInput() { Query = "texts", PageSize = 20 });

                Assert.That(searchResult.Items.Count, Is.EqualTo(0));
            });
        }
    }
}
