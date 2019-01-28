using System;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    public interface ISpeedReportDenormalizerFunctional : ICompositeFunctionalPartEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>> { }

    public class SpeedReportDenormalizerFunctional : ISpeedReportDenormalizerFunctional,
        IUpdateHandler<InterviewSummary, InterviewOnClientCreated>,
        IUpdateHandler<InterviewSummary, InterviewFromPreloadedDataCreated>,
        IUpdateHandler<InterviewSummary, InterviewDeleted>,
        IUpdateHandler<InterviewSummary, InterviewHardDeleted>,
        IUpdateHandler<InterviewSummary, InterviewRestored>,
        IUpdateHandler<InterviewSummary, InterviewCreated>,
        IUpdateHandler<InterviewSummary, TextQuestionAnswered>,
        IUpdateHandler<InterviewSummary, MultipleOptionsQuestionAnswered>,
        IUpdateHandler<InterviewSummary, SingleOptionQuestionAnswered>,
        IUpdateHandler<InterviewSummary, NumericRealQuestionAnswered>,
        IUpdateHandler<InterviewSummary, NumericIntegerQuestionAnswered>,
        IUpdateHandler<InterviewSummary, DateTimeQuestionAnswered>,
        IUpdateHandler<InterviewSummary, GeoLocationQuestionAnswered>,
        IUpdateHandler<InterviewSummary, MultipleOptionsLinkedQuestionAnswered>,
        IUpdateHandler<InterviewSummary, SingleOptionLinkedQuestionAnswered>,
        IUpdateHandler<InterviewSummary, TextListQuestionAnswered>,
        IUpdateHandler<InterviewSummary, QRBarcodeQuestionAnswered>,
        IUpdateHandler<InterviewSummary, PictureQuestionAnswered>,
        IUpdateHandler<InterviewSummary, AreaQuestionAnswered>,
        IUpdateHandler<InterviewSummary, AudioQuestionAnswered>
    {
        private readonly IReadSideRepositoryWriter<SpeedReportInterviewItem> speedReportRepository;

        private InterviewSummary RecordFirstAnswerIfNeeded(InterviewSummary interviewSummary, Guid interviewId, DateTime answerTime)
        {
            if (!interviewSummary.InterviewCommentedStatuses.Any())
                return interviewSummary;

            var lastStatus = interviewSummary.InterviewCommentedStatuses.LastOrDefault();
            if (lastStatus == null || lastStatus.Status != InterviewExportedAction.FirstAnswerSet)
                return interviewSummary;

            var reportInterviewItem = speedReportRepository.GetById(interviewId.FormatGuid());
            if (reportInterviewItem.FirstAnswerDate.HasValue)
                return interviewSummary;

            reportInterviewItem.FirstAnswerDate = answerTime;
            reportInterviewItem.SupervisorId = interviewSummary.TeamLeadId;
            reportInterviewItem.SupervisorName = interviewSummary.TeamLeadName;
            reportInterviewItem.InterviewerId = interviewSummary.ResponsibleId;
            reportInterviewItem.InterviewerName = interviewSummary.ResponsibleName;
            return interviewSummary;
        }

        private void RecordInterviewCreated(DateTime createdTime, Guid interviewId, Guid questionnaireId,
            long questionnaireVersion)
        {
            var state = new SpeedReportInterviewItem();
            state.CreatedDate = createdTime;
            state.InterviewId = interviewId.FormatGuid();
            state.QuestionnaireId = questionnaireId;
            state.QuestionnaireVersion = questionnaireVersion;
            speedReportRepository.Store(state, interviewId.FormatGuid());
        }

        public SpeedReportDenormalizerFunctional(IReadSideRepositoryWriter<SpeedReportInterviewItem> speedReportRepository) 
        {
            this.speedReportRepository = speedReportRepository;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewOnClientCreated> @event)
        {
            RecordInterviewCreated(
                createdTime: @event.Payload.OriginDate?.UtcDateTime ?? @event.EventTimeStamp,
                interviewId: state.InterviewId, 
                questionnaireId: state.QuestionnaireId, 
                questionnaireVersion: state.QuestionnaireVersion);
            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewCreated> @event)
        {
            RecordInterviewCreated(
                createdTime: @event.Payload.CreationTime ?? @event.EventTimeStamp,
                interviewId: state.InterviewId,
                questionnaireId: state.QuestionnaireId,
                questionnaireVersion: state.QuestionnaireVersion);
            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewFromPreloadedDataCreated> @event)
        {
            RecordInterviewCreated(
                createdTime: @event.Payload.OriginDate?.UtcDateTime ?? @event.EventTimeStamp,
                interviewId: state.InterviewId,
                questionnaireId: state.QuestionnaireId,
                questionnaireVersion: state.QuestionnaireVersion);
            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewDeleted> @event)
        {
            RemoveSpeedReportItem(@event.EventSourceId);
            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewHardDeleted> @event)
        {
            RemoveSpeedReportItem(@event.EventSourceId);
            return state;
        }

        private void RemoveSpeedReportItem(Guid interviewId)
        {
            speedReportRepository.Remove(interviewId.FormatGuid());
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewRestored> @event)
        {
            if (@event.Origin == Constants.HeadquartersSynchronizationOrigin)
                return state;

            var reportInterviewItem = new SpeedReportInterviewItem();
            var createdStatusRecord = state.InterviewCommentedStatuses.First(s => s.Status == InterviewExportedAction.Created);
            reportInterviewItem.InterviewId = state.SummaryId;
            reportInterviewItem.CreatedDate = createdStatusRecord.Timestamp;

            var firstAnswerSetStatusRecord = state.InterviewCommentedStatuses.FirstOrDefault(s => s.Status == InterviewExportedAction.FirstAnswerSet);
            if (firstAnswerSetStatusRecord != null)
            {
                reportInterviewItem.FirstAnswerDate = firstAnswerSetStatusRecord.Timestamp;
                reportInterviewItem.SupervisorId = firstAnswerSetStatusRecord.SupervisorId;
                reportInterviewItem.SupervisorName = firstAnswerSetStatusRecord.SupervisorName;
                reportInterviewItem.InterviewerId = firstAnswerSetStatusRecord.InterviewerId;
                reportInterviewItem.InterviewerName = firstAnswerSetStatusRecord.InterviewerName;
            }

            speedReportRepository.Store(reportInterviewItem, state.SummaryId);

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<TextQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(state, @event.EventSourceId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<MultipleOptionsQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(state, @event.EventSourceId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(state, @event.EventSourceId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<NumericRealQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(state, @event.EventSourceId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(state, @event.EventSourceId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<DateTimeQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(state, @event.EventSourceId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<GeoLocationQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(state, @event.EventSourceId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(state, @event.EventSourceId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<SingleOptionLinkedQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(state, @event.EventSourceId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<TextListQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(state, @event.EventSourceId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<QRBarcodeQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(state, @event.EventSourceId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<PictureQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(state, @event.EventSourceId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AreaQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(state, @event.EventSourceId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AudioQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(state, @event.EventSourceId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }
    }
}
