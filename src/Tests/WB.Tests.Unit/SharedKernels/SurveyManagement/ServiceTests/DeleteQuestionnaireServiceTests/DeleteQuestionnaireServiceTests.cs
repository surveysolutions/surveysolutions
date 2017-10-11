using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NSubstitute;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DeleteQuestionnaireServiceTests
{
    internal class DeleteQuestionnaireServiceTests : DeleteQuestionnaireServiceTestContext
    {
        [Test]
        public async Task when_delete_questionnaire_and_one_interview()
        {
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            long questionnaireVersion = 5;
            Guid userId = Guid.Parse("22222222222222222222222222222222");
            Guid interviewId = Guid.Parse("33333333333333333333333333333333");

            var commandServiceMock = Substitute.For<ICommandService>();
            var plainQuestionnaireRepository = new Mock<IQuestionnaireStorage>();
            var interviewsToDeleteFactoryMock = new Mock<IInterviewsToDeleteFactory>();

            var interviewQueue = new Queue<List<InterviewSummary>>();
            interviewQueue.Enqueue(new List<InterviewSummary>() {new InterviewSummary() {InterviewId = interviewId}});
            interviewQueue.Enqueue(new List<InterviewSummary>());
            interviewsToDeleteFactoryMock.Setup(x => x.Load(questionnaireId, questionnaireVersion))
                .Returns(interviewQueue.Dequeue);

            var deleteQuestionnaireService = CreateDeleteQuestionnaireService(commandService: commandServiceMock,
                interviewsToDeleteFactory: interviewsToDeleteFactoryMock.Object,
                questionnaireStorage: plainQuestionnaireRepository.Object,
                questionnaireBrowseItemStorage:
                    Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(
                        _ =>
                            _.GetById(Moq.It.IsAny<string>()) ==
                            new QuestionnaireBrowseItem
                            {
                                Disabled = false,
                                QuestionnaireId = questionnaireId,
                                Version = questionnaireVersion
                            }));


            await deleteQuestionnaireService.DeleteQuestionnaire(questionnaireId, questionnaireVersion, userId);

            commandServiceMock.Received(1).Execute(
                Arg.Is<DisableQuestionnaire>(
                    _ =>
                        _.QuestionnaireId == questionnaireId && _.QuestionnaireVersion == questionnaireVersion &&
                        _.ResponsibleId == userId), Arg.Any<string>());

            commandServiceMock.Received(1)
                .Execute(Arg.Is<DeleteQuestionnaire>(
                    _ =>
                        _.QuestionnaireId == questionnaireId && _.QuestionnaireVersion == questionnaireVersion &&
                        _.ResponsibleId == userId), Arg.Any<string>());

            commandServiceMock.Received(1)
                .Execute(Arg.Is<HardDeleteInterview>(_ => _.InterviewId == interviewId && _.UserId == userId),
                    Moq.It.IsAny<string>());
        }
    }
}
