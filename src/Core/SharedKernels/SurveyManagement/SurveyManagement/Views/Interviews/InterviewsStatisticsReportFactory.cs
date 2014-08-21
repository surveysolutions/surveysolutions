using System;
using System.Linq;
using System.Linq.Expressions;
using Main.Core.View;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviews
{
    public class InterviewsStatisticsReportFactory : IViewFactory<InterviewsStatisticsReportInputModel, InterviewsStatisticsReportView>
    {
        private readonly IQueryableReadSideRepositoryReader<StatisticsLineGroupedByDateAndTemplate> interviewSummaryReader;

        public InterviewsStatisticsReportFactory(
            IQueryableReadSideRepositoryReader<StatisticsLineGroupedByDateAndTemplate> interviewSummaryReader)
        {
            this.interviewSummaryReader = interviewSummaryReader;
        }

        public InterviewsStatisticsReportView Load(InterviewsStatisticsReportInputModel input)
        {
            Expression<Func<StatisticsLineGroupedByDateAndTemplate, bool>> predicate = (s) => (s.QuestionnaireId == input.QuestionnaireId);

            if (input.QuestionnaireVersion.HasValue)
            {
                predicate = predicate.AndCondition(x => (x.QuestionnaireVersion == input.QuestionnaireVersion));
            }

            var stat = this.interviewSummaryReader.Query(_ => _.Where(predicate)).OrderBy(_ => _.Date);

            var r = new InterviewsStatisticsReportView();

            var firstDate = stat.First().Date;
            var lastDate = input.CurrentDate;

            var daysCount = (lastDate - firstDate).TotalDays;

            //for (var i = 0; i < daysCount; i++)
            //{
            //    var dateTick = firstDate.AddDays(i);
            //    if (dateTick)
            //        r.Stats[]
            //}

            return r;
        }
    }
}