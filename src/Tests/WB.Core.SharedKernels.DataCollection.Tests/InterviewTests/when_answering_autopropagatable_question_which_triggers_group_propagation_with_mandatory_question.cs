﻿using System;
using System.Collections.Generic;
using System.Linq;
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

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    [Ignore("C#, KP-4386 Rosters")]
    internal class when_answering_autopropagatable_question_which_triggers_group_propagation_with_mandatory_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            propagatedGroupId = Guid.Parse("11111111111111111111111111111111");

            mandatoryQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            questionWhichIsForcesPropagationId = Guid.Parse("22222222222222222222222222222222");


            var questionnaire = Mock.Of<IQuestionnaire>(_

                                                        => _.HasQuestion(questionWhichIsForcesPropagationId) == true
                                                        && _.GetQuestionType(questionWhichIsForcesPropagationId) == QuestionType.AutoPropagate
                                                        && _.IsQuestionInteger(questionWhichIsForcesPropagationId) == true
                                                        && _.GetRosterGroupsByRosterSizeQuestion(questionWhichIsForcesPropagationId) == new Guid[] { propagatedGroupId }

                                                        && _.HasGroup(propagatedGroupId) == true
                                                        && _.GetRosterLevelForGroup(propagatedGroupId) == 1
                                                        //&& _.GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(propagatedGroupId) == new Guid[] { propagatedGroupId }
                                                        && _.GetRostersFromTopToSpecifiedGroup(propagatedGroupId) == new Guid[] { propagatedGroupId }

                                                        && _.GetRosterLevelForQuestion(mandatoryQuestionId)==1
                                                        && _.GetRostersFromTopToSpecifiedQuestion(mandatoryQuestionId) == new Guid[] { propagatedGroupId }
                                                        //&& _.GetUnderlyingMandatoryQuestions(propagatedGroupId) == new Guid[]{mandatoryQuestionId}
                                                        );

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            interview = CreateInterview(questionnaireId: questionnaireId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerNumericIntegerQuestion(userId, questionWhichIsForcesPropagationId, new decimal[] { }, DateTime.Now, 1);


        It should_not_raise_AnswersDeclaredValid_event = () =>
             eventContext.ShouldNotContainEvent<AnswersDeclaredValid>(@event
                 => @event.Questions.Any(question => question.Id == mandatoryQuestionId));

        It should_raise_AnswersDeclaredInvalid_event = () =>
            eventContext.ShouldContainEvent<AnswersDeclaredInvalid>(@event
                 => @event.Questions.Any(question => question.Id == mandatoryQuestionId));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIsForcesPropagationId;
        private static Guid propagatedGroupId;
        private static Guid mandatoryQuestionId;
    }
}
