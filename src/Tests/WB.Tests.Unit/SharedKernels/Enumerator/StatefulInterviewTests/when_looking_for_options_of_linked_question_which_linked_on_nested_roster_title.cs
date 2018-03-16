using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_looking_for_options_of_linked_question_which_linked_on_nested_roster_title : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireRepository =
                Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId,
                    Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
                    {
                        Create.Entity.FixedRoster(
                            fixedTitles:
                                new[]
                                {
                                    new FixedRosterTitle(1, "first fixed roster"),
                                    new FixedRosterTitle(2, "second fixed roster")
                                },
                            children: new[]
                            {
                                Create.Entity.FixedRoster(rosterId: sourceOfLinkedToRosterId,
                                    fixedTitles:
                                        new[]
                                        {
                                            new FixedRosterTitle(1, "first NESTED fixed roster"),
                                            new FixedRosterTitle(2, "second NESTED fixed roster")
                                        },
                                    children: new[]
                                    {
                                        Create.Entity.NumericIntegerQuestion()
                                    })
                            }),
                        Create.Entity.MultyOptionsQuestion(linkedToQuestionIdentity.Id, linkedToRosterId: sourceOfLinkedToRosterId)
                    })));
            BecauseOf();
        }

        private void BecauseOf() => interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

        [NUnit.Framework.Test] public void should_order_options_by_roster_sort_index_at_first () 
        {
            interview.GetLinkedMultiOptionQuestion(linkedToQuestionIdentity).Options.Select(a => a.First()).ToArray().Should().BeEquivalentTo(new int[] { 1, 1, 2, 2 });
        }

        [NUnit.Framework.Test] public void should_order_options_by_nested_roster_sort_index_in_scope_of_parent_roster () 
        {
            interview.GetLinkedMultiOptionQuestion(linkedToQuestionIdentity).Options.Select(a => a.Last()).ToArray().Should().BeEquivalentTo(new int[] { 1, 2, 1, 2 });
        }

        private static IQuestionnaireStorage questionnaireRepository;
        private static StatefulInterview interview;
        private static Guid questionnaireId = Guid.Parse("44444444444444444444444444444444");
        private static Identity linkedToQuestionIdentity = Identity.Create(Guid.Parse("33333333333333333333333333333333"), Create.Entity.RosterVector());
        private static Guid sourceOfLinkedToRosterId = Guid.Parse("22222222222222222222222222222222");
    }
}
