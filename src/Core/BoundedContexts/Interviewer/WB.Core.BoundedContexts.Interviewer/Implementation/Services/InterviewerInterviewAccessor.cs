using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Events;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Interviewer.Implementation.Storage;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.WriteSide;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerInterviewAccessor : IInterviewerInterviewAccessor
    {
        private readonly IPlainStorage<QuestionnaireView> questionnaireRepository;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPlainStorage<InterviewMultimediaView> interviewMultimediaViewRepository;
        private readonly IPlainStorage<InterviewFileView> interviewFileViewRepository;
        private readonly ICommandService commandService;
        private readonly IInterviewerPrincipal principal;
        private readonly IInterviewerEventStorage eventStore;
        private readonly IEventSourcedAggregateRootRepositoryWithCache aggregateRootRepositoryWithCache;
        private readonly ISnapshotStoreWithCache snapshotStoreWithCache;
        private readonly IJsonAllTypesSerializer synchronizationSerializer;
        private readonly IInterviewEventStreamOptimizer eventStreamOptimizer;
        private readonly ILogger logger;

        public InterviewerInterviewAccessor(
            IPlainStorage<QuestionnaireView> questionnaireRepository,
            IPlainStorage<InterviewView> interviewViewRepository,
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewRepository,
            IPlainStorage<InterviewFileView> interviewFileViewRepository,
            ICommandService commandService,
            IInterviewerPrincipal principal,
            IInterviewerEventStorage eventStore,
            IEventSourcedAggregateRootRepositoryWithCache aggregateRootRepositoryWithCache,
            ISnapshotStoreWithCache snapshotStoreWithCache,
            IJsonAllTypesSerializer synchronizationSerializer,
            IInterviewEventStreamOptimizer eventStreamOptimizer,
            ILogger logger)
        {
            this.questionnaireRepository = questionnaireRepository;
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
            this.logger = logger;
        }

        public void RemoveInterview(Guid interviewId)
        {
            this.aggregateRootRepositoryWithCache.CleanCache();
            this.snapshotStoreWithCache.CleanCache();

            this.eventStore.RemoveEventSourceById(interviewId);

            this.interviewViewRepository.Remove(interviewId.FormatGuid());

            this.RemoveInterviewImages(interviewId);
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

        public async Task<InterviewPackageApiView> GetInteviewEventsPackageOrNullAsync(Guid interviewId)
        {
            InterviewView interview = await Task.FromResult(this.interviewViewRepository.GetById(interviewId.FormatGuid()));

            return await Task.Run(() => this.BuildInterviewPackageOrNull(interview));
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
                FeaturedQuestionsMeta = interview.AnswersOnPrefilledQuestions.Select(ToFeaturedQuestionMeta).ToList()
            };

            return new InterviewPackageApiView
            {
                InterviewId = interview.InterviewId,
                Events = this.synchronizationSerializer.Serialize(eventsToSend),
                MetaInfo = metadata
            };
        }

        private FeaturedQuestionMeta ToFeaturedQuestionMeta(InterviewAnswerOnPrefilledQuestionView prefilledQuestion)
        {
            return new FeaturedQuestionMeta(prefilledQuestion.QuestionId, prefilledQuestion.QuestionText,
                prefilledQuestion.Answer);
        }

        private AggregateRootEvent[] BuildEventStreamOfLocalChangesToSend(Guid interviewId)
        {
            List<CommittedEvent> storedEvents = this.eventStore.Read(interviewId, 0).ToList();

            this.ThrowIfEventSequenceIsBroken(storedEvents, interviewId);

            var optimizedEvents = this.eventStreamOptimizer.RemoveEventsNotNeededToBeSent(storedEvents);

            AggregateRootEvent[] eventsToSend = optimizedEvents
                .Select(storedEvent => new AggregateRootEvent(storedEvent))
                .ToArray();

            return eventsToSend;
        }

        private void ThrowIfEventSequenceIsBroken(List<CommittedEvent> events, Guid interviewId)
        {
            for (int index = 0; index < events.Count; index++)
            {
                CommittedEvent @event = events[index];
                int expectedEventSequence = index + 1;

                if (expectedEventSequence != @event.EventSequence)
                {
                    var message = $"Expected event sequence {expectedEventSequence} is missing. Event stream is not full. Interview ID: {interviewId.FormatGuid()}.";
                    this.logger.Error(message);
                    throw new ArgumentException(message);
                }
            }
        }

        public async Task CreateInterviewAsync(InterviewApiView info, InterviewerInterviewApiView details)
        {
            var questionnaireView = this.questionnaireRepository.GetById(info.QuestionnaireIdentity.ToString());

            var interviewStatus = info.IsRejected ? InterviewStatus.RejectedBySupervisor :  InterviewStatus.InterviewerAssigned;
            var interviewDetails = this.synchronizationSerializer.Deserialize<InterviewSynchronizationDto>(details.Details);

            var createInterviewFromSynchronizationMetadataCommand = new CreateInterviewFromSynchronizationMetadata(
                interviewId: info.Id,
                userId: this.principal.CurrentUserIdentity.UserId,
                questionnaireId: info.QuestionnaireIdentity.QuestionnaireId,
                questionnaireVersion: info.QuestionnaireIdentity.Version,
                status: interviewStatus,
                featuredQuestionsMeta: details.AnswersOnPrefilledQuestions ?? new AnsweredQuestionSynchronizationDto[0],
                comments: interviewDetails.Comments,
                rejectedDateTime: interviewDetails.RejectDateTime,
                interviewerAssignedDateTime: interviewDetails.InterviewerAssignedDateTime,
                valid: true,
                createdOnClient: questionnaireView.Census);

           var synchronizeInterviewCommand = new SynchronizeInterviewCommand(
                interviewId: info.Id,
                userId: this.principal.CurrentUserIdentity.UserId,
                sycnhronizedInterview: interviewDetails);

            await this.commandService.ExecuteAsync(createInterviewFromSynchronizationMetadataCommand);
            await this.commandService.ExecuteAsync(synchronizeInterviewCommand);
        }
    }
}