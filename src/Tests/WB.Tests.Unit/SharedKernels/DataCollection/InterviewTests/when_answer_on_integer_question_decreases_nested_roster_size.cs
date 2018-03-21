using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_on_integer_question_decreases_nested_roster_size : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            rosterGroupId = Guid.Parse("11111111111111111111111111111111");
            parentRosterGroupId = Guid.Parse("21111111111111111111111111111111");
            questionWhichIncreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: numericQuestionId),
                Create.Entity.Roster(rosterId: parentRosterGroupId, rosterSizeQuestionId:numericQuestionId, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(id: questionWhichIncreasesRosterSizeId),

                    Create.Entity.Roster(rosterId: rosterGroupId, rosterSizeQuestionId: questionWhichIncreasesRosterSizeId, rosterSizeSourceType: RosterSizeSourceType.Question),
                }),
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(Create.Event.NumericIntegerQuestionAnswered(numericQuestionId, RosterVector.Empty, 1));
            interview.Apply(Create.Event.RosterInstancesAdded(parentRosterGroupId, new decimal[0], 0, null));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterGroupId, new decimal[] { 0 }, 0, null));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, questionWhichIncreasesRosterSizeId, new decimal[] { 0 }, DateTime.Now,
                1));

            eventContext = new EventContext();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() =>
            interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[] { 0 }, DateTime.Now, 0);

        [NUnit.Framework.Test] public void should_raise_RosterInstancesRemoved_event () =>
            eventContext.ShouldContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterGroupId && instance.RosterInstanceId == 0 && instance.OuterRosterVector.Length == 1 && instance.OuterRosterVector[0] == 0));

        [NUnit.Framework.Test] public void should_not_raise_RosterInstancesAdded_event () =>
            eventContext.ShouldNotContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterGroupId));

        [NUnit.Framework.Test] public void should_not_raise_GroupsEnabled_event () =>
            eventContext.ShouldNotContainEvent<GroupsEnabled>(@event
                => @event.Groups.Any(g => g == Create.Identity(rosterGroupId, 1)));

        [NUnit.Framework.Test] public void should_not_raise_QuestionsEnabled_event () =>
            eventContext.ShouldNotContainEvent<QuestionsEnabled>(@event
                => @event.Questions.Any(q => q == Create.Identity(questionWhichIncreasesRosterSizeId, 1)));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIncreasesRosterSizeId;
        private static Guid rosterGroupId;
        private static Guid parentRosterGroupId;
        private static Guid numericQuestionId = Guid.Parse("22222222222222222222222222222227");
    }
}
