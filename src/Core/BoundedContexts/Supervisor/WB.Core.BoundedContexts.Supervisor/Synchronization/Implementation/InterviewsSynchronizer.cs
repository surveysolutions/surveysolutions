﻿using System;
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
using Raven.Client.Linq;
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
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class InterviewsSynchronizer : IInterviewsSynchronizer
    {
        private readonly IAtomFeedReader feedReader;
        private readonly IHeadquartersSettings settings;
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
        private readonly Func<HttpMessageHandler> httpMessageHandler;
        private readonly IInterviewSynchronizationFileStorage interviewSynchronizationFileStorage;

        public InterviewsSynchronizer(
            IAtomFeedReader feedReader,
            IHeadquartersSettings settings, 
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
            Func<HttpMessageHandler> httpMessageHandler, IInterviewSynchronizationFileStorage interviewSynchronizationFileStorage)
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
            this.interviewSynchronizationFileStorage = interviewSynchronizationFileStorage;
        }

        public void PullInterviewsForSupervisors(Guid[] supervisorIds)
        {
            this.StoreEventsToLocalStorage();

            foreach (var localSupervisor in supervisorIds)
            {
                List<LocalInterviewFeedEntry> events = 
                    this.plainStorage.Query(_ => _.Where(x => x.SupervisorId == localSupervisor.FormatGuid() && !x.Processed))
                                     .OrderByDescending(x => x.Timestamp).ToList();

                this.headquartersPullContext.PushMessage(string.Format("Synchronizing interviews for supervisor '{0}'. Events count: {1}", localSupervisor, events.Count()));
                
                foreach (var interviewFeedEntry in events)
                {
                    try
                    {
                        if (interviewFeedEntry.Processed) continue;

                        switch (interviewFeedEntry.EntryType)
                        {
                            case EntryType.SupervisorAssigned:
                                var interviewDetails = this.GetInterviewDetails(interviewFeedEntry).Result;
                                if (!this.IsResponsiblePresent(interviewDetails.UserId))
                                    continue;

                                this.CreateOrUpdateInterviewFromHeadquarters(interviewDetails, interviewFeedEntry.SupervisorId);
                                break;
                            case EntryType.InterviewUnassigned:
                                this.CancelInterview(interviewFeedEntry.InterviewId, interviewFeedEntry.UserId);
                                this.MarkOtherInterviewEventsAsProcessed(events, interviewFeedEntry);
                                break;
                            case EntryType.InterviewDeleted:
                                this.HardDeleteInterview(interviewFeedEntry.InterviewId, interviewFeedEntry.UserId);
                                this.MarkOtherInterviewEventsAsProcessed(events, interviewFeedEntry);
                                break;
                            case EntryType.InterviewRejected:
                                var rejectedInterview = this.GetInterviewDetails(interviewFeedEntry).Result;
                                if (!this.IsResponsiblePresent(rejectedInterview.UserId))
                                    continue;

                                this.RejectInterview(interviewFeedEntry.InterviewId, interviewFeedEntry.SupervisorId, rejectedInterview);
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

        private void MarkOtherInterviewEventsAsProcessed(IEnumerable<LocalInterviewFeedEntry> events, LocalInterviewFeedEntry interviewFeedEntry)
        {
            events.Where(x => x.InterviewId == interviewFeedEntry.InterviewId && !x.Processed && x.EntryId != interviewFeedEntry.EntryId)
                
                .ToList()
                .ForEach(x => {
                    x.Processed = true;
                    this.plainStorage.Store(x, x.EntryId);
                    this.headquartersPullContext.PushMessage(string.Format("Event {0}, '{1}' is marked as processed because it is overriden by {2}, '{3}'",
                        x.EntryId, x.EntryType, interviewFeedEntry.EntryId, interviewFeedEntry.EntryType));
                });
        }

        private async Task<InterviewSynchronizationDto> GetInterviewDetails(LocalInterviewFeedEntry interviewFeedEntry)
        {
            this.headquartersPullContext.PushMessage(string.Format("Loading interview using {0} URL", interviewFeedEntry.InterviewUri));
            InterviewSynchronizationDto interviewDetails =
                await this.headquartersInterviewReader.GetInterviewByUri(interviewFeedEntry.InterviewUri).ConfigureAwait(false);
            return interviewDetails;
        }

        private void RejectInterview(string interviewId, string supervisorId, InterviewSynchronizationDto interviewDetails)
        {
            if (interviewDetails.Id == Guid.Empty)
                return;

            if (IsQuestionnaireDeleted(interviewDetails.QuestionnaireId, interviewDetails.QuestionnaireVersion))
                return;

            var supervisorIdGuid = Guid.Parse(supervisorId);

            this.headquartersPullContext.PushMessage(string.Format("Interview {0} was rejected by HQ", interviewId));

            var interviewSummary = this.interviewSummaryRepositoryWriter.GetById(interviewId);
            if (interviewSummary == null)

            {
                this.executeCommand(new CreateInterviewCreatedOnClientCommand(interviewId: interviewDetails.Id,
                    userId: interviewDetails.UserId, questionnaireId: interviewDetails.QuestionnaireId,
                    questionnaireVersion: interviewDetails.QuestionnaireVersion, status: interviewDetails.Status,
                    featuredQuestionsMeta: new AnsweredQuestionSynchronizationDto[0], isValid: true));
            }

            this.executeCommand(new RejectInterviewFromHeadquartersCommand(interviewDetails.Id,
                interviewDetails.UserId,
                supervisorIdGuid,
                interviewDetails.UserId,
                interviewDetails,
                DateTime.Now));
        }

        public void Push(Guid userId)
        {
            this.PushInterviewData(userId);
            this.PushInterviewFile();
        }

        private void PushInterviewData(Guid userId)
        {
            List<Guid> interviewsToPush = this.GetInterviewsToPush();
            this.headquartersPushContext.PushMessage(string.Format("Found {0} interviews to push.", interviewsToPush.Count));

            for (int interviewIndex = 0; interviewIndex < interviewsToPush.Count; interviewIndex++)
            {
                var interviewId = interviewsToPush[interviewIndex];

                try
                {
                    this.headquartersPushContext.PushMessage(string.Format("Pushing interview {0} ({1} out of {2}).", interviewId.FormatGuid(),
                        interviewIndex + 1, interviewsToPush.Count));
                    this.PushInterview(interviewId, userId);
                    this.interviewSynchronizationFileStorage.MoveInterviewsBinaryDataToSyncFolder(interviewId);
                    this.headquartersPushContext.PushMessage(string.Format("Interview {0} successfully pushed.", interviewId.FormatGuid()));
                }
                catch (Exception exception)
                {
                    this.logger.Error(string.Format("Failed to push interview {0} to Headquarters.", interviewId.FormatGuid()), exception);
                    this.headquartersPushContext.PushError(string.Format(
                        "Failed to push interview {0}. Error message: {1}. Exception messages: {2}",
                        interviewId.FormatGuid(), exception.Message,
                        string.Join(Environment.NewLine, exception.UnwrapAllInnerExceptions().Select(x => x.Message))));
                }
            }
        }

        private void PushInterviewFile()
        {
            this.headquartersPushContext.PushMessage("Getting interviews files to be pushed.");
            var files = this.interviewSynchronizationFileStorage.GetBinaryFilesFromSyncFolder();
            this.headquartersPushContext.PushMessage(string.Format("Found {0} files to push.", files.Count));

            for (int interviewIndex = 0; interviewIndex < files.Count; interviewIndex++)
            {
                var interviewFile = files[interviewIndex];

                try
                {
                    this.headquartersPushContext.PushMessage(string.Format("Pushing file {0} for interview {1} ({2} out of {3}).", interviewFile.FileName, interviewFile.InterviewId.FormatGuid(),
                        interviewIndex + 1, files.Count));
                    this.PushFile(interviewFile);
                    this.interviewSynchronizationFileStorage.RemoveBinaryDataFromSyncFolder(interviewFile.InterviewId, interviewFile.FileName);
                    this.headquartersPushContext.PushMessage(string.Format("File {0} for interview {1} successfully pushed.", interviewFile.FileName, interviewFile.InterviewId.FormatGuid()));
                }
                catch (Exception exception)
                {
                    this.logger.Error(string.Format("Failed to push file {0} for interview {1} to Headquarters.", interviewFile.FileName, interviewFile.InterviewId.FormatGuid()), exception);
                    this.headquartersPushContext.PushError(string.Format(
                        "Failed to push file {0} for interview {1}. Error message: {2}. Exception messages: {3}",
                        interviewFile.FileName, interviewFile.InterviewId.FormatGuid(), exception.Message,
                        string.Join(Environment.NewLine, exception.UnwrapAllInnerExceptions().Select(x => x.Message))));
                }
            }
        }

        private bool IsQuestionnaireDeleted(Guid id, long version)
        {
            var questionnaire = plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);
            return questionnaire == null || questionnaire.IsDeleted;
        }
        private bool IsResponsiblePresent(Guid userId)
        {
            return users.GetById(userId) != null;
        }
        private bool IsInterviewPresent(Guid interviewId)
        {
            var interviewSummary = this.interviewSummaryRepositoryWriter.GetById(interviewId);
            return interviewSummary != null && !interviewSummary.IsDeleted;
        }

        private void CreateOrUpdateInterviewFromHeadquarters(InterviewSynchronizationDto interviewDetails, string supervisorId)
        {
            if (interviewDetails.Id == Guid.Empty)
                return;

            if (IsQuestionnaireDeleted(interviewDetails.QuestionnaireId, interviewDetails.QuestionnaireVersion))
                return;
            
            var supervisorIdGuid = Guid.Parse(supervisorId);

            this.headquartersPullContext.PushMessage(string.Format("Interview {0} was assigned by HQ for supervisor {1}", interviewDetails.Id, interviewDetails.UserId));
            this.executeCommand(new SynchronizeInterviewFromHeadquarters(interviewDetails.Id, interviewDetails.UserId, supervisorIdGuid, interviewDetails, DateTime.Now));
        }

        private void CancelInterview(string interviewId, string userId)
        {
            Guid interviewIdGuid = Guid.Parse(interviewId);

            if (!IsInterviewPresent(interviewIdGuid))
                return;

            Guid userIdGuid = Guid.Parse(userId);

            this.headquartersPullContext.PushMessage(string.Format("Interview {0} was canceled by HQ", interviewId));
            this.executeCommand(new CancelInterviewByHQSynchronizationCommand(interviewId: interviewIdGuid, userId: userIdGuid));
        }

        private void HardDeleteInterview(string interviewId, string userId)
        {
            Guid interviewIdGuid = Guid.Parse(interviewId);

            var interviewSummary = this.interviewSummaryRepositoryWriter.GetById(interviewId);
            if (interviewSummary == null)
                return;

            Guid userIdGuid = Guid.Parse(userId);

            this.headquartersPullContext.PushMessage(string.Format("Interview {0} was deleted by HQ", interviewId));
            this.executeCommand(new HardDeleteInterview(interviewId: interviewIdGuid, userId: userIdGuid));
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


            var featuredQuestionList = interviewSummary.WasCreatedOnClient
                ? interviewSummary.AnswersToFeaturedQuestions
                    .Select(
                        featuredQuestion =>
                            new FeaturedQuestionMeta(featuredQuestion.Key, featuredQuestion.Value.Title, featuredQuestion.Value.Answer)).ToList()
                : null;

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
                FeaturedQuestionsMeta = featuredQuestionList
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
            using (var client = new HttpClient(this.httpMessageHandler()).AppendAuthToken(this.settings))
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

        private void PushFile(InterviewBinaryDataDescriptor interviewFile)
        {
            using (var client = new HttpClient(this.httpMessageHandler()).AppendAuthToken(this.settings))
            {
                var request = new HttpRequestMessage(HttpMethod.Post,
                    string.Format("{0}?interviewId={1}&fileName={2}", this.settings.FilePushUrl, interviewFile.InterviewId,
                        interviewFile.FileName))
                { Content = new ByteArrayContent(interviewFile.GetData()) };
                
                
                HttpResponseMessage response = client.SendAsync(request).Result;

                string result = response.Content.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(string.Format("Failed to send  file {0} for interview {1}. Server response: {2}",
                        interviewFile.FileName, interviewFile.InterviewId, result));
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
                .Where(
                    storedEvent =>
                        storedEvent.Origin != Constants.HeadquartersSynchronizationOrigin &&
                            storedEvent.Origin != Constants.CapiSynchronizationOrigin)
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