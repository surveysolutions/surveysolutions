using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_on_integer_question_special_negative_value_decreasing_roster_size : InterviewTestsContext
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            rosterGroupId = Guid.Parse("11111111111111111111111111111111");

            questionWhichDecreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                children: new IComposite[]
                {
                    Create.Entity.NumericQuestion(questionId: questionWhichDecreasesRosterSizeId, 
                        isInteger:true, 
                        options: Create.Entity.Options(-1) ),
                    Create.Entity.Roster(rosterId: rosterGroupId,
                        rosterSizeQuestionId: questionWhichDecreasesRosterSizeId),
                }));

            var questionnaireRepository =
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId,
                questionnaireRepository: questionnaireRepository);
            interview.AnswerNumericIntegerQuestion(userId, questionWhichDecreasesRosterSizeId, RosterVector.Empty, 
                DateTime.Now, 1);
            eventContext = new EventContext();

            //AA
            interview.AnswerNumericIntegerQuestion(userId, questionWhichDecreasesRosterSizeId, RosterVector.Empty,
                DateTime.Now, -1);
        }

        [Test]
        public void should_raise_RosterInstancesRemoved_event() =>
            eventContext.ShouldContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterGroupId && instance.RosterInstanceId == 0));

        [Test]
        public void should_not_raise_RosterInstancesAdded_event() =>
            eventContext.ShouldNotContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterGroupId));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichDecreasesRosterSizeId;
        private static Guid rosterGroupId;
    }
}
