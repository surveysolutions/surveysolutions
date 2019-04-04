using System;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Tests.Abc;

namespace WB.Tests.Integration.ReportTests.SpeedReportTests
{
    internal class SpeedReportContext : ReportContext
    {
        protected SpeedReportFactory CreateSpeedReport(IQueryableReadSideRepositoryReader<InterviewSummary> summaries,
            IQueryableReadSideRepositoryReader<SpeedReportInterviewItem> speedReportStorage)
        {
            return new SpeedReportFactory(summaries, speedReportStorage);
        }

        
        protected static InterviewSummary CreateCompletedInterviewWithDuration(TimeSpan interviewingTotalTime,
            Guid supervisorId, 
            Guid responsibleId,
            DateTime dateTime,
            Guid? questionnaireId = null, 
            long? questionnaireVersion = null)
        {
            var interviewId = Guid.NewGuid();
            var interview = Create.Entity.InterviewSummary(interviewingTotalTime: interviewingTotalTime,
                interviewId: interviewId,
                teamLeadId: supervisorId,
                responsibleId: responsibleId,
                questionnaireId: questionnaireId,
                questionnaireVersion: questionnaireVersion);
            interview.SummaryId = interviewId.FormatGuid();

            var interviewCommentedStatus = Create.Entity.InterviewCommentedStatus(
                status: InterviewExportedAction.FirstAnswerSet,
                timestamp: dateTime.AddMinutes(1),
                timeSpanWithPreviousStatus: TimeSpan.FromMinutes(1),
                supervisorId: supervisorId,
                interviewSummary: interview);
            interviewCommentedStatus.InterviewSummary = interview;
            interview.InterviewCommentedStatuses.Add(interviewCommentedStatus);
            return interview;
        }

        protected static SpeedReportInterviewItem CreateSpeedReportItemForInterview(InterviewSummary interview)
        {
            return Create.Entity.SpeedReportInterviewItem(interview);
        }
    }
}
