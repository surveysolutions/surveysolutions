using System;
using Ncqrs.Eventing.Storage;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.File
{
    public class FileDeleted
    {
        public Guid PublicKey { get; set; }
    }
}