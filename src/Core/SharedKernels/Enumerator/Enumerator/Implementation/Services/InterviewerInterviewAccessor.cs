using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Events;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.WriteSide;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
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
        private readonly IJsonAllTypesSerializer synchronizationSerializer;
        private readonly IInterviewEventStreamOptimizer eventStreamOptimizer;
        private readonly IViewModelEventRegistry eventRegistry;
        private readonly IPlainStorage<InterviewSequenceView, Guid> interviewSequenceViewRepository;
        private readonly ILiteEventBus eventBus;
        private readonly ILogger logger;

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
            IJsonAllTypesSerializer synchronizationSerializer,
            IInterviewEventStreamOptimizer eventStreamOptimizer,
            IViewModelEventRegistry eventRegistry, 
            IPlainStorage<InterviewSequenceView, Guid> interviewSequenceViewRepository,
            ILiteEventBus eventBus,
            ILogger logger)
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
            this.synchronizationSerializer = synchronizationSerializer;
            this.eventStreamOptimizer = eventStreamOptimizer;
            this.eventRegistry = eventRegistry;
            this.interviewSequenceViewRepository = interviewSequenceViewRepository;
            this.eventBus = eventBus;
            this.logger = logger;
        }

        public void RemoveInterview(Guid interviewId)
        {
            this.aggregateRootRepositoryWithCache.CleanCache();

            this.interviewViewRepository.Remove(interviewId.FormatGuid());
            this.interviewSequenceViewRepository.Remove(interviewId);

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
        
        public InterviewPackageApiView GetInterviewEventsPackageOrNull(InterviewPackageContainer packageContainer)
        {
            InterviewView interview = this.interviewViewRepository.GetById(packageContainer.InterviewId.FormatGuid());
            var optimizedEvents = packageContainer.Events;

            AggregateRootEvent[] eventsToSend = optimizedEvents
                .Select(storedEvent => new AggregateRootEvent(storedEvent))
                .ToArray();

            if (eventsToSend.Length == 0)
                return null;

            var questionnaireIdentity = QuestionnaireIdentity.Parse(interview.QuestionnaireId);

            var metadata = new InterviewMetaInfo(this.prefilledQuestions.Where(x => x.InterviewId == interview.InterviewId).Select(ToFeaturedQuestionMeta).ToList())
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
                TemplateVersion = questionnaireIdentity.Version
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

        private IReadOnlyCollection<CommittedEvent> GetAllEventsToSend(Guid interviewId)
        {
            var minVersion = this.eventStore.GetLastEventKnownToHq(interviewId) + 1;
            return this.eventStore.Read(interviewId, minVersion)
                .ToReadOnlyCollection();
        }

        private IReadOnlyCollection<CommittedEvent> GetFilteredEventsToSend(Guid interviewId)
        {
            var lastCompleteSequence = this.eventStore.GetMaxSequenceForAnyEvent(interviewId, nameof(InterviewCompleted), nameof(InterviewModeChanged));
            var lastComplete = this.eventStore.GetEventByEventSequence(interviewId, lastCompleteSequence);

            return this.eventStreamOptimizer.FilterEventsToBeSent(
                this.eventStore.Read(interviewId, this.eventStore.GetLastEventKnownToHq(interviewId) + 1),
                lastComplete?.CommitId);
        }

        public InterviewPackageContainer GetInterviewEventStreamContainer(Guid interviewId, bool needCompress)
        {
            var events = needCompress
                ? this.GetFilteredEventsToSend(interviewId)
                : this.GetAllEventsToSend(interviewId);
            return new InterviewPackageContainer(interviewId, events);
        }

        public void CheckAndProcessInterviewsToFixViews()
        {
            var registeredItems = this.interviewViewRepository.LoadAll().Select(i => i.InterviewId).ToList();
            var itemsInStore = this.eventStore.GetListOfAllItemsIds();
            var orphans = itemsInStore.Except(registeredItems);
            
            foreach (var orphan in orphans)
            {
                logger.Info($"Processing orphan interview {orphan}");
                try
                {
                    List<CommittedEvent> storedEvents = this.eventStore.Read(orphan, 0).ToList();
                    this.eventBus.PublishCommittedEvents(storedEvents);
                }
                catch (Exception e)
                {
                    logger.Error($"Error on processing orphan interview {orphan}", e);
                }
            }
            
            foreach (var registeredItem in registeredItems)
            {
                var sequence =  this.eventStore.GetMaxSequenceForAnyEvent(registeredItem, nameof(InterviewStatusChanged));
                var lastStatus = this.eventStore.GetEventByEventSequence(registeredItem, sequence);

                if (lastStatus.Payload is InterviewStatusChanged status)
                {
                    string itemId = registeredItem.FormatGuid();
                    if (this.interviewViewRepository.GetById(itemId).Status != status.Status)
                    {
                        logger.Info($"Processing incorrect interview {itemId}");
                        try
                        {
                            this.interviewViewRepository.Remove(itemId);
                            List<CommittedEvent> storedEvents = this.eventStore.Read(registeredItem, 0).ToList();

                            this.eventBus.PublishCommittedEvents(storedEvents);
                        }
                        catch (Exception e)
                        {
                            logger.Error($"Error on processing incorrect interview {itemId}", e);
                        }
                    }
                }
            }
        }

        public void MarkEventsAsReceivedByHQ(Guid interviewId)
        {
            eventStore.MarkAllEventsAsReceivedByHq(interviewId);
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
