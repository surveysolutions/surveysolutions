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

            Dictionary<InterviewStatus, Dictionary<DateTime, int>> countsByStatusAndDate =
                this.GetCountsForQuestionnaireGroupedByStatusAndDate(questionnaireId, questionnaireVersion);

            return GetStatisticsInOldFormat(minDate, maxDate, countsByStatusAndDate);
        }

        private static StatisticsGroupedByDateAndTemplate GetStatisticsInOldFormat(DateTime minDate, DateTime maxDate,
            Dictionary<InterviewStatus, Dictionary<DateTime, int>> countsByStatusAndDate)
        {
            var result = new StatisticsGroupedByDateAndTemplate();

            DateTime date = minDate;
            while (date <= maxDate)
            {
                result.StatisticsByDate.Add(
                    date,
                    new QuestionnaireStatisticsForChart
                    {
                        CreatedCount = GetCumulativeCountForStatusUpToSpecifiedDate(InterviewStatus.Created, date, countsByStatusAndDate),
                        SupervisorAssignedCount = GetCumulativeCountForStatusUpToSpecifiedDate(InterviewStatus.SupervisorAssigned, date, countsByStatusAndDate),
                        InterviewerAssignedCount = GetCumulativeCountForStatusUpToSpecifiedDate(InterviewStatus.InterviewerAssigned, date, countsByStatusAndDate),
                        CompletedCount = GetCumulativeCountForStatusUpToSpecifiedDate(InterviewStatus.Completed, date, countsByStatusAndDate), 
                        ApprovedBySupervisorCount = GetCumulativeCountForStatusUpToSpecifiedDate(InterviewStatus.ApprovedBySupervisor, date, countsByStatusAndDate),
                        RejectedBySupervisorCount = GetCumulativeCountForStatusUpToSpecifiedDate(InterviewStatus.RejectedBySupervisor, date, countsByStatusAndDate),
                        ApprovedByHeadquartersCount = GetCumulativeCountForStatusUpToSpecifiedDate(InterviewStatus.ApprovedByHeadquarters, date, countsByStatusAndDate),
                        RejectedByHeadquartersCount = GetCumulativeCountForStatusUpToSpecifiedDate(InterviewStatus.RejectedByHeadquarters, date, countsByStatusAndDate),
                    });

                date = date.AddDays(1);
            }

            return result;
        }

        private Dictionary<InterviewStatus, Dictionary<DateTime, int>> GetCountsForQuestionnaireGroupedByStatusAndDate(Guid questionnaireId, long questionnaireVersion)
        {
            var rawCountsByStatusAndDate = this.cumulativeReportStatusChangeStorage.Query(_ => _
                .Where(change => change.QuestionnaireId == questionnaireId)
                .Where(change => change.QuestionnaireVersion == questionnaireVersion)
                .GroupBy(change => new {change.Status, change.Date})
                .Select(grouping => new
                {
                    Status = grouping.Key.Status,
                    Date = grouping.Key.Date,
                    Count = grouping.Sum(change => (int?) change.ChangeValue) ?? 0, // https://nhibernate.jira.com/browse/NH-2130
                }));

            var countsByStatusAndDate = new Dictionary<InterviewStatus, Dictionary<DateTime, int>>();

            foreach (var countForStatusAndDate in rawCountsByStatusAndDate)
            {
                countsByStatusAndDate.Add(countForStatusAndDate.Status, countForStatusAndDate.Date, countForStatusAndDate.Count);
            }

            return countsByStatusAndDate;
        }

        private static int GetCumulativeCountForStatusUpToSpecifiedDate(InterviewStatus status, DateTime date,
            Dictionary<InterviewStatus, Dictionary<DateTime, int>> countsByStatusAndDate)
        {
            return
                countsByStatusAndDate
                    .GetOrNull(status)?
                    .Where(pair => pair.Key <= date)
                    .Sum(pair => pair.Value)
                ?? 0;
        }
    }
}