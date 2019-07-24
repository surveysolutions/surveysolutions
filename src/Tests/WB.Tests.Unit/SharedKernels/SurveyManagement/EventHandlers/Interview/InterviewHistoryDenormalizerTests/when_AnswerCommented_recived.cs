using System;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    internal class when_AnswerCommented_received : InterviewHistoryDenormalizerTestContext
    {
        [NUnit.Framework.Test]
        public void should_action_of_first_record_be_CommentSet()
        {
            Guid interviewId = Guid.NewGuid();
            InterviewHistoryView interviewHistoryView;
            Guid questionId = Guid.Parse("11111111111111111111111111111111");
            string variableName = "q1";
            string comment = "comment";

            interviewHistoryView = CreateInterviewHistoryView();
            var interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer(questionnaire: CreateQuestionnaireExportStructure(questionId, variableName));

            // Act
            interviewHistoryView = interviewExportedDataDenormalizer.Update(interviewHistoryView, CreatePublishableEvent(() =>
                    new AnswerCommented(Guid.NewGuid(), questionId, new decimal[] { 1 }, DateTime.Now, comment, Guid.NewGuid()),
                interviewId));

            // Assert
            interviewHistoryView.Records[0].Action.Should().Be(InterviewHistoricalAction.CommentSet);
            interviewHistoryView.Records[0].Parameters["question"].Should().Be(variableName);
            interviewHistoryView.Records[0].Parameters["comment"].Should().Be(comment);
            interviewHistoryView.Records[0].Parameters["roster"].Should().Be("1");
        }
    }
}
