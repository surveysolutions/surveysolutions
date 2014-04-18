using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Supervisor.Interviews;
using WB.Core.BoundedContexts.Supervisor.Questionnaires;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
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
        private readonly SynchronizationContext synchronizationContext;

        public InterviewsSynchronizer(IAtomFeedReader feedReader, 
            HeadquartersSettings settings, 
            ILogger logger,
            ICommandService commandService,
            IQueryablePlainStorageAccessor<LocalInterviewFeedEntry> plainStorage, 
            IQueryableReadSideRepositoryReader<UserDocument> users,
            IPlainQuestionnaireRepository plainQuestionnaireRepository,
            IHeadquartersQuestionnaireReader headquartersQuestionnaireReader,
            IHeadquartersInterviewReader headquartersInterviewReader,
            SynchronizationContext synchronizationContext)
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
            if (synchronizationContext == null) throw new ArgumentNullException("synchronizationContext");

            this.feedReader = feedReader;
            this.settings = settings;
            this.logger = logger;
            this.commandService = commandService;
            this.plainStorage = plainStorage;
            this.users = users;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.headquartersQuestionnaireReader = headquartersQuestionnaireReader;
            this.headquartersInterviewReader = headquartersInterviewReader;
            this.synchronizationContext = synchronizationContext;
        }

        public void Synchronize()
        {
            this.StoreEventsToLocalStorage();

            var localSupervisors = users.Query(_ => _.Where(x => x.Roles.Contains(UserRoles.Supervisor)));

            foreach (var localSupervisor in localSupervisors)
            {
                this.synchronizationContext.PushMessage(string.Format("Synchronizing interviews for supervisor {0}", localSupervisor.UserName));

                IEnumerable<LocalInterviewFeedEntry> events = 
                    this.plainStorage.Query(_ => _.Where(x => x.SupervisorId == localSupervisor.PublicKey.FormatGuid() && !x.Processed));
                foreach (var interviewFeedEntry in events)
                {
                    try
                    {
                        Uri interviewUri = interviewFeedEntry.InterviewUri;
                        var interview = this.headquartersInterviewReader.GetInterviewByUri(interviewUri).Result;

                        string questionnaireDetailsUrl = this.settings.QuestionnaireDetailsEndpoint
                            .Replace("{id}", interview.QuestionnaireId.FormatGuid())
                            .Replace("{version}", interview.QuestionnaireVersion.ToString());

                        switch (interviewFeedEntry.EntryType)
                        {
                            case EntryType.SupervisorAssigned:
                                this.StoreQuestionnaireDocumentFromHeadquartersIfNeeded(
                                    interview.QuestionnaireId,
                                    interview.QuestionnaireVersion,
                                    new Uri(questionnaireDetailsUrl));
                                this.CreateOrUpdateInterviewFromHeadquarters(interviewFeedEntry.InterviewUri, interviewFeedEntry.UserId,
                                    interviewFeedEntry.SupervisorId);
                                break;
                            case EntryType.InterviewUnassigned:
                                this.StoreQuestionnaireDocumentFromHeadquartersIfNeeded(
                                    interview.QuestionnaireId,
                                    interview.QuestionnaireVersion,
                                    new Uri(questionnaireDetailsUrl));
                                this.CreateOrUpdateInterviewFromHeadquarters(interviewFeedEntry.InterviewUri, interviewFeedEntry.UserId,
                                    interviewFeedEntry.SupervisorId);
                                this.commandService.Execute(new DeleteInterviewCommand(Guid.Parse(interviewFeedEntry.InterviewId),
                                    Guid.Empty));
                                break;
                            default:
                                this.logger.Warn(string.Format(
                                    "Unknown event of type {0} received in interviews feed. It was skipped and marked as processed with error. EventId: {1}",
                                    interviewFeedEntry.EntryType, interviewFeedEntry.EntryId));
                                break;
                        }

                        interviewFeedEntry.Processed = true;
                        interviewFeedEntry.ProcessedWithError = false;
                    }
                    catch (AggregateException ex)
                    {
                        foreach (var inner in ex.InnerExceptions)
                        {
                            this.MarkAsProcessedWithError(interviewFeedEntry, inner);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.MarkAsProcessedWithError(interviewFeedEntry, ex);
                        this.logger.Error(string.Format("Interviews synchronization error in event {0}.", interviewFeedEntry.EntryId), ex);
                    }
                    finally
                    {
                        this.plainStorage.Store(interviewFeedEntry, interviewFeedEntry.EntryId);
                    }
                }
            }
        }

        private void MarkAsProcessedWithError(LocalInterviewFeedEntry interviewFeedEntry, Exception ex)
        {
            interviewFeedEntry.ProcessedWithError = true;
            this.synchronizationContext.PushError(string.Format("Error while processing event {0}. ErrorMessage: {1}",
                interviewFeedEntry.EntryId, ex.Message));
        }

        private void StoreQuestionnaireDocumentFromHeadquartersIfNeeded(Guid questionnaireId, long questionnaireVersion, Uri questionnareUri)
        {
            if (this.IsQuestionnnaireAlreadyStoredLocally(questionnaireId, questionnaireVersion))
                return;

            this.synchronizationContext.PushMessage(string.Format("Loading questionnaire using {0} URL", questionnareUri));
            QuestionnaireDocument questionnaireDocument = this.headquartersQuestionnaireReader.GetQuestionnaireByUri(questionnareUri).Result;

            this.plainQuestionnaireRepository.StoreQuestionnaire(questionnaireId, questionnaireVersion, questionnaireDocument);
            this.commandService.Execute(new RegisterPlainQuestionnaire(questionnaireId, questionnaireVersion));
        }

        private bool IsQuestionnnaireAlreadyStoredLocally(Guid id, long version)
        {
            QuestionnaireDocument localQuestionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            return localQuestionnaireDocument != null;
        }

        private void CreateOrUpdateInterviewFromHeadquarters(Uri interviewUri, string supervisorId, string userId)
        {
            InterviewSynchronizationDto interviewDto = this.headquartersInterviewReader.GetInterviewByUri(interviewUri).Result;

            Guid userIdGuid = Guid.Parse(userId);

            this.commandService.Execute(new SynchronizeInterviewFromHeadquarters(interviewDto.Id, userIdGuid, interviewDto, DateTime.Now));
        }

        private void StoreEventsToLocalStorage()
        {
            var lastStoredEntry = this.plainStorage.Query(_ => _.OrderByDescending(x => x.Timestamp).Select(x => x.EntryId).FirstOrDefault());

            IEnumerable<AtomFeedEntry<LocalInterviewFeedEntry>> remoteEvents = 
                this.feedReader
                    .ReadAfterAsync<LocalInterviewFeedEntry>(this.settings.InterviewsFeedUrl, lastStoredEntry)
                    .Result;
            this.synchronizationContext.PushMessage(string.Format("Received {0} events from {1} feed", remoteEvents.Count(), this.settings.InterviewsFeedUrl));

            var newEvents = new List<LocalInterviewFeedEntry>();
            foreach (AtomFeedEntry<LocalInterviewFeedEntry> remoteEvent in remoteEvents)
            {
                var feedEntry = remoteEvent.Content;
                feedEntry.InterviewUri = remoteEvent.Links.Single(x => x.Rel == "enclosure").Href;
                newEvents.Add(feedEntry);
            }

            this.plainStorage.Store(newEvents.Select(x => Tuple.Create(x, x.EntryId)));
        }
    }
}