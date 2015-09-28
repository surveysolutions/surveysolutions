using System;
using Ncqrs.Eventing.Storage;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.File
{
    public class FileMetaUpdated
    {
        public string Description { get; set; }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }
    }
}