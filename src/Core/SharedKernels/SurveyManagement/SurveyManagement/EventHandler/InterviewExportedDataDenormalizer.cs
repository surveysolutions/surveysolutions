using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class InterviewExportedDataDenormalizer : 
        BaseDenormalizer, IAtomicEventHandler,
        IEventHandler<InterviewApprovedByHQ>, 
        IEventHandler<SupervisorAssigned>,
        IEventHandler<InterviewerAssigned>,
        IEventHandler<InterviewCompleted>,
        IEventHandler<InterviewRestarted>,
        IEventHandler<InterviewApproved>,
        IEventHandler<InterviewDeleted>,
        IEventHandler<InterviewHardDeleted>,
        IEventHandler<InterviewRejected>,
        IEventHandler<InterviewRejectedByHQ>,
        IEventHandler<TextQuestionAnswered>,
        IEventHandler<MultipleOptionsQuestionAnswered>,
        IEventHandler<SingleOptionQuestionAnswered>,
        IEventHandler<NumericRealQuestionAnswered>,
        IEventHandler<NumericIntegerQuestionAnswered>,
        IEventHandler<DateTimeQuestionAnswered>,
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        IEventHandler<SingleOptionLinkedQuestionAnswered>,
        IEventHandler<TextListQuestionAnswered>,
        IEventHandler<QRBarcodeQuestionAnswered>,
        IEventHandler<PictureQuestionAnswered>,
        IEventHandler<InterviewRestored>
    {
        private readonly InterviewExportedAction[] listOfActionsAfterWhichFirstAnswerSetAtionShouldBeRecorded = new[] { InterviewExportedAction.InterviewerAssigned, InterviewExportedAction.RejectedBySupervisor, InterviewExportedAction.Restarted };
        private readonly IReadSideKeyValueStorage<RecordFirstAnswerMarkerView> recordFirstAnswerMarkerViewWriter;
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage;
        private readonly IReadSideRepositoryWriter<InterviewStatuses> statuses;
        private readonly IDataExportRepositoryWriter dataExportWriter;

        public InterviewExportedDataDenormalizer(IDataExportRepositoryWriter dataExportWriter,
            IReadSideKeyValueStorage<RecordFirstAnswerMarkerView> recordFirstAnswerMarkerViewWriter, 
            IReadSideRepositoryWriter<UserDocument> userDocumentWriter, 
            IReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage, IReadSideRepositoryWriter<InterviewStatuses> statuses)
        {
            this.dataExportWriter = dataExportWriter;
            this.recordFirstAnswerMarkerViewWriter = recordFirstAnswerMarkerViewWriter;
            this.users = userDocumentWriter;
            this.interviewSummaryStorage = interviewSummaryStorage;
            this.statuses = statuses;
        }

        public override object[] Writers
        {
            get { return new object[] { dataExportWriter, recordFirstAnswerMarkerViewWriter }; }
        }

        public void CleanWritersByEventSource(Guid eventSourceId)
        {
            this.dataExportWriter.DeleteInterview(eventSourceId);
            this.recordFirstAnswerMarkerViewWriter.Remove(eventSourceId);
        }

        public override object[] Readers
        {
            get { return new object[] { users, interviewSummaryStorage, statuses }; }
        }

        public void Handle(IPublishedEvent<InterviewApprovedByHQ> evnt)
        {
            RecordAction(InterviewExportedAction.ApproveByHeadquarter, evnt.Payload.UserId, evnt.EventSourceId, evnt.EventTimeStamp);
        }

        private void UpdateInterviewData(Guid interviewId, Guid userId, DateTime timeStamp, InterviewExportedAction action)
        {
            this.dataExportWriter.AddExportedDataByInterview(interviewId);

            RecordAction(action, userId, interviewId, timeStamp);
        }

        private void RecordAction(InterviewExportedAction action, Guid userId, Guid interviewId, DateTime timeStamp)
        {
            this.dataExportWriter.AddInterviewAction(action, interviewId, userId, timeStamp);
            if (listOfActionsAfterWhichFirstAnswerSetAtionShouldBeRecorded.Contains(action))
                recordFirstAnswerMarkerViewWriter.Store(
                    new RecordFirstAnswerMarkerView(interviewId),
                    interviewId);
            else
                recordFirstAnswerMarkerViewWriter.Remove(interviewId);
        }

        private void RecordFirstAnswerIfNeeded(Guid interviewId, Guid userId, DateTime answerTime)
        {
            var recordFirstAnswerMarkerView = this.recordFirstAnswerMarkerViewWriter.GetById(interviewId);
            if (recordFirstAnswerMarkerView == null)
                return;

            UserDocument responsible = this.users.GetById(userId);

            if (responsible == null || !responsible.Roles.Contains(UserRoles.Operator))
                return;

            this.dataExportWriter.AddInterviewAction(InterviewExportedAction.FirstAnswerSet, interviewId, userId, answerTime);
            this.recordFirstAnswerMarkerViewWriter.Remove(interviewId);
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            UpdateInterviewData(evnt.EventSourceId, evnt.Payload.UserId, evnt.EventTimeStamp, InterviewExportedAction.SupervisorAssigned);
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            RecordAction(InterviewExportedAction.InterviewerAssigned, evnt.Payload.UserId, evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
        {
            UpdateInterviewData(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.CompleteTime ?? evnt.EventTimeStamp, InterviewExportedAction.Completed);
        }

        public void Handle(IPublishedEvent<InterviewRestarted> evnt)
        {
            RecordAction(InterviewExportedAction.Restarted, evnt.Payload.UserId, evnt.EventSourceId, evnt.Payload.RestartTime ?? evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewApproved> evnt)
        {
            UpdateInterviewData(evnt.EventSourceId, evnt.Payload.UserId, evnt.EventTimeStamp, InterviewExportedAction.ApproveBySupervisor);
        }

        public void Handle(IPublishedEvent<InterviewRejected> evnt)
        {
            UpdateInterviewData(evnt.EventSourceId, evnt.Payload.UserId, evnt.EventTimeStamp, InterviewExportedAction.RejectedBySupervisor);
        }

        public void Handle(IPublishedEvent<InterviewRejectedByHQ> evnt)
        {
            RecordAction(InterviewExportedAction.RejectedByHeadquarter, evnt.Payload.UserId, evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<TextQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<TextListQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<PictureQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            this.dataExportWriter.DeleteInterview(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            this.dataExportWriter.DeleteInterview(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewRestored> evnt)
        {
            var interviewSummary = interviewSummaryStorage.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
            {
                return;
            }

            var interviewStatusHistory = this.statuses.GetById(evnt.EventSourceId);
            if (interviewStatusHistory == null)
            {
                throw new NullReferenceException(string.Format("Missing interview status changes history for interview {0}", evnt.EventSourceId));
            }

            foreach (var interviewCommentedStatus in interviewStatusHistory.InterviewCommentedStatuses)
            {
                var action = this.GetInterviewExportedAction(interviewCommentedStatus.Status);
                if (!action.HasValue)
                    continue;

                this.dataExportWriter.AddInterviewAction(action.Value, evnt.EventSourceId, interviewCommentedStatus.StatusChangeOriginatorId, interviewCommentedStatus.Timestamp);
            }
        }

        private InterviewExportedAction? GetInterviewExportedAction(InterviewStatus status)
        {
            switch (status)
            {
                case  InterviewStatus.ApprovedByHeadquarters:
                    return InterviewExportedAction.ApproveByHeadquarter;
                case InterviewStatus.SupervisorAssigned:
                    return InterviewExportedAction.SupervisorAssigned;
                case InterviewStatus.InterviewerAssigned:
                    return InterviewExportedAction.InterviewerAssigned;
                case InterviewStatus.Completed:
                    return InterviewExportedAction.Completed;
                case InterviewStatus.Restarted:
                    return InterviewExportedAction.Restarted;
                case InterviewStatus.ApprovedBySupervisor:
                    return InterviewExportedAction.ApproveBySupervisor;
                case InterviewStatus.RejectedBySupervisor:
                    return InterviewExportedAction.RejectedBySupervisor;
                case InterviewStatus.RejectedByHeadquarters:
                    return InterviewExportedAction.RejectedByHeadquarter;
            }
            return null;
        }
    }
}
