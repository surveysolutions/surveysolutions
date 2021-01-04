using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dapper;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    public class ChartStatisticsViewFactory : IChartStatisticsViewFactory
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IAllUsersAndQuestionnairesFactory questionnairesFactory;
        private readonly INativeReadSideStorage<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireRepository;

        public ChartStatisticsViewFactory(IUnitOfWork unitOfWork, 
            IAllUsersAndQuestionnairesFactory questionnairesFactory, 
            INativeReadSideStorage<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireRepository)
        {
            this.unitOfWork = unitOfWork;
            this.questionnairesFactory = questionnairesFactory;
            this.cumulativeReportStatusChangeStorage = cumulativeReportStatusChangeStorage;
            this.questionnaireRepository = questionnaireRepository;
        }

        private static readonly int[] AllowedStatuses =
        {
            (int) InterviewStatus.Completed,
            (int) InterviewStatus.RejectedBySupervisor,
            (int) InterviewStatus.ApprovedBySupervisor,
            (int) InterviewStatus.RejectedByHeadquarters,
            (int) InterviewStatus.ApprovedByHeadquarters
        };

        public List<QuestionnaireVersionsComboboxViewItem> GetQuestionnaireListWithData()
        {
            var questionnaireListWithData = cumulativeReportStatusChangeStorage
                .Query(_ => _
                    .Where(q => AllowedStatuses.Contains((int)q.Status))
                    .Select(q => q.QuestionnaireIdentity).Distinct().ToList());

            return this.questionnaireRepository
                .Query(q => q.Where(item => !item.IsDeleted && questionnaireListWithData.Contains(item.Id)).ToList())
                .GetQuestionnaireComboboxViewItems();
        }

        public ChartStatisticsView Load(ChartStatisticsInputModel input)
        {
            var questionnairesList = this.questionnairesFactory.GetQuestionnaires(input.QuestionnaireId, input.QuestionnaireVersion)
                .Select(id => id.ToString()).ToList();

            // ReSharper disable StringLiteralTypo
            var dates = this.unitOfWork.Session.Connection.QuerySingle<(DateTime? min, DateTime? max)>(
                $@"with dates as (
                    select date from cumulativereportstatuschanges
                    where questionnaireidentity = any(@questionnairesList) and status = any(@allowedStatuses)
                  ) select min(date), max(date) from dates", 
                 new { questionnairesList, AllowedStatuses });

            if (dates.min == null && dates.max == null) // we have no data at all
            {
                return new ChartStatisticsView();
            }

            var leftEdge = new[] { input.From ?? dates.min?.AddDays(-1), input.To ?? dates.min?.AddDays(-1) }.Min() ?? DateTime.MinValue;
            var rightEdge = new[] {input.From ?? dates.min ?? DateTime.MinValue, input.To ?? DateTime.UtcNow}.Max();
            
            var queryParams = new
            {
                questionnairesList,
                AllowedStatuses,
                // we should always build report from the very beginning, this param will ensure that data accumulated correctly
                minDateQuery = FormatDate(new[] { input.From ?? dates.min?.AddDays(-1), dates.min }.Min() ?? DateTime.MinValue),
                minDate = FormatDate(leftEdge),
                maxDate = FormatDate(rightEdge)
            };

            // dates CTE will generate gape-less interval of dates
            // timespan CTE will produce cross product of dates and available statuses
            // report CTE will produce report over all data
            // do not move date filtering inside report CTE as it will affect partition query
            var rawData = this.unitOfWork.Session.Connection.Query<(DateTime date, InterviewStatus status, long count)>(
                $@"with 
                        dates as (select generate_series(@minDateQuery::date, @maxDate::date, interval '1 day')::date as date),
                        timespan as (select date, status from dates as date, unnest(@AllowedStatuses) as status),
                        report as 
                        (
                            select span.date, span.status, 
                                    sum(sum(coalesce(cum.changevalue, 0))) over (partition by span.status order by span.date) as count
                            from timespan as span  
                            left join cumulativereportstatuschanges cum on cum.date = span.date and cum.status = span.status
                                and cum.questionnaireidentity = any(@questionnairesList)
                            group by 1,2 order by 1
                        )
                       select date, status, count
                       from report where date >= @minDate::date and date <= @maxDate::date", queryParams);
            // ReSharper restore StringLiteralTypo
            
            var view = new ChartStatisticsView();
            var statusMap = new Dictionary<InterviewStatus, ChartStatisticsDataSet>();
            
            foreach (var row in rawData)
            {
                if (!statusMap.ContainsKey(row.status))
                {
                    var dSet = new ChartStatisticsDataSet {Status = row.status};
                    statusMap.Add(row.status, dSet);
                    view.DataSets.Add(dSet);
                }

                var dataSet = statusMap[row.status];
                dataSet.AddOrReplaceLast(FormatDate(row.date), row.count);
            }

            view.DataSets = view.DataSets.Where(ds => ds.AllZeros == false).ToList();
            view.From = FormatDate(leftEdge);
            view.To = FormatDate(rightEdge);
            view.MaxDate = FormatDate(dates.max.Value);
            view.MinDate = FormatDate(dates.min.Value);

            return view;
        }

        private static string FormatDate(DateTime x)
        {
            return x.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
