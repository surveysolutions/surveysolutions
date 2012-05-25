using System;
using RavenQuestionnaire.Core.Commands;

namespace RavenQuestionnaire.Core.Documents
{
    public class EventDocument
    {
        //public string EventId { get; set; }
        public Guid PublicKey { get; set; }
        public Guid ClientPublicKey { get; set; }
        public DateTime CreationDate { get; set; }
        public ICommand Command { get; set; }

        
        public EventDocument()
        {
            CreationDate = DateTime.UtcNow;
        }
        public EventDocument(ICommand command, Guid publicKey, Guid clientPublicKey):this()
        {
            PublicKey = publicKey;
            ClientPublicKey = clientPublicKey;
            Command = command;
        }

    }
}
