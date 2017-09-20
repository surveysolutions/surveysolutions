using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    internal class OldschoolChartStatisticsDataProvider : IOldschoolChartStatisticsDataProvider
    {
        private readonly INativeReadSideStorage<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage;

        public OldschoolChartStatisticsDataProvider(INativeReadSideStorage<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage)
        {
            this.cumulativeReportStatusChangeStorage = cumulativeReportStatusChangeStorage;
        }

        public StatisticsGroupedByDateAndTemplate GetStatisticsInOldFormat(Guid questionnaireId, long questionnaireVersion)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion).ToString();
            var count =
                this.cumulativeReportStatusChangeStorage.QueryOver( _ =>_
                .Where(change => change.QuestionnaireIdentity == questionnaireIdentity)
                .Select(Projections.Count<CumulativeReportStatusChange>(x => x.EntryId))
                .SingleOrDefault<int>());

            if (count == 0)
                return null;

            var minMaxDate = this.cumulativeReportStatusChangeStorage.QueryOver(_ => _
                .Where(change => change.QuestionnaireIdentity == questionnaireIdentity)
                .Select(Projections.Min<CumulativeReportStatusChange>(x => x.Date), Projections.Max<CumulativeReportStatusChange>(x => x.Date))
                .SingleOrDefault<object[]>());

            var minDate = (DateTime)minMaxDate[0];
            var maxDate = (DateTime)minMaxDate[1];

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
            var questionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion).ToString();

            var rawCountsByStatusAndDate = this.cumulativeReportStatusChangeStorage.Query(_ => _
                .Where(change => change.QuestionnaireIdentity == questionnaireIdentity)
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