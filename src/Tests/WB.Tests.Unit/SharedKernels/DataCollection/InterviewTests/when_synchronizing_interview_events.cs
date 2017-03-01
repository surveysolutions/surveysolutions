﻿using System;
using System.Linq;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_synchronizing_interview_events : InterviewTestsContext
    {
        Establish context = () =>
        {
            eventContext = new EventContext();
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.Version == questionnaireVersion);

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
            interview.Apply(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, ""));
            interview.Apply(new InterviewerAssigned(userId, userId, DateTime.Now));
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

         Because of = () =>
            interview.SynchronizeInterviewEvents(userId, questionnaireId, questionnaireVersion,
                InterviewStatus.Completed, eventsToPublish, false);

        It should_raise_all_passed_events = () =>
            eventsToPublish.All(x => eventContext.Events.Any(publishedEvent => publishedEvent.Payload.Equals(x)));

        private static EventContext eventContext;
        private static Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        private static Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static long questionnaireVersion = 18;

        private static Interview interview;

        private static IEvent[] eventsToPublish = new IEvent[] {new AnswersDeclaredInvalid(new Identity[0]), new GroupsEnabled(new Identity[0])};
    }
}
