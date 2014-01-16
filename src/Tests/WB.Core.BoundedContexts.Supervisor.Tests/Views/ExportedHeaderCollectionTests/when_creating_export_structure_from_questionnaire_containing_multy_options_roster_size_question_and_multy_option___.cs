using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Views.ExportedHeaderCollectionTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_multy_options_roster_size_question_and_multy_option_linked_question : QuestionnaireExportStructureTestsContext
    {
        Establish context = () =>
        {
            rosterSizeQuestionId = Guid.Parse("AAF000AAA111EE2DD2EE111AAA000FFF");
            var rosterGroupId = Guid.Parse("00F000AAA111EE2DD2EE111AAA000FFF");
            linkedQuestionId = Guid.Parse("BBF000AAA111EE2DD2EE111AAA000FFF");
            referencedQuestionId = Guid.Parse("CCF000AAA111EE2DD2EE111AAA000FFF");

            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new MultyOptionsQuestion("multy options roster size question")
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.MultyOption,
                    Answers =
                        new List<IAnswer>()
                        {
                            new Answer() { AnswerText = "option 1", AnswerValue = "op1" },
                            new Answer() { AnswerText = "option 2", AnswerValue = "opt2" }
                        }
                },
                new Group("roster group")
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion() { PublicKey = referencedQuestionId, QuestionType = QuestionType.Numeric },
                        new MultyOptionsQuestion() { LinkedToQuestionId = referencedQuestionId,PublicKey = linkedQuestionId, QuestionType = QuestionType.MultyOption}
                    }
                });
        };

        Because of = () =>
            questionnaireExportStructure = CreateQuestionnaireExportStructure(questionnaireDocument);

        It should_create_header_with_2_column = () =>
            questionnaireExportStructure.HeaderToLevelMap[rosterSizeQuestionId].HeaderItems[linkedQuestionId].ColumnNames.Length.ShouldEqual(2);

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static Guid linkedQuestionId;
        private static Guid referencedQuestionId;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid rosterSizeQuestionId;
    }
}
