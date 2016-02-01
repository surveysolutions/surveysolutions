using System;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_synchronizing_from_hq_interview_which_existed_before : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.NewGuid();

            var questionnaire = Mock.Of<IQuestionnaire>();

            var questionnaireRepository = Mock.Of<IPlainQuestionnaireRepository>(repository
                => repository.GetHistoricalQuestionnaire(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()) == questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
        };

        Because of = () => exception = Catch.Only<InterviewException>(() => interview.SynchronizeInterviewFromHeadquarters(interview.EventSourceId, Guid.NewGuid(), Guid.NewGuid(), Create.InterviewSynchronizationDto(), DateTime.Now));

        It should_throw_InterviewException = () => exception.ShouldNotBeNull();

        private static Interview interview;
        private static InterviewException exception;
    }
}