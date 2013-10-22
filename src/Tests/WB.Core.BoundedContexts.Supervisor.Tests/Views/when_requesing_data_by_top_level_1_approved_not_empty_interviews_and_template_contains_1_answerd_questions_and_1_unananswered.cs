using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Views
{
    [Subject(typeof (InterviewDataExportFactory))]
    internal class when_requesing_data_by_top_level_1_approved_not_empty_interviews_and_template_contains_1_answerd_questions_and_1_unananswered :
        InterviewDataExportFactoryTestContext
    {
        private Establish context = () =>
        {
            answeredQuestionId = Guid.Parse("10000000000000000000000000000000");
            unansweredQuestionId = Guid.Parse("11111111111111111111111111111111");

            variableNameAndQuestionId = new Dictionary<string, Guid>
            {
                { "q1", answeredQuestionId },
                { "q2", unansweredQuestionId }
            };

            interviewDataExportFactory = CreateInterviewDataExportFactoryForQuestionnarieCreatedByMethod(
                () => CreateQuestionnaireDocument(variableNameAndQuestionId),
                () => CreateInterviewWithAnswers(variableNameAndQuestionId.Values.Take(1)), 1);
        };

        private Because of = () =>
            result = interviewDataExportFactory.Load(new InterviewDataExportInputModel(Guid.NewGuid(), 1, null));

        private It should_records_count_equals_1 = () =>
            result.Records.Length.ShouldEqual(1);

        private It should__first_record_have_1_answers = () =>
            result.Records[0].Questions.Count.ShouldEqual(1);

        private It should_first_record_id_equals_0 = () =>
            result.Records[0].RecordId.ShouldEqual(0);

        private It should_answered_question_be_not_empty = () =>
           result.Records[0].Questions.ShouldQuestionHasOneNotEmptyAnswer(answeredQuestionId);

        private It should_unanswered_question_be_empty = () =>
          result.Records[0].Questions.ShouldQuestionHasNoAnswers(unansweredQuestionId);

        private It should_header_column_count_be_equal_2 = () =>
            result.Header.Count().ShouldEqual(2);

        private static InterviewDataExportFactory interviewDataExportFactory;
        private static InterviewDataExportView result;
        private static Dictionary<string, Guid> variableNameAndQuestionId;
        private static Guid answeredQuestionId;
        private static Guid unansweredQuestionId;
    }
}
