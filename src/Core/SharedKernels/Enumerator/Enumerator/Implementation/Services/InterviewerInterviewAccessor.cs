using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Events;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.WriteSide;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class InterviewerInterviewAccessor : IInterviewerInterviewAccessor
    {
        private readonly IPlainStorage<QuestionnaireView> questionnaireRepository;
        private readonly IPlainStorage<PrefilledQuestionView> prefilledQuestions;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPlainStorage<InterviewMultimediaView> interviewMultimediaViewRepository;
        private readonly IPlainStorage<InterviewFileView> interviewFileViewRepository;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IEnumeratorEventStorage eventStore;
        private readonly IEventSourcedAggregateRootRepositoryWithCache aggregateRootRepositoryWithCache;
        private readonly ISnapshotStoreWithCache snapshotStoreWithCache;
        private readonly IJsonAllTypesSerializer synchronizationSerializer;
        private readonly IInterviewEventStreamOptimizer eventStreamOptimizer;
        private readonly ILiteEventRegistry eventRegistry;

        public InterviewerInterviewAccessor(
            IPlainStorage<QuestionnaireView> questionnaireRepository,
            IPlainStorage<PrefilledQuestionView> prefilledQuestions,
            IPlainStorage<InterviewView> interviewViewRepository,
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewRepository,
            IPlainStorage<InterviewFileView> interviewFileViewRepository,
            ICommandService commandService,
            IPrincipal principal,
            IEnumeratorEventStorage eventStore,
            IEventSourcedAggregateRootRepositoryWithCache aggregateRootRepositoryWithCache,
            ISnapshotStoreWithCache snapshotStoreWithCache,
            IJsonAllTypesSerializer synchronizationSerializer,
            IInterviewEventStreamOptimizer eventStreamOptimizer,
            ILiteEventRegistry eventRegistry)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.prefilledQuestions = prefilledQuestions;
            this.interviewViewRepository = interviewViewRepository;
            this.interviewMultimediaViewRepository = interviewMultimediaViewRepository;
            this.interviewFileViewRepository = interviewFileViewRepository;
            this.commandService = commandService;
            this.principal = principal;
            this.eventStore = eventStore;
            this.aggregateRootRepositoryWithCache = aggregateRootRepositoryWithCache;
            this.snapshotStoreWithCache = snapshotStoreWithCache;
            this.synchronizationSerializer = synchronizationSerializer;
            this.eventStreamOptimizer = eventStreamOptimizer;
            this.eventRegistry = eventRegistry;
        }

        public void RemoveInterview(Guid interviewId)
        {
            this.aggregateRootRepositoryWithCache.CleanCache();
            this.snapshotStoreWithCache.CleanCache();

            this.interviewViewRepository.Remove(interviewId.FormatGuid());

            this.RemoveInterviewImages(interviewId);
            this.eventStore.RemoveEventSourceById(interviewId);
            this.eventRegistry.RemoveAggregateRoot(interviewId.FormatGuid());
        }

        private void RemoveInterviewImages(Guid interviewId)
        {
            var imageViews = this.interviewMultimediaViewRepository.Where(image => image.InterviewId == interviewId);

            foreach (var interviewMultimediaView in imageViews)
            {
                this.interviewFileViewRepository.Remove(interviewMultimediaView.FileId);
            }
            this.interviewMultimediaViewRepository.Remove(imageViews);
        }

        public InterviewPackageApiView GetInteviewEventsPackageOrNull(Guid interviewId)
        {
            InterviewView interview = this.interviewViewRepository.GetById(interviewId.FormatGuid());

            return this.BuildInterviewPackageOrNull(interview);
        }

        public IReadOnlyCollection<CommittedEvent> GetPendingInteviewEvents(Guid interviewId)
        {
            List<CommittedEvent> storedEvents = this.eventStore.GetPendingEvents(interviewId);
            return storedEvents.AsReadOnly();
        }

        private InterviewPackageApiView BuildInterviewPackageOrNull(InterviewView interview)
        {
            AggregateRootEvent[] eventsToSend = this.BuildEventStreamOfLocalChangesToSend(interview.InterviewId);

            if (eventsToSend.Length == 0)
                return null;

            var questionnaireIdentity = QuestionnaireIdentity.Parse(interview.QuestionnaireId);

            var metadata = new InterviewMetaInfo
            {
                PublicKey = interview.InterviewId,
                ResponsibleId = interview.ResponsibleId,
                Status = (int)interview.Status,
                RejectDateTime = interview.RejectedDateTime,
                InterviewerAssignedDateTime = interview.InterviewerAssignedDateTime,
                TemplateId = questionnaireIdentity.QuestionnaireId,
                Comments = interview.LastInterviewerOrSupervisorComment,
                Valid = true,
                CreatedOnClient = interview.Census,
                TemplateVersion = questionnaireIdentity.Version,
                FeaturedQuestionsMeta = this.prefilledQuestions.Where(x => x.InterviewId == interview.InterviewId).Select(ToFeaturedQuestionMeta).ToList()
            };

            return new InterviewPackageApiView
            {
                InterviewId = interview.InterviewId,
                Events = this.synchronizationSerializer.Serialize(eventsToSend),
                MetaInfo = metadata
            };
        }

        private FeaturedQuestionMeta ToFeaturedQuestionMeta(PrefilledQuestionView prefilledQuestion)
        {
            return new FeaturedQuestionMeta(prefilledQuestion.QuestionId, prefilledQuestion.QuestionText,
                prefilledQuestion.Answer);
        }

        private AggregateRootEvent[] BuildEventStreamOfLocalChangesToSend(Guid interviewId)
        {
            List<CommittedEvent> storedEvents = this.eventStore.GetPendingEvents(interviewId).ToList();

            var optimizedEvents = this.eventStreamOptimizer.RemoveEventsNotNeededToBeSent(storedEvents);

            AggregateRootEvent[] eventsToSend = optimizedEvents
                .Select(storedEvent => new AggregateRootEvent(storedEvent))
                .ToArray();

            return eventsToSend;
        }

        public async Task CreateInterviewAsync(InterviewApiView info, InterviewerInterviewApiView details)
        {
            var questionnaireView = this.questionnaireRepository.GetById(info.QuestionnaireIdentity.ToString());

            var interviewDetails = this.synchronizationSerializer.Deserialize<InterviewSynchronizationDto>(details.Details);
            var interviewStatus = interviewDetails.Status;

            var synchronizeInterviewCommand = new SynchronizeInterviewCommand(
                interviewId: info.Id,
                userId: this.principal.CurrentUserIdentity.UserId,
                featuredQuestionsMeta: details.AnswersOnPrefilledQuestions ?? new AnsweredQuestionSynchronizationDto[0],
                createdOnClient: questionnaireView.Census,
                initialStatus: interviewStatus,
                sycnhronizedInterview: interviewDetails);

            await this.commandService.ExecuteAsync(synchronizeInterviewCommand);
        }
    }
}
