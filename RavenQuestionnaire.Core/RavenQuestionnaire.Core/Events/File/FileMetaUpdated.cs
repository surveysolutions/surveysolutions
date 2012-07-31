using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.Events.File
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:FileMetaUpdated")]
    public class FileMetaUpdated
    {
        public Guid PublicKey { get; set; }

        public string Title { get;  set; }

        public string Description { get;  set; }
    }
}
