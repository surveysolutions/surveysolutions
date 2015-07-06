using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.User;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;
using WB.Core.SharedKernels.SurveyManagement.Views;

namespace WB.Core.BoundedContexts.Headquarters.Users.Denormalizers
{
    internal class UsersFeedDenormalizer : BaseDenormalizer,
                                    IEventHandler<NewUserCreated>,
                                    IEventHandler<UserChanged>,
                                    IEventHandler<UserArchived>,
                                    IEventHandler<UserUnarchived>
    {
        private readonly IReadSideRepositoryWriter<UserChangedFeedEntry> usersFeed;
        private readonly IReadSideRepositoryWriter<UserDocument> users;

        public UsersFeedDenormalizer(IReadSideRepositoryWriter<UserChangedFeedEntry> usersFeed,
            IReadSideRepositoryWriter<UserDocument> users)
        {
            this.usersFeed = usersFeed;
            this.users = users;
        }

        public override object[] Writers
        {
            get { return new object[] { usersFeed }; }
        }

        public override object[] Readers
        {
            get { return new object[] { users}; }
        }

        public void Handle(IPublishedEvent<NewUserCreated> evnt)
        {
            if (evnt.Payload.Roles.HasSupervisorApplicationRole())
            {
                var supervisorId = GetSupervisorId(evnt);

                string eventId = evnt.EventIdentifier.FormatGuid();
                usersFeed.Store(new UserChangedFeedEntry(supervisorId, eventId, UserFeedEntryType.UpdateOrCreate)
                {
                    ChangedUserId = evnt.Payload.PublicKey.FormatGuid(),
                    Timestamp = evnt.EventTimeStamp
                }, eventId);
            }
        }

        public void Handle(IPublishedEvent<UserChanged> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            if (item.Roles.HasSupervisorApplicationRole())
            {
                var supervisorId = GetSupervisorId(item);

                var eventId = evnt.EventIdentifier.FormatGuid();
                usersFeed.Store(new UserChangedFeedEntry(supervisorId, eventId, UserFeedEntryType.UpdateOrCreate)
                {
                    ChangedUserId = evnt.EventSourceId.FormatGuid(),
                    Timestamp = evnt.EventTimeStamp
                }, eventId);
            }
        }

        public void Handle(IPublishedEvent<UserArchived> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            if (item.Roles.Contains(UserRoles.Operator))
            {
                var supervisorId = GetSupervisorId(item);

                var eventId = evnt.EventIdentifier.FormatGuid();
                usersFeed.Store(new UserChangedFeedEntry(supervisorId, eventId, UserFeedEntryType.Archive)
                {
                    ChangedUserId = evnt.EventSourceId.FormatGuid(),
                    Timestamp = evnt.EventTimeStamp
                }, eventId);
            }
        }

        public void Handle(IPublishedEvent<UserUnarchived> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            if (item.Roles.Contains(UserRoles.Operator))
            {
                var supervisorId = GetSupervisorId(item);

                var eventId = evnt.EventIdentifier.FormatGuid();
                usersFeed.Store(new UserChangedFeedEntry(supervisorId, eventId, UserFeedEntryType.Unarchive)
                {
                    ChangedUserId = evnt.EventSourceId.FormatGuid(),
                    Timestamp = evnt.EventTimeStamp
                }, eventId);
            }
        }

        private static string GetSupervisorId(IPublishedEvent<NewUserCreated> evnt)
        {
            string supervisorId = null;

            if (evnt.Payload.Roles.Contains(UserRoles.Supervisor))
            {
                supervisorId = evnt.Payload.PublicKey.FormatGuid();
            }
            else if (evnt.Payload.Roles.Contains(UserRoles.Operator))
            {
                supervisorId = evnt.Payload.Supervisor.Id.FormatGuid();
            }
            
            return supervisorId;
        }

        private static string GetSupervisorId(UserDocument item)
        {
            string supervisorId = null;

            if (item.Roles.Contains(UserRoles.Supervisor))
            {
                supervisorId = item.PublicKey.FormatGuid();
            }
            else if (item.Roles.Contains(UserRoles.Operator))
            {
                supervisorId = item.Supervisor.Id.FormatGuid();
            }
            
            return supervisorId;
        }
    }
}