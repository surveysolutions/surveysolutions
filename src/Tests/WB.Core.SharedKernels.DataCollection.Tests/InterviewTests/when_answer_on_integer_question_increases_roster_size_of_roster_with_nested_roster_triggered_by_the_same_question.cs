using System;
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
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_answer_on_integer_question_increases_roster_size_of_roster_with_nested_roster_triggered_by_the_same_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            nestedRosterGroupId = Guid.Parse("11111111111111111111111111111111");
            parentRosterGroupId = Guid.Parse("21111111111111111111111111111111");
            questionWhichIncreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");


            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.HasQuestion(questionWhichIncreasesRosterSizeId) == true
                    && _.GetQuestionType(questionWhichIncreasesRosterSizeId) == QuestionType.Numeric
                    && _.IsQuestionInteger(questionWhichIncreasesRosterSizeId) == true
                    &&
                    _.GetRosterGroupsByRosterSizeQuestion(questionWhichIncreasesRosterSizeId) ==
                        new[] { parentRosterGroupId, nestedRosterGroupId }

                    && _.HasGroup(nestedRosterGroupId) == true
                    && _.GetRosterLevelForGroup(nestedRosterGroupId) == 2
                    && _.GetRosterLevelForGroup(parentRosterGroupId) == 1

                    && _.GetRostersFromTopToSpecifiedGroup(nestedRosterGroupId) == new[] { parentRosterGroupId, nestedRosterGroupId }
                    && _.GetRostersFromTopToSpecifiedGroup(parentRosterGroupId) == new[] { parentRosterGroupId }
                    && _.GetRostersFromTopToSpecifiedQuestion(questionWhichIncreasesRosterSizeId) == new Guid[0]

                    && _.GetNestedRostersOfGroupById(parentRosterGroupId) == new[] { nestedRosterGroupId }
                    && _.GetRosterSizeQuestion(nestedRosterGroupId) == questionWhichIncreasesRosterSizeId);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            interview = CreateInterview(questionnaireId: questionnaireId);

            interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[0], DateTime.Now, 2);
        //    interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[0], DateTime.Now, 1);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[0], DateTime.Now, 1);

        It should_raise_RosterInstancesRemoved_event_which_contains_1_last_row_of_parentRoster = () =>
            eventContext.ShouldContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances.Count(instance => instance.GroupId == parentRosterGroupId && instance.RosterInstanceId == 1 && instance.OuterRosterVector.Length == 0) == 1);

        It should_raise_RosterInstancesRemoved_event_which_contains_first_row_of_nestedRoster_in_last_row_of_parentRoster = () =>
           eventContext.ShouldContainEvent<RosterInstancesRemoved>(@event
               => @event.Instances.Count(instance => instance.GroupId == nestedRosterGroupId && instance.RosterInstanceId == 0 && instance.OuterRosterVector.SequenceEqual(new decimal[] { 1 })) == 1);

        It should_raise_RosterInstancesRemoved_event_which_contains_second_row_of_nestedRoster_in_last_row_of_parentRoster = () =>
           eventContext.ShouldContainEvent<RosterInstancesRemoved>(@event
               => @event.Instances.Count(instance => instance.GroupId == nestedRosterGroupId && instance.RosterInstanceId == 1 && instance.OuterRosterVector.SequenceEqual(new decimal[] { 1 })) == 1);
  

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIncreasesRosterSizeId;
        private static Guid nestedRosterGroupId;
        private static Guid parentRosterGroupId;
    }
}
