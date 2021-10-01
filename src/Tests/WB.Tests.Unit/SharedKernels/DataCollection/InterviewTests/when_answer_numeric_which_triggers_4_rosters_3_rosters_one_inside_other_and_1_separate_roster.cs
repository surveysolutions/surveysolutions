using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Spec;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_numeric_which_triggers_4_rosters_3_rosters_one_inside_other_and_1_separate_roster : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion("roster size question")
                {
                    PublicKey = rosterSizeQuestionId,
                    IsInteger = true
                },
                new Group("top level roster")
                {
                    PublicKey = Guid.Parse("11111111111111111111111111111111"),
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new Group("nested roster")
                        {
                            PublicKey = Guid.Parse("21111111111111111111111111111111"),
                            IsRoster = true,
                            RosterSizeSource = RosterSizeSourceType.Question,
                            RosterSizeQuestionId = rosterSizeQuestionId,
                            Children = new List<IComposite>
                            {
                                new Group("nested nested roster")
                                {
                                    PublicKey = Guid.Parse("31111111111111111111111111111111"),
                                    IsRoster = true,
                                    RosterSizeSource = RosterSizeSourceType.Question,
                                    RosterSizeQuestionId = rosterSizeQuestionId,
                                }
                            }.ToReadOnlyCollection()
                        }
                    }.ToReadOnlyCollection()
                },
                new Group("separate roster")
                {
                    PublicKey = Guid.Parse("22222222222222222222222222222222"),
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                Create.Entity.PlainQuestionnaire(questionnaire, 1));

            eventContext = new EventContext();
            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() =>
            interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, new decimal[0], DateTime.Now, 2);

        [NUnit.Framework.Test] public void should_produce_one_event_roster_instance_added () =>
            eventContext.GetEvents<RosterInstancesAdded>().Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_put_16_instances_to_RosterInstancesAdded_event () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event => @event.Instances.Length == 16);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid rosterSizeQuestionId;
    }
}
