using System;
using System.Collections.Generic;
using FluentAssertions;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests
{
      internal class when_creating_export_structure_from_questionnaire_containing_roter_triggered_by_TextListQuestion : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            rosterSizeQuestionId = Guid.Parse("AAF000AAA111EE2DD2EE111AAA000FFF");
            var rosterGroupId = Guid.Parse("00F000AAA111EE2DD2EE111AAA000FFF");
            questionInsideRosterId = Guid.Parse("CCF000AAA111EE2DD2EE111AAA000FFF");

            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new TextListQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.TextList,
                    MaxAnswerCount = maxAnswerCount
                },
                new Group()
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IQuestionnaireEntity>
                    {
                        new NumericQuestion() { PublicKey = questionInsideRosterId, QuestionType = QuestionType.Numeric }
                    }
                });

            exportViewFactory = CreateExportViewFactory();
            BecauseOf();
        }

        public void BecauseOf() =>
            questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument);

        [NUnit.Framework.Test] public void should_create_header_with_1_column () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterSizeQuestionId }].HeaderItems[questionInsideRosterId].ColumnHeaders.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_create_header_with_5_columns_at_first_level () =>
          questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].HeaderItems[rosterSizeQuestionId].ColumnHeaders.Count.Should().Be(5);

        [NUnit.Framework.Test] public void should_create_header_with_nullable_level_labels () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].LevelLabels.Should().BeNull();

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static Guid questionInsideRosterId;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid rosterSizeQuestionId;
        private static int maxAnswerCount=5;
        private QuestionnaireExportStructureFactory exportViewFactory;
    }
}
