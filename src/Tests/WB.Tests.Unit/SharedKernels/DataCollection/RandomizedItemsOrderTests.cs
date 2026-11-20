using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Utils;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    [TestOf(typeof(RandomizedItemsOrder))]
    public class RandomizedItemsOrderTests
    {
        private static readonly Guid ChapterId = Guid.Parse("cccccccccccccccccccccccccccccccc");

        [Test]
        public void should_calculate_same_sort_keys_as_export_service()
        {
            // same values are asserted in WB.Services.Export.Tests InterviewItemsOrderExporterTests
            Assert.That(RandomizedItemsOrder.GetItemSortKey(0.5, Id.g1, Create.Entity.RosterVector(1, 2), Id.g2),
                Is.EqualTo(4518185135581417452UL));
            Assert.That(RandomizedItemsOrder.GetRosterInstanceSortKey(0.5, Id.g1, Create.Entity.RosterVector(3)),
                Is.EqualTo(11100533646074343573UL));
        }

        [Test]
        public void should_randomize_only_consecutive_flagged_siblings()
        {
            var questionnaire = CreateQuestionnaire(
                Randomized(Create.Entity.TextQuestion(Id.g1)),
                Randomized(Create.Entity.TextQuestion(Id.g2)),
                Randomized(Create.Entity.Roster(Id.g3)),
                Create.Entity.TextQuestion(Id.g4),
                Randomized(Create.Entity.Group(Id.g5)),
                Randomized(Create.Entity.StaticText(Id.g6)),
                Create.Entity.Variable(Id.g7),
                Randomized(Create.Entity.TextQuestion(Id.g8)));

            for (int i = 0; i < 50; i++)
            {
                var order = RandomizedItemsOrder.GetChildrenOrder(questionnaire, i / 50.0, ChapterId, RosterVector.Empty);

                Assert.That(order.Take(3), Is.EquivalentTo(new[] { Id.g1, Id.g2, Id.g3 }));
                Assert.That(order[3], Is.EqualTo(Id.g4));
                Assert.That(order.Skip(4).Take(2), Is.EquivalentTo(new[] { Id.g5, Id.g6 }));
                Assert.That(order[6], Is.EqualTo(Id.g7));
                Assert.That(order[7], Is.EqualTo(Id.g8));
            }
        }

        [Test]
        public void should_produce_same_order_for_same_seed_and_different_orders_for_different_seeds()
        {
            var questionnaire = CreateQuestionnaire(Enumerable.Range(1, 6)
                .Select(i => (IComposite)Randomized(Create.Entity.TextQuestion(Guid.NewGuid()))).ToArray());

            var first = RandomizedItemsOrder.GetChildrenOrder(questionnaire, 0.42, ChapterId, RosterVector.Empty);
            var second = RandomizedItemsOrder.GetChildrenOrder(questionnaire, 0.42, ChapterId, RosterVector.Empty);
            Assert.That(second, Is.EqualTo(first));

            var permutations = Enumerable.Range(0, 100)
                .Select(i => string.Join(",", RandomizedItemsOrder.GetChildrenOrder(questionnaire, Guid.NewGuid().GetHashCode() / (double)int.MaxValue, ChapterId, RosterVector.Empty)))
                .Distinct()
                .Count();
            Assert.That(permutations, Is.GreaterThan(10));
        }

        [Test]
        public void should_not_change_order_of_remaining_items_when_some_are_filtered_out()
        {
            var questionnaire = CreateQuestionnaire(Enumerable.Range(1, 6)
                .Select(i => (IComposite)Randomized(Create.Entity.TextQuestion(Guid.NewGuid()))).ToArray());
            var chapter = Create.Identity(ChapterId);
            var children = questionnaire.GetChildEntityIds(ChapterId).Select(id => Create.Identity(id)).ToList();

            var all = RandomizedItemsOrder.OrderForDisplay(questionnaire, 0.3, chapter, children).ToList();
            var filtered = RandomizedItemsOrder.OrderForDisplay(questionnaire, 0.3, chapter, children.Where((x, i) => i % 2 == 0)).ToList();

            Assert.That(filtered, Is.EqualTo(all.Where(x => filtered.Contains(x))));
        }

        [Test]
        public void should_keep_relative_order_of_existing_roster_instances_when_instances_are_added_or_removed()
        {
            var rosterId = Id.g1;
            var questionnaire = CreateQuestionnaire(Randomized(Create.Entity.Roster(rosterId)));
            var chapter = Create.Identity(ChapterId);
            var instances = Enumerable.Range(1, 10).Select(i => Create.Identity(rosterId, i)).ToList();

            var all = RandomizedItemsOrder.OrderForDisplay(questionnaire, 0.7, chapter, instances).ToList();
            var withoutSome = RandomizedItemsOrder.OrderForDisplay(questionnaire, 0.7, chapter, instances.Where(x => x.RosterVector.Last() % 3 != 0)).ToList();
            var withNew = RandomizedItemsOrder.OrderForDisplay(questionnaire, 0.7, chapter, instances.Append(Create.Identity(rosterId, 11))).ToList();

            Assert.That(all, Is.Not.EqualTo(instances));
            Assert.That(withoutSome, Is.EqualTo(all.Where(x => x.RosterVector.Last() % 3 != 0)));
            Assert.That(withNew.Where(x => x.RosterVector.Last() != 11), Is.EqualTo(all));
        }

        [Test]
        public void should_not_reorder_roster_instances_when_roster_is_not_randomized()
        {
            var rosterId = Id.g1;
            var questionnaire = CreateQuestionnaire(Create.Entity.Roster(rosterId), Randomized(Create.Entity.TextQuestion(Id.g2)));
            var instances = Enumerable.Range(1, 10).Select(i => Create.Identity(rosterId, i)).ToList();

            var ordered = RandomizedItemsOrder.OrderForDisplay(questionnaire, 0.7, Create.Identity(ChapterId), instances);

            Assert.That(ordered, Is.EqualTo(instances));
        }

        [Test]
        public void should_ignore_randomization_flag_for_cover_section_children()
        {
            var coverQuestion = Randomized(Create.Entity.TextQuestion(Id.g1));
            var document = Create.Entity.QuestionnaireDocumentWithCover(coverId: Id.gA, children: new IComposite[]
            {
                Create.Entity.Group(Id.gA, children: new IComposite[] { coverQuestion, Randomized(Create.Entity.TextQuestion(Id.g2)) }),
                Create.Entity.Group(Id.gB, children: new IComposite[] { Randomized(Create.Entity.TextQuestion(Id.g3)) })
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(document);

            Assert.That(questionnaire.IsOrderRandomized(Id.g1), Is.False);
            Assert.That(questionnaire.HasRandomizedChildren(Id.gA), Is.False);
            Assert.That(questionnaire.IsOrderRandomized(Id.g3), Is.True);
            Assert.That(questionnaire.HasRandomizedChildren(Id.gB), Is.True);
        }

        [Test]
        public void interview_should_return_section_entities_in_deterministic_randomized_order()
        {
            var questionIds = Enumerable.Range(1, 8).Select(i => Guid.NewGuid()).ToArray();
            var document = Create.Entity.QuestionnaireDocumentWithOneChapter(ChapterId,
                questionIds.Select(id => (IComposite)Randomized(Create.Entity.TextQuestion(id))).ToArray());
            var interview = SetUp.StatefulInterview(document);

            var entities = interview.GetUnderlyingInterviewerEntities(Create.Identity(ChapterId)).Select(x => x.Id).ToList();

            var expected = RandomizedItemsOrder.GetChildrenOrder(Create.Entity.PlainQuestionnaire(document),
                new InterviewProperties(interview.Id).IRnd(), ChapterId, RosterVector.Empty);
            Assert.That(entities, Is.EqualTo(expected));
            Assert.That(interview.GetUnderlyingInterviewerEntities(Create.Identity(ChapterId)).Select(x => x.Id), Is.EqualTo(entities));
        }

        private static T Randomized<T>(T entity) where T : IConditional
        {
            entity.RandomizeOrder = true;
            return entity;
        }

        private static Core.SharedKernels.DataCollection.Implementation.Entities.PlainQuestionnaire CreateQuestionnaire(params IComposite[] children)
            => Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(ChapterId, children));
    }
}
