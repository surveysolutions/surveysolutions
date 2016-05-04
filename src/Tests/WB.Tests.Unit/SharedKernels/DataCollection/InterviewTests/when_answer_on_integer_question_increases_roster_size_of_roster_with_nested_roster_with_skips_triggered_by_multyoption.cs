﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_on_integer_question_increases_roster_size_of_roster_with_nested_roster_with_skips_triggered_by_multyoption : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            nestedRosterGroupId = Guid.Parse("11111111111111111111111111111111");
            parentRosterGroupId = Guid.Parse("21111111111111111111111111111111");
            questionWhichIncreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");
            multyOptionQuestionWhichIncreasesNestedRosterSizeId = Guid.Parse("31111111111111111111111111111111");

            var questionnaire = Mock.Of<IQuestionnaire>(_

                => _.HasQuestion(questionWhichIncreasesRosterSizeId) == true
                    && _.GetQuestionType(questionWhichIncreasesRosterSizeId) == QuestionType.Numeric
                    && _.IsQuestionInteger(questionWhichIncreasesRosterSizeId) == true
                    && _.GetRosterGroupsByRosterSizeQuestion(questionWhichIncreasesRosterSizeId) == new[] { parentRosterGroupId }

                    && _.HasQuestion(multyOptionQuestionWhichIncreasesNestedRosterSizeId) == true
                    && _.GetQuestionType(multyOptionQuestionWhichIncreasesNestedRosterSizeId) == QuestionType.MultyOption
                    &&
                    _.GetRosterGroupsByRosterSizeQuestion(multyOptionQuestionWhichIncreasesNestedRosterSizeId) ==
                        new[] { nestedRosterGroupId }
                    && _.GetRostersFromTopToSpecifiedQuestion(multyOptionQuestionWhichIncreasesNestedRosterSizeId) == new Guid[0]
                    && _.GetRosterLevelForQuestion(multyOptionQuestionWhichIncreasesNestedRosterSizeId) == 0
                    && _.GetAnswerOptionTitle(multyOptionQuestionWhichIncreasesNestedRosterSizeId, 5) == "t1"

                    && _.HasGroup(nestedRosterGroupId) == true
                    && _.GetRosterLevelForGroup(nestedRosterGroupId) == 2
                    && _.GetRosterLevelForGroup(parentRosterGroupId) == 1
                    //&& _.GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(nestedRosterGroupId) == new[] { parentRosterGroupId, nestedRosterGroupId }
                    && _.GetRostersFromTopToSpecifiedGroup(nestedRosterGroupId) == new[] { parentRosterGroupId, nestedRosterGroupId }
                    && _.GetRostersFromTopToSpecifiedGroup(parentRosterGroupId) == new[] { parentRosterGroupId }
                    && _.GetRostersFromTopToSpecifiedQuestion(questionWhichIncreasesRosterSizeId) == new Guid[0]

                    && _.GetNestedRostersOfGroupById(parentRosterGroupId) == new[] { nestedRosterGroupId }
                    && _.GetRosterSizeQuestion(nestedRosterGroupId) == multyOptionQuestionWhichIncreasesNestedRosterSizeId);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(new MultipleOptionsQuestionAnswered(userId, multyOptionQuestionWhichIncreasesNestedRosterSizeId, new decimal[0],
                DateTime.Now, new decimal[] { 5 }));
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[0], DateTime.Now, 1);

        It should_raise_RosterInstancesAdded_event_for_first_row = () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == parentRosterGroupId && instance.RosterInstanceId == 0 && instance.OuterRosterVector.Length == 0));

        It should_raise_RosterInstancesAdded_event_for_first_nested_row = () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == nestedRosterGroupId && instance.RosterInstanceId == 5 && instance.OuterRosterVector.SequenceEqual(new decimal[] { 0 })));

        It should_raise_RosterRowsTitleChanged_event_for_first_nested_row = () =>
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event
                => @event.ChangedInstances.Count(row =>
                    row.Title == "t1" && row.RosterInstance.GroupId == nestedRosterGroupId && row.RosterInstance.RosterInstanceId == 5 &&
                        row.RosterInstance.OuterRosterVector.SequenceEqual(new decimal[] { 0 })) == 1);

        It should_not_raise_RosterInstancesRemoved_event = () =>
            eventContext.ShouldNotContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances.Any(instance => instance.GroupId == nestedRosterGroupId));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIncreasesRosterSizeId;
        private static Guid multyOptionQuestionWhichIncreasesNestedRosterSizeId;
        private static Guid nestedRosterGroupId;
        private static Guid parentRosterGroupId;
    }
}
