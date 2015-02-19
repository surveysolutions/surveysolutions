using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.User;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class SynchronizationDenormalizer : BaseDenormalizer, 
        IEventHandler<InterviewStatusChanged>, 
        IEventHandler<InterviewerAssigned>, 
        IEventHandler<InterviewHardDeleted>, 
        IEventHandler<QuestionnaireDeleted>, 
        IEventHandler<QuestionnaireAssemblyImported>,
        IEventHandler<TemplateImported>,
        IEventHandler<PlainQuestionnaireRegistered>,
        IEventHandler<NewUserCreated>,
        IEventHandler<UserChanged>,
        IEventHandler<UserLocked>,
        IEventHandler<UserUnlocked>,
        IEventHandler<UserLockedBySupervisor>,
        IEventHandler<UserUnlockedBySupervisor>
    {
        private readonly ISynchronizationDataStorage syncStorage;
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnriePropagationStructures;
        private readonly IReadSideKeyValueStorage<InterviewData> interviews;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummarys;
        private readonly IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        public SynchronizationDenormalizer(ISynchronizationDataStorage syncStorage, 
            IReadSideRepositoryWriter<UserDocument> users,
            IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnriePropagationStructures,
            IReadSideKeyValueStorage<InterviewData> interviews,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummarys,
            IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor, 
            IPlainQuestionnaireRepository plainQuestionnaireRepository)
        {
            this.syncStorage = syncStorage;
            this.users = users;
            this.questionnriePropagationStructures = questionnriePropagationStructures;
            this.interviews = interviews;
            this.interviewSummarys = interviewSummarys;
            this.questionnareAssemblyFileAccessor = questionnareAssemblyFileAccessor;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        public override object[] Writers
        {
            get { return new[] { syncStorage }; }
        }

        public override object[] Readers
        {
            get { return new object[] { users, questionnriePropagationStructures, interviews, interviewSummarys }; }

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
                if (this.IsNewStatusCompletedOrDeleted(newStatus))
                {
                    var interviewSummary = interviewSummarys.GetById(evnt.EventSourceId);
                    if (interviewSummary == null)
                        return;

                    if (this.IsInterviewWereRejectedAtLeastOnceBeboreOrNotCreateOnClient(interviewSummary))
                        this.syncStorage.MarkInterviewForClientDeleting(evnt.EventSourceId, interviewSummary.ResponsibleId, evnt.EventTimeStamp);
                }
            }
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            var interviewSummary = interviewSummarys.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return;

            if (this.IsInterviewWereRejectedAtLeastOnceBeboreOrNotCreateOnClient(interviewSummary))
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

            this.syncStorage.MarkInterviewForClientDeleting(evnt.EventSourceId, interviewSummary.ResponsibleId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            this.syncStorage.DeleteQuestionnaire(evnt.EventSourceId, evnt.Payload.QuestionnaireVersion, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<QuestionnaireAssemblyImported> evnt)
        {
            var assemblyAsBase64String = this.questionnareAssemblyFileAccessor.GetAssemblyAsBase64String(evnt.EventSourceId, evnt.Payload.Version);
            this.syncStorage.SaveQuestionnaireAssembly(evnt.EventSourceId, evnt.Payload.Version, assemblyAsBase64String, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            this.syncStorage.SaveQuestionnaire(evnt.Payload.Source, evnt.Payload.Version ?? evnt.EventSequence, evnt.Payload.AllowCensusMode,
                evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(evnt.EventSourceId, evnt.Payload.Version);
            this.syncStorage.SaveQuestionnaire(questionnaireDocument, evnt.Payload.Version, evnt.Payload.AllowCensusMode,
                 evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<NewUserCreated> evnt)
        {
            var doc = new UserDocument
            {
                UserName = evnt.Payload.Name,
                Password = evnt.Payload.Password,
                PublicKey = evnt.Payload.PublicKey,
                CreationDate = DateTime.UtcNow,
                Email = evnt.Payload.Email,
                IsLockedBySupervisor = evnt.Payload.IsLockedBySupervisor,
                IsLockedByHQ = evnt.Payload.IsLocked,
                Supervisor = evnt.Payload.Supervisor,
                Roles = new List<UserRoles>(evnt.Payload.Roles)
            };
            this.syncStorage.SaveUser(doc, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<UserChanged> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.Email = evnt.Payload.Email;
            item.Roles = evnt.Payload.Roles.ToList();
            item.Password = evnt.Payload.PasswordHash;

            this.syncStorage.SaveUser(item, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<UserLocked> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.IsLockedByHQ = true;
            this.syncStorage.SaveUser(item, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<UserUnlocked> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.IsLockedByHQ = false;
            this.syncStorage.SaveUser(item, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<UserLockedBySupervisor> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.IsLockedBySupervisor = true;
            this.syncStorage.SaveUser(item, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<UserUnlockedBySupervisor> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.IsLockedBySupervisor = false;
            this.syncStorage.SaveUser(item, evnt.EventTimeStamp);
        }

        private void ResendInterviewInNewStatus(InterviewData interviewData, InterviewStatus newStatus, string comments, DateTime timestamp)
        {
            if (interviewData == null)
                return;

            var interview = interviewData;

            var interviewSyncData = this.BuildSynchronizationDtoWhichIsAssignedToUser(interview, interview.ResponsibleId, newStatus, comments);

            this.syncStorage.SaveInterview(interviewSyncData, interview.ResponsibleId, timestamp);
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
            this.syncStorage.SaveInterview(interviewSyncData, interview.ResponsibleId, timestamp);
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
    }
}
