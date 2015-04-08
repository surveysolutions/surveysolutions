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

using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Raven.Client.Linq;
using WB.Core.BoundedContexts.Supervisor.Extensions;
using WB.Core.BoundedContexts.Supervisor.Interviews;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views;
using WB.Core.BoundedContexts.Supervisor.Questionnaires;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;
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
        private readonly IPlainStorageAccessor<LocalInterviewFeedEntry> plainStorage;
        private readonly IReadSideRepositoryReader<UserDocument> users;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IHeadquartersInterviewReader headquartersInterviewReader;
        private readonly HeadquartersPullContext headquartersPullContext;
        private readonly HeadquartersPushContext headquartersPushContext;
        private readonly IEventStore eventStore;
        private readonly IJsonUtils jsonUtils;
        private readonly IReadSideRepositoryReader<InterviewSummary> interviewSummaryRepositoryReader;
        private readonly IQueryableReadSideRepositoryReader<ReadyToSendToHeadquartersInterview> readyToSendInterviewsRepositoryReader;
        private readonly Func<HttpMessageHandler> httpMessageHandler;
        private readonly IInterviewSynchronizationFileStorage interviewSynchronizationFileStorage;
        private readonly IArchiveUtils archiver;
        private readonly IPlainTransactionManager plainTransactionManager;
        private readonly ITransactionManager cqrsTransactionManager;

        public InterviewsSynchronizer(
            IAtomFeedReader feedReader,
            IHeadquartersSettings settings, 
            ILogger logger,
            ICommandService commandService,
            IPlainStorageAccessor<LocalInterviewFeedEntry> plainStorage, 
            IQueryableReadSideRepositoryReader<UserDocument> users,
            IPlainQuestionnaireRepository plainQuestionnaireRepository,
            IHeadquartersInterviewReader headquartersInterviewReader,
            HeadquartersPullContext headquartersPullContext,
            HeadquartersPushContext headquartersPushContext,
            IEventStore eventStore,
            IJsonUtils jsonUtils,
            IReadSideRepositoryReader<InterviewSummary> interviewSummaryRepositoryReader,
            IQueryableReadSideRepositoryReader<ReadyToSendToHeadquartersInterview> readyToSendInterviewsRepositoryReader,
            Func<HttpMessageHandler> httpMessageHandler,
            IInterviewSynchronizationFileStorage interviewSynchronizationFileStorage,
            IArchiveUtils archiver,
            IPlainTransactionManager plainTransactionManager,
            ITransactionManager cqrsTransactionManager)
        {
            if (feedReader == null) throw new ArgumentNullException("feedReader");
            if (settings == null) throw new ArgumentNullException("settings");
            if (logger == null) throw new ArgumentNullException("logger");
            if (commandService == null) throw new ArgumentNullException("commandService");
            if (plainStorage == null) throw new ArgumentNullException("plainStorage");
            if (users == null) throw new ArgumentNullException("users");
            if (plainQuestionnaireRepository == null) throw new ArgumentNullException("plainQuestionnaireRepository");
            if (headquartersInterviewReader == null) throw new ArgumentNullException("headquartersInterviewReader");
            if (headquartersPullContext == null) throw new ArgumentNullException("headquartersPullContext");
            if (headquartersPushContext == null) throw new ArgumentNullException("headquartersPushContext");
            if (eventStore == null) throw new ArgumentNullException("eventStore");
            if (jsonUtils == null) throw new ArgumentNullException("jsonUtils");
            if (archiver == null) throw new ArgumentNullException("archiver");
            if (plainTransactionManager == null) throw new ArgumentNullException("plainTransactionManager");
            if (interviewSummaryRepositoryReader == null) throw new ArgumentNullException("interviewSummaryRepositoryReader");
            if (readyToSendInterviewsRepositoryReader == null) throw new ArgumentNullException("readyToSendInterviewsRepositoryReader");
            if (httpMessageHandler == null) throw new ArgumentNullException("httpMessageHandler");

            this.feedReader = feedReader;
            this.settings = settings;
            this.logger = logger;
            this.executeCommand = command => commandService.Execute(command, origin: Constants.HeadquartersSynchronizationOrigin);
            this.plainStorage = plainStorage;
            this.users = users;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.headquartersInterviewReader = headquartersInterviewReader;
            this.headquartersPullContext = headquartersPullContext;
            this.headquartersPushContext = headquartersPushContext;
            this.eventStore = eventStore;
            this.jsonUtils = jsonUtils;
            this.interviewSummaryRepositoryReader = interviewSummaryRepositoryReader;
            this.readyToSendInterviewsRepositoryReader = readyToSendInterviewsRepositoryReader;
            this.httpMessageHandler = httpMessageHandler;
            this.interviewSynchronizationFileStorage = interviewSynchronizationFileStorage;
            this.archiver = archiver;
            this.plainTransactionManager = plainTransactionManager;
            this.cqrsTransactionManager = cqrsTransactionManager;
        }

        public void PullInterviewsForSupervisors(Guid[] supervisorIds)
        {
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                this.StoreEventsToLocalStorage();
            });

            foreach (var localSupervisor in supervisorIds)
            {
                List<LocalInterviewFeedEntry> events = this.plainTransactionManager.ExecuteInPlainTransaction(() =>
                    this.plainStorage.Query(_ => _
                        .Where(x => x.SupervisorId == localSupervisor.FormatGuid() && !x.Processed)
                        .OrderByDescending(x => x.Timestamp)
                        .ToList()));

                this.headquartersPullContext.PushMessage(string.Format(Resources.InterviewsSynchronizer.SynchronizingInterviewsForSupervisor0EventsCount1Format, localSupervisor, events.Count()));
                
                foreach (var interviewFeedEntry in events)
                {
                    this.plainTransactionManager.ExecuteInPlainTransaction(() =>
                    {
                        try
                        {
                            if (interviewFeedEntry.Processed) return;

                            switch (interviewFeedEntry.EntryType)
                            {
                                case EntryType.SupervisorAssigned:
                                    var interviewDetails = this.GetInterviewDetails(interviewFeedEntry).Result;
                                    if (!this.IsResponsiblePresent(interviewDetails.UserId))
                                        return;

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
                                        return;

                                    this.RejectInterview(interviewFeedEntry.InterviewId, interviewFeedEntry.SupervisorId, rejectedInterview);
                                    break;
                                default:
                                    this.logger.Warn(string.Format(
                                        Resources.InterviewsSynchronizer.Unknowneventoftype0receivedininterviewsfeedItwasskippedandmarkedasprocessedwitherrorEventId1Format,
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
                            this.logger.Error(string.Format(Resources.InterviewsSynchronizer.Interviewssynchronizationerrorinevent0Format, interviewFeedEntry.EntryId), ex);
                        }
                        finally
                        {
                            this.plainStorage.Store(interviewFeedEntry, interviewFeedEntry.EntryId);
                        }
                    });
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
                    this.headquartersPullContext.PushMessage(string.Format(Resources.InterviewsSynchronizer.Event01ismarkedasprocessedbecauseitisoverridenby23Format,
                        x.EntryId, x.EntryType, interviewFeedEntry.EntryId, interviewFeedEntry.EntryType));
                });
        }

        private async Task<InterviewSynchronizationDto> GetInterviewDetails(LocalInterviewFeedEntry interviewFeedEntry)
        {
            this.headquartersPullContext.PushMessage(string.Format(Resources.InterviewsSynchronizer.Loadinginterviewusing0URLFormat, interviewFeedEntry.InterviewUri));
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

            this.headquartersPullContext.PushMessage(string.Format(Resources.InterviewsSynchronizer.Interview0wasrejectedbyHQFormat, interviewId));

            var interviewSummary = this.GetInterviewSummary(Guid.Parse(interviewId));
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

        private InterviewSummary GetInterviewSummary(Guid interviewId)
        {
            return this.cqrsTransactionManager.ExecuteInQueryTransaction(() =>
                this.interviewSummaryRepositoryReader.GetById(interviewId));
        }

        public void Push(Guid userId)
        {
            this.PushInterviewData(userId);
            this.PushInterviewFile();
        }

        private void PushInterviewData(Guid userId)
        {
            List<Guid> interviewsToPush = this.GetInterviewsToPush();
            this.headquartersPushContext.PushMessage(string.Format(Resources.InterviewsSynchronizer.Found0interviewstopushFormat, interviewsToPush.Count));

            for (int interviewIndex = 0; interviewIndex < interviewsToPush.Count; interviewIndex++)
            {
                var interviewId = interviewsToPush[interviewIndex];

                try
                {
                    this.headquartersPushContext.PushMessage(string.Format(Resources.InterviewsSynchronizer.Pushinginterview01outof2Format, interviewId.FormatGuid(),
                        interviewIndex + 1, interviewsToPush.Count));
                    this.PushInterview(interviewId, userId);
                    this.interviewSynchronizationFileStorage.MoveInterviewsBinaryDataToSyncFolder(interviewId);
                    this.headquartersPushContext.PushMessage(string.Format(Resources.InterviewsSynchronizer.Interview0successfullypushedFormat, interviewId.FormatGuid()));
                }
                catch (Exception exception)
                {
                    this.logger.Error(string.Format(Resources.InterviewsSynchronizer.Failed_to_push_interview__0__to_Headquarters_Format, interviewId.FormatGuid()), exception);
                    this.headquartersPushContext.PushError(string.Format(
                        Resources.InterviewsSynchronizer.Failed_to_push_interview__0___Error_message___1___Exception_messages___2_Format,
                        interviewId.FormatGuid(), exception.Message,
                        string.Join(Environment.NewLine, exception.UnwrapAllInnerExceptions().Select(x => x.Message))));
                }
            }
        }

        private void PushInterviewFile()
        {
            this.headquartersPushContext.PushMessage(Resources.InterviewsSynchronizer.Getting_interviews_files_to_be_pushed_);
            var files = this.interviewSynchronizationFileStorage.GetBinaryFilesFromSyncFolder();
            this.headquartersPushContext.PushMessage(string.Format(Resources.InterviewsSynchronizer.Found__0__files_to_push_Format, files.Count));

            for (int interviewIndex = 0; interviewIndex < files.Count; interviewIndex++)
            {
                var interviewFile = files[interviewIndex];

                try
                {
                    this.headquartersPushContext.PushMessage(string.Format(Resources.InterviewsSynchronizer.Pushing_file__0__for_interview__1____2__out_of__3_Format, interviewFile.FileName, interviewFile.InterviewId.FormatGuid(),
                        interviewIndex + 1, files.Count));
                    this.PushFile(interviewFile);
                    this.interviewSynchronizationFileStorage.RemoveBinaryDataFromSyncFolder(interviewFile.InterviewId, interviewFile.FileName);
                    this.headquartersPushContext.PushMessage(string.Format(Resources.InterviewsSynchronizer.File__0__for_interview__1__successfully_pushed_Format, interviewFile.FileName, interviewFile.InterviewId.FormatGuid()));
                }
                catch (Exception exception)
                {
                    this.logger.Error(string.Format(Resources.InterviewsSynchronizer.Failed_to_push_file__0__for_interview__1__to_Headquarters_Format, interviewFile.FileName, interviewFile.InterviewId.FormatGuid()), exception);
                    this.headquartersPushContext.PushError(string.Format(
                        Resources.InterviewsSynchronizer.Failed_to_push_file__0__for_interview__1___Error_message___2___Exception_messages___3_Format,
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
            return this.cqrsTransactionManager.ExecuteInQueryTransaction(() =>
                this.users.GetById(userId) != null);
        }
        private bool IsInterviewPresent(Guid interviewId)
        {
            var interviewSummary = this.GetInterviewSummary(interviewId);
            return interviewSummary != null && !interviewSummary.IsDeleted;
        }

        private void CreateOrUpdateInterviewFromHeadquarters(InterviewSynchronizationDto interviewDetails, string supervisorId)
        {
            if (interviewDetails.Id == Guid.Empty)
                return;

            if (IsQuestionnaireDeleted(interviewDetails.QuestionnaireId, interviewDetails.QuestionnaireVersion))
                return;
            
            var supervisorIdGuid = Guid.Parse(supervisorId);

            this.headquartersPullContext.PushMessage(string.Format(Resources.InterviewsSynchronizer.Interview__0__was_assigned_by_HQ_for_supervisor__1_Format, interviewDetails.Id, interviewDetails.UserId));
            this.executeCommand(new SynchronizeInterviewFromHeadquarters(interviewDetails.Id, interviewDetails.UserId, supervisorIdGuid, interviewDetails, DateTime.Now));
        }

        private void CancelInterview(string interviewId, string userId)
        {
            Guid interviewIdGuid = Guid.Parse(interviewId);

            if (!IsInterviewPresent(interviewIdGuid))
                return;

            Guid userIdGuid = Guid.Parse(userId);

            this.headquartersPullContext.PushMessage(string.Format(Resources.InterviewsSynchronizer.Interview__0__was_canceled_by_HQFormat, interviewId));
            this.executeCommand(new CancelInterviewByHqSynchronizationCommand(interviewId: interviewIdGuid, userId: userIdGuid));
        }

        private void HardDeleteInterview(string interviewId, string userId)
        {
            Guid interviewIdGuid = Guid.Parse(interviewId);

            var interviewSummary = this.GetInterviewSummary(interviewIdGuid);
            if (interviewSummary == null)
                return;

            Guid userIdGuid = Guid.Parse(userId);

            this.headquartersPullContext.PushMessage(string.Format(Resources.InterviewsSynchronizer.Interview__0__was_deleted_by_HQFormat, interviewId));
            this.executeCommand(new HardDeleteInterview(interviewId: interviewIdGuid, userId: userIdGuid));
        }

        private void StoreEventsToLocalStorage()
        {
            var lastStoredEntry = this.plainStorage.Query(_ => _.OrderByDescending(x => x.Timestamp).Select(x => x.EntryId).FirstOrDefault());

            IEnumerable<AtomFeedEntry<LocalInterviewFeedEntry>> remoteEvents = 
                this.feedReader
                    .ReadAfterAsync<LocalInterviewFeedEntry>(this.settings.InterviewsFeedUrl, lastStoredEntry)
                    .Result;
            this.headquartersPullContext.PushMessage(string.Format(Resources.InterviewsSynchronizer.Received__0__events_from__1__feedFormat, remoteEvents.Count(), this.settings.InterviewsFeedUrl));

            var newEvents = new List<LocalInterviewFeedEntry>();
            foreach (AtomFeedEntry<LocalInterviewFeedEntry> remoteEvent in remoteEvents)
            {
                var feedEntry = remoteEvent.Content;
                feedEntry.InterviewUri = remoteEvent.Links.Single(x => x.Rel == "enclosure").Href;
                newEvents.Add(feedEntry);
            }

            this.plainStorage.Store(newEvents.Select(x => Tuple.Create(x, (object)x.EntryId)));
        }


        private List<Guid> GetInterviewsToPush()
        {
            return this.cqrsTransactionManager.ExecuteInQueryTransaction(() =>
                this.readyToSendInterviewsRepositoryReader
                    .QueryAll()
                    .Select(interview => interview.InterviewId)
                    .ToList());
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
                this.logger.Info(string.Format(Resources.InterviewsSynchronizer.Interview__0__was_not_sent_to_Headquarters_because_there_are_no_events_which_should_be_sent_Format, interviewId.FormatGuid()));
            }

            this.MarkInterviewAsSentToHeadquarters(interviewId, userId);
        }

        private void MarkInterviewAsSentToHeadquarters(Guid interviewId, Guid userId)
        {
            this.executeCommand(new MarkInterviewAsSentToHeadquarters(interviewId, userId));
        }

        private string GetInterviewDataToBeSentAsString(Guid interviewId, AggregateRootEvent[] eventsToSend)
        {
            InterviewSummary interviewSummary = this.GetInterviewSummary(interviewId);

            InterviewCommentedStatus lastInterviewCommentedStatus = interviewSummary.CommentedStatusesHistory.LastOrDefault();
            string lastComment = lastInterviewCommentedStatus != null ? lastInterviewCommentedStatus.Comment : string.Empty;


            var featuredQuestionList = interviewSummary.WasCreatedOnClient
                ? interviewSummary.AnswersToFeaturedQuestions
                    .Select(
                        featuredQuestion =>
                            new FeaturedQuestionMeta(featuredQuestion.Questionid, featuredQuestion.Title, featuredQuestion.Answer)).ToList()
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
                Content = this.archiver.CompressString(this.jsonUtils.Serialize(eventsToSend)),
                IsCompressed = true,
                ItemType = SyncItemType.Interview,
                MetaInfo = this.archiver.CompressString(this.jsonUtils.Serialize(metadata)),
                RootId = interviewId
            };

            return this.jsonUtils.Serialize(syncItem);
        }

        private void SendInterviewData(Guid interviewId, string interviewData)
        {
            using (var client = new HttpClient(this.httpMessageHandler()).AppendAuthToken(this.settings))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, string.Format("{0}?interviewId={1}", this.settings.InterviewsPushUrl, interviewId)) {
                    Content = new StringContent(interviewData)
                };

                HttpResponseMessage response = client.SendAsync(request).Result;

                string result = response.Content.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(string.Format(Resources.InterviewsSynchronizer.Failed_to_send_interview__0___Server_response___1_Format,
                        interviewId, result));
                }

                bool serverOperationSucceeded;

                try
                {
                    serverOperationSucceeded = this.jsonUtils.Deserialize<bool>(result);
                }
                catch (Exception exception)
                {
                    throw new Exception(
                        string.Format(Resources.InterviewsSynchronizer.Failed_to_read_server_response_while_sending_interview__0___Server_response___1_Format, interviewId, result),
                        exception);
                }

                if (!serverOperationSucceeded)
                {
                    throw new Exception(string.Format(Resources.InterviewsSynchronizer.Failed_to_send_interview__0__because_server_returned_negative_response_Format, interviewId));
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
                    throw new Exception(string.Format(Resources.InterviewsSynchronizer.Failed_to_send__file__0__for_interview__1___Server_response___2_Format,
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
                        storedEvent.Origin != Constants.HeadquartersSynchronizationOrigin)
                .Select(storedEvent => new AggregateRootEvent(storedEvent))
                .ToArray();

            return eventsToSend;
        }

        private void MarkAsProcessedWithError(LocalInterviewFeedEntry interviewFeedEntry, Exception ex)
        {
            interviewFeedEntry.ProcessedWithError = true;
            this.headquartersPullContext.PushError(string.Format(Resources.InterviewsSynchronizer.Error_while_processing_event__0___ErrorMessage___1___Exception_messages___2_Format,
                interviewFeedEntry.EntryId, ex.Message, string.Join(Environment.NewLine, ex.UnwrapAllInnerExceptions().Select(x => x.Message))));
        }
    }
}