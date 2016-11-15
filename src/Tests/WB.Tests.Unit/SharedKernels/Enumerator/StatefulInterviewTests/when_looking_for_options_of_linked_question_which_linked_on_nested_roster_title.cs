using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_looking_for_options_of_linked_question_which_linked_on_nested_roster_title : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireRepository =
                Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId,
                    Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
                    {
                        Create.Entity.FixedRoster(
                            fixedRosterTitles:
                                new[]
                                {
                                    new FixedRosterTitle(1, "first fixed roster"),
                                    new FixedRosterTitle(2, "second fixed roster")
                                },
                            children: new[]
                            {
                                Create.Entity.FixedRoster(rosterId: sourceOfLinkedToRosterId,
                                    fixedRosterTitles:
                                        new[]
                                        {
                                            new FixedRosterTitle(1, "first NESTED fixed roster"),
                                            new FixedRosterTitle(2, "second NESTED fixed roster")
                                        },
                                    children: new[]
                                    {
                                        Create.Entity.NumericIntegerQuestion(sourceOfLinkedToRosterId)
                                    })
                            }),
                        Create.Entity.MultyOptionsQuestion(linkedToQuestionIdentity.Id, linkedToRosterId: sourceOfLinkedToRosterId)
                    })));
        };

        Because of = () => interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

        It should_order_options_by_roster_sort_index_at_first = () =>
        {
            interview.GetLinkedMultiOptionQuestion(linkedToQuestionIdentity)
            .Options.Select(a => a.First()).ToArray().ShouldEqual(new decimal[] { 1, 1, 2, 2 });
        };

        It should_order_options_by_nested_roster_sort_index_in_scope_of_parent_roster = () =>
        {
            interview.GetLinkedMultiOptionQuestion(linkedToQuestionIdentity)
            .Options.Select(a => a.Last()).ToArray().ShouldEqual(new decimal[] { 1, 2, 1, 2 });
        };

        private static IQuestionnaireStorage questionnaireRepository;
        private static StatefulInterview interview;
        private static Guid questionnaireId = Guid.Parse("44444444444444444444444444444444");
        private static Identity linkedToQuestionIdentity = Identity.Create(Guid.Parse("33333333333333333333333333333333"), Create.Entity.RosterVector(0));
        private static Guid sourceOfLinkedToRosterId = Guid.Parse("22222222222222222222222222222222");
    }
}