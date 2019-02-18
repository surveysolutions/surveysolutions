using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.Implementation;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireValidatorTests
{
    internal class when_validating_CloneQuestionnaire_command_and_different_questionnaire_with_such_title_already_exists
    {
        [Test]
        public void should_throw_QuestionnaireException()
        {
            Guid importedQuestionnaireId = Id.g1;
            Guid differentQuestionnaireId = Id.g2;

            var newTitle = "title";
            var command = Create.Command.CloneQuestionnaire(newTitle: newTitle, questionnaireId: importedQuestionnaireId);

            var questionnaireBrowseItemStorage = new InMemoryPlainStorageAccessor<QuestionnaireBrowseItem>();
            questionnaireBrowseItemStorage.Store(Create.Entity.QuestionnaireBrowseItem(title: newTitle, questionnaireId: differentQuestionnaireId), "id");

            var validator = Create.Service.QuestionnaireNameValidator(questionnaireBrowseItemStorage: questionnaireBrowseItemStorage);

            // Act
            var exception = Assert.Throws<QuestionnaireException>(() =>
                validator.Validate(null, command));

            // Assert
            exception.Message.Should().Contain(newTitle);
        }
    }
}
