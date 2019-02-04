using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Integration.QuestionnaireSearchStorageTests
{
    [TestOf(typeof(QuestionnaireSearchStorage))]
    internal class when_search_with_many_words_in_query : QuestionnaireSearchStorageContext
    {
        [Test]
        public void should_search_entity_by_text()
        {
            var questionId = Guid.NewGuid();
            var questionnaireId = Guid.NewGuid();
            var questionnaireTitle = "q-title";

            RunActionInScope(sl =>
            {
                var questionnaireListViewItemRepository = sl.GetInstance<IPlainStorageAccessor<QuestionnaireListViewItem>>();
                questionnaireListViewItemRepository.Store(CreateQuestionnaireListViewItem(questionnaireId, questionnaireTitle), questionnaireId.FormatGuid());

                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                searchStorage.AddOrUpdateEntity(questionnaireId, Abc.Create.Entity.TextQuestion(questionId, text: "test car dog question text"));
            });

            RunActionInScope(sl =>
            {
                var searchStorage = sl.GetInstance<IQuestionnaireSearchStorage>();
                var searchResult = searchStorage.Search(new SearchInput() { Query = "cars dogs", PageSize = 20 });

                Assert.That(searchResult.Items.Count, Is.EqualTo(1));
                Assert.That(searchResult.Items.Single().EntityId, Is.EqualTo(questionId));
                Assert.That(searchResult.Items.Single().EntityType, Is.EqualTo(QuestionType.Text.ToString()));
                Assert.That(searchResult.Items.Single().QuestionnaireId, Is.EqualTo(questionnaireId));
                Assert.That(searchResult.Items.Single().QuestionnaireTitle, Is.EqualTo(questionnaireTitle));
            });
        }
    }
}
