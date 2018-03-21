using System;
using FluentAssertions;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireNameValidatorTests
{
    internal class when_validating_ImportFromDesigner_command_and_other_deleted_questionnaire_with_such_title_exists
    {
        Establish context = () =>
        {
            command = Create.Command.ImportFromDesigner(title: title, questionnaireId: importedQuestionnaireId);

            var questionnaireBrowseItemStorage = Create.Storage.InMemoryPlainStorage<QuestionnaireBrowseItem>();

            var deletedQuestionnaireBrowseItem = Create.Entity.QuestionnaireBrowseItem(title: title,
                questionnaireId: differentQuestionnaireId, deleted: true);

            questionnaireBrowseItemStorage.Store(deletedQuestionnaireBrowseItem, differentQuestionnaireId);

            validator = Create.Service.QuestionnaireNameValidator(questionnaireBrowseItemStorage: questionnaireBrowseItemStorage);
        };

        Because of = () => exception = Catch.Exception(() => validator.Validate(null, command));

        It should_not_throw_exception = () =>
            exception.Should().BeNull();

        private static QuestionnaireNameValidator validator;
        private static Exception exception;
        private static ImportFromDesigner command;
        private static string title = "The Title";
        private static Guid importedQuestionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid differentQuestionnaireId = Guid.Parse("22222222222222222222222222222222");
    }
}
