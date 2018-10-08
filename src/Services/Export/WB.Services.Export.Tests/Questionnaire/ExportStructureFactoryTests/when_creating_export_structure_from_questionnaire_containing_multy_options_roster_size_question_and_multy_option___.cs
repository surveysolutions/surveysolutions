using System;
using System.Collections.Generic;
using FluentAssertions;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_multy_options_roster_size_question_and_multy_option___ :
        ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            rosterSizeQuestionId = Guid.Parse("AAF000AAA111EE2DD2EE111AAA000FFF");
            var rosterGroupId = Guid.Parse("00F000AAA111EE2DD2EE111AAA000FFF");
            var rosterGroupId2 = Guid.Parse("00F000AAA111EE2DD2EE111AAA000BBB");
            linkedQuestionId = Guid.Parse("BBF000AAA111EE2DD2EE111AAA000FFF");
            referencedQuestionId = Guid.Parse("CCF000AAA111EE2DD2EE111AAA000FFF");
            var questionId = Guid.Parse("BBF000AAA111EE2DD2EE111AAA000AAA");

            questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(
                Create.MultyOptionsQuestion(id: rosterSizeQuestionId, options: new List<Answer>()
                {
                    new Answer { AnswerText = "option 1", AnswerValue = "1" },
                    new Answer { AnswerText = "option 2", AnswerValue = "2" }
                }),
                Create.Roster(rosterId: rosterGroupId, rosterSizeQuestionId: rosterSizeQuestionId, children: new List<IQuestionnaireEntity>
                {
                    Create.NumericIntegerQuestion(id: referencedQuestionId),
                    Create.MultyOptionsQuestion(id: linkedQuestionId, linkedToQuestionId: referencedQuestionId)
                }),
                Create.Roster(rosterId: rosterGroupId2, rosterSizeQuestionId: rosterSizeQuestionId, children: new List<IQuestionnaireEntity>
                {
                    Create.NumericIntegerQuestion(id: questionId)
                }));

            QuestionnaireExportStructureFactory = CreateExportViewFactory();
            BecauseOf();
        }

        private void BecauseOf() =>
            questionnaireExportStructure = QuestionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaireDocument);

        [NUnit.Framework.Test]
        public void should_create_header_with_2_column() =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterSizeQuestionId }].HeaderItems[linkedQuestionId]
                .ColumnHeaders.Count.Should().Be(2);

        [NUnit.Framework.Test]
        public void should_create_header_with_nullable_level_labels() =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].LevelLabels.Should().BeNull();

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static QuestionnaireExportStructureFactory QuestionnaireExportStructureFactory;
        private static Guid linkedQuestionId;
        private static Guid referencedQuestionId;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid rosterSizeQuestionId;
    }
}
