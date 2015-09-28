using System;
using Ncqrs.Eventing.Storage;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.User
{
    public class UserUnlocked
    {
        public Guid PublicKey { get; set; }
    }
}