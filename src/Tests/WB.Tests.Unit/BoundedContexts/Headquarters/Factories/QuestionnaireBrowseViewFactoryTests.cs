using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Factories
{
    [TestOf(typeof(QuestionnaireBrowseViewFactory))]
    public class QuestionnaireBrowseViewFactoryTests
    {
        [Test]
        public void should_not_include_deleted_questionnaires()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.gA, 1);
            var item = Create.Entity.QuestionnaireBrowseItem(deleted: true, questionnaireId: Id.gA, version: 1, questionnaireIdentity: questionnaireIdentity);
            var storage = new InMemoryPlainStorageAccessor<QuestionnaireBrowseItem>();
            storage.Store(item, questionnaireIdentity.ToString());

            var viewFactory = CreateViewFactory(storage);
            
            // act
            var uniqueQuestionnaireIds = viewFactory.UniqueQuestionnaireIds(null, 5);

            // assert
            Assert.That(uniqueQuestionnaireIds.Items, Is.Empty);
            Assert.That(uniqueQuestionnaireIds.TotalCount, Is.EqualTo(0));
        }

        [Test]
        public void should_search_by_title()
        {
            var itemIdentity = Create.Entity.QuestionnaireIdentity(Id.gA, 1);
            var item1Identity = Create.Entity.QuestionnaireIdentity(Id.gA, 2);
            var item2Identity = Create.Entity.QuestionnaireIdentity(Id.gB, 3);
            var item3Identity = Create.Entity.QuestionnaireIdentity(Id.gB, 4);

            var item = Create.Entity.QuestionnaireBrowseItem(questionnaireId: itemIdentity.QuestionnaireId, itemIdentity.Version, questionnaireIdentity: itemIdentity, title: "a questionnaire1");

            var item1 = Create.Entity.QuestionnaireBrowseItem(questionnaireId: item1Identity.QuestionnaireId, item1Identity.Version, questionnaireIdentity: item1Identity, title: "Questionnaire1");

            var item2 = Create.Entity.QuestionnaireBrowseItem(questionnaireId: item2Identity.QuestionnaireId, item2Identity.Version, questionnaireIdentity: item2Identity, title: "questionnaire to be renamed");
            
            var item3 = Create.Entity.QuestionnaireBrowseItem(questionnaireId: item3Identity.QuestionnaireId, item3Identity.Version, questionnaireIdentity: item3Identity, title: "title new");

            var storage = new InMemoryPlainStorageAccessor<QuestionnaireBrowseItem>();
            storage.Store(item, itemIdentity.ToString());
            storage.Store(item1, item1Identity.ToString());
            storage.Store(item2, item2Identity.ToString());
            storage.Store(item3, item3Identity.ToString());

            var viewFactory = CreateViewFactory(storage);
            
            // act
            var uniqueQuestionnaireIds = viewFactory.UniqueQuestionnaireIds("quest", 5);

            // assert
            Assert.That(uniqueQuestionnaireIds.Items, Has.Count.EqualTo(2));
            Assert.That(uniqueQuestionnaireIds.Items[0].Id, Is.EqualTo(itemIdentity.QuestionnaireId));
            Assert.That(uniqueQuestionnaireIds.Items[1].Id, Is.EqualTo(item2Identity.QuestionnaireId));
        }

        private QuestionnaireBrowseViewFactory CreateViewFactory(
            IPlainStorageAccessor<QuestionnaireBrowseItem> storage = null)
        {
            return new QuestionnaireBrowseViewFactory(
                storage ?? new InMemoryPlainStorageAccessor<QuestionnaireBrowseItem>(),
                Mock.Of<IWebInterviewConfigProvider>());
        }
    }
}
