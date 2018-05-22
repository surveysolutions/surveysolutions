using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Jobs;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics
{
    /// <summary>
    /// Handle some interview events to eneque reports update schedule
    /// </summary>
    internal class ReportTableDataDenormalizer :
        BaseDenormalizer,
        IEventHandler<AnswersDeclaredInvalid>,
        IEventHandler<AnswersDeclaredValid>,
        IEventHandler<QuestionsDisabled>,
        IEventHandler<QuestionsEnabled>,
        IEventHandler<SingleOptionQuestionAnswered>,
        IEventHandler<MultipleOptionsQuestionAnswered>,
        IEventHandler<NumericIntegerQuestionAnswered>,
        IEventHandler<NumericRealQuestionAnswered>,
        IEventHandler<YesNoQuestionAnswered>,
        IEventHandler<SingleOptionLinkedQuestionAnswered>
    {
        private readonly IRefreshReportsTask refreshReportsTask;
        public override object[] Writers => Array.Empty<object>();

        public ReportTableDataDenormalizer(IRefreshReportsTask refreshReportsTask)
        {
            this.refreshReportsTask = refreshReportsTask;
        }

        private void ScheduleReportTableDataUpdate()
        {
            refreshReportsTask.ScheduleRefresh();
        }

        public void Handle(IPublishedEvent<AnswersDeclaredInvalid> evnt) => ScheduleReportTableDataUpdate();
        public void Handle(IPublishedEvent<AnswersDeclaredValid> evnt) => ScheduleReportTableDataUpdate();
        public void Handle(IPublishedEvent<QuestionsDisabled> evnt) => ScheduleReportTableDataUpdate();
        public void Handle(IPublishedEvent<QuestionsEnabled> evnt) => ScheduleReportTableDataUpdate();
        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt) => ScheduleReportTableDataUpdate();
        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt) => ScheduleReportTableDataUpdate();
        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> evnt) => ScheduleReportTableDataUpdate();
        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> evnt) => ScheduleReportTableDataUpdate();
        public void Handle(IPublishedEvent<YesNoQuestionAnswered> evnt) => ScheduleReportTableDataUpdate();
        public void Handle(IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt) => ScheduleReportTableDataUpdate();
    }
}
