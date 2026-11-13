using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    [TestFixture]
    [TestOf(typeof(InterviewItemsOrderExporter))]
    internal class InterviewItemsOrderExporterTests
    {
        private static readonly Guid SectionId = Id.gA;
        private static readonly Guid RosterId = Id.gB;
        private static readonly Guid Q1 = Id.g1, Q2 = Id.g2, Q3 = Id.g3, Q4 = Id.g4, Q5 = Id.g5;

        [Test]
        public void should_calculate_same_sort_keys_as_interview_applications()
        {
            // same values are asserted in WB.Tests.Unit RandomizedItemsOrderTests
            RandomizedItemsOrder.GetItemSortKey(0.5, Id.g1, new RosterVector(1, 2), Id.g2)
                .Should().Be(ItemSortKey);
            RandomizedItemsOrder.GetRosterInstanceSortKey(0.5, Id.g1, new RosterVector(3))
                .Should().Be(RosterInstanceSortKey);
        }

        internal const ulong ItemSortKey = 4518185135581417452UL;
        internal const ulong RosterInstanceSortKey = 11100533646074343573UL;

        [Test]
        public void should_not_create_files_when_questionnaire_has_no_randomization()
        {
            var questionnaire = Create.QuestionnaireDocument(variableName: "q",
                children: Create.Group(SectionId, "s", Create.TextQuestion(Q1), Create.TextQuestion(Q2)));

            var exporter = new InterviewItemsOrderExporter(Mock.Of<ICsvWriter>());

            exporter.GetFileNames(questionnaire).Should().BeEmpty();
            exporter.Export(questionnaire, new List<InterviewEntity>(), Id.gF, "11-11-11-11").Should().BeEmpty();
        }

        [Test]
        public void should_export_visible_items_in_display_order_with_consecutive_positions()
        {
            var questionnaire = CreateQuestionnaire();
            var interviewId = Id.gF;
            var entities = CreateInterviewEntities(interviewId);

            var exporter = new InterviewItemsOrderExporter(Mock.Of<ICsvWriter>());

            var result = exporter.Export(questionnaire, entities, interviewId, "11-11-11-11");

            result.Keys.Should().BeEquivalentTo("s__order");
            var rows = result["s__order"];

            var itemRows = rows.Where(r => r[2] == SectionId.FormatGuid()).ToList();
            itemRows.Select(r => r[6]).Should().Equal("1", "2", "3", "4");
            itemRows.Should().OnlyContain(r => r[0] == "11-11-11-11" && r[1] == interviewId.FormatGuid() && r[3] == "");

            var expectedOrder = RandomizedItemsOrder
                .GetChildrenOrder(questionnaire, interviewId.GetRandomDouble(), questionnaire.Find<Group>(SectionId)!, RosterVector.Empty)
                .Select(x => x.PublicKey)
                .Where(x => x != Q2)
                .Select(x => x.FormatGuid());
            itemRows.Select(r => r[4]).Should().Equal(expectedOrder);
            itemRows.Select(r => r[4]).Skip(2).Should().Equal(Q4.FormatGuid(), RosterId.FormatGuid());
            itemRows.Select(r => r[5]).Should().Equal("Question", "Question", "Question", "Roster");

            var instanceRows = rows.Where(r => r[2] == RosterId.FormatGuid()).ToList();
            instanceRows.Select(r => r[5]).Should().OnlyContain(t => t == "RosterInstance");
            instanceRows.Select(r => r[6]).Should().Equal("1", "2", "3");
            instanceRows.Select(r => r[3]).Should().BeEquivalentTo("1", "2", "3");
            instanceRows.Select(r => r[3]).Should().Equal(new[] { 1, 2, 3 }
                .OrderBy(x => RandomizedItemsOrder.GetRosterInstanceSortKey(interviewId.GetRandomDouble(), RosterId, new RosterVector(x)))
                .Select(x => x.ToString()));
        }

        [Test]
        public void should_keep_relative_order_of_roster_instances_when_one_is_removed()
        {
            var questionnaire = CreateQuestionnaire();
            var interviewId = Id.gF;
            var exporter = new InterviewItemsOrderExporter(Mock.Of<ICsvWriter>());

            var allInstances = exporter.Export(questionnaire, CreateInterviewEntities(interviewId), interviewId, "key")["s__order"]
                .Where(r => r[5] == "RosterInstance").Select(r => r[3]).ToList();

            var withoutSecond = CreateInterviewEntities(interviewId)
                .Where(e => !(e.Identity.RosterVector.Length == 1 && e.Identity.RosterVector.Last() == 2)).ToList();
            var remainingInstances = exporter.Export(questionnaire, withoutSecond, interviewId, "key")["s__order"]
                .Where(r => r[5] == "RosterInstance").Select(r => r[3]).ToList();

            remainingInstances.Should().Equal(allInstances.Where(x => x != "2"));
        }

        [Test]
        public void should_produce_identical_data_on_repeated_exports()
        {
            var questionnaire = CreateQuestionnaire();
            var interviewId = Id.gF;
            var exporter = new InterviewItemsOrderExporter(Mock.Of<ICsvWriter>());

            var first = exporter.Export(questionnaire, CreateInterviewEntities(interviewId), interviewId, "key")["s__order"];
            var second = exporter.Export(questionnaire, CreateInterviewEntities(interviewId), interviewId, "key")["s__order"];

            second.Should().BeEquivalentTo(first, o => o.WithStrictOrdering());
        }

        [Test]
        public void should_write_header_for_every_level_with_randomization()
        {
            var questionnaire = CreateQuestionnaire();
            var csvWriter = new Mock<ICsvWriter>();
            var exporter = new InterviewItemsOrderExporter(csvWriter.Object);

            exporter.WriteHeaders(questionnaire, "base", "tab");

            csvWriter.Verify(x => x.WriteData(System.IO.Path.Combine("base", "s__order.tab"),
                It.Is<IEnumerable<string[]>>(d => d.Single().SequenceEqual(new[]
                    { "interview__key", "interview__id", "parent__id", "roster__instance", "item__id", "item__type", "item__order" })),
                It.IsAny<string>()), Times.Once);
            csvWriter.VerifyNoOtherCalls();
        }

        private static QuestionnaireDocument CreateQuestionnaire()
        {
            var roster = Create.Roster(RosterId, "r", Create.TextQuestion(Q5));
            roster.RandomizeOrder = true;

            var q1 = Create.TextQuestion(Q1); q1.RandomizeOrder = true;
            var q2 = Create.TextQuestion(Q2); q2.RandomizeOrder = true;
            var q3 = Create.TextQuestion(Q3); q3.RandomizeOrder = true;
            var q4 = Create.TextQuestion(Q4);

            return Create.QuestionnaireDocument(variableName: "q",
                children: Create.Group(SectionId, "s", q1, q2, q3, q4, roster));
        }

        private static List<InterviewEntity> CreateInterviewEntities(Guid interviewId)
        {
            var entities = new List<InterviewEntity>
            {
                Create.InterviewEntity(interviewId, EntityType.Section, Create.Identity(SectionId)),
                Create.InterviewEntity(interviewId, EntityType.Question, Create.Identity(Q1)),
                Create.InterviewEntity(interviewId, EntityType.Question, Create.Identity(Q2), isEnabled: false),
                Create.InterviewEntity(interviewId, EntityType.Question, Create.Identity(Q3)),
                Create.InterviewEntity(interviewId, EntityType.Question, Create.Identity(Q4)),
            };

            foreach (var code in new[] { 1, 2, 3 })
            {
                entities.Add(Create.InterviewEntity(interviewId, EntityType.Section, Create.Identity(RosterId, code)));
                entities.Add(Create.InterviewEntity(interviewId, EntityType.Question, Create.Identity(Q5, code)));
            }

            return entities;
        }
    }
}
