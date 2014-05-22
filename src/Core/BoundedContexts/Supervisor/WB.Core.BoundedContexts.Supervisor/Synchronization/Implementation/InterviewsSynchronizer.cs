using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Main.Core;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Supervisor.Extensions;
using WB.Core.BoundedContexts.Supervisor.Interviews;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views;
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
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class InterviewsSynchronizer : IInterviewsSynchronizer
    {
        private readonly IAtomFeedReader feedReader;
        private readonly HeadquartersSettings settings;
        private readonly ILogger logger;
        private readonly Action<ICommand> executeCommand;
        private readonly IQueryablePlainStorageAccessor<LocalInterviewFeedEntry> plainStorage;
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IHeadquartersQuestionnaireReader headquartersQuestionnaireReader;
        private readonly IHeadquartersInterviewReader headquartersInterviewReader;
        private readonly HeadquartersPullContext headquartersPullContext;
        private readonly HeadquartersPushContext headquartersPushContext;
        private readonly IEventStore eventStore;
        private readonly IJsonUtils jsonUtils;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryRepositoryWriter;
        private readonly IQueryableReadSideRepositoryWriter<ReadyToSendToHeadquartersInterview> readyToSendInterviewsRepositoryWriter;
        private readonly HttpMessageHandler httpMessageHandler;

        public InterviewsSynchronizer(
            IAtomFeedReader feedReader,
            HeadquartersSettings settings, 
            ILogger logger,
            ICommandService commandService,
            IQueryablePlainStorageAccessor<LocalInterviewFeedEntry> plainStorage, 
            IQueryableReadSideRepositoryReader<UserDocument> users,
            IPlainQuestionnaireRepository plainQuestionnaireRepository,
            IHeadquartersQuestionnaireReader headquartersQuestionnaireReader,
            IHeadquartersInterviewReader headquartersInterviewReader,
            HeadquartersPullContext headquartersPullContext,
            HeadquartersPushContext headquartersPushContext,
            IEventStore eventStore,
            IJsonUtils jsonUtils,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryRepositoryWriter,
            IQueryableReadSideRepositoryWriter<ReadyToSendToHeadquartersInterview> readyToSendInterviewsRepositoryWriter,
            HttpMessageHandler httpMessageHandler)
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
            if (headquartersPullContext == null) throw new ArgumentNullException("headquartersPullContext");
            if (headquartersPushContext == null) throw new ArgumentNullException("headquartersPushContext");
            if (eventStore == null) throw new ArgumentNullException("eventStore");
            if (jsonUtils == null) throw new ArgumentNullException("jsonUtils");
            if (interviewSummaryRepositoryWriter == null) throw new ArgumentNullException("interviewSummaryRepositoryWriter");
            if (readyToSendInterviewsRepositoryWriter == null) throw new ArgumentNullException("readyToSendInterviewsRepositoryWriter");
            if (httpMessageHandler == null) throw new ArgumentNullException("httpMessageHandler");

            this.feedReader = feedReader;
            this.settings = settings;
            this.logger = logger;
            this.executeCommand = command => commandService.Execute(command, origin: Constants.HeadquartersSynchronizationOrigin);
            this.plainStorage = plainStorage;
            this.users = users;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.headquartersQuestionnaireReader = headquartersQuestionnaireReader;
            this.headquartersInterviewReader = headquartersInterviewReader;
            this.headquartersPullContext = headquartersPullContext;
            this.headquartersPushContext = headquartersPushContext;
            this.eventStore = eventStore;
            this.jsonUtils = jsonUtils;
            this.interviewSummaryRepositoryWriter = interviewSummaryRepositoryWriter;
            this.readyToSendInterviewsRepositoryWriter = readyToSendInterviewsRepositoryWriter;
            this.httpMessageHandler = httpMessageHandler;
        }

        public void Pull()
        {
            this.StoreEventsToLocalStorage();

            var localSupervisors = users.Query(_ => _.Where(x => x.Roles.Any(role => role == UserRoles.Supervisor)));

            foreach (var localSupervisor in localSupervisors)
            {
                IEnumerable<LocalInterviewFeedEntry> events = 
                    this.plainStorage.Query(_ => _.Where(x => x.SupervisorId == localSupervisor.PublicKey.FormatGuid() && !x.Processed));

                this.headquartersPullContext.PushMessage(string.Format("Synchronizing interviews for supervisor '{0}'. Events count: {1}", localSupervisor.UserName, events.Count()));
                
                foreach (var interviewFeedEntry in events)
                {
                    try
                    {
                        var interviewDetails = this.GetInterviewDetails(interviewFeedEntry).Result;

                        string questionnaireDetailsUrl = this.settings.QuestionnaireDetailsEndpoint
                            .Replace("{id}", interviewDetails.QuestionnaireId.FormatGuid())
                            .Replace("{version}", interviewDetails.QuestionnaireVersion.ToString());

                        switch (interviewFeedEntry.EntryType)
                        {
                            case EntryType.SupervisorAssigned:
                                this.StoreQuestionnaireDocumentFromHeadquartersIfNeeded(interviewDetails.QuestionnaireId, interviewDetails.QuestionnaireVersion, new Uri(questionnaireDetailsUrl));
                                this.CreateOrUpdateInterviewFromHeadquarters(interviewDetails, interviewFeedEntry.SupervisorId, interviewFeedEntry.UserId);
                                break;

                            case EntryType.InterviewUnassigned:
                                this.CancelInterview(interviewFeedEntry.InterviewId, interviewFeedEntry.UserId);
                                break;
                            case EntryType.InterviewRejected:
                                this.RejectInterview(interviewFeedEntry, interviewDetails);
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

        private async Task<InterviewSynchronizationDto> GetInterviewDetails(LocalInterviewFeedEntry interviewFeedEntry)
        {
            this.headquartersPullContext.PushMessage(string.Format("Loading interview using {0} URL", interviewFeedEntry.InterviewUri));
            InterviewSynchronizationDto interviewDetails =
                await this.headquartersInterviewReader.GetInterviewByUri(interviewFeedEntry.InterviewUri).ConfigureAwait(false);
            return interviewDetails;
        }

        private void RejectInterview(LocalInterviewFeedEntry feedEntry, InterviewSynchronizationDto interviewDetails)
        {
            var userIdGuid = Guid.Parse(feedEntry.UserId);
            var supervisorIdGuid = Guid.Parse(feedEntry.SupervisorId);

            this.headquartersPullContext.PushMessage(string.Format("Applying interview rejected by HQ on {0} interview", feedEntry.InterviewId));
            this.executeCommand(new RejectInterviewFromHeadquartersCommand(interviewDetails.Id, 
                userIdGuid, 
                supervisorIdGuid, 
                feedEntry.InterviewId != null ? (Guid?)Guid.Parse(feedEntry.InterviewerId) : null,
                interviewDetails, 
                DateTime.Now, 
                feedEntry.Comment));
        }

        public void Push(Guid userId)
        {
            this.headquartersPushContext.PushMessage("Getting interviews to be pushed.");
            List<Guid> interviewsToPush = this.GetInterviewsToPush();
            this.headquartersPushContext.PushMessage(string.Format("Found {0} interviews to push.", interviewsToPush.Count));

            for (int interviewIndex = 0; interviewIndex < interviewsToPush.Count; interviewIndex++)
            {
                var interviewId = interviewsToPush[interviewIndex];

                try
                {
                    this.headquartersPushContext.PushMessage(string.Format("Pushing interview {0} ({1} out of {2}).", interviewId.FormatGuid(), interviewIndex + 1, interviewsToPush.Count));
                    this.PushInterview(interviewId, userId);
                    this.headquartersPushContext.PushMessage(string.Format("Interview {0} successfully pushed.", interviewId.FormatGuid()));
                }
                catch (Exception exception)
                {
                    this.logger.Error(string.Format("Failed to push interview {0} to Headquarters.", interviewId.FormatGuid()), exception);
                    this.headquartersPushContext.PushError(string.Format("Failed to push interview {0}. Error message: {1}. Exception messages: {2}",
                        interviewId.FormatGuid(), exception.Message, string.Join(Environment.NewLine, exception.UnwrapAllInnerExceptions().Select(x => x.Message))));
                }
            }
        }


        private void StoreQuestionnaireDocumentFromHeadquartersIfNeeded(Guid questionnaireId, long questionnaireVersion, Uri questionnareUri)
        {
            if (this.IsQuestionnnaireAlreadyStoredLocally(questionnaireId, questionnaireVersion))
                return;

            this.headquartersPullContext.PushMessage(string.Format("Loading questionnaire using {0} URL", questionnareUri));
            QuestionnaireDocument questionnaireDocument = this.headquartersQuestionnaireReader.GetQuestionnaireByUri(questionnareUri).Result;

            this.plainQuestionnaireRepository.StoreQuestionnaire(questionnaireId, questionnaireVersion, questionnaireDocument);
            this.executeCommand(new RegisterPlainQuestionnaire(questionnaireId, questionnaireVersion));
        }

        private bool IsQuestionnnaireAlreadyStoredLocally(Guid id, long version)
        {
            QuestionnaireDocument localQuestionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            return localQuestionnaireDocument != null;
        }

        private void CreateOrUpdateInterviewFromHeadquarters(InterviewSynchronizationDto interviewDetails, string supervisorId, string userId)
        {
            var userIdGuid = Guid.Parse(userId);
            var supervisorIdGuid = Guid.Parse(supervisorId);

            this.executeCommand(new SynchronizeInterviewFromHeadquarters(interviewDetails.Id, userIdGuid, supervisorIdGuid, interviewDetails, DateTime.Now));
        }

        private void CancelInterview(string interviewId, string userId)
        {
            Guid interviewIdGuid = Guid.Parse(interviewId);
            Guid userIdGuid = Guid.Parse(userId);

            this.executeCommand(new CancelInterviewByHQSynchronizationCommand(interviewId: interviewIdGuid, userId: userIdGuid));
        }

        private void StoreEventsToLocalStorage()
        {
            var lastStoredEntry = this.plainStorage.Query(_ => _.OrderByDescending(x => x.Timestamp).Select(x => x.EntryId).FirstOrDefault());

            IEnumerable<AtomFeedEntry<LocalInterviewFeedEntry>> remoteEvents = 
                this.feedReader
                    .ReadAfterAsync<LocalInterviewFeedEntry>(this.settings.InterviewsFeedUrl, lastStoredEntry)
                    .Result;
            this.headquartersPullContext.PushMessage(string.Format("Received {0} events from {1} feed", remoteEvents.Count(), this.settings.InterviewsFeedUrl));

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
            return this.readyToSendInterviewsRepositoryWriter
                .QueryAll()
                .Select(interview => interview.InterviewId)
                .ToList();
        }

        private void PushInterview(Guid interviewId, Guid userId)
        {
            AggregateRootEvent[] eventsToSend = this.BuildEventStreamOfLocalChangesToSend(interviewId);

            if (eventsToSend.Length > 0)
            {
                string dataToBeSent = this.GetInterviewDataToBeSentAsString(interviewId, eventsToSend);

                this.SendInterviewData(interviewId, dataToBeSent);
            }
            else
            {
                this.logger.Info(string.Format("Interview {0} was not sent to Headquarters because there are no events which should be sent.", interviewId.FormatGuid()));
            }

            this.MarkInterviewAsSentToHeadquarters(interviewId, userId);
        }

        private void MarkInterviewAsSentToHeadquarters(Guid interviewId, Guid userId)
        {
            this.executeCommand(new MarkInterviewAsSentToHeadquarters(interviewId, userId));
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

        private void SendInterviewData(Guid interviewId, string interviewData)
        {
            using (var client = new HttpClient(this.httpMessageHandler).AppendAuthToken(this.settings))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, this.settings.InterviewsPushUrl) {
                    Content = new StringContent(interviewData)
                };

                HttpResponseMessage response = client.SendAsync(request).Result;

                string result = response.Content.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(string.Format("Failed to send interview {0}. Server response: {1}",
                        interviewId, result));
                }

                bool serverOperationSucceeded;

                try
                {
                    serverOperationSucceeded = this.jsonUtils.Deserrialize<bool>(result);
                }
                catch (Exception exception)
                {
                    throw new Exception(
                        string.Format("Failed to read server response while sending interview {0}. Server response: {1}", interviewId, result),
                        exception);
                }

                if (!serverOperationSucceeded)
                {
                    throw new Exception(string.Format("Failed to send interview {0} because server returned negative response.", interviewId));
                }
            }
        }

        private AggregateRootEvent[] BuildEventStreamOfLocalChangesToSend(Guid interviewId)
        {
            List<CommittedEvent> storedEvents = this.eventStore.ReadFrom(interviewId, 0, long.MaxValue).ToList();

            int indexOfLastEventSentToHeadquarters = storedEvents.FindLastIndex(storedEvent => storedEvent.Payload is InterviewSentToHeadquarters);
            int countOfEventsSentToHeadquarters = indexOfLastEventSentToHeadquarters + 1;

            AggregateRootEvent[] eventsToSend = storedEvents
                .Skip(countOfEventsSentToHeadquarters)
                .Where(storedEvent => storedEvent.Origin != Constants.HeadquartersSynchronizationOrigin)
                .Select(storedEvent => new AggregateRootEvent(storedEvent))
                .ToArray();

            return eventsToSend;
        }

        private void MarkAsProcessedWithError(LocalInterviewFeedEntry interviewFeedEntry, Exception ex)
        {
            interviewFeedEntry.ProcessedWithError = true;
            this.headquartersPullContext.PushError(string.Format("Error while processing event {0}. ErrorMessage: {1}. Exception messages: {2}",
                interviewFeedEntry.EntryId, ex.Message, string.Join(Environment.NewLine, ex.UnwrapAllInnerExceptions().Select(x => x.Message))));
        }
    }
}