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

namespace WB.Core.SharedKernels.SurveyManagement.Tests.Factories.ExportViewFactoryTests
{
    internal class WhenCreatingExportViewFactoryFromContaining2AutopropagatedGroupsWithOneTriggerMax2RowsAndMultyOptionLinkedQuestionToHeader : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            numericTriggerQuestionId = Guid.NewGuid();
            linkedQuestionId = Guid.NewGuid();
            referencedQuestionId = Guid.NewGuid();

            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion("i am auto propagate") { PublicKey = numericTriggerQuestionId, MaxValue = 2 },
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

        private Because of = () =>
            questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument, 1);

        It should_create_header_with_2_column = () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { numericTriggerQuestionId }].HeaderItems[linkedQuestionId].ColumnNames.Length.ShouldEqual(2);

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
