using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.Factories.ExportViewFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_roster_max_2_rows_and_MultyOptionLinkedQuestion : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            autoPropagateQuestionId = Guid.NewGuid();
            var rosterGroupId = Guid.NewGuid();
            linkedQuestionId = Guid.NewGuid();
            referencedQuestionId = Guid.NewGuid();

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new AutoPropagateQuestion("i am auto propagate")
                {
                    PublicKey = autoPropagateQuestionId,
                    MaxValue = 2,
                    Triggers = new List<Guid> { rosterGroupId },
                    QuestionType = QuestionType.AutoPropagate
                },
                new Group("roster group")
                {
                    PublicKey = rosterGroupId,
                    Propagated = Propagate.AutoPropagated,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion() { PublicKey = referencedQuestionId, QuestionType = QuestionType.Numeric },
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
            headerStructureForLevel = exportViewFactory.CreateQuestionnaireExportStructure(questionnaire, 1);

        It should_create_header_with_2_column = () =>
            headerStructureForLevel.HeaderToLevelMap[autoPropagateQuestionId].HeaderItems[linkedQuestionId].ColumnNames.Length.ShouldEqual(2);

        private static QuestionnaireExportStructure headerStructureForLevel;
        private static ExportViewFactory exportViewFactory;
        private static Guid linkedQuestionId;
        private static Guid referencedQuestionId;
        private static QuestionnaireDocument questionnaire;
        private static Guid autoPropagateQuestionId;
    }
}
