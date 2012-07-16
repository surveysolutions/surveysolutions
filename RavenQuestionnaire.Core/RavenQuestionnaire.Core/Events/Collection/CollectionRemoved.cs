using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.Events.Collection
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:CollectionRemoved")]
    public class CollectionRemoved
    {

    }
}
