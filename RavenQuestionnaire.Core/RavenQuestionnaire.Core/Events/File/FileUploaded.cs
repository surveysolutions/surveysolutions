using System;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.Events.File
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:FileUploaded")]
    public class FileUploaded
    {
        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string OriginalFile { get; set; }

     //   public string ThumbFile { get; set; }

    }
}
