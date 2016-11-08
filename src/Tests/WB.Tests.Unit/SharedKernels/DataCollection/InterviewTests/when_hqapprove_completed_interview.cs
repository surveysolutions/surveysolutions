using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_hqapprove_completed_interview : InterviewTestsContext
    {
        private Establish context = () =>
        {
            userId = Guid.Parse("AAAA0000AAAA00000000AAAA0000AAAA");
            supervisorId = Guid.Parse("BBAA0000AAAA00000000AAAA0000AAAA");
            questionnaireId = Guid.Parse("33333333333333333333333333333333");

            questionId = Guid.Parse("43333333333333333333333333333333");

            var questionnaire = Mock.Of<IQuestionnaire>();

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            interview.AssignInterviewer(supervisorId, userId, DateTime.Now);
            interview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.Completed));

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.HqApprove(userId, string.Empty);

        It should_raise_two_events = () =>
            eventContext.Events.Count().ShouldEqual(2);

        It should_raise_InterviewApprovedByHQ_event = () =>
            eventContext.ShouldContainEvent<InterviewApprovedByHQ>(@event => @event.UserId == userId);

        It should_raise_InterviewStatusChanged_event = () =>
            eventContext.ShouldContainEvent<InterviewStatusChanged>(@event => @event.Status == InterviewStatus.ApprovedByHeadquarters);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static Guid userId;
        private static Guid supervisorId;

        private static Guid questionnaireId;
        private static Guid questionId;

        private static EventContext eventContext;
        private static Interview interview;
    }
}
