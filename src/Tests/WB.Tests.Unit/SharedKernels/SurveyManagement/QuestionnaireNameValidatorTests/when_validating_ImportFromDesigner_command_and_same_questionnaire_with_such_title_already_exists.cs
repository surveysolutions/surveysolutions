using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireNameValidatorTests
{
    internal class when_validating_ImportFromDesigner_command_and_same_questionnaire_with_such_title_already_exists
    {
        Establish context = () =>
        {
            command = Create.Command.ImportFromDesigner(title: title, questionnaireId: importedQuestionnaireId);

            IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> questionnaireBrowseItemStorage =
                Setup.QueryableReadSideRepositoryReaderByQueryResultType<QuestionnaireBrowseItem, List<QuestionnaireBrowseItem>>(new[]
                {
                    Create.QuestionnaireBrowseItem(title: title, questionnaireId: importedQuestionnaireId),
                });

            validator = Create.QuestionnaireNameValidator(questionnaireBrowseItemStorage: questionnaireBrowseItemStorage);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                validator.Validate(null, command));

        It should_not_throw_exception = () =>
            exception.ShouldEqual(null);

        private static QuestionnaireNameValidator validator;
        private static Exception exception;
        private static ImportFromDesigner command;
        private static string title = "The Title";
        private static Guid importedQuestionnaireId = Guid.Parse("11111111111111111111111111111111");
    }
}