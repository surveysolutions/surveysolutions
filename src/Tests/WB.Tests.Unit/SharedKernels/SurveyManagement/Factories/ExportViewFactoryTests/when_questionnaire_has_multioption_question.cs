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
    internal class when_questionnaire_has_multioption_question : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            questionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.MultyOptionsQuestion(id: questionId, 
                variable:"mult",
                options: new List<Answer> {
                    Create.Answer("foo", 28), Create.Answer("bar", 42)
                }));

            exportViewFactory = CreateExportViewFactory();
        };

        Because of = () => questionnaaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaire, 1);

        It should_fill_multioption_question_header_title = () =>
        {
            HeaderStructureForLevel headerStructureForLevel = questionnaaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()];
            ExportedHeaderItem exportedHeaderItem = headerStructureForLevel.HeaderItems[questionId];

            exportedHeaderItem.ColumnNames.Length.ShouldEqual(2);
            exportedHeaderItem.ColumnNames.SequenceEqual(new[] { "mult__28", "mult__42" }).ShouldBeTrue();
            exportedHeaderItem.ColumnValues.Length.ShouldEqual(2);
            exportedHeaderItem.ColumnValues.SequenceEqual(new[] {28m, 42m}).ShouldBeTrue();
        };

        static ExportViewFactory exportViewFactory;
        static QuestionnaireExportStructure questionnaaireExportStructure;
        static QuestionnaireDocument questionnaire;
        static Guid questionId;
    }
}