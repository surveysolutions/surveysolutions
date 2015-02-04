using System;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization.MetaInfo;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class InterviewSynchronizationDenormalizer : BaseSynchronizationDenormalizer, 
        IEventHandler<InterviewStatusChanged>, 
        IEventHandler<InterviewerAssigned>, 
        IEventHandler<InterviewHardDeleted>
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnriePropagationStructures;
        private readonly IReadSideKeyValueStorage<InterviewData> interviews;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummarys;

        public static Guid AssemblySeed = new Guid("371EF2E6-BF1D-4E36-927D-2AC13C41EF7B");
        private readonly IMetaInfoBuilder metaBuilder;

        public InterviewSynchronizationDenormalizer(IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnriePropagationStructures,
            IReadSideKeyValueStorage<InterviewData> interviews,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummarys, 
            IArchiveUtils archiver, 
            IJsonUtils jsonUtils,
            IMetaInfoBuilder metaBuilder,
            IReadSideRepositoryWriter<SynchronizationDelta> syncStorage,
            IQueryableReadSideRepositoryReader<SynchronizationDelta> syncStorageReader)
            : base(archiver, jsonUtils, syncStorage, syncStorageReader)
        {
            this.questionnriePropagationStructures = questionnriePropagationStructures;
            this.interviews = interviews;
            this.interviewSummarys = interviewSummarys;
            this.metaBuilder = metaBuilder;
        }

        public override object[] Writers
        {
            get { return new object[] { syncStorage }; }
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
                if (this.IsNewStatusCompletedOrDeleted(newStatus) && this.IsInterviewWereRejectedAtLeastOnceBeboreOrNotCreateOnClient(evnt.EventSourceId))
                    this.MarkInterviewForClientDeleting(evnt.EventSourceId, null, evnt.EventTimeStamp);
            }
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            if (this.IsInterviewWereRejectedAtLeastOnceBeboreOrNotCreateOnClient(evnt.EventSourceId))
            {
                var interviewWithVersion = interviews.GetById(evnt.EventSourceId);
                if (interviewWithVersion == null)
                    return;

                var interview = interviewWithVersion;
                if (interview.Status != InterviewStatus.RejectedByHeadquarters)
                    this.ResendInterviewForPerson(interview, evnt.Payload.InterviewerId, evnt.EventTimeStamp);
            }
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            var interviewSummary = interviewSummarys.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return;

            this.MarkInterviewForClientDeleting(evnt.EventSourceId, interviewSummary.ResponsibleId, evnt.EventTimeStamp);
        }

        private void ResendInterviewInNewStatus(InterviewData interviewData, InterviewStatus newStatus, string comments, DateTime timestamp)
        {
            if (interviewData == null)
                return;

            var interview = interviewData;

            var interviewSyncData = this.BuildSynchronizationDtoWhichIsAssignedToUser(interview, interview.ResponsibleId, newStatus, comments);

            this.SaveInterview(interviewSyncData, interview.ResponsibleId, timestamp);
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
            this.SaveInterview(interviewSyncData, interview.ResponsibleId, timestamp);
        }

        private bool IsNewStatusRejectedBySupervisor(InterviewStatus newStatus)
        {
            return newStatus == InterviewStatus.RejectedBySupervisor;
        }

        private bool IsNewStatusCompletedOrDeleted(InterviewStatus newStatus)
        {
            return newStatus == InterviewStatus.Completed || newStatus == InterviewStatus.Deleted;
        }

        private bool IsInterviewWereRejectedAtLeastOnceBeboreOrNotCreateOnClient(Guid interviewId)
        {
            var interviewSummary = interviewSummarys.GetById(interviewId);
            if (interviewSummary == null)
                return false;

            return !interviewSummary.WasCreatedOnClient ||
                interviewSummary.CommentedStatusesHistory.Any(s => s.Status == InterviewStatus.RejectedBySupervisor);
        }

        public void SaveInterview(InterviewSynchronizationDto doc, Guid responsibleId, DateTime timestamp)
        {
            var syncItem = CreateSyncItem(doc.Id, SyncItemType.Questionnare, GetItemAsContent(doc), GetItemAsContent(metaBuilder.GetInterviewMetaInfo(doc)));

            StoreChunk(syncItem, responsibleId, timestamp);
        }

        public void MarkInterviewForClientDeleting(Guid id, Guid? responsibleId, DateTime timestamp)
        {
            var syncItem = CreateSyncItem(id, SyncItemType.DeleteQuestionnare, id.ToString(), string.Empty);

            StoreChunk(syncItem, responsibleId, timestamp);
        }
    }
}
