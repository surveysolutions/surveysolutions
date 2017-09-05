using System;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    internal abstract class AbstractTeamsAndStatusesReport
    {
        protected readonly INativeReadSideStorage<InterviewSummary> interviewsReader;

        protected AbstractTeamsAndStatusesReport(INativeReadSideStorage<InterviewSummary> interviewsReader)
        {
            this.interviewsReader = interviewsReader;
        }

        protected static IQueryable<InterviewSummary> FilterByQuestionnaireOrTeamLead(IQueryable<InterviewSummary> _, TeamsAndStatusesInputModel input)
        {
            if (input.TemplateId.HasValue)
            {
                _ = _.Where(x => x.QuestionnaireId == input.TemplateId);
            }

            if (input.TemplateVersion.HasValue)
            {
                _ = _.Where(x => x.QuestionnaireVersion == input.TemplateVersion);
            }

            if (input.ViewerId.HasValue)
            {
                _ = _.Where(x => x.TeamLeadId == input.ViewerId);
            }

            return _;
        }

        protected TeamsAndStatusesReportLine QueryReportTotalRow(TeamsAndStatusesInputModel input)
        {
            return this.interviewsReader.Query(_ =>
                    FilterByQuestionnaireOrTeamLead(_, input).Select(x => new
                        {
                            SupervisorAssigned = x.Status == InterviewStatus.SupervisorAssigned ? 1 : 0,
                            InterviewerAssigned = x.Status == InterviewStatus.InterviewerAssigned ? 1 : 0,
                            Completed = x.Status == InterviewStatus.Completed ? 1 : 0,
                            RejectedBySupervisor = x.Status == InterviewStatus.RejectedBySupervisor ? 1 : 0,
                            ApprovedBySupervisor = x.Status == InterviewStatus.ApprovedBySupervisor ? 1 : 0,
                            RejectedByHeadquarters = x.Status == InterviewStatus.RejectedByHeadquarters ? 1 : 0,
                            ApprovedByHeadquarters = x.Status == InterviewStatus.ApprovedByHeadquarters ? 1 : 0
                        })
                        .GroupBy(x => 1)
                        .Select(x => new TeamsAndStatusesReportLine
                        {
                            SupervisorAssignedCount = (int?)x.Sum(y => y.SupervisorAssigned) ?? 0,
                            InterviewerAssignedCount = (int?)x.Sum(y => y.InterviewerAssigned) ?? 0,
                            CompletedCount = (int?)x.Sum(y => y.Completed) ?? 0,
                            RejectedBySupervisorCount = (int?)x.Sum(y => y.RejectedBySupervisor) ?? 0,
                            ApprovedBySupervisorCount = (int?)x.Sum(y => y.ApprovedBySupervisor) ?? 0,
                            RejectedByHeadquartersCount = (int?)x.Sum(y => y.RejectedByHeadquarters) ?? 0,
                            ApprovedByHeadquartersCount = (int?)x.Sum(y => y.ApprovedByHeadquarters) ?? 0,
                            TotalCount = x.Count()
                        }))
                .FirstOrDefault();
        }

        protected virtual Expression<Func<InterviewSummary, bool>> CreateFilterExpression(TeamsAndStatusesInputModel input)
        {
            Expression<Func<InterviewSummary, bool>> result = null;

            if (input.TemplateId.HasValue)
            {
                result = x => x.QuestionnaireId == input.TemplateId && x.QuestionnaireVersion == input.TemplateVersion;
            }

            if (input.ViewerId.HasValue)
            {
                result = result == null 
                    ? x => x.TeamLeadId == input.ViewerId
                    : result.AndCondition(x => x.TeamLeadId == input.ViewerId);
            }

            return result ?? (x => x.SummaryId != null); 
        }

        public abstract TeamsAndStatusesReportView Load(TeamsAndStatusesInputModel input);

        public ReportView GetReport(TeamsAndStatusesInputModel model)
        {
            var view = this.Load(model);
            view.TotalRow.Responsible = Report.COLUMN_TOTAL;

            return new ReportView
            {
                Headers = new[]
                {
                    Report.COLUMN_TEAM_MEMBER, Report.COLUMN_SUPERVISOR_ASSIGNED, Report.COLUMN_INTERVIEWER_ASSIGNED,
                    Report.COLUMN_COMPLETED, Report.COLUMN_REJECTED_BY_SUPERVISOR,
                    Report.COLUMN_APPROVED_BY_SUPERVISOR, Report.COLUMN_REJECTED_BY_HQ, Report.COLUMN_APPROVED_BY_HQ,
                    Report.COLUMN_TOTAL
                },
                Data = new[] {view.TotalRow}.Concat(view.Items).Select(x => new object[]
                {
                    x.Responsible, x.SupervisorAssignedCount, x.InterviewerAssignedCount, x.CompletedCount,
                    x.RejectedBySupervisorCount, x.ApprovedBySupervisorCount, x.RejectedByHeadquartersCount,
                    x.ApprovedByHeadquartersCount, x.TotalCount
                }).ToArray()
            };
        }
    }
}