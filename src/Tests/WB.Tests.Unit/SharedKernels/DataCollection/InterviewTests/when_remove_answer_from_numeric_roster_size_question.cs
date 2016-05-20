﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_remove_answer_from_numeric_roster_size_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            rosterId = Guid.Parse("21111111111111111111111111111111");
            questionWhichIncreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");

            QuestionnaireDocument questionnaire = Create.Other.QuestionnaireDocument(id: questionnaireId,
                children: new IComposite[]
                {
                    Create.Other.NumericIntegerQuestion(id: questionWhichIncreasesRosterSizeId, variable: "num"),
                    Create.Other.Roster(rosterId: rosterId, variable: "ros",
                        rosterSizeQuestionId: questionWhichIncreasesRosterSizeId,
                        rosterSizeSourceType: RosterSizeSourceType.Question)
                });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, new PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[0],
                DateTime.Now, 2);
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.RemoveAnswer(questionWhichIncreasesRosterSizeId, new decimal[0], userId, DateTime.Now);

        It should_raise_AnswerRemoved_event_for_first_row = () =>
            eventContext.ShouldContainEvent<AnswerRemoved>(@event
                => @event.QuestionId == questionWhichIncreasesRosterSizeId && !@event.RosterVector.Any());

        It should_not_raise_RosterInstancesAdded_event = () =>
            eventContext.ShouldNotContainEvent<RosterInstancesAdded>();

        It should_raise_RosterInstancesRemoved_event_for_first_roster_row = () =>
            eventContext.ShouldContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterId && instance.RosterInstanceId==0));

        It should_raise_RosterInstancesRemoved_event_for_second_roster_row = () =>
          eventContext.ShouldContainEvent<RosterInstancesRemoved>(@event
              => @event.Instances.Any(instance => instance.GroupId == rosterId && instance.RosterInstanceId == 1));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIncreasesRosterSizeId;
        private static Guid rosterId;
    }
}