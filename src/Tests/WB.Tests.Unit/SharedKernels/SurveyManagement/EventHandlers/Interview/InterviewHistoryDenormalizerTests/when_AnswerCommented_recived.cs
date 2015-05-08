using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    internal class when_AnswerCommented_recived : InterviewHistoryDenormalizerTestContext
    {
        Establish context = () =>
        {
            interviewHistoryView = CreateInterviewHistoryView();
            interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer(questionnaire: CreateQuestionnaireExportStructure(questionId, variableName));
        };

        Because of = () =>
            interviewHistoryView = interviewExportedDataDenormalizer.Update(interviewHistoryView, CreatePublishableEvent(() => new AnswerCommented(Guid.NewGuid(), questionId, new decimal[] { 1 }, DateTime.Now, comment),
                interviewId));

        It should_action_of_first_record_be_CommentSet = () =>
            interviewHistoryView.Records[0].Action.ShouldEqual(InterviewHistoricalAction.CommentSet);

        It should_first_record_has_commented_question_variable_name_in_parameters = () =>
            interviewHistoryView.Records[0].Parameters["question"].ShouldEqual(variableName);

        It should_first_record_has_commented_question_comment_in_parameters = () =>
            interviewHistoryView.Records[0].Parameters["comment"].ShouldEqual(comment);

        It should_first_record_has_commented_question_roster_vector_in_parameters = () =>
            interviewHistoryView.Records[0].Parameters["roster"].ShouldEqual("1");


        private static InterviewHistoryDenormalizer interviewExportedDataDenormalizer;
        private static Guid interviewId = Guid.NewGuid();
        private static InterviewHistoryView interviewHistoryView;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static string variableName = "q1";
        private static string comment = "comment";
    }
}
