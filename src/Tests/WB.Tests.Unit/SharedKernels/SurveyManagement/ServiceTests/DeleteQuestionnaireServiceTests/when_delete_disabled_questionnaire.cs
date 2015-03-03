using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DeleteQuestionnaireTemplate;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DeleteQuestionnaireServiceTests
{
    internal class when_delete_disabled_questionnaire : DeleteQuestionnaireServiceTestContext
    {
        Establish context = () =>
        {
            commandServiceMock = new Mock<ICommandService>();
            deleteQuestionnaireService = CreateDeleteQuestionnaireService(commandService: commandServiceMock.Object,
                questionnaireBrowseItemStorage:
                    Mock.Of<IReadSideRepositoryReader<QuestionnaireBrowseItem>>(
                        _ => _.GetById(Moq.It.IsAny<string>()) == new QuestionnaireBrowseItem() {Disabled = true}));
        };

        Because of = () =>
                deleteQuestionnaireService.DeleteQuestionnaire(questionnaireId, questionnaireVersion, userId);

        It should_never_execute_DisableQuestionnaire_Command = () =>
            commandServiceMock.Verify(x => x.Execute(Moq.It.IsAny<DisableQuestionnaire>(), Moq.It.IsAny<string>()), Times.Never);

        private static DeleteQuestionnaireService deleteQuestionnaireService;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 5;
        private static Guid userId = Guid.Parse("22222222222222222222222222222222");
        private static Mock<ICommandService> commandServiceMock;
    }
}
