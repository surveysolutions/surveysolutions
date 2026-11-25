using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    [TestOf(typeof(RandomizedItemsOrder))]
    public class RandomizedAnswerOptionsOrderTests
    {
        private static readonly Guid ChapterId = Guid.Parse("cccccccccccccccccccccccccccccccc");
        private static readonly int[] Codes = Enumerable.Range(1, 10).ToArray();

        [Test]
        public void should_calculate_stable_sort_keys()
        {
            // changing these values changes option order in already existing interviews
            Assert.That(RandomizedItemsOrder.GetAnswerOptionSortKey(0.5, Id.g1, Create.Entity.RosterVector(1, 2), 3),
                Is.EqualTo(ExpectedAnswerOptionKey));
            Assert.That(RandomizedItemsOrder.GetLinkedAnswerOptionSortKey(0.5, Id.g1, Create.Entity.RosterVector(1), Create.Entity.RosterVector(2, 3)),
                Is.EqualTo(ExpectedLinkedAnswerOptionKey));
        }

        private const ulong ExpectedAnswerOptionKey = 6663368733250769673UL;
        private const ulong ExpectedLinkedAnswerOptionKey = 884176144042385174UL;

        [Test]
        public void should_randomize_only_categorical_questions_displayed_as_list()
        {
            var questionnaire = CreateQuestionnaire(
                Randomized(Create.Entity.SingleOptionQuestion(Id.g1)),
                Randomized(Create.Entity.MultipleOptionsQuestion(Id.g2, answers: Codes)),
                Randomized(Create.Entity.MultipleOptionsQuestion(Id.g3, isYesNo: true, answers: Codes)),
                Randomized(Create.Entity.SingleOptionQuestion(Id.g4, isFilteredCombobox: true)),
                Randomized(Create.Entity.SingleOptionQuestion(Id.g5, cascadeFromQuestionId: Id.g1)),
                Randomized(Create.Entity.TextQuestion(Id.g6)),
                Create.Entity.SingleOptionQuestion(Id.g7));

            Assert.That(questionnaire.ShouldRandomizeAnswerOptions(Id.g1), Is.True);
            Assert.That(questionnaire.ShouldRandomizeAnswerOptions(Id.g2), Is.True);
            Assert.That(questionnaire.ShouldRandomizeAnswerOptions(Id.g3), Is.True);
            Assert.That(questionnaire.ShouldRandomizeAnswerOptions(Id.g4), Is.False);
            Assert.That(questionnaire.ShouldRandomizeAnswerOptions(Id.g5), Is.False);
            Assert.That(questionnaire.ShouldRandomizeAnswerOptions(Id.g6), Is.False);
            Assert.That(questionnaire.ShouldRandomizeAnswerOptions(Id.g7), Is.False);
            Assert.That(questionnaire.IsOrderRandomized(Id.g1), Is.False);
        }

        [Test]
        public void should_not_change_order_when_flag_is_not_set()
        {
            var questionnaire = CreateQuestionnaire(Create.Entity.MultipleOptionsQuestion(Id.g1, answers: Codes));

            var ordered = RandomizedItemsOrder.OrderAnswerOptionsForDisplay(questionnaire, 0.3, Create.Identity(Id.g1), Codes, x => x);

            Assert.That(ordered, Is.EqualTo(Codes));
        }

        [Test]
        public void should_produce_same_order_for_same_interview_and_different_orders_for_different_interviews()
        {
            var questionnaire = CreateQuestionnaire(Randomized(Create.Entity.MultipleOptionsQuestion(Id.g1, answers: Codes)));
            var question = Create.Identity(Id.g1);

            var first = RandomizedItemsOrder.OrderAnswerOptionsForDisplay(questionnaire, 0.42, question, Codes, x => x).ToList();
            var second = RandomizedItemsOrder.OrderAnswerOptionsForDisplay(questionnaire, 0.42, question, Codes.Reverse(), x => x).ToList();

            Assert.That(first, Is.EquivalentTo(Codes));
            Assert.That(second, Is.EqualTo(first));

            var permutations = Enumerable.Range(0, 100)
                .Select(i => string.Join(",", RandomizedItemsOrder.OrderAnswerOptionsForDisplay(questionnaire,
                    RandomizedItemsOrder.GetSeed(Guid.NewGuid()), question, Codes, x => x)))
                .Distinct()
                .Count();
            Assert.That(permutations, Is.GreaterThan(10));
        }

        [Test]
        public void should_randomize_questions_and_roster_instances_independently()
        {
            var questionnaire = CreateQuestionnaire(
                Randomized(Create.Entity.MultipleOptionsQuestion(Id.g1, answers: Codes)),
                Randomized(Create.Entity.MultipleOptionsQuestion(Id.g2, answers: Codes)));

            string Order(Identity identity) => string.Join(",",
                RandomizedItemsOrder.OrderAnswerOptionsForDisplay(questionnaire, 0.42, identity, Codes, x => x));

            var orders = new[]
            {
                Order(Create.Identity(Id.g1)),
                Order(Create.Identity(Id.g2)),
                Order(Create.Identity(Id.g1, 1)),
                Order(Create.Identity(Id.g1, 2)),
                Order(Create.Identity(Id.g1, 1, 1)),
            };

            Assert.That(orders.Distinct().Count(), Is.EqualTo(orders.Length));
            Assert.That(Order(Create.Identity(Id.g1, 2)), Is.EqualTo(orders[3]));
        }

        [Test]
        public void should_keep_relative_order_when_options_are_filtered_added_or_restored()
        {
            var questionnaire = CreateQuestionnaire(Randomized(Create.Entity.SingleOptionQuestion(Id.g1)));
            var question = Create.Identity(Id.g1, 3);

            var all = RandomizedItemsOrder.OrderAnswerOptionsForDisplay(questionnaire, 0.7, question, Codes, x => x).ToList();
            var filtered = RandomizedItemsOrder.OrderAnswerOptionsForDisplay(questionnaire, 0.7, question, Codes.Where(x => x % 3 != 0), x => x).ToList();
            var withNew = RandomizedItemsOrder.OrderAnswerOptionsForDisplay(questionnaire, 0.7, question, Codes.Append(42), x => x).ToList();

            Assert.That(all, Is.Not.EqualTo(Codes));
            Assert.That(filtered, Is.EqualTo(all.Where(x => x % 3 != 0)));
            Assert.That(withNew.Where(x => x != 42), Is.EqualTo(all));
        }

        [Test]
        public void should_order_linked_options_by_stable_identity()
        {
            var questionnaire = CreateQuestionnaire(Randomized(Create.Entity.SingleOptionQuestion(Id.g1, linkedToRosterId: Id.g2)));
            var question = Create.Identity(Id.g1);
            var options = Enumerable.Range(1, 10).Select(i => Create.Entity.RosterVector(i)).ToList();

            var all = RandomizedItemsOrder.OrderLinkedAnswerOptionsForDisplay(questionnaire, 0.7, question, options, x => x).ToList();
            var reversed = RandomizedItemsOrder.OrderLinkedAnswerOptionsForDisplay(questionnaire, 0.7, question, Enumerable.Reverse(options), x => x).ToList();
            var filtered = RandomizedItemsOrder.OrderLinkedAnswerOptionsForDisplay(questionnaire, 0.7, question, options.Where(x => x.Last() % 2 == 0), x => x).ToList();

            Assert.That(all, Is.EquivalentTo(options));
            Assert.That(all, Is.Not.EqualTo(options));
            Assert.That(reversed, Is.EqualTo(all));
            Assert.That(filtered, Is.EqualTo(all.Where(x => x.Last() % 2 == 0)));
        }

        [Test]
        public void interview_should_return_options_in_deterministic_randomized_order()
        {
            var document = Create.Entity.QuestionnaireDocumentWithOneChapter(ChapterId,
                Randomized(Create.Entity.MultipleOptionsQuestion(Id.g1, answers: Codes)),
                Create.Entity.MultipleOptionsQuestion(Id.g2, variable: "mo_question2", answers: Codes));
            IStatefulInterview interview = SetUp.StatefulInterview(document);

            var options = interview.GetTopFilteredOptionsForQuestion(Create.Identity(Id.g1), null, null, 200, null).Select(x => x.Value).ToList();

            var expected = RandomizedItemsOrder.OrderAnswerOptionsForDisplay(Create.Entity.PlainQuestionnaire(document),
                new InterviewProperties(interview.Id).IRnd(), Create.Identity(Id.g1), Codes, x => x);
            Assert.That(options, Is.EqualTo(expected));
            Assert.That(options, Is.Not.EqualTo(Codes));
            Assert.That(interview.GetTopFilteredOptionsForQuestion(Create.Identity(Id.g1), null, null, 200, null).Select(x => x.Value), Is.EqualTo(options));

            var notRandomizedOptions = interview.GetTopFilteredOptionsForQuestion(Create.Identity(Id.g2), null, null, 200, null).Select(x => x.Value);
            Assert.That(notRandomizedOptions, Is.EqualTo(Codes));
        }

        private static T Randomized<T>(T question) where T : IQuestion
        {
            question.RandomizeAnswerOptions = true;
            return question;
        }

        private static IQuestionnaire CreateQuestionnaire(params IComposite[] children)
            => Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(ChapterId, children));
    }
}
