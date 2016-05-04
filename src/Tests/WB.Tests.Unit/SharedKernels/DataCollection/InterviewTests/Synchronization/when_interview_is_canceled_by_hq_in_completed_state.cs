using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Synchronization
{
    internal class when_interview_is_canceled_by_hq_in_completed_state : InterviewTestsContext
    {
        Establish context = () =>
        {
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(), _ => true);

            interview = CreateInterview(userId: userId, questionnaireRepository: questionnaireRepository);
            interview.Apply(new InterviewStatusChanged(InterviewStatus.Completed, null));
       
            eventContext = new EventContext();
        };

        Because of = () => interview.CancelByHQSynchronization(userId);

        It should_not_delete_interview = () => eventContext.ShouldNotContainEvent<InterviewDeleted>();
        It should_not_change_status_of_the_interview = () => eventContext.ShouldNotContainEvent<InterviewStatusChanged>(x => x.Status == InterviewStatus.Deleted);

        static Interview interview;
        static Guid userId;
        static EventContext eventContext;
    }
}

