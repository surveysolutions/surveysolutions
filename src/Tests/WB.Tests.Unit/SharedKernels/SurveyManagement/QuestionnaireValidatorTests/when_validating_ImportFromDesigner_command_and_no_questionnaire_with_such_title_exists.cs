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

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireValidatorTests
{
    internal class when_validating_ImportFromDesigner_command_and_no_questionnaire_with_such_title_exists
    {
        [Test] public void should_not_throw_exception () 
        {
            command = Create.Command.ImportFromDesigner(title: title, questionnaireId: importedQuestionnaireId);

            var questionnaireBrowseItemStorage = Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>();

            Mock.Get(questionnaireBrowseItemStorage)
                .Setup(reader => reader.Query(Moq.It.IsAny<Func<IQueryable<QuestionnaireBrowseItem>, List<QuestionnaireBrowseItem>>>()))
                .Returns<Func<IQueryable<QuestionnaireBrowseItem>, List<QuestionnaireBrowseItem>>>(query => query.Invoke(new[]
                {
                      Create.Entity.QuestionnaireBrowseItem(title: "different title", variable:null,  questionnaireId: differentQuestionnaireId),
                }.AsQueryable()));

            validator = Create.Service.QuestionnaireNameValidator(questionnaireBrowseItemStorage: questionnaireBrowseItemStorage);

            Assert.DoesNotThrow(() => validator.Validate(null, command));
        }

        private static QuestionnaireValidator validator;
        private static ImportFromDesigner command;
        private static string title = "The Title";
        private static Guid importedQuestionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid differentQuestionnaireId = Guid.Parse("22222222222222222222222222222222");
    }
}
