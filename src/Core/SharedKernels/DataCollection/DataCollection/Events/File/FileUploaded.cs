using System;
using Ncqrs.Eventing.Storage;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.File
{
    public class FileUploaded
    {
        public string Description { get; set; }

        public string OriginalFile { get; set; }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }
    }
}