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
    internal class TeamsAndStatusesReport: ITeamsAndStatusesReport
    {
        protected readonly INativeReadSideStorage<InterviewSummary> interviewsReader;

        public TeamsAndStatusesReport(INativeReadSideStorage<InterviewSummary> interviewsReader)
        {
            this.interviewsReader = interviewsReader;
        }

        private static IQueryable<InterviewSummary> FilterByQuestionnaireOrTeamLead(IQueryable<InterviewSummary> _, TeamsAndStatusesInputModel input)
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

        private TeamsAndStatusesReportLine GetTotalRow(TeamsAndStatusesInputModel input)
            => this.interviewsReader.Query(_ => FilterByQuestionnaireOrTeamLead(_, input)
                    .Select(x => new
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
                        SupervisorAssignedCount = (int?) x.Sum(y => y.SupervisorAssigned) ?? 0,
                        InterviewerAssignedCount = (int?) x.Sum(y => y.InterviewerAssigned) ?? 0,
                        CompletedCount = (int?) x.Sum(y => y.Completed) ?? 0,
                        RejectedBySupervisorCount = (int?) x.Sum(y => y.RejectedBySupervisor) ?? 0,
                        ApprovedBySupervisorCount = (int?) x.Sum(y => y.ApprovedBySupervisor) ?? 0,
                        RejectedByHeadquartersCount = (int?) x.Sum(y => y.RejectedByHeadquarters) ?? 0,
                        ApprovedByHeadquartersCount = (int?) x.Sum(y => y.ApprovedByHeadquarters) ?? 0,
                        TotalCount = x.Count()
                    }))
                .FirstOrDefault();

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

        private TeamsAndStatusesReportView Load(TeamsAndStatusesInputModel input, bool forAdminOrHq)
        {
            var query = this.interviewsReader.Query(_ => FilterByQuestionnaireOrTeamLead(_, input)
                .Select(x => new
                {
                    ResponsibleId = forAdminOrHq ? x.TeamLeadId : x.ResponsibleId,
                    Responsible = forAdminOrHq ? x.TeamLeadName : x.ResponsibleName,
                    SupervisorAssignedCount = x.Status == InterviewStatus.SupervisorAssigned ? 1 : 0,
                    InterviewerAssignedCount = x.Status == InterviewStatus.InterviewerAssigned ? 1 : 0,
                    CompletedCount = x.Status == InterviewStatus.Completed ? 1 : 0,
                    RejectedBySupervisorCount = x.Status == InterviewStatus.RejectedBySupervisor ? 1 : 0,
                    ApprovedBySupervisorCount = x.Status == InterviewStatus.ApprovedBySupervisor ? 1 : 0,
                    RejectedByHeadquartersCount = x.Status == InterviewStatus.RejectedByHeadquarters ? 1 : 0,
                    ApprovedByHeadquartersCount = x.Status == InterviewStatus.ApprovedByHeadquarters ? 1 : 0
                })
                .GroupBy(x => new { x.Responsible, x.ResponsibleId })
                .Select(x => new 
                {
                    ResponsibleId = x.Key.ResponsibleId,
                    Responsible = x.Key.Responsible,
                    SupervisorAssignedCount = (int?)x.Sum(y => y.SupervisorAssignedCount) ?? 0,
                    InterviewerAssignedCount = (int?)x.Sum(y => y.InterviewerAssignedCount) ?? 0,
                    CompletedCount = (int?)x.Sum(y => y.CompletedCount) ?? 0,
                    RejectedBySupervisorCount = (int?)x.Sum(y => y.RejectedBySupervisorCount) ?? 0,
                    ApprovedBySupervisorCount = (int?)x.Sum(y => y.ApprovedBySupervisorCount) ?? 0,
                    RejectedByHeadquartersCount = (int?)x.Sum(y => y.RejectedByHeadquartersCount) ?? 0,
                    ApprovedByHeadquartersCount = (int?)x.Sum(y => y.ApprovedByHeadquartersCount) ?? 0,
                    TotalCount = x.Count()
                }));

            var totalStatistics = GetTotalRow(input);
            

            var totalCount = this.interviewsReader.CountDistinctWithRecursiveIndex(x =>
            {
                x = x.Where(CreateFilterExpression(input));

                if (forAdminOrHq) x.Select(y => y.TeamLeadId);
                else x.Select(y => y.ResponsibleId);

                return x;
            });

            return new TeamsAndStatusesReportView
            {
                TotalCount = totalCount,
                Items = query.OrderUsingSortExpression(input.Order)
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize)
                    .ToList()
                    .Select(x => new TeamsAndStatusesReportLine
                    {
                        ResponsibleId = x.ResponsibleId,
                        Responsible = x.Responsible,
                        SupervisorAssignedCount = x.SupervisorAssignedCount,
                        InterviewerAssignedCount = x.InterviewerAssignedCount,
                        CompletedCount = x.CompletedCount,
                        RejectedBySupervisorCount = x.RejectedBySupervisorCount,
                        ApprovedBySupervisorCount = x.ApprovedBySupervisorCount,
                        RejectedByHeadquartersCount = x.RejectedByHeadquartersCount,
                        ApprovedByHeadquartersCount = x.ApprovedByHeadquartersCount,
                        TotalCount = x.TotalCount
                    }),
                TotalRow = totalStatistics
            };
        }

        public TeamsAndStatusesReportView GetBySupervisors(TeamsAndStatusesByHqInputModel input) => this.Load(input, true);

        public TeamsAndStatusesReportView GetBySupervisorAndDependentInterviewers(TeamsAndStatusesInputModel input) => this.Load(input, false);
        
        public ReportView GetReport(TeamsAndStatusesInputModel model)
        {
            var forAdminOrHq = model is TeamsAndStatusesByHqInputModel;

            var view = this.Load(model, forAdminOrHq);
            view.TotalRow.Responsible = forAdminOrHq ? Strings.AllTeams : Strings.AllInterviewers;

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
