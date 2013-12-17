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
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_answer_on_integer_question_decreases_roster_size : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            rosterGroupId = Guid.Parse("11111111111111111111111111111111");

            questionWhichDecreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");


            var questionnaire = Mock.Of<IQuestionnaire>(_

                                                        => _.HasQuestion(questionWhichDecreasesRosterSizeId) == true
                                                        && _.GetQuestionType(questionWhichDecreasesRosterSizeId) == QuestionType.Numeric
                                                        && _.IsQuestionInteger(questionWhichDecreasesRosterSizeId) == true
                                                        && _.IsQuestionInteger(questionWhichDecreasesRosterSizeId) == true
                                                        && _.GetRosterGroupsByRosterSizeQuestion(questionWhichDecreasesRosterSizeId) == new Guid[] { rosterGroupId }

                                                        && _.HasGroup(rosterGroupId) == true
                                                        && _.GetRosterLevelForGroup(rosterGroupId) == 1
                                                        && _.GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(rosterGroupId) == new Guid[] { rosterGroupId }
                                                        && _.GetParentRosterGroupsAndGroupItselfIfRosterStartingFromTop(rosterGroupId) == new Guid[] { rosterGroupId });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            interview = CreateInterview(questionnaireId: questionnaireId);
            interview.AnswerNumericIntegerQuestion(userId, questionWhichDecreasesRosterSizeId, new decimal[] { }, DateTime.Now, 1);
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerNumericIntegerQuestion(userId, questionWhichDecreasesRosterSizeId, new decimal[] { }, DateTime.Now, 0);

        It should_raise_RosterRowRemoved_event = () =>
          eventContext.ShouldContainEvent<RosterRowRemoved>(@event
              => @event.GroupId == rosterGroupId && @event.RosterInstanceId == 0);

        It should_not_raise_RosterRowAdded_event = () =>
            eventContext.ShouldNotContainEvent<RosterRowAdded>(@event
                => @event.GroupId == rosterGroupId);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichDecreasesRosterSizeId;
        private static Guid rosterGroupId;
    }
}
