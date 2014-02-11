using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Views.ExportedHeaderCollectionTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_2_autopropagated_groups_with_one_trigger_max_2_rows_and_multy_option_linked_question_to_header : QuestionnaireExportStructureTestsContext
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
        };

        Because of = () =>
            questionnaireExportStructure = CreateQuestionnaireExportStructure(questionnaireDocument);

        It should_create_header_with_2_column = () =>
            questionnaireExportStructure.HeaderToLevelMap[numericTriggerQuestionId].HeaderItems[linkedQuestionId].ColumnNames.Length.ShouldEqual(2);

        It should_create_header_with_nullable_level_labels = () =>
            questionnaireExportStructure.HeaderToLevelMap[questionnaireDocument.PublicKey].LevelLabels.ShouldBeNull();

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static Guid linkedQuestionId;
        private static Guid referencedQuestionId;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid numericTriggerQuestionId;
    }
}
