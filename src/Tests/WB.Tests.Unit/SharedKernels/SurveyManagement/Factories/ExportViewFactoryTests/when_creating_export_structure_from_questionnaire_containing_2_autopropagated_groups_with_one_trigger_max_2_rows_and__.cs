using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_2_autopropagated_groups_with_one_trigger_max_2_rows_and__ : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            numericTriggerQuestionId = Guid.NewGuid();
            linkedQuestionId = Guid.NewGuid();
            referencedQuestionId = Guid.NewGuid();

            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion("i am auto propagate") { PublicKey = numericTriggerQuestionId },
                new Group("i am roster1")
                {
                    IsRoster = true,
                    RosterSizeQuestionId = numericTriggerQuestionId,
                    Children =
                        new List<IComposite>
                        {
                            new NumericQuestion() { PublicKey = referencedQuestionId, QuestionType = QuestionType.Numeric }
                        }
                },
                new Group("i am roster2")
                {
                    IsRoster = true,
                    RosterSizeQuestionId = numericTriggerQuestionId,
                    Children =
                        new List<IComposite>
                        {
                            new MultyOptionsQuestion()
                            {
                                LinkedToQuestionId = referencedQuestionId,
                                PublicKey = linkedQuestionId,
                                QuestionType = QuestionType.MultyOption
                            }
                        }
                });
            exportViewFactory = CreateExportViewFactory();
        };

        Because of = () =>
            questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument, 1);

        It should_create_header_with_40_column = () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { numericTriggerQuestionId }].HeaderItems[linkedQuestionId].ColumnNames.Length.ShouldEqual(40);

        It should_create_header_with_nullable_level_labels = () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].LevelLabels.ShouldBeNull();

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static ExportViewFactory exportViewFactory;
        private static Guid linkedQuestionId;
        private static Guid referencedQuestionId;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid numericTriggerQuestionId;
    }
}
