using System;
using System.Collections.Generic;
using FluentAssertions;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_2_rosters_triggered_by_1_TextListQuestion : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new TextListQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.TextList,
                    MaxAnswerCount = maxAnswerCount
                },
                new Group()
                {
                    PublicKey = rosterGroup1Id,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IQuestionnaireEntity>
                    {
                        new NumericQuestion() { PublicKey = questionInsideRoster1Id, QuestionType = QuestionType.Numeric }
                    }
                },
                new Group()
                {
                    PublicKey = rosterGroup2Id,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IQuestionnaireEntity>
                    {
                        new NumericQuestion() { PublicKey = questionInsideRoster2Id, QuestionType = QuestionType.Numeric }
                    }
                });

            exportViewFactory = CreateExportViewFactory();
            BecauseOf();
        }

        public void BecauseOf() =>
            questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument);

        [NUnit.Framework.Test] public void should_create_header_with_1_column () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterSizeQuestionId }].HeaderItems[questionInsideRoster1Id].ColumnHeaders.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_create_header_with_5_columns_at_first_level () =>
          questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].HeaderItems[rosterSizeQuestionId].ColumnHeaders.Count.Should().Be(maxAnswerCount);

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static Guid questionInsideRoster1Id = Guid.Parse("CCF000AAA111EE2DD2EE111AAA000FFF");
        private static Guid questionInsideRoster2Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid rosterSizeQuestionId = Guid.Parse("AAF000AAA111EE2DD2EE111AAA000FFF");
        private static Guid rosterGroup1Id = Guid.Parse("00F000AAA111EE2DD2EE111AAA000FFF");
        private static Guid rosterGroup2Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static int maxAnswerCount = 5;

        private static Guid questionnaireId = Guid.Parse("ABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private QuestionnaireExportStructureFactory exportViewFactory;
    }
}
