using System;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.Events.Collection
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:CollectionItemRemoved")]
    public class CollectionItemRemoved
    {

    }
}
