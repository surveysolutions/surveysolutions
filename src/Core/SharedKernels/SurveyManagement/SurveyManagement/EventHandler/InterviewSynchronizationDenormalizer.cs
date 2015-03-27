using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
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
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaries;
        private readonly IJsonUtils jsonUtils;
        private readonly IOrderableSyncPackageWriter<InterviewSyncPackageMeta, InterviewSyncPackageContent> syncPackageWriter;

        private readonly IReadSideRepositoryWriter<InterviewResponsible> interviewResponsibleStorageWriter;

        private const string CounterId = "InterviewSyncPackageСounter";

        private readonly IInterviewSynchronizationDtoFactory synchronizationDtoFactory;
        private readonly IMetaInfoBuilder metaBuilder;

        public InterviewSynchronizationDenormalizer(
            IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnriePropagationStructures,
            IReadSideKeyValueStorage<InterviewData> interviews,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaries,
            IJsonUtils jsonUtils,
            IMetaInfoBuilder metaBuilder,
            IOrderableSyncPackageWriter<InterviewSyncPackageMeta, InterviewSyncPackageContent> syncPackageWriter,
            IReadSideRepositoryWriter<InterviewResponsible> interviewResponsibleStorageWriter,
            IInterviewSynchronizationDtoFactory synchronizationDtoFactory)
        {
            this.questionnriePropagationStructures = questionnriePropagationStructures;
            this.interviews = interviews;
            this.interviewSummaries = interviewSummaries;
            this.metaBuilder = metaBuilder;
            this.jsonUtils = jsonUtils;
            this.syncPackageWriter = syncPackageWriter;
            this.interviewResponsibleStorageWriter = interviewResponsibleStorageWriter;
            this.synchronizationDtoFactory = synchronizationDtoFactory;
        }

        public override object[] Writers
        {
            get { return new object[] { this.syncPackageWriter, this.interviewResponsibleStorageWriter }; }
        }

        public override object[] Readers
        {
            get { return new object[] { questionnriePropagationStructures, interviews, this.interviewSummaries }; }

        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            var newStatus = evnt.Payload.Status;

            switch (newStatus)
            {
                case InterviewStatus.Completed:
                case InterviewStatus.Deleted:
                    var interviewSummary = this.interviewSummaries.GetById(evnt.EventSourceId);
                    if (interviewSummary == null)
                        return;

                    this.MarkInterviewForClientDeleting(evnt.EventSourceId, interviewSummary.ResponsibleId, evnt.EventTimeStamp, interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);
                    break;

                case InterviewStatus.RejectedBySupervisor:
                    var interviewWithVersion = interviews.GetById(evnt.EventSourceId);
                    this.ResendInterviewInNewStatus(interviewWithVersion, newStatus, evnt.Payload.Comment, evnt.EventTimeStamp);
                    break;
            }
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            var interviewSummary = this.interviewSummaries.GetById(evnt.EventSourceId);
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
                interviewResponsibleInfo = new InterviewResponsible
                {
                    Id = evnt.EventSourceId.FormatGuid(),
                    InterviewId = evnt.EventSourceId
                };

            interviewResponsibleInfo.UserId = evnt.Payload.InterviewerId;
            interviewResponsibleStorageWriter.Store(interviewResponsibleInfo, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            var interviewSummary = this.interviewSummaries.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return;

            this.MarkInterviewForClientDeleting(evnt.EventSourceId, interviewSummary.ResponsibleId, evnt.EventTimeStamp,
                interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);
        }

        private void ResendInterviewInNewStatus(InterviewData interviewData, InterviewStatus newStatus, string comments, DateTime timestamp)
        {
            if (interviewData == null)
                return;

            var interviewSyncData = this.synchronizationDtoFactory.BuildFrom(interviewData, interviewData.ResponsibleId, newStatus, comments);

            this.SaveInterview(interviewSyncData, interviewData.ResponsibleId, timestamp, interviewData.QuestionnaireId, interviewData.QuestionnaireVersion);
        }

        private void ResendInterviewForPerson(InterviewData interview, Guid responsibleId, DateTime timestamp)
        {
            InterviewSynchronizationDto interviewSyncData = this.synchronizationDtoFactory.BuildFrom(interview, responsibleId, InterviewStatus.InterviewerAssigned, null);
            this.SaveInterview(interviewSyncData, interview.ResponsibleId, timestamp, interview.QuestionnaireId, interview.QuestionnaireVersion);
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

        public void StoreChunk(
            Guid interviewId,
            Guid questionnaireId,
            long questionnaireVersion,
            Guid? userId,
            string itemType,
            string content,
            string metaInfo,
            DateTime timestamp)
        {
            var syncPackageMeta = new InterviewSyncPackageMeta(
                       interviewId,
                       questionnaireId,
                       questionnaireVersion,
                       timestamp,
                       userId,
                       itemType,
                       string.IsNullOrEmpty(content) ? 0 : content.Length,
                       string.IsNullOrEmpty(metaInfo) ? 0 : metaInfo.Length);

            var syncPackageContent = new InterviewSyncPackageContent(content, metaInfo);

            syncPackageWriter.Store(syncPackageContent, syncPackageMeta, interviewId.FormatGuid(), CounterId);
        }
    }
}
