using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Spec;
using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_text_list_question_decreasing_roster_size_of_roster_with_nested_roster : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            nestedRosterGroupId = Guid.Parse("11111111111111111111111111111111");
            parentRosterGroupId = Guid.Parse("21111111111111111111111111111111");

            textListQuestionId = Guid.Parse("22222222222222222222222222222222");
            nestedTextListQuestionId = Guid.Parse("32222222222222222222222222222222");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(
               new TextListQuestion("roster size question")
               {
                   PublicKey = textListQuestionId
               },
               new Group("top level roster")
               {
                   PublicKey = parentRosterGroupId,
                   IsRoster = true,
                   RosterSizeSource = RosterSizeSourceType.Question,
                   RosterSizeQuestionId = textListQuestionId,
                   Children = new List<IComposite>
                   {
                        new TextListQuestion("nested roster size question")
                        {
                            PublicKey = nestedTextListQuestionId
                        },
                        new Group("nested roster")
                        {
                            PublicKey = nestedRosterGroupId,
                            IsRoster = true,
                            RosterSizeSource = RosterSizeSourceType.Question,
                            RosterSizeQuestionId = nestedTextListQuestionId
                        }
                   }.ToReadOnlyCollection()
               });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, Create.Entity.PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            interview.Apply(new TextListQuestionAnswered(userId, textListQuestionId, new decimal[] { }, DateTime.Now, previousAnswer));
            interview.Apply(Create.Event.RosterInstancesAdded(parentRosterGroupId, emptyRosterVector, 1, sortIndex: null));
            interview.Apply(Create.Event.RosterInstancesAdded(parentRosterGroupId, emptyRosterVector, 2, sortIndex: null));

            interview.Apply(new TextListQuestionAnswered(userId, nestedTextListQuestionId, new decimal[] { 1 }, DateTime.Now, previousAnswer));
            interview.Apply(Create.Event.RosterInstancesAdded(nestedRosterGroupId, new decimal[] { 1 }, 1, sortIndex: null));
            interview.Apply(Create.Event.RosterInstancesAdded(nestedRosterGroupId, new decimal[] { 1 }, 2, sortIndex: null));

            eventContext = new EventContext();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        private void BecauseOf() =>
            interview.AnswerTextListQuestion(userId, textListQuestionId, new decimal[0], DateTime.Now, new Tuple<decimal, string>[] { new Tuple<decimal, string>(2, "Answer 2") });


        [NUnit.Framework.Test] public void should_raise_TextListQuestionAnswered_event () =>
            eventContext.ShouldContainEvent<TextListQuestionAnswered>();

        [NUnit.Framework.Test] public void should_raise_RosterInstancesRemoved_event_with_3_instances () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count().Should().Be(3);

        [NUnit.Framework.Test] public void should_raise_RosterInstancesRemoved_event_with_2_instances_where_GroupId_equals_to_rosterAId () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count(instance => instance.GroupId == parentRosterGroupId).Should().Be(1);

        [NUnit.Framework.Test] public void should_raise_RosterInstancesRemoved_event_with_2_instances_where_GroupId_equals_to_rosterBId () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count(instance => instance.GroupId == nestedRosterGroupId).Should().Be(2);

        [NUnit.Framework.Test] public void should_raise_RosterInstancesRemoved_event_with_2_instances_where_roster_instance_id_equals_to_1 () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count(instance => instance.RosterInstanceId == 1).Should().Be(2);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid textListQuestionId;
        private static Guid nestedTextListQuestionId;
        private static Guid nestedRosterGroupId;
        private static decimal[] emptyRosterVector = new decimal[] { };
        private static Guid parentRosterGroupId;

        private static Tuple<decimal, string>[] previousAnswer = new[]
            {
                new Tuple<decimal, string>(1, "Answer 1"),
                new Tuple<decimal, string>(2, "Answer 2")
            };
    }
}
