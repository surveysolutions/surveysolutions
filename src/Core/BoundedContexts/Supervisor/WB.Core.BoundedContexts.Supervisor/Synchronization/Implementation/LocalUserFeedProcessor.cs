using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Supervisor.Users;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views.User;

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
                this.headquartersPullContext.PushMessageFormat("Processing {0} non processed events for supervisor '{1}'", events.Count(), localSupervisor.UserName);

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

                this.headquartersPullContext.PushMessageFormat("Applying user changes for user {0} with id {1}", deserializedUserDetails.UserName, deserializedUserDetails.PublicKey);
                this.UpdateOrCreateUser(deserializedUserDetails);

                foreach (var appliedChange in userChanges)
                {
                    this.headquartersPullContext.PushMessageFormat("Marking local event {0} as processed", appliedChange.EntryId);
                    appliedChange.IsProcessed = true;
                }

                this.localFeedStorage.Store(changeThatShouldBeApplied);
            }
            catch (ApplicationException e)
            {
                this.logger.Error(string.Format("Error occured while processing users feed event. EventId {0}. Event marked as processed with error.", changeThatShouldBeApplied.EntryId), e);

                this.headquartersPullContext.PushError(string.Format("Failed to process event {0}. Message: {1}. InnerMessage: {2}", changeThatShouldBeApplied.EntryId, e.Message, e.InnerException != null ? e.InnerException.Message : "No inner exception"));
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
                    userRoles, userDetails.IsLockedBySupervisor, userShouldBeLockedByHq, userDetails.Supervisor);
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