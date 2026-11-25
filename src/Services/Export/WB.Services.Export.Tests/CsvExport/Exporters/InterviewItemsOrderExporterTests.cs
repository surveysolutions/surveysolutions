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
        private static readonly Guid NestedRosterId = Id.gC, StaticTextId = Id.g7;
        private static readonly Guid Q1 = Id.g1, Q2 = Id.g2, Q3 = Id.g3, Q4 = Id.g4, Q5 = Id.g5, Q6 = Id.g6;

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
        public void should_export_only_randomized_items_with_positions_starting_from_1_in_every_group()
        {
            var questionnaire = CreateQuestionnaire();
            var interviewId = Id.gF;
            var entities = CreateInterviewEntities(interviewId);

            var exporter = new InterviewItemsOrderExporter(Mock.Of<ICsvWriter>());

            var result = exporter.Export(questionnaire, entities, interviewId, "11-11-11-11");

            result.Keys.Should().BeEquivalentTo("s__order", "r__order");
            var rows = result["s__order"];

            rows.Should().OnlyContain(r => r.Length == 6 && r[0] == "11-11-11-11" && r[1] == interviewId.FormatGuid());
            rows.Select(r => r[3]).Should().NotContain(new[] { "q2", "q4" });

            var firstGroup = rows.Where(r => r[2] == "1").ToList();
            var expectedOrder = RandomizedItemsOrder
                .GetChildrenOrder(questionnaire, interviewId.GetRandomDouble(), questionnaire.Find<Group>(SectionId)!, RosterVector.Empty)
                .Select(x => x.PublicKey)
                .Where(x => x == Q1 || x == Q3 || x == StaticTextId)
                .Select(x => x == Q1 ? "q1" : x == Q3 ? "q3" : StaticTextId.FormatGuid());
            firstGroup.Select(r => r[3]).Should().Equal(expectedOrder);
            firstGroup.Where(r => r[3] == StaticTextId.FormatGuid()).Select(r => r[4]).Should().Equal("StaticText");
            firstGroup.Select(r => r[5]).Should().Equal("1", "2", "3");

            var secondGroup = rows.Where(r => r[2] == "2").ToList();
            secondGroup.Select(r => r[3]).Should().Equal("r");
            secondGroup.Select(r => r[4]).Should().Equal("Roster");
            secondGroup.Select(r => r[5]).Should().Equal("1");

            rows.Should().HaveCount(4);
        }

        [Test]
        public void should_export_roster_instances_and_items_inside_roster_with_roster_level_keys()
        {
            var questionnaire = CreateQuestionnaire();
            var interviewId = Id.gF;
            var exporter = new InterviewItemsOrderExporter(Mock.Of<ICsvWriter>());

            var rows = exporter.Export(questionnaire, CreateInterviewEntities(interviewId), interviewId, "11-11-11-11")["r__order"];

            rows.Should().OnlyContain(r => r.Length == 7 && r[0] == "11-11-11-11" && r[1] == interviewId.FormatGuid());

            var instanceRows = rows.Where(r => r[5] == "RosterInstance").ToList();
            instanceRows.Select(r => r[2]).Should().Equal("1", "2", "3");
            instanceRows.Should().OnlyContain(r => r[3] == "" && r[4] == "r");
            var expectedInstanceOrder = new[] { 1, 2, 3 }
                .OrderBy(x => RandomizedItemsOrder.GetRosterInstanceSortKey(interviewId.GetRandomDouble(), RosterId, new RosterVector(x)))
                .ToList();
            instanceRows.Select(r => r[6]).Should().Equal(new[] { 1, 2, 3 }
                .Select(x => (expectedInstanceOrder.IndexOf(x) + 1).ToString()));

            foreach (var code in new[] { 1, 2, 3 })
            {
                var itemRows = rows.Where(r => r[2] == code.ToString() && r[5] == "Question").ToList();
                var expectedItems = RandomizedItemsOrder
                    .GetChildrenOrder(questionnaire, interviewId.GetRandomDouble(), questionnaire.Find<Group>(RosterId)!, new RosterVector(code))
                    .Select(x => x.PublicKey == Q5 ? "q5" : "q6");
                itemRows.Select(r => r[4]).Should().Equal(expectedItems);
                itemRows.Select(r => r[3]).Should().OnlyContain(g => g == "1");
                itemRows.Select(r => r[6]).Should().Equal("1", "2");
            }

            rows.Should().HaveCount(9);
        }

        [Test]
        public void should_keep_relative_order_of_roster_instances_when_one_is_removed()
        {
            var questionnaire = CreateQuestionnaire();
            var interviewId = Id.gF;
            var exporter = new InterviewItemsOrderExporter(Mock.Of<ICsvWriter>());

            List<string> GetInstancesInDisplayOrder(List<InterviewEntity> entities) =>
                exporter.Export(questionnaire, entities, interviewId, "key")["r__order"]
                    .Where(r => r[5] == "RosterInstance")
                    .OrderBy(r => int.Parse(r[6]))
                    .Select(r => r[2])
                    .ToList();

            var allInstances = GetInstancesInDisplayOrder(CreateInterviewEntities(interviewId));

            var remainingInstances = GetInstancesInDisplayOrder(CreateInterviewEntities(interviewId)
                .Where(e => !(e.Identity.RosterVector.Length == 1 && e.Identity.RosterVector.Last() == 2)).ToList());

            remainingInstances.Should().Equal(allInstances.Where(x => x != "2"));
        }

        [Test]
        public void should_produce_identical_data_on_repeated_exports()
        {
            var questionnaire = CreateQuestionnaire();
            var interviewId = Id.gF;
            var exporter = new InterviewItemsOrderExporter(Mock.Of<ICsvWriter>());

            var first = exporter.Export(questionnaire, CreateInterviewEntities(interviewId), interviewId, "key");
            var second = exporter.Export(questionnaire, CreateInterviewEntities(interviewId), interviewId, "key");

            second["s__order"].Should().BeEquivalentTo(first["s__order"], o => o.WithStrictOrdering());
            second["r__order"].Should().BeEquivalentTo(first["r__order"], o => o.WithStrictOrdering());
        }

        [Test]
        public void should_write_header_with_key_columns_of_data_file_for_every_level_with_randomization()
        {
            var questionnaire = CreateQuestionnaire();
            var csvWriter = new Mock<ICsvWriter>();
            var exporter = new InterviewItemsOrderExporter(csvWriter.Object);

            exporter.WriteHeaders(Create.QuestionnaireExportStructure(questionnaire), questionnaire, "base", "tab");

            csvWriter.Verify(x => x.WriteData(System.IO.Path.Combine("base", "s__order.tab"),
                It.Is<IEnumerable<string[]>>(d => d.Single().SequenceEqual(new[]
                    { "interview__key", "interview__id", "randomization__group", "variable", "type", "order" })),
                It.IsAny<string>()), Times.Once);
            csvWriter.Verify(x => x.WriteData(System.IO.Path.Combine("base", "r__order.tab"),
                It.Is<IEnumerable<string[]>>(d => d.Single().SequenceEqual(new[]
                    { "interview__key", "interview__id", "r__id", "randomization__group", "variable", "type", "order" })),
                It.IsAny<string>()), Times.Once);
            csvWriter.VerifyNoOtherCalls();
        }

        [Test]
        public void should_use_parent_roster_id_columns_for_nested_roster_level()
        {
            var nestedQuestion = Create.TextQuestion(Q6, variable: "q6"); nestedQuestion.RandomizeOrder = true;
            var nestedRoster = Create.Roster(NestedRosterId, "nested", nestedQuestion, Create.TextQuestion(Q5, variable: "q5"));
            var questionnaire = Create.QuestionnaireDocument(variableName: "q",
                children: Create.Group(SectionId, "s", Create.Roster(RosterId, "r", nestedRoster)));

            var header = new InterviewItemsOrderExporter(Mock.Of<ICsvWriter>())
                .GetHeader(Create.QuestionnaireExportStructure(questionnaire), questionnaire, NestedRosterId);

            header.Should().Equal("interview__key", "interview__id", "r__id", "nested__id", "randomization__group", "variable", "type", "order");
        }

        [Test]
        public void should_write_header_for_randomized_sections_of_questionnaire()
        {
            var section1 = Create.Group(SectionId, "s1", Create.TextQuestion(Q1)); section1.RandomizeOrder = true;
            var section2 = Create.Group(Id.gD, "s2", Create.TextQuestion(Q2)); section2.RandomizeOrder = true;
            var questionnaire = Create.QuestionnaireDocument(variableName: "q", children: new[] { section1, section2 });
            var csvWriter = new Mock<ICsvWriter>();

            new InterviewItemsOrderExporter(csvWriter.Object)
                .WriteHeaders(Create.QuestionnaireExportStructure(questionnaire), questionnaire, "base", "tab");

            csvWriter.Verify(x => x.WriteData(System.IO.Path.Combine("base", "q__order.tab"),
                It.Is<IEnumerable<string[]>>(d => d.Single().SequenceEqual(new[]
                    { "interview__key", "interview__id", "randomization__group", "variable", "type", "order" })),
                It.IsAny<string>()), Times.Once);
        }

        private static QuestionnaireDocument CreateQuestionnaire()
        {
            var q5 = Create.TextQuestion(Q5, variable: "q5"); q5.RandomizeOrder = true;
            var q6 = Create.TextQuestion(Q6, variable: "q6"); q6.RandomizeOrder = true;
            var roster = Create.Roster(RosterId, "r", q5, q6);
            roster.RandomizeOrder = true;

            var q1 = Create.TextQuestion(Q1, variable: "q1"); q1.RandomizeOrder = true;
            var q2 = Create.TextQuestion(Q2, variable: "q2"); q2.RandomizeOrder = true;
            var q3 = Create.TextQuestion(Q3, variable: "q3"); q3.RandomizeOrder = true;
            var staticText = Create.StaticText(StaticTextId); staticText.RandomizeOrder = true;
            var q4 = Create.TextQuestion(Q4, variable: "q4");

            return Create.QuestionnaireDocument(variableName: "q",
                children: Create.Group(SectionId, "s", q1, q2, q3, staticText, q4, roster));
        }

        private static List<InterviewEntity> CreateInterviewEntities(Guid interviewId)
        {
            var entities = new List<InterviewEntity>
            {
                Create.InterviewEntity(interviewId, EntityType.Section, Create.Identity(SectionId)),
                Create.InterviewEntity(interviewId, EntityType.Question, Create.Identity(Q1)),
                Create.InterviewEntity(interviewId, EntityType.Question, Create.Identity(Q2), isEnabled: false),
                Create.InterviewEntity(interviewId, EntityType.Question, Create.Identity(Q3)),
                Create.InterviewEntity(interviewId, EntityType.StaticText, Create.Identity(StaticTextId)),
                Create.InterviewEntity(interviewId, EntityType.Question, Create.Identity(Q4)),
            };

            foreach (var code in new[] { 1, 2, 3 })
            {
                entities.Add(Create.InterviewEntity(interviewId, EntityType.Section, Create.Identity(RosterId, code)));
                entities.Add(Create.InterviewEntity(interviewId, EntityType.Question, Create.Identity(Q5, code)));
                entities.Add(Create.InterviewEntity(interviewId, EntityType.Question, Create.Identity(Q6, code)));
            }

            return entities;
        }
    }
}
