using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Views.ExportedHeaderCollectionTests
{
    internal class when_adding_multy_option_linked_question_to_collection_from_questionnaire_containing_fixed_roster : ExportedHeaderCollectionTestsContext
    {
        Establish context = () =>
        {
            var rosterGroupId = Guid.Parse("00F000AAA111EE2DD2EE111AAA000FFF");
            linkedQuestionId = Guid.Parse("BBF000AAA111EE2DD2EE111AAA000FFF");
            referencedQuestionId = Guid.Parse("CCF000AAA111EE2DD2EE111AAA000FFF");

            var questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new Group("roster group")
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    RosterFixedTitles = new string[] { "1", "2" }
                });

            var referenceInfoForLinkedQuestions = CreateReferenceInfoForLinkedQuestionsWithOneLink(linkedQuestionId, rosterGroupId,
                referencedQuestionId);

            headerCollection = CreateExportedHeaderCollection(referenceInfoForLinkedQuestions, questionnaireDocument);
        };

        Because of = () =>
            headerCollection.Add(new MultyOptionsQuestion() { LinkedToQuestionId = referencedQuestionId, PublicKey = linkedQuestionId, QuestionType = QuestionType.MultyOption });

        It should_create_header_with_2_column = () =>
            headerCollection[linkedQuestionId].ColumnNames.Length.ShouldEqual(2);

        private static ExportedHeaderCollection headerCollection;
        private static Guid linkedQuestionId;
        private static Guid referencedQuestionId;
    }
}
