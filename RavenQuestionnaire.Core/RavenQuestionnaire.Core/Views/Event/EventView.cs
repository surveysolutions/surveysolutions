using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;

namespace RavenQuestionnaire.Core.Views.Event
{
    public class EventView
    {
        public string Id { get; set; }
        public Guid PublicKey { get; set; }
        public DateTime CreationDate { get; set; }
        public ICommand Command { get; set; }

        public EventView(string id, Guid publicKey, DateTime creationDate , ICommand command)
        {
            this.Id = id;
            this.PublicKey = publicKey;
            this.CreationDate = creationDate;
            this.Command = command;
        }
    }
}
