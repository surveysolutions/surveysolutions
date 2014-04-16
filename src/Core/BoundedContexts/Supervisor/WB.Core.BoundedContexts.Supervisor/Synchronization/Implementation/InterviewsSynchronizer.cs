using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.ServiceModel;
using Raven.Client.Linq;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
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

        public InterviewsSynchronizer(IAtomFeedReader feedReader, 
            HeadquartersSettings settings, 
            ILogger logger,
            ICommandService commandService,
            IQueryablePlainStorageAccessor<LocalInterviewFeedEntry> plainStorage, 
            IQueryableReadSideRepositoryReader<UserDocument> users)
        {
            if (feedReader == null) throw new ArgumentNullException("feedReader");
            if (settings == null) throw new ArgumentNullException("settings");
            if (logger == null) throw new ArgumentNullException("logger");
            if (commandService == null) throw new ArgumentNullException("commandService");
            if (plainStorage == null) throw new ArgumentNullException("plainStorage");
            if (users == null) throw new ArgumentNullException("users");
            this.feedReader = feedReader;
            this.settings = settings;
            this.logger = logger;
            this.commandService = commandService;
            this.plainStorage = plainStorage;
            this.users = users;
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
                                this.GetQuestionnaireDocumentFromHeadquartersIfNeeded(interviewFeedEntry.QuestionnaireUri);
                                this.CreateOrUpdateInterviewFromHeadquarters(interviewFeedEntry.InterviewUri);
                                break;
                            case EntryType.InterviewUnassigned:
                                this.GetQuestionnaireDocumentFromHeadquartersIfNeeded(interviewFeedEntry.QuestionnaireUri);
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

        private void GetQuestionnaireDocumentFromHeadquartersIfNeeded(Uri questionnareUrl)
        {
            // TODO
        }

        private void CreateOrUpdateInterviewFromHeadquarters(Uri interviewUrl)
        {
            // TODO
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