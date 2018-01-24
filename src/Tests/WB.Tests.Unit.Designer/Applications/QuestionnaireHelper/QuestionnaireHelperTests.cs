using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.Infrastructure.Implementation;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireHelper
{
    [TestOf(typeof(UI.Designer.Code.QuestionnaireHelper))]
    internal class QuestionnaireHelperTests : QuestionnaireHelperTestContext
    {
        [Test]
        public void when_getting_questionnaire_data_by_shared_questionnaires_should_return_shared_questionnaires_only()
        {
            //arrange
            var userId = Guid.Parse("11111111111111111111111111111111");
            
            var questionnaireListItemsRepository = new InMemoryPlainStorageAccessor<QuestionnaireListViewItem>();
            questionnaireListItemsRepository.Store(new[]
            {
                Create.QuestionnaireListViewItem(Guid.NewGuid(), true, userId, new[] {Create.SharedPerson(userId)}),
                Create.QuestionnaireListViewItem(Guid.NewGuid(), true, Guid.NewGuid(), new[] {Create.SharedPerson(userId)}),
                Create.QuestionnaireListViewItem(Guid.NewGuid(), false, userId, new[] {Create.SharedPerson(userId)}),
                Create.QuestionnaireListViewItem(Guid.NewGuid(), false, Guid.NewGuid(), new[] {Create.SharedPerson(userId)}),
                Create.QuestionnaireListViewItem(Guid.NewGuid(), false, Guid.NewGuid(), new[] {Create.SharedPerson(userId)})
            }.Select(x => new Tuple<QuestionnaireListViewItem, object>(x, x.QuestionnaireId)));

            var questionnaireHelper = new UI.Designer.Code.QuestionnaireHelper(
                new QuestionnaireListViewFactory(questionnaireListItemsRepository,
                    new InMemoryPlainStorageAccessor<QuestionnaireListViewFolder>()));
            //act
            var result = questionnaireHelper.GetQuestionnaires(userId, false, QuestionnairesType.Shared, null);
            //assert
            Assert.That(result.Count, Is.EqualTo(3));
        }
    }
}