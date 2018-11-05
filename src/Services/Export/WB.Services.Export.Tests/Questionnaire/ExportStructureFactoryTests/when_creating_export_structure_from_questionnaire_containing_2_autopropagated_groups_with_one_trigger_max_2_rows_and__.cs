using System;
using FluentAssertions;
using Moq;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests
{
     internal class when_creating_export_structure_from_questionnaire_containing_2_autopropagated_groups_with_one_trigger_max_2_rows_and__ : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(
                Create.NumericIntegerQuestion(numericTriggerQuestionId),
                Create.Roster(roster1Id, rosterSizeQuestionId: numericTriggerQuestionId, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, children: new []
                {
                    Create.NumericIntegerQuestion(linkedToQuestionId)
                }),
                Create.Roster(roster2Id, rosterSizeQuestionId: numericTriggerQuestionId, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, children: new[]
                {
                    Create.MultyOptionsQuestion(linkedQuestionId, linkedToQuestionId: linkedToQuestionId)
                }));

            exportViewFactory = CreateExportViewFactory();
            BecauseOf();
        }

        public void BecauseOf() =>
            questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument);

        [NUnit.Framework.Test] public void should_create_header_with_60_column () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { roster2Id }].HeaderItems[linkedQuestionId].ColumnHeaders.Count.Should().Be(60);

        [NUnit.Framework.Test] public void should_create_header_with_nullable_level_labels () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].LevelLabels.Should().BeNull();

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static QuestionnaireDocument questionnaireDocument;
        private static readonly Guid numericTriggerQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid linkedQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid linkedToQuestionId = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid roster1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid roster2Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private QuestionnaireExportStructureFactory exportViewFactory;
    }
}
