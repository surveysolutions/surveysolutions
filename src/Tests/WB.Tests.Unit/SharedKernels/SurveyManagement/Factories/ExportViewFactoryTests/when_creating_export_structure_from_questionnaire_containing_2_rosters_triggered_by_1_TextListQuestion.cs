using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_2_rosters_triggered_by_1_TextListQuestion : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new TextListQuestion("text list roster size question")
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.TextList,
                    MaxAnswerCount = maxAnswerCount
                },
                new Group("roster group 1")
                {
                    PublicKey = rosterGroup1Id,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion() { PublicKey = questionInsideRoster1Id, QuestionType = QuestionType.Numeric }
                    }
                },
                new Group("roster group 2")
                {
                    PublicKey = rosterGroup2Id,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion() { PublicKey = questionInsideRoster2Id, QuestionType = QuestionType.Numeric }
                    }
                });
            exportViewFactory = CreateExportViewFactory();
        };

        Because of = () =>
            questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument, 1);

        It should_create_header_with_1_column = () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterSizeQuestionId }].HeaderItems[questionInsideRoster1Id].ColumnNames.Length.ShouldEqual(1);

        It should_create_header_with_5_columns_at_first_level = () =>
          questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].HeaderItems[rosterSizeQuestionId].ColumnNames.Length.ShouldEqual(maxAnswerCount);

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static ExportViewFactory exportViewFactory;
        private static Guid questionInsideRoster1Id = Guid.Parse("CCF000AAA111EE2DD2EE111AAA000FFF");
        private static Guid questionInsideRoster2Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid rosterSizeQuestionId = Guid.Parse("AAF000AAA111EE2DD2EE111AAA000FFF");
        private static Guid rosterGroup1Id = Guid.Parse("00F000AAA111EE2DD2EE111AAA000FFF");
        private static Guid rosterGroup2Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static int maxAnswerCount = 5;
    }
}
