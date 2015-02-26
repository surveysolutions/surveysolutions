using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DeleteQuestionnaireTemplate;
using WB.Core.SharedKernels.SurveyManagement.Services.DeleteQuestionnaireTemplate;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DeleteQuestionnaireServiceTests
{
    internal class when_delete_questionnaire_and_one_interview_which_thows_exception : DeleteQuestionnaireServiceTestContext
    {
        Establish context = () =>
        {
            commandServiceMock = new Mock<ICommandService>();
            commandServiceMock.Setup(
                x =>
                    x.Execute(Moq.It.Is<HardDeleteInterview>(_ => _.InterviewId == interviewId && _.UserId == userId),
                        Moq.It.IsAny<string>())).Throws<NullReferenceException>();

            plainQuestionnaireRepository = new Mock<IPlainQuestionnaireRepository>();
            interviewsToDeleteFactoryMock = new Mock<IInterviewsToDeleteFactory>();

            var interviewQueue = new Queue<List<InterviewSummary>>();
            interviewQueue.Enqueue(new List<InterviewSummary>() { new InterviewSummary() { InterviewId = interviewId } });
            interviewQueue.Enqueue(new List<InterviewSummary>());
            interviewsToDeleteFactoryMock.Setup(x => x.Load(questionnaireId, questionnaireVersion))
                .Returns(interviewQueue.Dequeue);

            deleteQuestionnaireService = CreateDeleteQuestionnaireService(commandService: commandServiceMock.Object,
                interviewsToDeleteFactory: interviewsToDeleteFactoryMock.Object,
                plainQuestionnaireRepository: plainQuestionnaireRepository.Object,
                questionnaireBrowseItemStorage:
                    Mock.Of<IReadSideRepositoryReader<QuestionnaireBrowseItem>>(
                        _ => _.GetById(Moq.It.IsAny<string>()) == new QuestionnaireBrowseItem() { Disabled = false, QuestionnaireId = questionnaireId, Version = questionnaireVersion }));
        };

        Because of = () =>
               deleteQuestionnaireService.DeleteQuestionnaire(questionnaireId, questionnaireVersion, userId).Wait();

        It should_once_execute_DisableQuestionnaire_Command = () =>
            commandServiceMock.Verify(x => x.Execute(Moq.It.Is<DisableQuestionnaire>(_ => _.QuestionnaireId == questionnaireId && _.QuestionnaireVersion == questionnaireVersion && _.ResponsibleId == userId), Moq.It.IsAny<string>()), Times.Once);

        It should_never_execute_DeleteQuestionnaire_Command = () =>
            commandServiceMock.Verify(x => x.Execute(Moq.It.Is<DeleteQuestionnaire>(_ => _.QuestionnaireId == questionnaireId && _.QuestionnaireVersion == questionnaireVersion && _.ResponsibleId == userId), Moq.It.IsAny<string>()), Times.Never);


        It should_once_call_DeleteQuestionnaireDocument = () =>
            plainQuestionnaireRepository.Verify(x => x.DeleteQuestionnaireDocument(questionnaireId, questionnaireVersion), Times.Once);


        private static DeleteQuestionnaireService deleteQuestionnaireService;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 5;
        private static Guid userId = Guid.Parse("22222222222222222222222222222222");
        private static Guid interviewId = Guid.Parse("33333333333333333333333333333333");
        private static Mock<ICommandService> commandServiceMock;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepository;
        private static Mock<IInterviewsToDeleteFactory> interviewsToDeleteFactoryMock;
    }
}
