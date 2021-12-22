using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_cloning_questionnaire_and_new_title_has_invalid_characters : QuestionnaireTestsContext
    {
        [Test]
        public void should_throw_exception()
        {
            var invalidTitle = "Invalid [Title>";
            QuestionnaireIdentity questionnaireIdentity
                = Create.Entity.QuestionnaireIdentity(Id.gA, 3);

            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage
                = SetUp.PlainStorageAccessorWithOneEntity(
                    id: questionnaireIdentity.ToString(), entity: Create.Entity.QuestionnaireBrowseItem());

            IQuestionnaireStorage questionnaireStorage =
                SetUp.QuestionnaireRepositoryWithOneQuestionnaire(
                    Create.Entity.QuestionnaireDocument(questionnaireIdentity.QuestionnaireId));
            
            var questionnaire = Create.AggregateRoot.Questionnaire(
                questionnaireStorage:questionnaireStorage,
                questionnaireBrowseItemStorage: questionnaireBrowseItemStorage);

            var questionnaireException = Assert.Throws<QuestionnaireException>(() =>
               questionnaire.CloneQuestionnaire(Create.Command.CloneQuestionnaire(
                   questionnaireIdentity: questionnaireIdentity, newTitle: invalidTitle)));

            questionnaireException.Message.ToLower().ToSeparateWords().Should().Contain("title", "not", "allowed");
        }
    }
}
