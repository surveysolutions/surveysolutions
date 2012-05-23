using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Event
{
    public class EventBrowseItem
    {
        public EventBrowseItem(Guid publickey, DateTime creationDate, ICommand command)
        {
            this.PublicKey = publickey;
            this.CreationDate = creationDate;
            this.Command = command;
        }


        public Guid PublicKey { get; private set; }
        public DateTime CreationDate { get; private set; }
        public ICommand Command { get;private set; }
    }
}
