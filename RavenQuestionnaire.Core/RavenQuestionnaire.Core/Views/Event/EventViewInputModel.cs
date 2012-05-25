using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Views.Event
{
    public class EventViewInputModel
    {
        public EventViewInputModel(Guid clientPublicKey)
        {
            ClientPublickKey = clientPublicKey;
        }

        public Guid ClientPublickKey { get; private set; }
    }
}
