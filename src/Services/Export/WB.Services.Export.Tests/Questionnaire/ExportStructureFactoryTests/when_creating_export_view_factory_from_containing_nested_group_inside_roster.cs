using System;
using System.Collections.Generic;
using FluentAssertions;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests
{
    internal class when_creating_export_view_factory_from_containing_nested_group_inside_roster : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionInsideNestedGroupId = Guid.NewGuid();
            rosterId = Guid.NewGuid();
            nestedGroupId = Guid.NewGuid();
            rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                Create.NumericIntegerQuestion(id: rosterSizeQuestionId),
                Create.Roster(rosterId: rosterId, rosterSizeQuestionId: rosterSizeQuestionId, children: new List<IQuestionnaireEntity>
                {
                    Create.Group(
                        children:
                        Create.NumericIntegerQuestion(id: questionInsideNestedGroupId,
                            variable: questionInsideNestedGroupVariableName, 
                            questionText: questionInsideNestedGroupTitle))
                }));

            QuestionnaireExportStructureFactory = CreateExportViewFactory();
            BecauseOf();

        }

        public void BecauseOf() =>
            questionnaireExportStructure = QuestionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaireDocument);

        [NUnit.Framework.Test] public void should_create_header_with_1_column_for_question_inside_nested_group () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterSizeQuestionId }].HeaderItems[questionInsideNestedGroupId].ColumnHeaders.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_create_header_with_1_column_for_question_inside_nested_group_with_name_equal_to_questions_variable_name () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterSizeQuestionId }].HeaderItems[questionInsideNestedGroupId].ColumnHeaders[0].Name
                .Should().Be(questionInsideNestedGroupVariableName);

        [NUnit.Framework.Test] public void should_create_header_with_1_column_for_question_inside_nested_group_with_title_equal_to_questions_title () =>
           questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterSizeQuestionId }].HeaderItems[questionInsideNestedGroupId].ColumnHeaders[0].Title
               .Should().Be(questionInsideNestedGroupTitle);


        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static QuestionnaireExportStructureFactory QuestionnaireExportStructureFactory;
        private static Guid rosterId;
        private static Guid nestedGroupId;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid questionInsideNestedGroupId;
        private static Guid rosterSizeQuestionId;
        private static string questionInsideNestedGroupVariableName="q1";
        private static string questionInsideNestedGroupTitle = "q title";
    }
}
