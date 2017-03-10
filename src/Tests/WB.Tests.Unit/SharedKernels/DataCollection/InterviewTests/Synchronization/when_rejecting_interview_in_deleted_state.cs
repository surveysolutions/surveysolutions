﻿using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Synchronization
{
    internal class when_rejecting_interview_from_HQ_in_deleted_state : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(), _ => true);

            interview = CreateInterview(questionnaireRepository: questionnaireRepository);
            interview.Apply(new InterviewStatusChanged(InterviewStatus.Deleted, null));

            eventContext = new EventContext();
        }; 

        Because of = () => interview.RejectInterviewFromHeadquarters(userId, Guid.NewGuid(), Guid.NewGuid(), new InterviewSynchronizationDto(), DateTime.Now);

        It should_restore_interview = () => eventContext.ShouldContainEvent<InterviewRestored>(@event => @event.UserId == userId);

        static Interview interview;
        static EventContext eventContext;
        static Guid userId = Guid.NewGuid();
    }
}