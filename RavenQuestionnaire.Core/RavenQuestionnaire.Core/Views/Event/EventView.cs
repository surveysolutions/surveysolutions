using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;

namespace RavenQuestionnaire.Core.Views.Event
{
    public class EventView
    {
        public Guid PublicKey { get; set; }
        public Guid ClientPublicKey { get; set; }
        public DateTime CreationDate { get; set; }
        public ICommand Command { get; set; }

        public EventView(Guid publicKey, Guid clientPublicKey, DateTime creationDate , ICommand command)
        {
            this.PublicKey = publicKey;
            this.ClientPublicKey = clientPublicKey;
            this.CreationDate = creationDate;
            this.Command = command;
        }
    }
}
