using System;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DeleteQuestionnaireServiceTests
{
    internal class when_delete_disabled_questionnaire : DeleteQuestionnaireServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            commandServiceMock = new Mock<ICommandService>();
            deleteQuestionnaireService = CreateDeleteQuestionnaireService(commandService: commandServiceMock.Object,
                questionnaireBrowseItemStorage:
                    Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(
                        _ => _.GetById(Moq.It.IsAny<string>()) == new QuestionnaireBrowseItem() {Disabled = true}));
            BecauseOf();
        }

        public void BecauseOf() =>
                deleteQuestionnaireService.DisableQuestionnaire(questionnaireId, questionnaireVersion, userId);

        [NUnit.Framework.Test] public void should_never_execute_DisableQuestionnaire_Command () =>
            commandServiceMock.Verify(x => x.Execute(Moq.It.IsAny<DisableQuestionnaire>(), Moq.It.IsAny<string>()), Times.Never);

        private static DeleteQuestionnaireService deleteQuestionnaireService;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 5;
        private static Guid userId = Guid.Parse("22222222222222222222222222222222");
        private static Mock<ICommandService> commandServiceMock;
    }
}
