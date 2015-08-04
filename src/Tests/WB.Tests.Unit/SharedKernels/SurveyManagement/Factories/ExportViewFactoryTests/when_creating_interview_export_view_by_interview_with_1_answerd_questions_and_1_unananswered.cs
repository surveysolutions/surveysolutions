using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewExportedDataEventHandlerTests;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_interview_export_view_by_interview_with_1_answerd_questions_and_1_unananswered:  ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            answeredQuestionId = Guid.Parse("10000000000000000000000000000000");
            unansweredQuestionId = Guid.Parse("11111111111111111111111111111111");
            variableNameAndQuestionId = new Dictionary<string, Guid>
            {
                { "q1", answeredQuestionId },
                { "q2", unansweredQuestionId }
            };
            questionnaireDocument = CreateQuestionnaireDocument(variableNameAndQuestionId);
            exportViewFactory = CreateExportViewFactory();
        };

        Because of = () =>
            result = exportViewFactory.CreateInterviewDataExportView(exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument, 1),
                CreateInterviewWithAnswers(variableNameAndQuestionId.Values.Take(1)));

        It should_records_count_equals_1 = () =>
            result.Levels[0].Records.Length.ShouldEqual(1);

        It should__first_record_have_1_answers = () =>
            result.Levels[0].Records[0].Questions.Length.ShouldEqual(2);

        It should_first_record_id_equals_0 = () =>
            result.Levels[0].Records[0].RecordId.ShouldEqual(result.Levels[0].Records[0].InterviewId.FormatGuid());

        It should_first_parent_ids_be_empty = () =>
           result.Levels[0].Records[0].ParentRecordIds.ShouldBeEmpty();

        It should_answered_question_be_not_empty = () =>
           result.Levels[0].Records[0].Questions.ShouldQuestionHasOneNotEmptyAnswer(answeredQuestionId);

        It should_unanswered_question_be_empty = () =>
          result.Levels[0].Records[0].Questions.ShouldQuestionHasNoAnswers(unansweredQuestionId);

        private static ExportViewFactory exportViewFactory;
        private static InterviewDataExportView result;
        private static Dictionary<string, Guid> variableNameAndQuestionId;
        private static Guid answeredQuestionId;
        private static Guid unansweredQuestionId;
        private static QuestionnaireDocument questionnaireDocument;
    }
}
