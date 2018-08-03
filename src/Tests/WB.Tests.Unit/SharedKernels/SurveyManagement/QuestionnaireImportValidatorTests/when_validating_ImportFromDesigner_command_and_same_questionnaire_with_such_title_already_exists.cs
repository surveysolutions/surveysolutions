using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireImportValidatorTests
{
    internal class when_validating_ImportFromDesigner_command_and_same_questionnaire_with_such_title_already_exists
    {
        [NUnit.Framework.Test] public void should_not_throw_exception () {
            command = Create.Command.ImportFromDesigner(title: title, questionnaireId: importedQuestionnaireId);

            var questionnaireBrowseItemStorage = Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>();

            Mock.Get(questionnaireBrowseItemStorage)
                .Setup(reader => reader.Query(Moq.It.IsAny<Func<IQueryable<QuestionnaireBrowseItem>, List<QuestionnaireBrowseItem>>>()))
                .Returns<Func<IQueryable<QuestionnaireBrowseItem>, List<QuestionnaireBrowseItem>>>(query => query.Invoke(new[]
                {
                    Create.Entity.QuestionnaireBrowseItem(title: title, questionnaireId: importedQuestionnaireId),
                }.AsQueryable()));

            validator = Create.Service.QuestionnaireNameValidator(questionnaireBrowseItemStorage: questionnaireBrowseItemStorage);

            Assert.DoesNotThrow(() => validator.Validate(null, command));
        }

        private static QuestionnaireImportValidator validator;
        private static ImportFromDesigner command;
        private static string title = "The Title";
        private static Guid importedQuestionnaireId = Guid.Parse("11111111111111111111111111111111");
    }
}
