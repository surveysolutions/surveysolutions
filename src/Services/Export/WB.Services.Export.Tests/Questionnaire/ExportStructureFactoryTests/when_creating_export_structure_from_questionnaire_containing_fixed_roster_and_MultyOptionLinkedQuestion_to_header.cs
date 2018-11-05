using System;
using FluentAssertions;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_fixed_roster_and_MultyOptionLinkedQuestion_to_header :ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            rosterGroupId = Guid.Parse("00F000AAA111EE2DD2EE111AAA000FFF");
            linkedQuestionId = Guid.Parse("BBF000AAA111EE2DD2EE111AAA000FFF");
            referencedQuestionId = Guid.Parse("CCF000AAA111EE2DD2EE111AAA000FFF");

            questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.Roster(rosterId: rosterGroupId,
                    obsoleteFixedTitles: new[] {"1", "2"}, 
                    children: new IQuestionnaireEntity[]
                    {
                        new NumericQuestion() {PublicKey = referencedQuestionId, QuestionType = QuestionType.Numeric},
                        new MultyOptionsQuestion()
                        {
                            LinkedToQuestionId = referencedQuestionId,
                            PublicKey = linkedQuestionId,
                            QuestionType = QuestionType.MultyOption
                        }
                    }));


            QuestionnaireExportStructureFactory = CreateExportViewFactory();
            BecauseOf();
        }

        private void BecauseOf() =>
            questionnaireExportStructure = QuestionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

        [NUnit.Framework.Test] public void should_create_header_with_2_column () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterGroupId }].HeaderItems[linkedQuestionId].ColumnHeaders.Count.Should().Be(2);

        [NUnit.Framework.Test] public void should_create_header_with_2_header_labels () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterGroupId }].LevelLabels.Length.Should().Be(2);

        [NUnit.Framework.Test] public void should_create_header_with_first_header_label_title_equal_1 () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterGroupId }].LevelLabels[0].Title.Should().Be("1");

        [NUnit.Framework.Test] public void should_create_header_with_first_header_label_caption_equal_1 () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterGroupId }].LevelLabels[0].Caption.Should().Be("0");

        [NUnit.Framework.Test] public void should_create_header_with_second_header_label_title_equal_1 () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterGroupId }].LevelLabels[1].Title.Should().Be("2");

        [NUnit.Framework.Test] public void should_create_header_with_second_header_label_caption_equal_1 () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterGroupId }].LevelLabels[1].Caption.Should().Be("1");

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static QuestionnaireExportStructureFactory QuestionnaireExportStructureFactory;
        private static Guid linkedQuestionId;
        private static Guid referencedQuestionId;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId;
    }
}
