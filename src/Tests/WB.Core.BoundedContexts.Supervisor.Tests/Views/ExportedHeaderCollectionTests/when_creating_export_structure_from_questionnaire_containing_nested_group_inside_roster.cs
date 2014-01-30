using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Views.ExportedHeaderCollectionTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_nested_group_inside_roster : QuestionnaireExportStructureTestsContext
    {
        Establish context = () =>
        {
            questionInsideNestedGroupId = Guid.NewGuid();
            rosterId = Guid.NewGuid();
            nestedGroupId = Guid.NewGuid();
            rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric
                },
                new Group()
                {
                    PublicKey = rosterId,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new Group("title")
                        {
                            PublicKey = nestedGroupId,
                            Children = new List<IComposite>
                            {
                                new NumericQuestion()
                                {
                                    PublicKey = questionInsideNestedGroupId,
                                    QuestionType = QuestionType.Numeric,
                                    StataExportCaption = questionInsideNestedGroupVariableName,
                                    QuestionText = questionInsideNestedGroupTitle
                                }
                            }
                        }
                    }
                });
        };

        Because of = () =>
            questionnaireExportStructure = CreateQuestionnaireExportStructure(questionnaireDocument);

        It should_create_header_with_1_column_for_question_inside_nested_group = () =>
            questionnaireExportStructure.HeaderToLevelMap[rosterSizeQuestionId].HeaderItems[questionInsideNestedGroupId].ColumnNames.Length.ShouldEqual(1);

        It should_create_header_with_1_title_for_question_inside_nested_group = () =>
           questionnaireExportStructure.HeaderToLevelMap[rosterSizeQuestionId].HeaderItems[questionInsideNestedGroupId].Titles.Length.ShouldEqual(1);

        It should_create_header_with_1_column_for_question_inside_nested_group_with_name_equal_to_questions_variable_name = () =>
            questionnaireExportStructure.HeaderToLevelMap[rosterSizeQuestionId].HeaderItems[questionInsideNestedGroupId].ColumnNames[0]
                .ShouldEqual(questionInsideNestedGroupVariableName);

        It should_create_header_with_1_column_for_question_inside_nested_group_with_title_equal_to_questions_title = () =>
           questionnaireExportStructure.HeaderToLevelMap[rosterSizeQuestionId].HeaderItems[questionInsideNestedGroupId].Titles[0]
               .ShouldEqual(questionInsideNestedGroupTitle);


        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static Guid rosterId;
        private static Guid nestedGroupId;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid questionInsideNestedGroupId;
        private static Guid rosterSizeQuestionId;
        private static string questionInsideNestedGroupVariableName="q1";
        private static string questionInsideNestedGroupTitle = "q title";
    }
}
