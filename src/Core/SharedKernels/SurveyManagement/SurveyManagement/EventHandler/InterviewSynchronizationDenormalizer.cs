using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    [Obsolete]
    internal class InterviewSynchronizationDenormalizer : BaseDenormalizer,
        IEventHandler<InterviewStatusChanged>,
        IEventHandler<InterviewerAssigned>,
        IEventHandler<InterviewHardDeleted>
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnriePropagationStructures;
        private readonly IReadSideKeyValueStorage<InterviewData> interviews;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaries;

        private readonly IReadSideRepositoryWriter<InterviewSyncPackageMeta> syncPackageWriter;
        private readonly IReadSideRepositoryWriter<InterviewResponsible> interviewResponsibleStorageWriter;

        private readonly IInterviewSynchronizationDtoFactory synchronizationDtoFactory;

        public InterviewSynchronizationDenormalizer(
            IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnriePropagationStructures,
            IReadSideKeyValueStorage<InterviewData> interviews,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaries,
            IReadSideRepositoryWriter<InterviewSyncPackageMeta> syncPackageWriter,
            IReadSideRepositoryWriter<InterviewResponsible> interviewResponsibleStorageWriter,
            IInterviewSynchronizationDtoFactory synchronizationDtoFactory)
        {
            this.questionnriePropagationStructures = questionnriePropagationStructures;
            this.interviews = interviews;
            this.interviewSummaries = interviewSummaries;
            this.syncPackageWriter = syncPackageWriter;
            this.interviewResponsibleStorageWriter = interviewResponsibleStorageWriter;
            this.synchronizationDtoFactory = synchronizationDtoFactory;
        }

        public override object[] Writers => new object[] { this.syncPackageWriter, this.interviewResponsibleStorageWriter };
        public override object[] Readers => new object[] { this.questionnriePropagationStructures, this.interviews, this.interviewSummaries };

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

                    this.StoreSynchronizationPackage(evnt.EventSourceId, interviewSummary.ResponsibleId,
                        SyncItemType.DeleteInterview, evnt.EventIdentifier.FormatGuid(), evnt.GlobalSequence);
                    break;

                case InterviewStatus.RejectedBySupervisor:
                    var interviewWithVersion = interviews.GetById(evnt.EventSourceId);
                    this.ResendInterviewInNewStatus(interviewWithVersion, 
                        newStatus, 
                        evnt.Payload.Comment, 
                        evnt.EventTimeStamp, 
                        evnt.EventIdentifier.FormatGuid(),
                        evnt.GlobalSequence);
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
                this.StoreSynchronizationPackage(evnt.EventSourceId, interviewResponsibleInfo.UserId,
                    SyncItemType.DeleteInterview, evnt.EventIdentifier.FormatGuid(), evnt.GlobalSequence);
            }

            if (this.IsInterviewWereRejectedAtLeastOnceBeboreOrNotCreateOnClient(interviewSummary))
            {
                var interviewWithVersion = this.interviews.GetById(evnt.EventSourceId);
                if (interviewWithVersion != null)
                {
                    if (interviewWithVersion.Status != InterviewStatus.RejectedByHeadquarters)
                        this.ResendInterviewForPerson(interviewWithVersion, 
                            evnt.Payload.InterviewerId, 
                            evnt.EventTimeStamp, 
                            evnt.EventIdentifier.FormatGuid(),
                            evnt.GlobalSequence);
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

            this.StoreSynchronizationPackage(evnt.EventSourceId, interviewSummary.ResponsibleId,
                SyncItemType.DeleteInterview, evnt.EventIdentifier.FormatGuid(), evnt.GlobalSequence);
        }

        private void ResendInterviewInNewStatus(InterviewData interviewData, InterviewStatus newStatus, string comments, DateTime timestamp, string packageId, long globalSequence)
        {
            if (interviewData == null)
                return;

            var interviewSyncData = this.synchronizationDtoFactory.BuildFrom(interviewData, interviewData.ResponsibleId, newStatus, comments, timestamp, null);

            this.SaveSynchronizationPackage(interviewSyncData, interviewData.ResponsibleId, packageId, globalSequence);
        }

        private void ResendInterviewForPerson(InterviewData interview, Guid responsibleId, DateTime timestamp, string packageId, long globalSequence)
        {
            InterviewSynchronizationDto interviewSyncData = this.synchronizationDtoFactory.BuildFrom(interview, responsibleId, InterviewStatus.InterviewerAssigned, null, null, timestamp);
            this.SaveSynchronizationPackage(interviewSyncData, interview.ResponsibleId, packageId, globalSequence);
        }

        private bool IsInterviewWereRejectedAtLeastOnceBeboreOrNotCreateOnClient(InterviewSummary interviewSummary)
        {
            return !interviewSummary.WasCreatedOnClient || interviewSummary.WasRejectedBySupervisor;
        }

        private void SaveSynchronizationPackage(InterviewSynchronizationDto doc, Guid responsibleId, string packageId, long globalSequence)
        {
            this.StoreSynchronizationPackage(doc.Id, responsibleId, SyncItemType.Interview, packageId, globalSequence);
        }

        public void StoreSynchronizationPackage(Guid interviewId,
            Guid userId,
            string itemType,
            string packageId,
            long globalSequence)
        {
            var syncPackageMeta = new InterviewSyncPackageMeta
            {
                InterviewId = interviewId,
                UserId = userId,
                ItemType = itemType,
                PackageId = $"{interviewId.FormatGuid()}${packageId}${userId.FormatGuid()}",
                SortIndex = globalSequence,
                SerializedPackageSize = 0
            };

            this.syncPackageWriter.Store(syncPackageMeta, syncPackageMeta.PackageId);
        }
    }
}
