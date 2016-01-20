using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Synchronization
{
    internal class when_rejecting_interview_by_HQ_in_any_other_state_then_Deleted_and_Approved_by_Supervisor : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.GetHistoricalQuestionnaire(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()))
                .Returns(Mock.Of<IQuestionnaire>());

            questionnaireRepository = questionnaireRepositoryMock.Object;

            statuses =
                Enum.GetNames(typeof (InterviewStatus))
                    .Select(i => (InterviewStatus) Enum.Parse(typeof (InterviewStatus), i))
                    .Where(i => !statusToExclude.Contains(i))
                    .ToArray();

            eventContext = new EventContext();
        };

        Because of = () =>
        {
            foreach (var interviewStatus in statuses)
            {
                var interview = CreateInterview(questionnaireRepository: questionnaireRepository);
                interview.Apply(new InterviewStatusChanged(interviewStatus, null));
                try
                {
                    interview.HqReject(userId, "comment");
                }
                catch (InterviewException)
                {
                    exceptionCount++;
                }
            }

        };

        It should_not_rise_event_InterviewStatusChanged_with_status_RejectedByHeadquarters = () => eventContext.ShouldNotContainEvent<InterviewStatusChanged>(@event => @event.Status == InterviewStatus.RejectedByHeadquarters);

        It should_throw_exception_for_each_attempt_to_rejet = () => exceptionCount.ShouldEqual(statuses.Length); 

     
        static EventContext eventContext;
        static Guid userId = Guid.NewGuid();
        private static InterviewStatus[] statuses;
        private static int exceptionCount = 0;
        private static InterviewStatus[] statusToExclude = new InterviewStatus[]
        { InterviewStatus.Deleted, InterviewStatus.ApprovedBySupervisor };

        private static IPlainQuestionnaireRepository questionnaireRepository;
    }
}
