using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Views.ExportedHeaderCollectionTests
{
    internal class when_questionnarie_template_contains_2_autopropagated_groups_with_one_trigger_max_2_rows_and_multy_option_linked_question : ExportedHeaderCollectionTestsContext
    {
        Establish context = () =>
        {
            var autoPropagateQuestionId = Guid.NewGuid();
            linkedQuestionId = Guid.NewGuid();
            referencedQuestionId = Guid.NewGuid();

            var questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new AutoPropagateQuestion("i am auto propagate") { PublicKey = autoPropagateQuestionId, MaxValue = 2 });

            var referenceInfoForLinkedQuestions = CreateReferenceInfoForLinkedQuestionsWithOneLink(linkedQuestionId, autoPropagateQuestionId,
                referencedQuestionId);

            headerCollection = CreateExportedHeaderCollection(referenceInfoForLinkedQuestions, questionnaireDocument);
        };

        Because of = () =>
            headerCollection.Add(new MultyOptionsQuestion() { LinkedToQuestionId = referencedQuestionId,PublicKey = linkedQuestionId, QuestionType = QuestionType.MultyOption});

        It should_create_header_with_2_column = () =>
            headerCollection[linkedQuestionId].ColumnNames.Length.ShouldEqual(2);

        private static ExportedHeaderCollection headerCollection;
        private static Guid linkedQuestionId;
        private static Guid referencedQuestionId;
    }
}
