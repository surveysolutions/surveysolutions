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
using WB.Core.SharedKernels.DataCollection;
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
        private readonly IAsyncPlainStorage<QuestionnaireView> questionnaireRepository;
        private readonly IAsyncPlainStorage<InterviewView> interviewViewRepository;
        private readonly IAsyncPlainStorage<InterviewMultimediaView> interviewMultimediaViewRepository;
        private readonly IAsyncPlainStorage<InterviewFileView> interviewFileViewRepository;
        private readonly ICommandService commandService;
        private readonly IInterviewerPrincipal principal;
        private readonly IInterviewerEventStorage eventStore;
        private readonly IAggregateRootRepositoryWithCache aggregateRootRepositoryWithCache;
        private readonly ISnapshotStoreWithCache snapshotStoreWithCache;
        private readonly ISerializer serializer;
        private readonly IInterviewEventStreamOptimizer eventStreamOptimizer;

        public InterviewerInterviewAccessor(
            IAsyncPlainStorage<QuestionnaireView> questionnaireRepository,
            IAsyncPlainStorage<InterviewView> interviewViewRepository,
            IAsyncPlainStorage<InterviewMultimediaView> interviewMultimediaViewRepository,
            IAsyncPlainStorage<InterviewFileView> interviewFileViewRepository,
            ICommandService commandService,
            IInterviewerPrincipal principal,
            IInterviewerEventStorage eventStore,
            IAggregateRootRepositoryWithCache aggregateRootRepositoryWithCache,
            ISnapshotStoreWithCache snapshotStoreWithCache,
            ISerializer serializer,
            IInterviewEventStreamOptimizer eventStreamOptimizer)
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
            this.serializer = serializer;
            this.eventStreamOptimizer = eventStreamOptimizer;
        }

        public async Task RemoveInterviewAsync(Guid interviewId)
        {
            await this.commandService.ExecuteAsync(new HardDeleteInterview(interviewId,
                this.principal.CurrentUserIdentity.UserId));

            this.aggregateRootRepositoryWithCache.CleanCache();
            this.snapshotStoreWithCache.CleanCache();

            this.eventStore.RemoveEventSourceById(interviewId);

            await this.RemoveInterviewImagesAsync(interviewId);
        }

        private async Task RemoveInterviewImagesAsync(Guid interviewId)
        {
            var imageViews = await this.interviewMultimediaViewRepository.WhereAsync(image => image.InterviewId == interviewId);

            foreach (var interviewMultimediaView in imageViews)
            {
                await this.interviewFileViewRepository.RemoveAsync(interviewMultimediaView.FileId);
            }
            await this.interviewMultimediaViewRepository.RemoveAsync(imageViews);
        }

        public async Task<InterviewPackageApiView> GetPackageByCompletedInterviewAsync(Guid interviewId)
        {
            InterviewView interview = await Task.FromResult(this.interviewViewRepository.GetById(interviewId.FormatGuid()));

            return await Task.Run(() => this.CreateSyncItem(interview));
        }

        private InterviewPackageApiView CreateSyncItem(InterviewView interview)
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

            return new InterviewPackageApiView
            {
                InterviewId = interview.InterviewId,
                Events = this.serializer.Serialize(eventsToSend),
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
            List<CommittedEvent> storedEvents = this.eventStore.ReadFrom(interviewId, 0, int.MaxValue).ToList();

            var optimizedEvents = this.eventStreamOptimizer.RemoveEventsNotNeededToBeSent(storedEvents);

            AggregateRootEvent[] eventsToSend = optimizedEvents
                .Select(storedEvent => new AggregateRootEvent(storedEvent))
                .ToArray();

            return eventsToSend;
        }

        public async Task CreateInterviewAsync(InterviewApiView info, InterviewerInterviewApiView details)
        {
            var questionnaireView = this.questionnaireRepository.GetById(info.QuestionnaireIdentity.ToString());

            var interviewStatus = info.IsRejected ? InterviewStatus.RejectedBySupervisor :  InterviewStatus.InterviewerAssigned;
            var interviewDetails = this.serializer.Deserialize<InterviewSynchronizationDto>(details.Details, TypeSerializationSettings.AllTypes);

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
                sycnhronizedInterview: interviewDetails
                );

            await this.commandService.ExecuteAsync(createInterviewFromSynchronizationMetadataCommand);
            await this.commandService.ExecuteAsync(synchronizeInterviewCommand);
        }
    }
}