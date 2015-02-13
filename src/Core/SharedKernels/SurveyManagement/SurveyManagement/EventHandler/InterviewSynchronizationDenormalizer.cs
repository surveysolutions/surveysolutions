﻿using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization.MetaInfo;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class InterviewSynchronizationDenormalizer : BaseDenormalizer, 
        IEventHandler<InterviewStatusChanged>, 
        IEventHandler<InterviewerAssigned>, 
        IEventHandler<InterviewHardDeleted>
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnriePropagationStructures;
        private readonly IReadSideKeyValueStorage<InterviewData> interviews;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummarys;
        private readonly IJsonUtils jsonUtils;
        private readonly IOrderableSyncPackageWriter<InterviewSyncPackageMetaInformation> interviewPackageStorageWriter;
        private readonly IReadSideKeyValueStorage<InterviewSyncPackageContent> interviewPackageContentStore;
        private readonly IReadSideRepositoryWriter<InterviewResponsible> interviewResponsibleStorageWriter;

        private readonly IMetaInfoBuilder metaBuilder;

        public InterviewSynchronizationDenormalizer(
            IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnriePropagationStructures,
            IReadSideKeyValueStorage<InterviewData> interviews,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummarys, 
            IJsonUtils jsonUtils,
            IMetaInfoBuilder metaBuilder,
            IOrderableSyncPackageWriter<InterviewSyncPackageMetaInformation> interviewPackageStorageWriter,
            IReadSideRepositoryWriter<InterviewResponsible> interviewResponsibleStorageWriter, IReadSideKeyValueStorage<InterviewSyncPackageContent> interviewPackageContentStore)
        {
            this.questionnriePropagationStructures = questionnriePropagationStructures;
            this.interviews = interviews;
            this.interviewSummarys = interviewSummarys;
            this.metaBuilder = metaBuilder;
            this.jsonUtils = jsonUtils;
            this.interviewPackageStorageWriter = interviewPackageStorageWriter;
            this.interviewResponsibleStorageWriter = interviewResponsibleStorageWriter;
            this.interviewPackageContentStore = interviewPackageContentStore;
        }

        public override object[] Writers
        {
            get { return new object[] { this.interviewPackageStorageWriter, interviewResponsibleStorageWriter, interviewPackageContentStore }; }
        }

        public override object[] Readers
        {
            get { return new object[] { questionnriePropagationStructures, interviews, interviewSummarys }; }

        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            var newStatus = evnt.Payload.Status;

            if (this.IsNewStatusRejectedBySupervisor(newStatus))
            {
                var interviewWithVersion = interviews.GetById(evnt.EventSourceId);
                this.ResendInterviewInNewStatus(interviewWithVersion, newStatus, evnt.Payload.Comment, evnt.EventTimeStamp);
            }
            else
            {
                var interviewSummary = interviewSummarys.GetById(evnt.EventSourceId);
                if (interviewSummary == null)
                    return;

                if (this.IsNewStatusCompletedOrDeleted(newStatus))
                    this.MarkInterviewForClientDeleting(evnt.EventSourceId, interviewSummary.ResponsibleId, evnt.EventTimeStamp, interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);
            }
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            var interviewSummary = interviewSummarys.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return;

            var interviewResponsibleInfo = interviewResponsibleStorageWriter.GetById(evnt.EventSourceId);
            if (interviewResponsibleInfo != null && interviewResponsibleInfo.UserId != evnt.Payload.InterviewerId && interviewSummary.ResponsibleRole == UserRoles.Operator)
            {
                this.MarkInterviewForClientDeleting(evnt.EventSourceId, interviewResponsibleInfo.UserId, evnt.EventTimeStamp,
                    interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);
            }

            if (this.IsInterviewWereRejectedAtLeastOnceBeboreOrNotCreateOnClient(interviewSummary))
            {
                var interviewWithVersion = this.interviews.GetById(evnt.EventSourceId);
                if (interviewWithVersion != null)
                {
                    if (interviewWithVersion.Status != InterviewStatus.RejectedByHeadquarters) 
                        this.ResendInterviewForPerson(interviewWithVersion, evnt.Payload.InterviewerId, evnt.EventTimeStamp);
                }
            }

            if (interviewResponsibleInfo == null)
                interviewResponsibleInfo = new InterviewResponsible { InterviewId = evnt.EventSourceId };

            interviewResponsibleInfo.UserId = evnt.Payload.InterviewerId;
            interviewResponsibleStorageWriter.Store(interviewResponsibleInfo, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            var interviewSummary = interviewSummarys.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return;

            this.MarkInterviewForClientDeleting(evnt.EventSourceId, interviewSummary.ResponsibleId, evnt.EventTimeStamp, 
                interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);

            //interviewResponsibleStorageWriter.Remove(evnt.EventSourceId);
        }

        private void ResendInterviewInNewStatus(InterviewData interviewData, InterviewStatus newStatus, string comments, DateTime timestamp)
        {
            if (interviewData == null)
                return;

            var interview = interviewData;

            var interviewSyncData = this.BuildSynchronizationDtoWhichIsAssignedToUser(interview, interview.ResponsibleId, newStatus, comments);

            this.SaveInterview(interviewSyncData, interview.ResponsibleId, timestamp, interviewData.QuestionnaireId, interviewData.QuestionnaireVersion);
        }

        private InterviewSynchronizationDto BuildSynchronizationDtoWhichIsAssignedToUser(InterviewData interview, Guid userId,
            InterviewStatus status, string comments)
        {
            var factory = new InterviewSynchronizationDtoFactory(this.questionnriePropagationStructures);
            return factory.BuildFrom(interview, userId, status, comments);
        }

        private void ResendInterviewForPerson(InterviewData interview, Guid responsibleId, DateTime timestamp)
        {
            InterviewSynchronizationDto interviewSyncData = this.BuildSynchronizationDtoWhichIsAssignedToUser(interview, responsibleId, InterviewStatus.InterviewerAssigned, null);
            this.SaveInterview(interviewSyncData, interview.ResponsibleId, timestamp, interview.QuestionnaireId, interview.QuestionnaireVersion);
        }

        private bool IsNewStatusRejectedBySupervisor(InterviewStatus newStatus)
        {
            return newStatus == InterviewStatus.RejectedBySupervisor;
        }

        private bool IsNewStatusCompletedOrDeleted(InterviewStatus newStatus)
        {
            return newStatus == InterviewStatus.Completed || newStatus == InterviewStatus.Deleted;
        }

        private bool IsInterviewWereRejectedAtLeastOnceBeboreOrNotCreateOnClient(InterviewSummary interviewSummary)
        {
            return !interviewSummary.WasCreatedOnClient ||
                interviewSummary.CommentedStatusesHistory.Any(s => s.Status == InterviewStatus.RejectedBySupervisor);
        }

        public void SaveInterview(InterviewSynchronizationDto doc, Guid responsibleId, DateTime timestamp,
            Guid questionnaireId, long questionnaireVersion)
        {
            this.StoreChunk(
                doc.Id, 
                questionnaireId, 
                questionnaireVersion, 
                responsibleId, 
                SyncItemType.Interview, 
                this.jsonUtils.Serialize(doc, TypeSerializationSettings.AllTypes), 
                this.jsonUtils.Serialize(this.metaBuilder.GetInterviewMetaInfo(doc), TypeSerializationSettings.AllTypes), 
                timestamp);
        }

        public void MarkInterviewForClientDeleting(Guid interviewId, Guid? responsibleId, DateTime timestamp,
            Guid questionnaireId, long questionnaireVersion)
        {
            this.StoreChunk(
                interviewId, 
                questionnaireId,
                questionnaireVersion, 
                responsibleId, 
                SyncItemType.DeleteInterview, 
                interviewId.ToString(), 
                string.Empty, 
                timestamp);
        }

        public void StoreChunk(Guid interviewId, Guid questionnaireId, long questionnaireVersion, Guid? userId, string itemType, string content, string metaInfo, DateTime timestamp)
        {
            long sortIndex = interviewPackageStorageWriter.GetNextOrder();

            var synchronizationDelta = new InterviewSyncPackageMetaInformation(interviewId, questionnaireId,
                questionnaireVersion, timestamp, userId, sortIndex, itemType,
                string.IsNullOrEmpty(content) ? 0 : content.Length, string.IsNullOrEmpty(metaInfo) ? 0 : metaInfo.Length);

            this.interviewPackageStorageWriter.Store(synchronizationDelta, synchronizationDelta.PackageId);

            this.interviewPackageContentStore.Store(new InterviewSyncPackageContent(synchronizationDelta.PackageId, content, metaInfo), synchronizationDelta.PackageId);
        }
    }
}
