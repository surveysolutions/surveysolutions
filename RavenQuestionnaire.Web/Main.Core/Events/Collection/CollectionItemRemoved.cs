using System;
using Ncqrs.Eventing.Storage;

namespace Main.Core.Events.Collection
{
    /// <summary>
    /// The collection item removed.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:CollectionItemRemoved")]
    public class CollectionItemRemoved
    {
    }
}