using System;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireNameValidatorTests
{
    internal class when_validating_ImportFromDesigner_command_and_other_deleted_questionnaire_with_such_title_exists
    {
        [NUnit.Framework.Test] public void should_not_throw_exception () {
            command = Create.Command.ImportFromDesigner(title: title, questionnaireId: importedQuestionnaireId);

            var questionnaireBrowseItemStorage = Create.Storage.InMemoryPlainStorage<QuestionnaireBrowseItem>();

            var deletedQuestionnaireBrowseItem = Create.Entity.QuestionnaireBrowseItem(title: title,
                questionnaireId: differentQuestionnaireId, deleted: true);

            questionnaireBrowseItemStorage.Store(deletedQuestionnaireBrowseItem, differentQuestionnaireId);

            validator = Create.Service.QuestionnaireNameValidator(questionnaireBrowseItemStorage: questionnaireBrowseItemStorage);

            validator.Validate(null, command);
        }

        private static QuestionnaireImportValidator validator;
        private static ImportFromDesigner command;
        private static string title = "The Title";
        private static Guid importedQuestionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid differentQuestionnaireId = Guid.Parse("22222222222222222222222222222222");
    }
}
