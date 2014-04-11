﻿using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.User;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;

namespace WB.Core.BoundedContexts.Headquarters.Users.Denormalizers
{
    public class UsersFeedDenormalizer : BaseDenormalizer,
                                    IEventHandler<NewUserCreated>,
                                    IEventHandler<UserChanged>
    {
        private readonly IReadSideRepositoryWriter<UserChangedFeedEntry> usersFeed;
        private readonly IReadSideRepositoryWriter<UserDocument> users;

        public UsersFeedDenormalizer(IReadSideRepositoryWriter<UserChangedFeedEntry> usersFeed,
            IReadSideRepositoryWriter<UserDocument> users)
        {
            this.usersFeed = usersFeed;
            this.users = users;
        }

        public void Handle(IPublishedEvent<NewUserCreated> evnt)
        {
            if (evnt.Payload.Roles.HasSupervisorApplicationRole())
            {
                var supervisorId = GetSupervisorId(evnt);

                string eventId = evnt.EventIdentifier.FormatGuid();
                usersFeed.Store(new UserChangedFeedEntry(supervisorId, eventId)
                {
                    ChangedUserId = evnt.Payload.PublicKey.FormatGuid(),
                    Timestamp = evnt.EventTimeStamp
                }, eventId);
            }
        }

        public void Handle(IPublishedEvent<UserChanged> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            if (evnt.Payload.Roles.HasSupervisorApplicationRole())
            {
                var supervisorId = GetSupervisorId(item);

                var eventId = evnt.EventIdentifier.FormatGuid();
                usersFeed.Store(new UserChangedFeedEntry(supervisorId, eventId)
                {
                    ChangedUserId = evnt.EventSourceId.FormatGuid(),
                    Timestamp = evnt.EventTimeStamp
                }, eventId);
            }
        }

        public override Type[] BuildsViews
        {
            get { return new Type[] { typeof(UserChangedFeedEntry) }; }
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
            if (supervisorId == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Only supervisor related events are supported by this denormalizer. EventId: {0}", evnt.EventIdentifier));
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