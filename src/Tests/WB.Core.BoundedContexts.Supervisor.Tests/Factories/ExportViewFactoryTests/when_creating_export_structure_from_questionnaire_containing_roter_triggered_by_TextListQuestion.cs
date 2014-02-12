using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Supervisor.Implementation.Factories;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Factories.ExportViewFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_roter_triggered_by_TextListQuestion : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            rosterSizeQuestionId = Guid.Parse("AAF000AAA111EE2DD2EE111AAA000FFF");
            var rosterGroupId = Guid.Parse("00F000AAA111EE2DD2EE111AAA000FFF");
            questionInsideRosterId = Guid.Parse("CCF000AAA111EE2DD2EE111AAA000FFF");

            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new TextListQuestion("text list roster size question")
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.TextList,
                    MaxAnswerCount = maxAnswerCount
                },
                new Group("roster group")
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion() { PublicKey = questionInsideRosterId, QuestionType = QuestionType.Numeric }
                    }
                });
            exportViewFactory = CreateExportViewFactory();
        };

        Because of = () =>
            questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument, 1);

        It should_create_header_with_1_column = () =>
            questionnaireExportStructure.HeaderToLevelMap[rosterSizeQuestionId].HeaderItems[questionInsideRosterId].ColumnNames.Length.ShouldEqual(1);

        It should_create_header_with_5_columns_at_first_level = () =>
          questionnaireExportStructure.HeaderToLevelMap[questionnaireDocument.PublicKey].HeaderItems[rosterSizeQuestionId].ColumnNames.Length.ShouldEqual(5);

        It should_create_header_with_nullable_level_labels = () =>
            questionnaireExportStructure.HeaderToLevelMap[questionnaireDocument.PublicKey].LevelLabels.ShouldBeNull();

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static ExportViewFactory exportViewFactory;
        private static Guid questionInsideRosterId;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid rosterSizeQuestionId;
        private static int maxAnswerCount=5;
    }
}
