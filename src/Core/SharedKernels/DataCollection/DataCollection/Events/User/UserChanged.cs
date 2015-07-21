using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.Storage;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.User
{
    [EventName("RavenQuestionnaire.Core:Events:UserChanged")]
    public class UserChanged
    {
        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string PersonName { get; set; }

        public string PhoneNumber { get; set; }
    }
}