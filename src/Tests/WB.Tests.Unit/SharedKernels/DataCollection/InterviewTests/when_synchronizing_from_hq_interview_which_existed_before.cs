using System;
using System.Collections.Generic;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_synchronizing_from_hq_interview_which_existed_before : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.NewGuid();

            var questionnaire = Mock.Of<IQuestionnaire>();

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                =>
                repository.GetHistoricalQuestionnaire(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()) == questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            interview = CreateInterview(questionnaireId: questionnaireId);
        };

        Because of = () => exception = Catch.Only<InterviewException>(() => interview.SynchronizeInterviewFromHeadquarters(interview.EventSourceId, Guid.NewGuid(), Guid.NewGuid(), Create.InterviewSynchronizationDto(), DateTime.Now));

        It should_raise_InterviewException = () =>
           exception.ShouldNotBeNull();

        private static Interview interview;
        private static InterviewException exception;
    }
}