using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Main.Core;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
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

        public LocalUserFeedProcessor(
            IQueryableReadSideRepositoryReader<UserDocument> users,
            ILocalFeedStorage localFeedStorage,
            ICommandService commandService,
            IHeadquartersUserReader headquartersUserReader,
            ILogger logger)
        {
            if (users == null) throw new ArgumentNullException("users");
            if (localFeedStorage == null) throw new ArgumentNullException("localFeedStorage");
            if (commandService == null) throw new ArgumentNullException("commandService");
            if (headquartersUserReader == null) throw new ArgumentNullException("headquartersUserReader");
            if (logger == null) throw new ArgumentNullException("logger");

            this.users = users;
            this.localFeedStorage = localFeedStorage;
            this.commandService = commandService;
            this.headquartersUserReader = headquartersUserReader;
            this.logger = logger;
        }

        public void Process()
        {
            var localSupervisors = users.Query(_ => _.Where(x => x.Roles.Contains(UserRoles.Supervisor)));

            foreach (var localSupervisor in localSupervisors)
            {
                IEnumerable<LocalUserChangedFeedEntry> events = this.localFeedStorage.GetNotProcessedSupervisorRelatedEvents(localSupervisor.PublicKey.FormatGuid());

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
                UserDocument deserializedUserDetails = this.headquartersUserReader.GetUserByUri(changeThatShouldBeApplied.UserDetailsUri).Result;

                this.UpdateOrCreateUser(deserializedUserDetails);

                foreach (var appliedChange in userChanges)
                {
                    appliedChange.IsProcessed = true;
                }

                this.localFeedStorage.Store(changeThatShouldBeApplied);
            }
            catch (ApplicationException e)
            {
                this.logger.Error(string.Format("Error occured while processing users feed event. EventId {0}. Event marked as processed with error.", changeThatShouldBeApplied.EntryId), e);
                changeThatShouldBeApplied.ProcessedWithError = true;

                this.localFeedStorage.Store(userChanges);
            }
        }

        private void UpdateOrCreateUser(UserDocument userDetails)
        {
            ICommand userCommand = null;

            if (this.users.GetById(userDetails.PublicKey) == null)
            {
                userCommand = new CreateUserCommand(userDetails.PublicKey, userDetails.UserName, userDetails.Password, userDetails.Email,
                    userDetails.Roles.ToArray(), userDetails.IsLocked, userDetails.Supervisor);
            }
            else
            {
                userCommand = new ChangeUserCommand(userDetails.PublicKey, userDetails.Email,
                    userDetails.Roles.ToArray(), userDetails.IsLocked, userDetails.Password);
            }

            this.commandService.Execute(userCommand);
        }
    }
}