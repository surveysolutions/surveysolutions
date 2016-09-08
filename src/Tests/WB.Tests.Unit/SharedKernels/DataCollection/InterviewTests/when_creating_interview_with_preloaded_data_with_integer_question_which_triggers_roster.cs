using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_with_preloaded_data_with_integer_question_which_triggers_roster : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            prefilledQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterGroupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCD");
            prefilledQuestionAnswer = 2;
            preloadedDataDto = new PreloadedDataDto(
                new[]
                {
                    new PreloadedLevelDto(new decimal[0], new Dictionary<Guid, object> { { prefilledQuestionId, prefilledQuestionAnswer } }),
                });
            answersTime = new DateTime(2013, 09, 01);

            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.GetQuestionType(prefilledQuestionId) == QuestionType.Numeric
                   && _.HasQuestion(prefilledQuestionId) == true
                   && _.IsQuestionInteger(prefilledQuestionId) == true
                   && _.GetRosterGroupsByRosterSizeQuestion(prefilledQuestionId) == new Guid[] { rosterGroupId }

                   && _.HasGroup(rosterGroupId) == true
                   && _.GetRosterLevelForGroup(rosterGroupId) == 1
                   && _.GetRostersFromTopToSpecifiedGroup(rosterGroupId) == new Guid[] {rosterGroupId});

            eventContext = new EventContext();

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
        };

        Because of = () =>
            interview.CreateInterviewWithPreloadedData(new CreateInterviewWithPreloadedData(interview.EventSourceId, userId, questionnaireId, 1, preloadedDataDto, answersTime, supervisorId, null));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_InterviewFromPreloadedDataCreated_event = () =>
            eventContext.ShouldContainEvent<InterviewFromPreloadedDataCreated>();

        It should_raise_NumericIntegerQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<NumericIntegerQuestionAnswered>(@event
                => @event.Answer == prefilledQuestionAnswer && @event.QuestionId == prefilledQuestionId);

        It should_raise_RosterInstancesAdded_event = () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.All(i => i.GroupId == rosterGroupId));


        private static EventContext eventContext;
        private static Guid userId;
        private static Guid questionnaireId;
        private static PreloadedDataDto preloadedDataDto;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Guid prefilledQuestionId;
        private static Guid rosterGroupId;
        private static int prefilledQuestionAnswer;
        private static Interview interview;
    }
}
