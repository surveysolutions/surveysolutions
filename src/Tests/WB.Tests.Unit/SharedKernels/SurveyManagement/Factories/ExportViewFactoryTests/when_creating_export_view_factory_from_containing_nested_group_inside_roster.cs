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
    internal class when_creating_export_view_factory_from_containing_nested_group_inside_roster : ExportViewFactoryTestsContext
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
            exportViewFactory = CreateExportViewFactory();
        };

        Because of = () =>
            questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument, 1);

        It should_create_header_with_1_column_for_question_inside_nested_group = () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterSizeQuestionId }].HeaderItems[questionInsideNestedGroupId].ColumnNames.Length.ShouldEqual(1);

        It should_create_header_with_1_title_for_question_inside_nested_group = () =>
           questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterSizeQuestionId }].HeaderItems[questionInsideNestedGroupId].Titles.Length.ShouldEqual(1);

        It should_create_header_with_1_column_for_question_inside_nested_group_with_name_equal_to_questions_variable_name = () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterSizeQuestionId }].HeaderItems[questionInsideNestedGroupId].ColumnNames[0]
                .ShouldEqual(questionInsideNestedGroupVariableName);

        It should_create_header_with_1_column_for_question_inside_nested_group_with_title_equal_to_questions_title = () =>
           questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterSizeQuestionId }].HeaderItems[questionInsideNestedGroupId].Titles[0]
               .ShouldEqual(questionInsideNestedGroupTitle);


        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static ExportViewFactory exportViewFactory;
        private static Guid rosterId;
        private static Guid nestedGroupId;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid questionInsideNestedGroupId;
        private static Guid rosterSizeQuestionId;
        private static string questionInsideNestedGroupVariableName="q1";
        private static string questionInsideNestedGroupTitle = "q title";
    }
}
