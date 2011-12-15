using System;
using RavenQuestionnaire.Core.Commands;

namespace RavenQuestionnaire.Core.Documents
{
    public class EventDocument
    {
        //public string EventId { get; set; }
        public Guid PublicKey { get; set; }
        public DateTime CreationDate { get; set; }
        public ICommand Command { get; set; }

        
        public EventDocument()
        {
            PublicKey = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }
        public EventDocument(ICommand command)
        {
            PublicKey = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
            Command = command;
        }

    }
}
