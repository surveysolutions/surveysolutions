using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories
{
    internal class SupervisorTeamsAndStatusesReport : ISupervisorTeamsAndStatusesReport
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewsReader;

        public SupervisorTeamsAndStatusesReport(IQueryableReadSideRepositoryReader<InterviewSummary> interviewsReader)
        {
            this.interviewsReader = interviewsReader;
        }

        public TeamsAndStatusesReportView Load(TeamsAndStatusesInputModel input)
        {
            var items = this.interviewsReader.Query(_ =>
            {
                var filteredInterviews = ApplyFilter(input, _);

                var responsibles = GetUsersToCountOn(input, filteredInterviews);

                var groupedResults = (from i in filteredInterviews
                                      where responsibles.Contains(i.ResponsibleId)
                                      group i by new { i.ResponsibleId, i.ResponsibleName, i.Status }
                                          into g
                                          select new CounterObject
                                          {
                                              ResponsibleId = g.Key.ResponsibleId,
                                              ResponsibleName = g.Key.ResponsibleName,
                                              Status = g.Key.Status,
                                              InterviewsCount = g.Count()
                                          }).ToList();

                var statistics = new List<StatisticsLineGroupedByUserAndTemplate>();
                foreach (var responsibleId in responsibles)
                {
                    statistics.Add(new StatisticsLineGroupedByUserAndTemplate
                    {
                        ResponsibleId = responsibleId,
                        ResponsibleName = groupedResults.FirstOrDefault(x => x.ResponsibleId == responsibleId).ResponsibleName,
                        SupervisorAssignedCount = GetCountInStatus(groupedResults, responsibleId, InterviewStatus.SupervisorAssigned),
                        InterviewerAssignedCount = GetCountInStatus(groupedResults, responsibleId, InterviewStatus.InterviewerAssigned),
                        CompletedCount = GetCountInStatus(groupedResults, responsibleId, InterviewStatus.Completed),
                        ApprovedBySupervisorCount = GetCountInStatus(groupedResults, responsibleId, InterviewStatus.ApprovedBySupervisor),
                        RejectedBySupervisorCount = GetCountInStatus(groupedResults, responsibleId, InterviewStatus.RejectedBySupervisor),
                        ApprovedByHeadquartersCount = GetCountInStatus(groupedResults, responsibleId, InterviewStatus.ApprovedByHeadquarters),
                        RejectedByHeadquartersCount = GetCountInStatus(groupedResults, responsibleId, InterviewStatus.RejectedByHeadquarters),
                        TotalCount = groupedResults.Where(x => x.ResponsibleId == responsibleId).Sum(x => x.InterviewsCount)
                    });
                }


                return statistics;
            });

            var rowCount = this.interviewsReader.Query(_ => ApplyFilter(input, _).Select(x => x.ResponsibleId).Distinct().Count());

            List<TeamsAndStatusesReportLine> currentPage = items.Select(doc => new TeamsAndStatusesReportLine {
                    SupervisorAssignedCount = doc.SupervisorAssignedCount,
                    InterviewerAssignedCount = doc.InterviewerAssignedCount,

                    CompletedCount = doc.CompletedCount,

                    ApprovedBySupervisorCount = doc.ApprovedBySupervisorCount,
                    RejectedBySupervisorCount = doc.RejectedBySupervisorCount,

                    ApprovedByHeadquartersCount = doc.ApprovedByHeadquartersCount,
                    RejectedByHeadquartersCount = doc.RejectedByHeadquartersCount,

                    TotalCount = doc.TotalCount,

                    QuestionnaireId = doc.QuestionnaireId,
                    QuestionnaireVersion = doc.QuestionnaireVersion,
                    ResponsibleId = doc.ResponsibleId,
                    ResponsibleName = doc.ResponsibleName
                }).ToList();

            return new TeamsAndStatusesReportView {
                    TotalCount = rowCount,
                    Items = currentPage
                };
        }

        private static int GetCountInStatus(List<CounterObject> groupedResults, Guid responsibleId, InterviewStatus status)
        {
            var resultsByTeamLeadAndStatus = groupedResults.SingleOrDefault(x => x.ResponsibleId == responsibleId && x.Status == status);
            if (resultsByTeamLeadAndStatus != null)
            {
                return resultsByTeamLeadAndStatus.InterviewsCount;
            }

            return 0;
        }

        private static List<Guid> GetUsersToCountOn(TeamsAndStatusesInputModel input, IQueryable<InterviewSummary> filteredInterviews)
        {
            var usersToCountOn = filteredInterviews.OrderUsingSortExpression(input.Order)
               .Select(x => new { x.ResponsibleName, x.ResponsibleId })
               .Distinct()
               .Skip((input.Page - 1) * input.PageSize)
               .Take(input.PageSize)
               .ToList()
               .Select(x => x.ResponsibleId)
               .ToList();

            return usersToCountOn;
        }

        private static IQueryable<InterviewSummary> ApplyFilter(TeamsAndStatusesInputModel input, IQueryable<InterviewSummary> _)
        {
            var filteredInterviews = _.Where(x => !x.IsDeleted);

            if (input.TemplateId.HasValue)
            {
                filteredInterviews = filteredInterviews.Where(x => x.QuestionnaireId == input.TemplateId);
            }

            if (input.TemplateVersion.HasValue)
            {
                filteredInterviews = filteredInterviews.Where(x => x.QuestionnaireVersion == input.TemplateVersion);
            }

            if (input.ViewerId.HasValue)
            {
                filteredInterviews = filteredInterviews.Where(x => x.TeamLeadId == input.ViewerId);
            }

            return filteredInterviews;
        }


        private class CounterObject
        {
            private InterviewStatus status;
            public int InterviewsCount { get; set; }

            public string ResponsibleName { get; set; }

            public Guid ResponsibleId { get; set; }

            public InterviewStatus Status
            {
                get { return status; }
                set { status = value; }
            }
        }
    }
}