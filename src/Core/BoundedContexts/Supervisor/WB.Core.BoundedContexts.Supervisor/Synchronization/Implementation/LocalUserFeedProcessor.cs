using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Main.Core;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.View.User;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using Newtonsoft.Json;
using Raven.Abstractions.Extensions;
using WB.Core.BoundedContexts.Supervisor.Users;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.User;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    public class LocalUserFeedProcessor : ILocalUserFeedProcessor
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;
        private readonly ILocalFeedStorage localFeedStorage;
        private readonly ICommandService commandService;
        private readonly IHeadquartersUserReader headquartersUserReader;
        private readonly ILogger logger;
        private readonly SynchronizationContext synchronizationContext;

        public LocalUserFeedProcessor(
            IQueryableReadSideRepositoryReader<UserDocument> users,
            ILocalFeedStorage localFeedStorage,
            ICommandService commandService,
            IHeadquartersUserReader headquartersUserReader,
            ILogger logger,
            SynchronizationContext synchronizationContext)
        {
            if (users == null) throw new ArgumentNullException("users");
            if (localFeedStorage == null) throw new ArgumentNullException("localFeedStorage");
            if (commandService == null) throw new ArgumentNullException("commandService");
            if (headquartersUserReader == null) throw new ArgumentNullException("headquartersUserReader");
            if (logger == null) throw new ArgumentNullException("logger");
            if (synchronizationContext == null) throw new ArgumentNullException("synchronizationContext");

            this.users = users;
            this.localFeedStorage = localFeedStorage;
            this.commandService = commandService;
            this.headquartersUserReader = headquartersUserReader;
            this.logger = logger;
            this.synchronizationContext = synchronizationContext;
        }

        public void Process()
        {
            var localSupervisors = users.Query(_ => _.Where(x => x.Roles.Contains(UserRoles.Supervisor)));

            foreach (var localSupervisor in localSupervisors)
            {
                synchronizationContext.PushMessage(string.Format("Processing events for supervisor {0} with Id {1}", localSupervisor.UserName, localSupervisor.PublicKey));
                IEnumerable<LocalUserChangedFeedEntry> events = this.localFeedStorage.GetNotProcessedSupervisorRelatedEvents(localSupervisor.PublicKey.FormatGuid());
                synchronizationContext.PushMessage(string.Format("Reveived {0} non processed events for supervisor {1}", events.Count(), localSupervisor.UserName));

                foreach (var userChanges in events.GroupBy(x => x.ChangedUserId))
                {
                    this.ProcessOneUserChanges(userChanges);
                }
            }
        }

        private void ProcessOneUserChanges(IEnumerable<LocalUserChangedFeedEntry> userChanges)
        {
            LocalUserChangedFeedEntry changeThatShouldBeApplied = userChanges.Last();
            try
            {
                UserView deserializedUserDetails = this.headquartersUserReader.GetUserByUri(changeThatShouldBeApplied.UserDetailsUri).Result;

                this.synchronizationContext.PushMessage(string.Format("Applying user changes for user {0} with id {1}", deserializedUserDetails.UserName, deserializedUserDetails.PublicKey));
                this.UpdateOrCreateUser(deserializedUserDetails);

                foreach (var appliedChange in userChanges)
                {
                    this.synchronizationContext.PushMessage(string.Format("Marking local event {0} as processed", appliedChange.EntryId));
                    appliedChange.IsProcessed = true;
                }

                this.localFeedStorage.Store(changeThatShouldBeApplied);
            }
            catch (ApplicationException e)
            {
                this.logger.Error(string.Format("Error occured while processing users feed event. EventId {0}. Event marked as processed with error.", changeThatShouldBeApplied.EntryId), e);
                this.synchronizationContext.PushError(string.Format("Failed to process event {0}. Message: {1}", changeThatShouldBeApplied.EntryId, e.Message));
                changeThatShouldBeApplied.ProcessedWithError = true;

                this.localFeedStorage.Store(userChanges);
            }
        }

        private void UpdateOrCreateUser(UserView userDetails)
        {
            ICommand userCommand = null;

            if (this.users.GetById(userDetails.PublicKey) == null)
            {
                userCommand = new CreateUserCommand(userDetails.PublicKey, userDetails.UserName, userDetails.Password, userDetails.Email,
                    userDetails.Roles.ToArray(), userDetails.isLockedBySupervisor, userDetails.IsLockedByHQ, userDetails.Supervisor);
            }
            else
            {
                userCommand = new ChangeUserCommand(userDetails.PublicKey, userDetails.Email,
                    userDetails.Roles.ToArray(), userDetails.isLockedBySupervisor, userDetails.IsLockedByHQ, userDetails.Password, Guid.Empty);
            }

            this.commandService.Execute(userCommand);
        }
    }
}