using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories
{
    internal class TeamsAndStatusesReport : ITeamsAndStatusesReport
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewsReader;

        public TeamsAndStatusesReport(IQueryableReadSideRepositoryReader<InterviewSummary> interviewsReader)
        {
            this.interviewsReader = interviewsReader;
        }

        public TeamsAndStatusesReportView Load(TeamsAndStatusesInputModel input)
        {
            var items = this.interviewsReader.Query(_ =>
            {
                var filteredInterviews = ApplyFilter(input, _);

                var supervisorAssignedStatusCount = CountInStatus(input, filteredInterviews, InterviewStatus.SupervisorAssigned);
                var interviewerAssignedCount = CountInStatus(input, filteredInterviews, InterviewStatus.InterviewerAssigned);
                var completedCount = CountInStatus(input, filteredInterviews, InterviewStatus.Completed);
                var approvedBySupervisorCount = CountInStatus(input, filteredInterviews, InterviewStatus.ApprovedBySupervisor);
                var rejectedBySupervisorCount = CountInStatus(input, filteredInterviews, InterviewStatus.RejectedBySupervisor);
                var approvedByHeadquartersCount = CountInStatus(input, filteredInterviews, InterviewStatus.ApprovedByHeadquarters);
                var rejectedByHeadquartersCount = CountInStatus(input, filteredInterviews, InterviewStatus.RejectedByHeadquarters);
                var totalCount = CountInStatus(input, filteredInterviews, null);

                var statistics = new List<StatisticsLineGroupedByUserAndTemplate>();
                foreach (var responsibleId in totalCount.Select(x => x.ResponsibleId).Distinct())
                {
                    Func<CounterObject, bool> findQuestionnaire =  x => x.ResponsibleId == responsibleId;
                    statistics.Add(new StatisticsLineGroupedByUserAndTemplate
                    {
                        ResponsibleId = responsibleId,
                        ResponsibleName = Monads.Maybe(() => totalCount.SingleOrDefault(findQuestionnaire).ResponsibleName),
                        SupervisorAssignedCount = Monads.Maybe(() => supervisorAssignedStatusCount.SingleOrDefault(findQuestionnaire).InterviewsCount),
                        InterviewerAssignedCount = Monads.Maybe(() => interviewerAssignedCount.SingleOrDefault(findQuestionnaire).InterviewsCount),
                        CompletedCount = Monads.Maybe(() => completedCount.SingleOrDefault(findQuestionnaire).InterviewsCount),
                        ApprovedBySupervisorCount = Monads.Maybe(() => approvedBySupervisorCount.SingleOrDefault(findQuestionnaire).InterviewsCount),
                        RejectedBySupervisorCount = Monads.Maybe(() => rejectedBySupervisorCount.SingleOrDefault(findQuestionnaire).InterviewsCount),
                        ApprovedByHeadquartersCount = Monads.Maybe(() => approvedByHeadquartersCount.SingleOrDefault(findQuestionnaire).InterviewsCount),
                        RejectedByHeadquartersCount = Monads.Maybe(() => rejectedByHeadquartersCount.SingleOrDefault(findQuestionnaire).InterviewsCount),
                        TotalCount = Monads.Maybe(() => totalCount.SingleOrDefault(findQuestionnaire).InterviewsCount)
                    });
                }

                return statistics;
            });

            var rowCount = this.interviewsReader.Query(_ => ApplyFilter(input, _).Select(x => x.ResponsibleId).Distinct().Count());

            List<TeamsAndStatusesReportLine> currentPage = items.Select(doc =>
                                                                                 new TeamsAndStatusesReportLine
                                                                                     {
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

            return new TeamsAndStatusesReportView
                {
                    TotalCount = rowCount,
                    Items = currentPage
                };
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

        private static List<CounterObject> CountInStatus(TeamsAndStatusesInputModel input, IQueryable<InterviewSummary> filteredInterviews, InterviewStatus? requestedStatus)
        {
            if (requestedStatus.HasValue)
            {
                filteredInterviews = filteredInterviews.Where(x => x.Status == requestedStatus);
            }

            var groupedResults = filteredInterviews
                                   .OrderUsingSortExpression(input.Order)
                                   .GroupBy(g => new { g.ResponsibleId, g.ResponsibleName });
            var result = groupedResults
                .Select(g => new CounterObject
                {
                    ResponsibleName = g.Key.ResponsibleName,
                    ResponsibleId = g.Key.ResponsibleId,
                    InterviewsCount = g.Count()
                })
                .Skip((input.Page - 1)*input.PageSize)
                .Take(input.PageSize)
                .ToList();
            return result;
        }

        public class CounterObject
        {
            public int InterviewsCount { get; set; }

            public string ResponsibleName { get; set; }

            public Guid ResponsibleId { get; set; }
        }
    }
}