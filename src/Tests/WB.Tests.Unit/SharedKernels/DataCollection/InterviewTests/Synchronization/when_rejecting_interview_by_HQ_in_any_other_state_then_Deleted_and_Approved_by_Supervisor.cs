using System;
using System.Linq;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Synchronization
{
    internal class when_rejecting_interview_by_HQ_in_any_other_state_then_Deleted_and_Approved_by_Supervisor_and_Completed : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(), _ => true);

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
        { InterviewStatus.Deleted, InterviewStatus.ApprovedBySupervisor, InterviewStatus.Completed };

        private static IQuestionnaireStorage questionnaireRepository;
    }
}
