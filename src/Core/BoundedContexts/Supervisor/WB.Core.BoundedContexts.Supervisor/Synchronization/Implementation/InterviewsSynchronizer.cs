using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.ServiceModel;
using Raven.Client.Linq;
using WB.Core.BoundedContexts.Supervisor.Interviews;
using WB.Core.BoundedContexts.Supervisor.Questionnaires;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class InterviewsSynchronizer : IInterviewsSynchronizer
    {
        private readonly IAtomFeedReader feedReader;
        private readonly HeadquartersSettings settings;
        private readonly ILogger logger;
        private readonly ICommandService commandService;
        private readonly IQueryablePlainStorageAccessor<LocalInterviewFeedEntry> plainStorage;
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IHeadquartersQuestionnaireReader headquartersQuestionnaireReader;
        private readonly IHeadquartersInterviewReader headquartersInterviewReader;

        public InterviewsSynchronizer(IAtomFeedReader feedReader, 
            HeadquartersSettings settings, 
            ILogger logger,
            ICommandService commandService,
            IQueryablePlainStorageAccessor<LocalInterviewFeedEntry> plainStorage, 
            IQueryableReadSideRepositoryReader<UserDocument> users,
            IPlainQuestionnaireRepository plainQuestionnaireRepository,
            IHeadquartersQuestionnaireReader headquartersQuestionnaireReader,
            IHeadquartersInterviewReader headquartersInterviewReader)
        {
            if (feedReader == null) throw new ArgumentNullException("feedReader");
            if (settings == null) throw new ArgumentNullException("settings");
            if (logger == null) throw new ArgumentNullException("logger");
            if (commandService == null) throw new ArgumentNullException("commandService");
            if (plainStorage == null) throw new ArgumentNullException("plainStorage");
            if (users == null) throw new ArgumentNullException("users");
            if (plainQuestionnaireRepository == null) throw new ArgumentNullException("plainQuestionnaireRepository");
            if (headquartersQuestionnaireReader == null) throw new ArgumentNullException("headquartersQuestionnaireReader");
            if (headquartersInterviewReader == null) throw new ArgumentNullException("headquartersInterviewReader");

            this.feedReader = feedReader;
            this.settings = settings;
            this.logger = logger;
            this.commandService = commandService;
            this.plainStorage = plainStorage;
            this.users = users;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.headquartersQuestionnaireReader = headquartersQuestionnaireReader;
            this.headquartersInterviewReader = headquartersInterviewReader;
        }

        public void Synchronize()
        {
            this.StoreEventsToLocalStorage();

            var localSupervisors = users.Query(_ => _.Where(x => x.Roles.Contains(UserRoles.Supervisor)));

            foreach (var localSupervisor in localSupervisors)
            {
                IEnumerable<LocalInterviewFeedEntry> events = this.plainStorage.Query(_ => _.Where(x => x.SupervisorId == localSupervisor.PublicKey.FormatGuid()));
                foreach (var interviewFeedEntry in events)
                {

                    try
                    {
                        switch (interviewFeedEntry.EntryType)
                        {
                            case EntryType.SupervisorAssigned:
                                this.StoreQuestionnaireDocumentFromHeadquartersIfNeeded(
                                    Guid.Parse(interviewFeedEntry.QuestionnaireId), interviewFeedEntry.QuestionnaireVersion,
                                    interviewFeedEntry.QuestionnaireUri);
                                this.CreateOrUpdateInterviewFromHeadquarters(interviewFeedEntry.InterviewUri);
                                break;
                            case EntryType.InterviewUnassigned:
                                this.StoreQuestionnaireDocumentFromHeadquartersIfNeeded(
                                    Guid.Parse(interviewFeedEntry.QuestionnaireId), interviewFeedEntry.QuestionnaireVersion,
                                    interviewFeedEntry.QuestionnaireUri);
                                this.CreateOrUpdateInterviewFromHeadquarters(interviewFeedEntry.InterviewUri);
                                this.commandService.Execute(new DeleteInterviewCommand(Guid.Parse(interviewFeedEntry.InterviewId), Guid.Empty));
                                break;
                            default:
                                this.logger.Warn(string.Format(
                                    "Unknown event of type {0} received in interviews feed. It was skipped and marked as processed with error. EventId: {1}",
                                    interviewFeedEntry.EntryType, interviewFeedEntry.EntryId));
                                break;
                        }

                        interviewFeedEntry.Processed = true;
                    }
                    catch (Exception ex)
                    {
                        interviewFeedEntry.ProcessedWithError = true;
                        this.logger.Error(string.Format("Interviews synchronization error in event {0}.", interviewFeedEntry.EntryId), ex);
                    }
                    finally
                    {
                        this.plainStorage.Store(interviewFeedEntry, interviewFeedEntry.EntryId);
                    }
                }
            }
        }

        private void StoreQuestionnaireDocumentFromHeadquartersIfNeeded(Guid questionnaireId, long questionnaireVersion, Uri questionnareUri)
        {
            if (this.IsQuestionnnaireAlreadyStoredLocally(questionnaireId, questionnaireVersion))
                return;

            QuestionnaireDocument questionnaireDocument = this.headquartersQuestionnaireReader.GetQuestionnaireByUri(questionnareUri).Result;

            this.plainQuestionnaireRepository.StoreQuestionnaire(questionnaireId, questionnaireVersion, questionnaireDocument);
            this.commandService.Execute(new RegisterPlainQuestionnaire(questionnaireId, questionnaireVersion));
        }

        private bool IsQuestionnnaireAlreadyStoredLocally(Guid id, long version)
        {
            QuestionnaireDocument localQuestionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            return localQuestionnaireDocument != null;
        }

        private void CreateOrUpdateInterviewFromHeadquarters(Uri interviewUri)
        {
            InterviewSynchronizationDto interviewDto = this.headquartersInterviewReader.GetInterviewByUri(interviewUri).Result;

            this.commandService.Execute(new ApplySynchronizationMetadata(
                interviewId: interviewDto.Id,
                userId: interviewDto.UserId,
                questionnaireId: interviewDto.QuestionnaireId,
                status: interviewDto.Status,
                featuredQuestionsMeta: null,
                comments: null,
                valid: true));

            this.commandService.Execute(new SynchronizeInterviewCommand(interviewDto.Id, Guid.Empty, interviewDto));
        }

        private void StoreEventsToLocalStorage()
        {
            var lastStoredEntry = this.plainStorage.Query(_ => _.OrderByDescending(x => x.Timestamp).Select(x => x.EntryId).FirstOrDefault());

            IEnumerable<AtomFeedEntry<LocalInterviewFeedEntry>> remoteEvents = this.feedReader.ReadAfterAsync<LocalInterviewFeedEntry>(this.settings.InterviewsFeedUrl, lastStoredEntry)
                .Result;

            var newEvents = new List<LocalInterviewFeedEntry>();
            foreach (AtomFeedEntry<LocalInterviewFeedEntry> remoteEvent in remoteEvents)
            {
                var feedEntry = remoteEvent.Content;
                feedEntry.QuestionnaireUri = remoteEvent.Links.Single(x => x.Rel == "related").Href;
                feedEntry.InterviewUri = remoteEvent.Links.Single(x => x.Rel == "enclosure").Href;
                newEvents.Add(feedEntry);
            }

            this.plainStorage.Store(newEvents.Select(x => Tuple.Create(x, x.EntryId)));
        }
    }
}