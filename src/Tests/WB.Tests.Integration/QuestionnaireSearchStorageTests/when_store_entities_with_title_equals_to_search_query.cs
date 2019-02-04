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
    internal class when_store_entities_with_title_equals_to_search_query : QuestionnaireSearchStorageContext
    {
        [Test]
        public void should_search_entities_by_text()
        {
            var questionnaireId = Guid.NewGuid();
            var questionnaireTitle = "q-title";

            RunActionInScope(sl =>
            {
                var questionnaireListViewItemRepository = sl.GetInstance<IPlainStorageAccessor<QuestionnaireListViewItem>>();
                questionnaireListViewItemRepository.Store(CreateQuestionnaireListViewItem(questionnaireId, questionnaireTitle), questionnaireId.FormatGuid());

                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                searchStorage.AddOrUpdateEntity(questionnaireId, new TextQuestion("question text") { PublicKey = Guid.NewGuid() });
                searchStorage.AddOrUpdateEntity(questionnaireId, new TextQuestion("text from tales") { PublicKey = Guid.NewGuid() });
            });

            RunActionInScope(sl =>
            {
                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                var searchResult = searchStorage.Search(new SearchInput() { Query = "texts", PageSize = 20 });

                Assert.That(searchResult.Items.Count, Is.EqualTo(2));
                Assert.That(searchResult.Items.First().Title, Is.EqualTo("text from tales"));
                Assert.That(searchResult.Items.Last().Title, Is.EqualTo("question text"));
            });
        }
    }
}
