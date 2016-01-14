using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.User
{
    public class UserChanged : IEvent
    {
        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string PersonName { get; set; }

        public string PhoneNumber { get; set; }
    }
}