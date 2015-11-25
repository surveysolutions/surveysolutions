using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_questionnaire_has_multioption_lined_question : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            multyOptionLinkedQuestionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");
            linkedSourceQuestionId = Guid.NewGuid();

            questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.Roster(rosterId: Guid.NewGuid(), variable: "row", fixedTitles: new[] {"1", "2"},
                    children: new[] {Create.TextQuestion(questionId: linkedSourceQuestionId, variable: "varTxt")}),
                Create.MultyOptionsQuestion(id: multyOptionLinkedQuestionId,
                    variable: "mult",
                    linkedToQuestionId: linkedSourceQuestionId));

            exportViewFactory = CreateExportViewFactory();
        };

        Because of = () => questionnaaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaire, 1);

        It should_fill_multioption_question_header_title = () =>
        {
            HeaderStructureForLevel headerStructureForLevel = questionnaaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()];
            ExportedHeaderItem exportedHeaderItem = headerStructureForLevel.HeaderItems[multyOptionLinkedQuestionId];

            exportedHeaderItem.ColumnNames.Length.ShouldEqual(2);
            exportedHeaderItem.ColumnNames.SequenceEqual(new[] { "mult_0", "mult_1" }).ShouldBeTrue();
            exportedHeaderItem.QuestionSubType.ShouldEqual(QuestionSubtype.MultyOption_Linked);
            exportedHeaderItem.QuestionType.ShouldEqual(QuestionType.MultyOption);
        };

        static ExportViewFactory exportViewFactory;
        static QuestionnaireExportStructure questionnaaireExportStructure;
        static QuestionnaireDocument questionnaire;
        static Guid multyOptionLinkedQuestionId;
        static Guid linkedSourceQuestionId;
    }
}