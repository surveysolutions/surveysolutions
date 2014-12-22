using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.AnswersByVariableDenormalizerTests
{
    internal class when_removing_answer_and_where_is_no_interview_brief_info : AnswersByVariableDenormalizerTestContext
    {
        Establish context = () =>
        {
            answersByVariableStorageMock = new Mock<IReadSideRepositoryWriter<AnswersByVariableCollection>>();
            interviewBriefStorage = Mock.Of<IReadSideRepositoryWriter<InterviewBrief>>();
            denormalizer = CreateAnswersByVariableDenormalizer(interviewBriefStorage, answersByVariableStorage: answersByVariableStorageMock.Object);
            evnt = CreateAnswerRemovedEvent(interviewId);
        };

        Because of = () => denormalizer.Handle(evnt);

        It should_not_store_any_view = () =>
            answersByVariableStorageMock.Verify(x => x.Store(Moq.It.IsAny<AnswersByVariableCollection>(), Moq.It.IsAny<string>()), Times.Never());

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewId = Guid.Parse("22222222222222222222222222222222");
        private static Mock<IReadSideRepositoryWriter<AnswersByVariableCollection>> answersByVariableStorageMock;
        private static IReadSideRepositoryWriter<InterviewBrief> interviewBriefStorage;
        private static AnswersByVariableDenormalizer denormalizer;
        private static IPublishedEvent<AnswersRemoved> evnt;
    }
}