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
        private readonly IQueryablePlainStorageAccessor<InterviewFeedEntry> plainStorage;
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;

        public InterviewsSynchronizer(IAtomFeedReader feedReader, 
            HeadquartersSettings settings, 
            ILogger logger,
            ICommandService commandService,
            IQueryablePlainStorageAccessor<InterviewFeedEntry> plainStorage, 
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
                IEnumerable<InterviewFeedEntry> events = this.plainStorage.Query(_ => _.Where(x => x.SupervisorId == localSupervisor.PublicKey.FormatGuid()));
                foreach (var interviewFeedEntry in events)
                {

                    try
                    {
                        Uri questionnareUrl = new Uri(""), interviewUrl = new Uri(""); // TODO: intialize from feed entry

                        switch (interviewFeedEntry.EntryType)
                        {
                            case EntryType.SupervisorAssigned:
                                this.GetQuestionnaireDocumentFromHeadquartersIfNeeded(questionnareUrl);
                                this.CreateOrUpdateInterviewFromHeadquarters(interviewUrl);
                                break;
                            case EntryType.InterviewUnassigned:
                                this.GetQuestionnaireDocumentFromHeadquartersIfNeeded(questionnareUrl);
                                this.CreateOrUpdateInterviewFromHeadquarters(interviewUrl);
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

            var remoteEvents = this.feedReader.ReadAfterAsync<InterviewFeedEntry>(this.settings.InterviewsFeedUrl, lastStoredEntry)
                .Result;

            IEnumerable<InterviewFeedEntry> newEvents = remoteEvents.Select(x => x.Content);
            this.plainStorage.Store(newEvents.Select(x => Tuple.Create(x, x.EntryId)));
        }
    }
}