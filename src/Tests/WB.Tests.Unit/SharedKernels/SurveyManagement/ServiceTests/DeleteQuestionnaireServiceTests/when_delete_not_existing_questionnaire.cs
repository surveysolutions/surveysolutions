using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.DeleteQuestionnaireTemplate;
using WB.Core.Infrastructure.CommandBus;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DeleteQuestionnaireServiceTests
{
    internal class when_delete_not_existing_questionnaire : DeleteQuestionnaireServiceTestContext
    {
        [Test] public void should_not_throw_ArgumentException () {
            commandServiceMock=new Mock<ICommandService>();
            deleteQuestionnaireService = CreateDeleteQuestionnaireService(commandService: commandServiceMock.Object);

            Assert.DoesNotThrow(() =>
                deleteQuestionnaireService.DisableQuestionnaire(questionnaireId, questionnaireVersion, userId));
        }

        private static DeleteQuestionnaireService deleteQuestionnaireService;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 5;
        private static Guid userId = Guid.Parse("22222222222222222222222222222222");
        private static Mock<ICommandService> commandServiceMock;
    }
}
