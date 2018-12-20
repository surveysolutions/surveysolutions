using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    internal class OldschoolChartStatisticsDataProvider : IOldschoolChartStatisticsDataProvider
    {
        private readonly INativeReadSideStorage<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireRepository;

        public OldschoolChartStatisticsDataProvider(INativeReadSideStorage<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireRepository)
        {
            this.cumulativeReportStatusChangeStorage = cumulativeReportStatusChangeStorage;
            this.questionnaireRepository = questionnaireRepository;
        }

        public StatisticsGroupedByDateAndTemplate GetStatisticsInOldFormat(QuestionnaireIdentity questionnaireId)
        {
            var questionnaireIdentity = questionnaireId?.ToString();

            var nonDeletedQuestionnaires = questionnaireIdentity == null
                ? questionnaireRepository.Query(_ => _.Where(i => !i.IsDeleted).Select(i => i.Id))
                : null;

            var count =
                this.cumulativeReportStatusChangeStorage.Query(_ =>
                {
                    if (questionnaireIdentity != null)
                        _ = _.Where(change => change.QuestionnaireIdentity == questionnaireIdentity);
                    else
                        _ = _.Where(change => nonDeletedQuestionnaires.Contains(change.QuestionnaireIdentity));
                    return _.Select(q => q.EntryId).Count();
                });

            if (count == 0)
                return null;

            var minMaxDate = this.cumulativeReportStatusChangeStorage.Query(_ =>
            {
                if (questionnaireIdentity != null)
                    _ = _.Where(change => change.QuestionnaireIdentity == questionnaireIdentity);
                else
                    _ = _.Where(change => nonDeletedQuestionnaires.Contains(change.QuestionnaireIdentity));
                var dates = _.Select(change => change.Date);
                return new
                    {
                        minDate = dates.Min(),
                        maxDate = dates.Max(),
                    };
            });

            var minDate = minMaxDate.minDate;
            var maxDate = minMaxDate.maxDate;

            Dictionary<InterviewStatus, Dictionary<DateTime, int>> countsByStatusAndDate =
                this.GetCountsForQuestionnaireGroupedByStatusAndDate(questionnaireId, nonDeletedQuestionnaires);

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

        private Dictionary<InterviewStatus, Dictionary<DateTime, int>> GetCountsForQuestionnaireGroupedByStatusAndDate(QuestionnaireIdentity questionnaireId, IQueryable<string> nonDeletedQuestionnaires)
        {
            var questionnaireIdentity = questionnaireId?.ToString();
            var rawCountsByStatusAndDate = this.cumulativeReportStatusChangeStorage.Query(_ =>
            {
                if (questionnaireIdentity != null)
                    _ = _.Where(change => change.QuestionnaireIdentity == questionnaireIdentity);
                else
                    _ = _.Where(change => nonDeletedQuestionnaires.Contains(change.QuestionnaireIdentity));

                return _
                    .GroupBy(change => new {change.Status, change.Date})
                    .Select(grouping => new
                    {
                        Status = grouping.Key.Status,
                        Date = grouping.Key.Date,
                        Count =
                            grouping.Sum(change => (int?) change.ChangeValue) ??
                            0, // https://nhibernate.jira.com/browse/NH-2130
                    });
            });

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
