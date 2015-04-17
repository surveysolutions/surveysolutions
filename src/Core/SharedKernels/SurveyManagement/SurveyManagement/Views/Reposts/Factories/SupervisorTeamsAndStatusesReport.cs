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

                var supervisorAssignedStatusCount = CountInStatus(filteredInterviews, responsibles, InterviewStatus.SupervisorAssigned);
                var interviewerAssignedCount = CountInStatus(filteredInterviews, responsibles, InterviewStatus.InterviewerAssigned);
                var completedCount = CountInStatus(filteredInterviews, responsibles, InterviewStatus.Completed);
                var approvedBySupervisorCount = CountInStatus(filteredInterviews, responsibles, InterviewStatus.ApprovedBySupervisor);
                var rejectedBySupervisorCount = CountInStatus(filteredInterviews, responsibles, InterviewStatus.RejectedBySupervisor);
                var approvedByHeadquartersCount = CountInStatus(filteredInterviews, responsibles, InterviewStatus.ApprovedByHeadquarters);
                var rejectedByHeadquartersCount = CountInStatus(filteredInterviews, responsibles, InterviewStatus.RejectedByHeadquarters);
                var totalCount = CountInStatus(filteredInterviews, responsibles, null);

                var statistics = new List<StatisticsLineGroupedByUserAndTemplate>();
                foreach (var responsibleId in responsibles)
                {
                    Func<CounterObject, bool> findByResponsible = x => x.ResponsibleId == responsibleId;
                    statistics.Add(new StatisticsLineGroupedByUserAndTemplate
                    {
                        ResponsibleId = responsibleId,
                        ResponsibleName = Monads.Maybe(() => totalCount.SingleOrDefault(findByResponsible).ResponsibleName),
                        SupervisorAssignedCount = Monads.Maybe(() => supervisorAssignedStatusCount.SingleOrDefault(findByResponsible).InterviewsCount),
                        InterviewerAssignedCount = Monads.Maybe(() => interviewerAssignedCount.SingleOrDefault(findByResponsible).InterviewsCount),
                        CompletedCount = Monads.Maybe(() => completedCount.SingleOrDefault(findByResponsible).InterviewsCount),
                        ApprovedBySupervisorCount = Monads.Maybe(() => approvedBySupervisorCount.SingleOrDefault(findByResponsible).InterviewsCount),
                        RejectedBySupervisorCount = Monads.Maybe(() => rejectedBySupervisorCount.SingleOrDefault(findByResponsible).InterviewsCount),
                        ApprovedByHeadquartersCount = Monads.Maybe(() => approvedByHeadquartersCount.SingleOrDefault(findByResponsible).InterviewsCount),
                        RejectedByHeadquartersCount = Monads.Maybe(() => rejectedByHeadquartersCount.SingleOrDefault(findByResponsible).InterviewsCount),
                        TotalCount = Monads.Maybe(() => totalCount.SingleOrDefault(findByResponsible).InterviewsCount)
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

        private static List<CounterObject> CountInStatus(IQueryable<InterviewSummary> filteredInterviews,
            IEnumerable<Guid> responsibles,
            InterviewStatus? requestedStatus)
        {
            filteredInterviews = filteredInterviews.Where(x => responsibles.Contains(x.ResponsibleId));

            if (requestedStatus.HasValue)
            {
                filteredInterviews = filteredInterviews.Where(x => x.Status == requestedStatus);
            }

            var groupedResults = filteredInterviews.GroupBy(g => new {g.ResponsibleId, g.ResponsibleName});
            var result = groupedResults.Select(g => new CounterObject
            {
                ResponsibleName = g.Key.ResponsibleName,
                ResponsibleId = g.Key.ResponsibleId,
                InterviewsCount = g.Count()
            }).ToList();

            return result;
        }

        private class CounterObject
        {
            public int InterviewsCount { get; set; }

            public string ResponsibleName { get; set; }

            public Guid ResponsibleId { get; set; }
        }
    }
}