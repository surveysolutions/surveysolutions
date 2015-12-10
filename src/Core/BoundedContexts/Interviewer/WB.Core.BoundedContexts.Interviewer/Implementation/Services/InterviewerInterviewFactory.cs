using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Events;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.WriteSide;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Events;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerInterviewFactory : IInterviewerInterviewFactory
    {
        private readonly IAsyncPlainStorage<QuestionnaireView> questionnaireRepository;
        private readonly IAsyncPlainStorage<EventView> eventRepository;
        private readonly IAsyncPlainStorage<InterviewView> interviewViewRepository;
        private readonly IAsyncPlainStorage<InterviewMultimediaView> interviewMultimediaViewRepository;
        private readonly IAsyncPlainStorage<InterviewFileView> interviewFileViewRepository;
        private readonly ICommandService commandService;
        private readonly IInterviewerPrincipal principal;
        private readonly ISerializer serializer;
        private readonly IStringCompressor compressor;
        private readonly IEventStore eventStore;
        private readonly IAggregateRootRepositoryWithCache aggregateRootRepositoryWithCache;

        public InterviewerInterviewFactory(
            IAsyncPlainStorage<QuestionnaireView> questionnaireRepository,
            IAsyncPlainStorage<EventView> eventRepository,
            IAsyncPlainStorage<InterviewView> interviewViewRepository,
            IAsyncPlainStorage<InterviewMultimediaView> interviewMultimediaViewRepository,
            IAsyncPlainStorage<InterviewFileView> interviewFileViewRepository,
            ICommandService commandService,
            IInterviewerPrincipal principal,
            ISerializer serializer,
            IStringCompressor compressor,
            IEventStore eventStore,
            IAggregateRootRepositoryWithCache aggregateRootRepositoryWithCache)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.eventRepository = eventRepository;
            this.interviewViewRepository = interviewViewRepository;
            this.interviewMultimediaViewRepository = interviewMultimediaViewRepository;
            this.interviewFileViewRepository = interviewFileViewRepository;
            this.commandService = commandService;
            this.principal = principal;
            this.serializer = serializer;
            this.compressor = compressor;
            this.eventStore = eventStore;
            this.aggregateRootRepositoryWithCache = aggregateRootRepositoryWithCache;
        }

        public async Task RemoveInterviewAsync(Guid interviewId)
        {
            await this.commandService.ExecuteAsync(new HardDeleteInterview(interviewId,
                this.principal.CurrentUserIdentity.UserId));

            this.aggregateRootRepositoryWithCache.CleanCache();

            var eventViews = await Task.Run(() => this.eventRepository.Query(
                events => events.Where(evnt => evnt.EventSourceId == interviewId).ToList()));
            await this.eventRepository.RemoveAsync(eventViews);

            await this.RemoveInterviewImagesAsync(interviewId);
        }

        private async Task RemoveInterviewImagesAsync(Guid interviewId)
        {
            var imageViews = this.interviewMultimediaViewRepository.Query(images =>
                images.Where(image => image.InterviewId == interviewId).ToList());

            foreach (var interviewMultimediaView in imageViews)
            {
                await this.interviewFileViewRepository.RemoveAsync(interviewMultimediaView.FileId);
            }
            await this.interviewMultimediaViewRepository.RemoveAsync(imageViews);
        }

        public async Task<string> GetPackageByCompletedInterviewAsync(Guid interviewId)
        {
            InterviewView interview = await Task.FromResult(this.interviewViewRepository.GetById(interviewId.FormatGuid()));

            return await Task.Run(() => this.CreateSyncItem(interview));
        }

        private string CreateSyncItem(InterviewView interview)
        {
            AggregateRootEvent[] eventsToSend = this.BuildEventStreamOfLocalChangesToSend(interview.InterviewId);

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

            var syncItem = new SyncItem
            {
                Content = this.compressor.CompressString(this.serializer.Serialize(eventsToSend, TypeSerializationSettings.AllTypes)),
                IsCompressed = true,
                ItemType = SyncItemType.Interview,
                MetaInfo = this.compressor.CompressString(this.serializer.Serialize(metadata, TypeSerializationSettings.AllTypes)),
                RootId = interview.InterviewId
            };

            return this.serializer.Serialize(syncItem);
        }

        private FeaturedQuestionMeta ToFeaturedQuestionMeta(InterviewAnswerOnPrefilledQuestionView prefilledQuestion)
        {
            return new FeaturedQuestionMeta(prefilledQuestion.QuestionId, prefilledQuestion.QuestionText,
                prefilledQuestion.Answer);
        }

        private AggregateRootEvent[] BuildEventStreamOfLocalChangesToSend(Guid interviewId)
        {
            List<CommittedEvent> storedEvents = this.eventStore.ReadFrom(interviewId, 0, int.MaxValue).ToList();

            AggregateRootEvent[] eventsToSend = storedEvents
                .Where(storedEvent=>!(storedEvent.Payload is InterviewAnswersFromSyncPackageRestored || storedEvent.Payload is InterviewOnClientCreated))
                .Select(storedEvent => new AggregateRootEvent(storedEvent))
                .ToArray();

            return eventsToSend;
        }

        public async Task CreateInterviewAsync(InterviewApiView info, InterviewDetailsApiView details)
        {
            var questionnaireView = await Task.FromResult(this.questionnaireRepository.GetById(info.QuestionnaireIdentity.ToString()));

            var answersOnPrefilledQuestions = details
                .AnswersOnPrefilledQuestions?
                .Select(prefilledQuestion => new AnsweredQuestionSynchronizationDto(prefilledQuestion.QuestionId, new decimal[0], prefilledQuestion.Answer, string.Empty))
                .ToArray();

            var interviewStatus = info.IsRejected ? InterviewStatus.RejectedBySupervisor :  InterviewStatus.InterviewerAssigned;

            var createInterviewFromSynchronizationMetadataCommand = new CreateInterviewFromSynchronizationMetadata(
                interviewId: info.Id,
                userId: this.principal.CurrentUserIdentity.UserId,
                questionnaireId: info.QuestionnaireIdentity.QuestionnaireId,
                questionnaireVersion: info.QuestionnaireIdentity.Version,
                status: interviewStatus,
                featuredQuestionsMeta: answersOnPrefilledQuestions ?? new AnsweredQuestionSynchronizationDto[0],
                comments: details.LastSupervisorOrInterviewerComment,
                rejectedDateTime: details.RejectedDateTime,
                interviewerAssignedDateTime: details.InterviewerAssignedDateTime,
                valid: true,
                createdOnClient: questionnaireView.Census);

            var synchronizeInterviewCommand = new SynchronizeInterviewCommand(
                interviewId: info.Id,
                userId: this.principal.CurrentUserIdentity.UserId,
                sycnhronizedInterview: new InterviewSynchronizationDto
                {
                    Id = info.Id,
                    Status = interviewStatus,
                    Comments = details.LastSupervisorOrInterviewerComment,
                    CreatedOnClient = questionnaireView.Census,
                    QuestionnaireId = info.QuestionnaireIdentity.QuestionnaireId,
                    QuestionnaireVersion = info.QuestionnaireIdentity.Version,
                    RejectDateTime = details.RejectedDateTime,
                    InterviewerAssignedDateTime = details.InterviewerAssignedDateTime,
                    UserId = this.principal.CurrentUserIdentity.UserId,
                    WasCompleted = details.WasCompleted,
                    Answers = details.Answers?.Select(this.ToAnsweredQuestionSynchronizationDto).ToArray() ?? new AnsweredQuestionSynchronizationDto[0],
                    DisabledGroups = new HashSet<InterviewItemId>(details.DisabledGroups?.Select(this.ToInterviewItemId) ?? new InterviewItemId[0]),
                    DisabledQuestions = new HashSet<InterviewItemId>(details.DisabledQuestions?.Select(this.ToInterviewItemId) ?? new InterviewItemId[0]),
                    InvalidAnsweredQuestions = new HashSet<InterviewItemId>(details.InvalidAnsweredQuestions?.Select(this.ToInterviewItemId) ?? new InterviewItemId[0]),
                    ValidAnsweredQuestions = new HashSet<InterviewItemId>(details.ValidAnsweredQuestions?.Select(this.ToInterviewItemId) ?? new InterviewItemId[0]),
                    RosterGroupInstances = details.RosterGroupInstances?.ToDictionary(roster => this.ToInterviewItemId(roster.Identity),
                        roster => roster.Instances?.Select(this.ToRosterSynchronizationDto).ToArray() ?? new RosterSynchronizationDto[0]) ??
                                           new Dictionary<InterviewItemId, RosterSynchronizationDto[]>()
                });

            await this.commandService.ExecuteAsync(createInterviewFromSynchronizationMetadataCommand);
            await this.commandService.ExecuteAsync(synchronizeInterviewCommand);
        }

        private AnsweredQuestionSynchronizationDto ToAnsweredQuestionSynchronizationDto(InterviewAnswerApiView answer)
        {
            return new AnsweredQuestionSynchronizationDto
            {
                Id = answer.QuestionId,
                QuestionRosterVector = answer.QuestionRosterVector ?? new decimal[0],
                AllComments = new CommentSynchronizationDto[0],
                Comments = answer.LastSupervisorOrInterviewerComment,
                Answer = this.serializer.Deserialize<object>(answer.JsonAnswer)
            };
        }

        private RosterSynchronizationDto ToRosterSynchronizationDto(RosterInstanceApiView roster)
        {
            return new RosterSynchronizationDto(
                rosterId: roster.RosterId,
                outerScopeRosterVector: roster.OuterScopeRosterVector?.ToArray() ?? new decimal[0],
                rosterInstanceId: roster.RosterInstanceId,
                sortIndex: roster.SortIndex,
                rosterTitle: roster.RosterTitle);
        }

        private InterviewItemId ToInterviewItemId(IdentityApiView identity)
        {
            return new InterviewItemId(identity.QuestionId, identity.RosterVector?.ToArray() ?? new decimal[0]);
        }
    }
}