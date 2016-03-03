using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Commands;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireNameValidatorTests
{
    internal class when_validating_ImportFromDesigner_command_and_no_questionnaire_with_such_title_exists
    {
        Establish context = () =>
        {
            command = Create.Command.ImportFromDesigner(title: title, questionnaireId: importedQuestionnaireId);

            var questionnaireBrowseItemStorage = Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>();

            Mock.Get(questionnaireBrowseItemStorage)
                .Setup(reader => reader.Query(Moq.It.IsAny<Func<IQueryable<QuestionnaireBrowseItem>, List<QuestionnaireBrowseItem>>>()))
                .Returns<Func<IQueryable<QuestionnaireBrowseItem>, List<QuestionnaireBrowseItem>>>(query => query.Invoke(new[]
                {
                      Create.QuestionnaireBrowseItem(title: "different title", questionnaireId: differentQuestionnaireId),
                }.AsQueryable()));

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
        private static Guid differentQuestionnaireId = Guid.Parse("22222222222222222222222222222222");
    }
}