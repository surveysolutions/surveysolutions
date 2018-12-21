using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dapper;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    public class ChartStatisticsViewFactory : IChartStatisticsViewFactory
    {
        private readonly IUnitOfWork unitOfWork;

        public ChartStatisticsViewFactory(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        private static readonly string[] AllowedStatuses =
        {
            ((int) InterviewStatus.Completed).ToString(),
            ((int) InterviewStatus.RejectedBySupervisor).ToString(),
            ((int) InterviewStatus.ApprovedBySupervisor).ToString(),
            ((int) InterviewStatus.RejectedByHeadquarters).ToString(),
            ((int) InterviewStatus.ApprovedByHeadquarters).ToString()
        };

        public ChartStatisticsView Load(ChartStatisticsInputModel input)
        {
            var questionnaireId = input.QuestionnaireId == null
                ? "%"
                : $"{input.QuestionnaireId.FormatGuid()}${input.QuestionnaireVersion?.ToString() ?? "%"}";

            // ReSharper disable StringLiteralTypo
            var rawData = this.unitOfWork.Session.Connection.Query<(DateTime date, InterviewStatus status, long count)>(
                $@"select cum.date, cum.status, sum(sum(cum.changevalue)) over (order by cum.date)
                    from readside.cumulativereportstatuschanges cum
                    inner join plainstore.questionnairebrowseitems qbi on cum.questionnaireidentity = qbi.id
                    where cum.questionnaireidentity like @questionnaire and cum.status in ({string.Join(",", AllowedStatuses)})
                            and qbi.isdeleted = false and cum.date > @minDate and cum.date < @maxDate
                    group by cum.date, cum.status 
                    order by cum.date, cum.status asc", new
                {
                    Questionnaire = questionnaireId,
                    minDate = input.From ?? DateTime.MinValue,
                    maxDate = input.To ?? DateTime.MaxValue
                });
            // ReSharper restore StringLiteralTypo


            var view = new ChartStatisticsView();
            var statusMap = new Dictionary<InterviewStatus, ChartStatisticsDataSet>();
            
            DateTime? minDate = null;
            DateTime? endDate = null;
            DateTime? lastDate = null;

            foreach (var row in rawData)
            {
                if (!statusMap.ContainsKey(row.status))
                {
                    var dSet = new ChartStatisticsDataSet {Status = row.status};
                    statusMap.Add(row.status, dSet);
                    view.DataSets.Add(dSet);

                    if (minDate != null && row.date > minDate)
                    {
                        // filling in zero values at beginning
                        var date = minDate.Value;
                        while (date != row.date)
                        {
                            dSet.Add(FormatDate(date), 0);
                            date = date.AddDays(1);
                        }
                    }
                }

                if (lastDate != null && lastDate != row.date)
                {
                    do
                    {
                        foreach (var set in view.DataSets)
                        {
                            set.FillGap(FormatDate(lastDate.Value));
                        }

                        lastDate = lastDate.Value.AddDays(1);
                    } while (lastDate != row.date);
                }

                var dataSet = statusMap[row.status];
                dataSet.Add(FormatDate(row.date), row.count);

                if (minDate == null)
                {
                    minDate = row.date;
                }

                endDate = row.date;
                lastDate = row.date;
            }
            
            view.From = minDate.HasValue ? FormatDate(minDate.Value) : null;
            view.To = endDate.HasValue ? FormatDate(endDate.Value) : null;
            view.StartDate = minDate.HasValue ? FormatDate(minDate.Value.AddDays(-1)) : null;

            return view;
        }

        private static string FormatDate(DateTime x)
        {
            return x.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
