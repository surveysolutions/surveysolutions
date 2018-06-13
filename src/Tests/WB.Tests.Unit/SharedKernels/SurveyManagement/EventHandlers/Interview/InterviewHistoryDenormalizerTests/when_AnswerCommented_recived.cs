using System;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    internal class when_AnswerCommented_recived : InterviewHistoryDenormalizerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            interviewHistoryView = CreateInterviewHistoryView();
            interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer(questionnaire: CreateQuestionnaireExportStructure(questionId, variableName));
            BecauseOf();
        }

        public void BecauseOf() =>
            interviewHistoryView = interviewExportedDataDenormalizer.Update(interviewHistoryView, CreatePublishableEvent(() => new AnswerCommented(Guid.NewGuid(), questionId, new decimal[] { 1 }, DateTime.Now, comment),
                interviewId));

        [NUnit.Framework.Test] public void should_action_of_first_record_be_CommentSet () =>
            interviewHistoryView.Records[0].Action.Should().Be(InterviewHistoricalAction.CommentSet);

        [NUnit.Framework.Test] public void should_first_record_has_commented_question_variable_name_in_parameters () =>
            interviewHistoryView.Records[0].Parameters["question"].Should().Be(variableName);

        [NUnit.Framework.Test] public void should_first_record_has_commented_question_comment_in_parameters () =>
            interviewHistoryView.Records[0].Parameters["comment"].Should().Be(comment);

        [NUnit.Framework.Test] public void should_first_record_has_commented_question_roster_vector_in_parameters () =>
            interviewHistoryView.Records[0].Parameters["roster"].Should().Be("1");


        private static InterviewParaDataEventHandler interviewExportedDataDenormalizer;
        private static Guid interviewId = Guid.NewGuid();
        private static InterviewHistoryView interviewHistoryView;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static string variableName = "q1";
        private static string comment = "comment";
    }
}
