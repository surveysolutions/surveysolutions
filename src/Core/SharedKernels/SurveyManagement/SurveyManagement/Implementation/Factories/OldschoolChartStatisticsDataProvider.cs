using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Factories
{
    internal class OldschoolChartStatisticsDataProvider : IOldschoolChartStatisticsDataProvider
    {
        private readonly IQueryableReadSideRepositoryReader<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage;

        public OldschoolChartStatisticsDataProvider(IQueryableReadSideRepositoryReader<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage)
        {
            this.cumulativeReportStatusChangeStorage = cumulativeReportStatusChangeStorage;
        }

        public StatisticsGroupedByDateAndTemplate GetStatisticsInOldFormat(Guid questionnaireId, long questionnaireVersion)
        {
            var count = this.cumulativeReportStatusChangeStorage.Query(_ => _
                .Where(change => change.QuestionnaireId == questionnaireId)
                .Where(change => change.QuestionnaireVersion == questionnaireVersion)
                .Count());

            if (count == 0)
                return null;

            var minDate = this.cumulativeReportStatusChangeStorage.Query(_ => _
                .Where(change => change.QuestionnaireId == questionnaireId)
                .Where(change => change.QuestionnaireVersion == questionnaireVersion)
                .Min(change => change.Date));

            var maxDate = this.cumulativeReportStatusChangeStorage.Query(_ => _
                .Where(change => change.QuestionnaireId == questionnaireId)
                .Where(change => change.QuestionnaireVersion == questionnaireVersion)
                .Max(change => change.Date));

            var result = new StatisticsGroupedByDateAndTemplate();

            DateTime date = minDate;
            while (date <= maxDate)
            {
                result.StatisticsByDate.Add(date,
                    this.GetOldSchoolStatsForDate(questionnaireId, questionnaireVersion, date));

                date = date.AddDays(1);
            }

            return result;
        }

        private QuestionnaireStatisticsForChart GetOldSchoolStatsForDate(Guid questionnaireId, long questionnaireVersion, DateTime date)
        {
            Dictionary<InterviewStatus, int> countsForStatuses = this.cumulativeReportStatusChangeStorage.Query(_ => _
                .Where(change => change.QuestionnaireId == questionnaireId)
                .Where(change => change.QuestionnaireVersion == questionnaireVersion)
                .Where(change => change.Date <= date)
                .GroupBy(change => change.Status)
                .Select(grouping => new { Status = grouping.Key, Count = grouping.Sum(change => (int?)change.ChangeValue) ?? 0 })
                .ToDictionary(x => x.Status, x => x.Count));

            return new QuestionnaireStatisticsForChart
            {
                CreatedCount = countsForStatuses.GetOrDefault(InterviewStatus.Created) ?? 0,
                SupervisorAssignedCount = countsForStatuses.GetOrDefault(InterviewStatus.SupervisorAssigned) ?? 0,
                InterviewerAssignedCount = countsForStatuses.GetOrDefault(InterviewStatus.InterviewerAssigned) ?? 0,
                CompletedCount = countsForStatuses.GetOrDefault(InterviewStatus.Completed) ?? 0,
                ApprovedBySupervisorCount = countsForStatuses.GetOrDefault(InterviewStatus.ApprovedBySupervisor) ?? 0,
                RejectedBySupervisorCount = countsForStatuses.GetOrDefault(InterviewStatus.RejectedBySupervisor) ?? 0,
                ApprovedByHeadquartersCount = countsForStatuses.GetOrDefault(InterviewStatus.ApprovedByHeadquarters) ?? 0,
                RejectedByHeadquartersCount = countsForStatuses.GetOrDefault(InterviewStatus.RejectedByHeadquarters) ?? 0,
            };
        }
    }
}