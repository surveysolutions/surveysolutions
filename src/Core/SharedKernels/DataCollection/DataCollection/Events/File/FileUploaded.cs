using System;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.File
{
    public class FileUploaded : IEvent
    {
        public string Description { get; set; }

        public string OriginalFile { get; set; }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }
    }
}