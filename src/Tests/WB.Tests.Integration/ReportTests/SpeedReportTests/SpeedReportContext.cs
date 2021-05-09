using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.ServicesIntegration.Export;
using WB.Tests.Abc;

namespace WB.Tests.Integration.ReportTests.SpeedReportTests
{
    internal class SpeedReportContext : ReportContext
    {
        protected SpeedReportFactory CreateSpeedReport(IQueryableReadSideRepositoryReader<InterviewSummary> summaries)
        {
            return new SpeedReportFactory(summaries);
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

        public InterviewSummary AppendSpeedReportInfo(InterviewSummary interviewSummary)
        {
            var firstAnswerSet = interviewSummary.InterviewCommentedStatuses.FirstOrDefault(s =>
                s.Status == InterviewExportedAction.FirstAnswerSet);
            var created = interviewSummary.InterviewCommentedStatuses.FirstOrDefault(s =>
                s.Status == InterviewExportedAction.Created);

            interviewSummary.CreatedDate = created?.Timestamp ?? DateTime.UtcNow;
            interviewSummary.FirstAnswerDate = firstAnswerSet?.Timestamp;
            interviewSummary.FirstInterviewerName = firstAnswerSet?.InterviewerName;
            interviewSummary.FirstInterviewerId = firstAnswerSet?.InterviewerId;
            interviewSummary.FirstSupervisorName = firstAnswerSet?.SupervisorName;
            interviewSummary.FirstSupervisorId = firstAnswerSet?.SupervisorId;

            return interviewSummary;
        }
    }
}
