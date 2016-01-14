using System;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.File
{
    public class FileDeleted : IEvent
    {
        public Guid PublicKey { get; set; }
    }
}