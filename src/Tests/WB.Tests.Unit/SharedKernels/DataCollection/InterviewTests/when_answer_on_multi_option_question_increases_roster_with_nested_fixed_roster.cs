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
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_on_multi_option_question_increases_roster_with_nested_fixed_roster : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            fixedRosterGroupId = Guid.Parse("11111111111111111111111111111111");
            rosterGroupId = Guid.Parse("21111111111111111111111111111111");
            questionWhichIncreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");


            var questionnaire = Mock.Of<IQuestionnaire>(_

                => _.HasQuestion(questionWhichIncreasesRosterSizeId) == true
                   && _.GetQuestionType(questionWhichIncreasesRosterSizeId) == QuestionType.MultyOption
                   && _.GetRosterGroupsByRosterSizeQuestion(questionWhichIncreasesRosterSizeId) == new[] { rosterGroupId }
                   && _.GetAnswerOptionsAsValues(questionWhichIncreasesRosterSizeId) == new decimal[] { 0 }

                   && _.HasGroup(fixedRosterGroupId) == true
                   && _.GetRosterLevelForGroup(fixedRosterGroupId) == 2
                   && _.GetRosterLevelForGroup(rosterGroupId) == 1

                   &&
                   _.GetRostersFromTopToSpecifiedGroup(fixedRosterGroupId) == new[] { rosterGroupId, fixedRosterGroupId }
                   && _.GetFixedRosterTitles(fixedRosterGroupId) == new[] { new FixedRosterTitle(1, "t1") }
                   && _.GetRostersFromTopToSpecifiedGroup(rosterGroupId) == new[] { rosterGroupId }
                   && _.GetRostersFromTopToSpecifiedQuestion(questionWhichIncreasesRosterSizeId) == new Guid[0]

                   && _.GetNestedRostersOfGroupById(rosterGroupId) == new[] { fixedRosterGroupId }
                   && _.GetFixedRosterGroups(rosterGroupId) == new[] { fixedRosterGroupId }
                   && _.GetRosterLevelForGroup(fixedRosterGroupId) == 2
                   &&
                   _.GetFixedRosterTitles(fixedRosterGroupId) ==
                   new[] { new FixedRosterTitle(0, title1), new FixedRosterTitle(1, title2) }
                );

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.AnswerMultipleOptionsQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[0], DateTime.Now,
                new decimal[] { 0 });

        It should_raise_RosterInstancesAdded_for_first_row_of_first_level_roster = () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                =>
                @event.Instances.Any(
                    instance =>
                        instance.GroupId == rosterGroupId && instance.RosterInstanceId == 0 &&
                        instance.OuterRosterVector.Length == 0));

        It should_raise_RosterInstancesAdded_for_first_row_of_fixed_roster_by_first_row = () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                =>
                @event.Instances.Any(
                    instance =>
                        instance.GroupId == fixedRosterGroupId && instance.RosterInstanceId == 0 &&
                        instance.OuterRosterVector.SequenceEqual(new decimal[] { 0 })));

        It should_rise_RosterRowsTitleChanged_event_with_title_of_first_row_of_fixed_roster_by_first_row = () =>
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event
                => @event.ChangedInstances.Any(row =>
                    row.RosterInstance.GroupId == fixedRosterGroupId && row.RosterInstance.RosterInstanceId == 0 &&
                    row.RosterInstance.OuterRosterVector.Length == 1 &&
                    row.RosterInstance.OuterRosterVector[0] == 0 && row.Title == title1));

        It should_rise_RosterRowsTitleChanged_event_with_title_of_first_row_of_fixed_roster_by_second_row =
            () =>
                eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event
                    => @event.ChangedInstances.Any(row =>
                        row.RosterInstance.GroupId == fixedRosterGroupId && row.RosterInstance.RosterInstanceId == 0 &&
                        row.RosterInstance.OuterRosterVector.Length == 1 && row.RosterInstance.OuterRosterVector[0] == 0 &&
                        row.Title == title1));

        It should_raise_RosterInstancesAdded_for_second_row_of_fixed_roster_by_first_row = () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                =>
                @event.Instances.Any(
                    instance =>
                        instance.GroupId == fixedRosterGroupId && instance.RosterInstanceId == 1 &&
                        instance.OuterRosterVector.SequenceEqual(new decimal[] { 0 })));

        It should_rise_RosterRowsTitleChanged_event_with_title_of_second_row_of_fixed_roster_by_first_row =
            () =>
                eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event
                    => @event.ChangedInstances.Any(row
                        =>
                        row.RosterInstance.GroupId == fixedRosterGroupId && row.RosterInstance.RosterInstanceId == 1 &&
                        row.RosterInstance.OuterRosterVector.SequenceEqual(new decimal[] { 0 }) && row.Title == title2));

        It should_raise_RosterRowsTitleChanged_event_with_title_of_first_nested_row = () =>
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event
                => @event.ChangedInstances.Any(row
                    =>
                    row.Title == "t1" && row.RosterInstance.GroupId == fixedRosterGroupId &&
                    row.RosterInstance.RosterInstanceId == 0 &&
                    row.RosterInstance.OuterRosterVector.SequenceEqual(new decimal[] { 0 })));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIncreasesRosterSizeId;
        private static Guid fixedRosterGroupId;
        private static Guid rosterGroupId;
        private static string title1 = "t1";
        private static string title2 = "t2";
    }
}
