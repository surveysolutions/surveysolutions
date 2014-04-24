using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Main.Core;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Supervisor.Extensions;
using WB.Core.BoundedContexts.Supervisor.Interviews;
using WB.Core.BoundedContexts.Supervisor.Questionnaires;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

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
        private readonly IEventStore eventStore;
        private readonly IJsonUtils jsonUtils;
        private readonly IQueryableReadSideRepositoryWriter<InterviewSummary> interviewSummaryRepositoryWriter;

        public InterviewsSynchronizer(IAtomFeedReader feedReader, 
            HeadquartersSettings settings, 
            ILogger logger,
            ICommandService commandService,
            IQueryablePlainStorageAccessor<LocalInterviewFeedEntry> plainStorage, 
            IQueryableReadSideRepositoryReader<UserDocument> users,
            IPlainQuestionnaireRepository plainQuestionnaireRepository,
            IHeadquartersQuestionnaireReader headquartersQuestionnaireReader,
            IHeadquartersInterviewReader headquartersInterviewReader,
            SynchronizationContext synchronizationContext,
            IEventStore eventStore,
            IJsonUtils jsonUtils,
            IQueryableReadSideRepositoryWriter<InterviewSummary> interviewSummaryRepositoryWriter)
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
            if (eventStore == null) throw new ArgumentNullException("eventStore");
            if (jsonUtils == null) throw new ArgumentNullException("jsonUtils");
            if (interviewSummaryRepositoryWriter == null) throw new ArgumentNullException("interviewSummaryRepositoryWriter");

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
            this.eventStore = eventStore;
            this.jsonUtils = jsonUtils;
            this.interviewSummaryRepositoryWriter = interviewSummaryRepositoryWriter;
        }

        public void Pull()
        {
            this.StoreEventsToLocalStorage();

            var localSupervisors = users.Query(_ => _.Where(x => x.Roles.Any(role => role == UserRoles.Supervisor)));

            foreach (var localSupervisor in localSupervisors)
            {
                IEnumerable<LocalInterviewFeedEntry> events = 
                    this.plainStorage.Query(_ => _.Where(x => x.SupervisorId == localSupervisor.PublicKey.FormatGuid() && !x.Processed));

                this.synchronizationContext.PushMessage(string.Format("Synchronizing interviews for supervisor '{0}'. Events count: {1}", localSupervisor.UserName, events.Count()));
                
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
                                this.CreateOrUpdateInterviewFromHeadquarters(interviewFeedEntry.InterviewUri, interviewFeedEntry.SupervisorId,
                                    interviewFeedEntry.UserId);
                                break;
                            case EntryType.InterviewUnassigned:
                                this.commandService.Execute(new DeleteInterviewCommand(
                                    interviewId: Guid.Parse(interviewFeedEntry.InterviewId),
                                    userId: Guid.Parse(interviewFeedEntry.UserId)));
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

        public void Push()
        {
            List<Guid> interviewsToPush = this.GetInterviewsToPush();

            foreach (var interviewId in interviewsToPush)
            {
                this.PushInterview(interviewId);
            }
        }


        private void MarkAsProcessedWithError(LocalInterviewFeedEntry interviewFeedEntry, Exception ex)
        {
            interviewFeedEntry.ProcessedWithError = true;
            this.synchronizationContext.PushError(string.Format("Error while processing event {0}. ErrorMessage: {1}. Exception messages: {2}",
                interviewFeedEntry.EntryId, ex.Message, string.Join(Environment.NewLine, ex.UnwrapAllInnerExceptions().Select(x => x.Message))));
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
            this.synchronizationContext.PushMessage(string.Format("Loading interview using {0} URL", interviewUri));
            InterviewSynchronizationDto interviewDto = this.headquartersInterviewReader.GetInterviewByUri(interviewUri).Result;

            var userIdGuid = Guid.Parse(userId);
            var supervisorIdGuid = Guid.Parse(supervisorId);

            this.commandService.Execute(new SynchronizeInterviewFromHeadquarters(interviewDto.Id, userIdGuid, supervisorIdGuid, interviewDto, DateTime.Now));
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


        private List<Guid> GetInterviewsToPush()
        {
            // TODO: TLK: replace with QueryAll or custom denormalizer

            return this.interviewSummaryRepositoryWriter.Query(_ => _
                //.Where(summary => summary.Status == InterviewStatus.ApprovedBySupervisor)
                .Select(summary => summary.InterviewId)
                .ToList());
        }

        private void PushInterview(Guid interviewId)
        {
            AggregateRootEvent[] eventsToSend = this.BuildEventStreamOfLocalChangesToSend(interviewId);

            if (eventsToSend.Length == 0)
                return;

            string dataToBeSent = this.GetInterviewDataToBeSentAsString(interviewId, eventsToSend);

            bool interviewSuccessfullySent = this.SendInterviewData(interviewId, dataToBeSent);

            if (interviewSuccessfullySent)
            {
                this.MarkInterviewAsSentToHeadquarters(interviewId);
            }
        }

        private void MarkInterviewAsSentToHeadquarters(Guid interviewId)
        {
            // TODO: TLK
        }

        private string GetInterviewDataToBeSentAsString(Guid interviewId, AggregateRootEvent[] eventsToSend)
        {
            InterviewSummary interviewSummary = this.interviewSummaryRepositoryWriter.GetById(interviewId);

            InterviewCommentedStatus lastInterviewCommentedStatus = interviewSummary.CommentedStatusesHistory.LastOrDefault();
            string lastComment = lastInterviewCommentedStatus != null ? lastInterviewCommentedStatus.Comment : string.Empty;

            var metadata = new InterviewMetaInfo
            {
                PublicKey = interviewId,
                ResponsibleId = interviewSummary.ResponsibleId,
                Status = (int)interviewSummary.Status,
                TemplateId = interviewSummary.QuestionnaireId,
                Comments = lastComment,
                Valid = !interviewSummary.HasErrors,
                CreatedOnClient = interviewSummary.WasCreatedOnClient,
                TemplateVersion = interviewSummary.QuestionnaireVersion,
            };

            var syncItem = new SyncItem
            {
                Content = PackageHelper.CompressString(this.jsonUtils.GetItemAsContent(eventsToSend)),
                IsCompressed = true,
                ItemType = SyncItemType.Questionnare,
                MetaInfo = PackageHelper.CompressString(this.jsonUtils.GetItemAsContent(metadata)),
                Id = interviewId
            };

            return this.jsonUtils.GetItemAsContent(syncItem);
        }

        private bool SendInterviewData(Guid interviewId, string interviewData)
        {
            using (var client = new HttpClient().AppendAuthToken(this.settings))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, this.settings.InterviewsPushUrl) {
                    Content = new StringContent(interviewData)
                };

                HttpResponseMessage response = client.SendAsync(request).Result;

                string result = response.Content.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode)
                {
                    this.logger.Error(string.Format("Failed to send interview {0}. Server response: {1}",
                        interviewId, result));

                    return false;
                }

                bool serverOperationSucceeded;

                try
                {
                    serverOperationSucceeded = this.jsonUtils.Deserrialize<bool>(result);
                }
                catch (Exception exception)
                {
                    this.logger.Error(
                        string.Format("Failed to read server response while sending interview {0}. Server response: {1}", interviewId, result),
                        exception);

                    return false;
                }

                if (!serverOperationSucceeded)
                {
                    this.logger.Error(string.Format("Failed to send interview {0} because server returned negative response.", interviewId));
                }

                return serverOperationSucceeded;
            }
        }

        private AggregateRootEvent[] BuildEventStreamOfLocalChangesToSend(Guid interviewId)
        {
            // TODO: filter event stream

            List<CommittedEvent> storedEvents = this.eventStore.ReadFrom(interviewId, 0, long.MaxValue).ToList();

            AggregateRootEvent[] eventsToSend = storedEvents.Select(storedEvent => new AggregateRootEvent(storedEvent)).ToArray();

            return eventsToSend;
        }
    }
}