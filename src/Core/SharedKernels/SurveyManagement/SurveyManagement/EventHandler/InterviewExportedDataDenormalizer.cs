using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Odbc;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using Quartz.Util;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class InterviewExportedDataDenormalizer : 
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
        IEventHandler<NumericQuestionAnswered>,
        IEventHandler<NumericIntegerQuestionAnswered>,
        IEventHandler<DateTimeQuestionAnswered>,
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        IEventHandler<SingleOptionLinkedQuestionAnswered>,
        IEventHandler<TextListQuestionAnswered>,
        IEventHandler<QRBarcodeQuestionAnswered>,
        IEventHandler<PictureQuestionAnswered>,

        IEventHandler
    {
        private readonly InterviewExportedAction[] listOfActionsAfterWhichFirstAnswerSetAtionShouldBeRecorded = new[] { InterviewExportedAction.InterviewerAssigned, InterviewExportedAction.RejectedBySupervisor, InterviewExportedAction.Restarted };
        private readonly IReadSideRepositoryWriter<RecordFirstAnswerMarkerView> recordFirstAnswerMarkerViewWriter;
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IDataExportRepositoryWriter dataExportWriter;

        public InterviewExportedDataDenormalizer(IDataExportRepositoryWriter dataExportWriter,
            IReadSideRepositoryWriter<RecordFirstAnswerMarkerView> recordFirstAnswerMarkerViewWriter, 
            IReadSideRepositoryWriter<UserDocument> userDocumentWriter)
        {
            this.dataExportWriter = dataExportWriter;
            this.recordFirstAnswerMarkerViewWriter = recordFirstAnswerMarkerViewWriter;
            this.users = userDocumentWriter;
        }

        public string Name { get { return this.GetType().Name; } }

        public Type[] UsesViews
        {
            get { return new Type[] { typeof(InterviewData), typeof(UserDocument), typeof(RecordFirstAnswerMarkerView), typeof(QuestionnaireExportStructure) }; }
        }

        public Type[] BuildsViews { get { return new Type[] { typeof(InterviewDataExportView), typeof(RecordFirstAnswerMarkerView) }; } }

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

        public void Handle(IPublishedEvent<NumericQuestionAnswered> evnt)
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
    }
}
