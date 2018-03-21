using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_on_integer_question_increases_roster_size_of_roster_with_nested_roster_with_skips_triggered_by_numeric : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            nestedRosterGroupId = Guid.Parse("11111111111111111111111111111111");
            parentRosterGroupId = Guid.Parse("21111111111111111111111111111111");
            questionWhichIncreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");
            questionWhichIncreasesNestedRosterSizeId = Guid.Parse("31111111111111111111111111111111");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: questionWhichIncreasesRosterSizeId),
                Create.Entity.NumericIntegerQuestion(id: questionWhichIncreasesNestedRosterSizeId),

                Create.Entity.Roster(rosterId: parentRosterGroupId, rosterSizeQuestionId: questionWhichIncreasesRosterSizeId, children: new IComposite[]
                {
                    Create.Entity.Roster(rosterId: nestedRosterGroupId, rosterSizeQuestionId: questionWhichIncreasesNestedRosterSizeId),
                }),
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(new NumericIntegerQuestionAnswered(userId, questionWhichIncreasesNestedRosterSizeId, new decimal[0],
                DateTime.Now, 1));
            eventContext = new EventContext();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() =>
           interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[0], DateTime.Now, 1);

        [NUnit.Framework.Test] public void should_raise_RosterInstancesAdded_event_for_first_row () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == parentRosterGroupId && instance.RosterInstanceId == 0 && instance.OuterRosterVector.Length == 0));

        [NUnit.Framework.Test] public void should_raise_RosterInstancesAdded_event_for_first_nested_row () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == nestedRosterGroupId && instance.RosterInstanceId == 0 && instance.OuterRosterVector.SequenceEqual(new decimal[] { 0 })));

        [NUnit.Framework.Test] public void should_raise_RosterRowTitleChanged_event_for_first_nested_row () =>
         eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event
             => @event.ChangedInstances.Any(x => x.RosterInstance.GroupId == nestedRosterGroupId && 
                                                 x.RosterInstance.RosterInstanceId == 0 && x.Title=="1" &&
                                                 x.RosterInstance.OuterRosterVector.SequenceEqual(new decimal[] { 0 })));

        [NUnit.Framework.Test] public void should_not_raise_RosterInstancesRemoved_event () =>
            eventContext.ShouldNotContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances.Any(instance => instance.GroupId == nestedRosterGroupId));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIncreasesRosterSizeId;
        private static Guid questionWhichIncreasesNestedRosterSizeId;
        private static Guid nestedRosterGroupId;
        private static Guid parentRosterGroupId;
    }
}
