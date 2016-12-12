using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_on_multy_option_question_increases_roster_size : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            rosterGroupId = Guid.Parse("11111111111111111111111111111111");

            multyOptionRosterSizeId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId: multyOptionRosterSizeId, answers: new [] { 1, 2, 3 }),

                Create.Entity.Roster(rosterId: rosterGroupId, rosterSizeQuestionId: multyOptionRosterSizeId),
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            var synchronizationDto = Create.Entity.InterviewSynchronizationDto(interviewId: interview.EventSourceId,
                status: InterviewStatus.RejectedBySupervisor,
                userId: userId,
                questionnaireId: questionnaireId,
                questionnaireVersion: questionnaire.Version,
                answers: new[] { Create.Entity.AnsweredQuestionSynchronizationDto(multyOptionRosterSizeId, new decimal[] { }, new decimal[] { 1 }) },
                rosterGroupInstances: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>
                {
                    {
                        new InterviewItemId(rosterGroupId, new decimal[] {}),
                        new[]
                        {
                            new RosterSynchronizationDto(rosterGroupId, new decimal[] {}, 1.0m, null, string.Empty),
                        }
                    }
                },
                wasCompleted: true);

            interview.SynchronizeInterview(
                userId,
                 synchronizationDto);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerMultipleOptionsQuestion(userId, multyOptionRosterSizeId, new decimal[] { }, DateTime.Now, new []{1,2});

        It should_raise_RosterInstancesAdded_event = () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterGroupId && instance.RosterInstanceId == 2));

        It should_not_raise_RosterInstancesAdded_event = () =>
            eventContext.ShouldNotContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterGroupId && instance.RosterInstanceId == 1));

        It should_not_raise_RosterInstancesRemoved_event = () =>
            eventContext.ShouldNotContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterGroupId && instance.RosterInstanceId == 1));

        private static EventContext eventContext;
        private static StatefulInterview interview;
        private static Guid userId;
        private static Guid multyOptionRosterSizeId;
        private static Guid rosterGroupId;
    }
}
