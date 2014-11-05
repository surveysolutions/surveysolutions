using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.View.User;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Supervisor.Users;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    public class LocalUserFeedProcessor : ILocalUserFeedProcessor
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;
        private readonly ILocalFeedStorage localFeedStorage;
        private readonly Action<ICommand> executeCommand;
        private readonly IHeadquartersUserReader headquartersUserReader;
        private readonly ILogger logger;
        private readonly HeadquartersPullContext headquartersPullContext;

        public LocalUserFeedProcessor(
            IQueryableReadSideRepositoryReader<UserDocument> users,
            ILocalFeedStorage localFeedStorage,
            ICommandService commandService,
            IHeadquartersUserReader headquartersUserReader,
            ILogger logger,
            HeadquartersPullContext headquartersPullContext)
        {
            if (users == null) throw new ArgumentNullException("users");
            if (localFeedStorage == null) throw new ArgumentNullException("localFeedStorage");
            if (commandService == null) throw new ArgumentNullException("commandService");
            if (headquartersUserReader == null) throw new ArgumentNullException("headquartersUserReader");
            if (logger == null) throw new ArgumentNullException("logger");
            if (headquartersPullContext == null) throw new ArgumentNullException("headquartersPullContext");

            this.users = users;
            this.localFeedStorage = localFeedStorage;
            this.executeCommand = command => commandService.Execute(command, origin: Constants.HeadquartersSynchronizationOrigin);
            this.headquartersUserReader = headquartersUserReader;
            this.logger = logger;
            this.headquartersPullContext = headquartersPullContext;
        }

        public Guid[] PullUsersAndReturnListOfSynchronizedSupervisorsId()
        {
            var localSupervisors = users.Query(_ => _.Where(x => x.Roles.Contains(UserRoles.Supervisor)));

            foreach (var localSupervisor in localSupervisors)
            {
                IEnumerable<LocalUserChangedFeedEntry> events = this.localFeedStorage.GetNotProcessedSupervisorEvents(localSupervisor.PublicKey.FormatGuid());
                this.headquartersPullContext.PushMessage(string.Format(Resources.InterviewsSynchronizer.Processing__0__non_processed_events_for_supervisor___1__Format, events.Count(), localSupervisor.UserName));

                foreach (var userChanges in events.GroupBy(x => x.ChangedUserId))
                {
                    this.ProcessOneUserChanges(userChanges);
                }
            }
            return localSupervisors.Select(s => s.PublicKey).ToArray();
        }

        private void ProcessOneUserChanges(IEnumerable<LocalUserChangedFeedEntry> userChanges)
        {
            LocalUserChangedFeedEntry changeThatShouldBeApplied = userChanges.Last();
            try
            {
                UserView deserializedUserDetails = this.headquartersUserReader.GetUserByUri(changeThatShouldBeApplied.UserDetailsUri).Result;

                this.headquartersPullContext.PushMessage(string.Format(Resources.InterviewsSynchronizer.Applying_user_changes_for_user__0__with_id__1_Format, deserializedUserDetails.UserName, deserializedUserDetails.PublicKey));
                this.UpdateOrCreateUser(deserializedUserDetails);

                foreach (var appliedChange in userChanges)
                {
                    this.headquartersPullContext.PushMessage(string.Format(Resources.InterviewsSynchronizer.Marking_local_event__0__as_processed_Format, appliedChange.EntryId));
                    appliedChange.IsProcessed = true;
                }

                this.localFeedStorage.Store(changeThatShouldBeApplied);
            }
            catch (ApplicationException e)
            {
                this.logger.Error(string.Format(Resources.InterviewsSynchronizer.Error_occured_while_processing_users_feed_event__EventId__0___Event_marked_as_processed_with_error_Format, changeThatShouldBeApplied.EntryId), e);

                this.headquartersPullContext.PushError(string.Format(Resources.InterviewsSynchronizer.Failed_to_process_event__0___Message___1___InnerMessage___2_Format, changeThatShouldBeApplied.EntryId, e.Message, e.InnerException != null ? e.InnerException.Message : "No inner exception"));
                changeThatShouldBeApplied.ProcessedWithError = true;

                this.localFeedStorage.Store(userChanges);
            }
        }

        private void UpdateOrCreateUser(UserView userDetails)
        {
            ICommand userCommand = null;

            UserRoles[] userRoles = userDetails.Roles.ToArray();
            bool userShouldBeLockedByHq = userDetails.IsLockedByHQ || userRoles.Contains(UserRoles.Headquarter);

            if (this.users.GetById(userDetails.PublicKey) == null)
            {
                userCommand = new CreateUserCommand(userDetails.PublicKey, userDetails.UserName, userDetails.Password, userDetails.Email,
                    userRoles, userDetails.isLockedBySupervisor, userShouldBeLockedByHq, userDetails.Supervisor);
            }
            else
            {
                userCommand = new ChangeUserCommand(userDetails.PublicKey, userDetails.Email,
                    userRoles, null, userShouldBeLockedByHq, userDetails.Password, Guid.Empty);
            }

            this.executeCommand(userCommand);
        }
    }
}