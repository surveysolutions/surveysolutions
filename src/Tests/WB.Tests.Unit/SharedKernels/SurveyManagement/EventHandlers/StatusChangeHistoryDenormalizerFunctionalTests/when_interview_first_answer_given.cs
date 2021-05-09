using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.ServicesIntegration.Export;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.StatusChangeHistoryDenormalizerFunctionalTests
{
    internal class when_interview_first_answer_given : StatusChangeHistoryDenormalizerFunctionalTestContext
    {
        [Test]
        public void should_record_first_answer_status()
        {
            var interviewStatuses = Create.Entity.InterviewSummary(statuses: new [] { Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.InterviewerAssigned, statusId: Guid.Parse("11111111111111111111111111111111")) } );

            // Act
            var result = CreateStatusChangeHistoryDenormalizerFunctional().Update(interviewStatuses, Create.PublishedEvent.TextQuestionAnswered(interviewId: Guid.NewGuid()));

            // Assert
            Assert.That(result.InterviewCommentedStatuses.Last(), Has.Property(nameof(InterviewCommentedStatus.Status)).EqualTo(InterviewExportedAction.FirstAnswerSet));
        }
    }
}
