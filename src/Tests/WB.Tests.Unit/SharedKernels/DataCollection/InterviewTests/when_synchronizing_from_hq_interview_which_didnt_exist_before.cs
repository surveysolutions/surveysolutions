﻿using System;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_synchronizing_from_hq_interview_which_didnt_exist_before : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(), _ => true);

            interview = Create.Other.Interview(questionnaireRepository: questionnaireRepository);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => interview.SynchronizeInterviewFromHeadquarters(interview.EventSourceId, Guid.NewGuid(), Guid.NewGuid(), Create.Entity.InterviewSynchronizationDto(), DateTime.Now);

        It should_raise_InterviewCreated_event = () =>
           eventContext.ShouldContainEvent<InterviewCreated>();

        private static Interview interview;

        private static EventContext eventContext;
    }
}