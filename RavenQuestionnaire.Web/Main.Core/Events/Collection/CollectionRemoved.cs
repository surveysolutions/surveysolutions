using System;
using Ncqrs.Eventing.Storage;

namespace Main.Core.Events.Collection
{
    /// <summary>
    /// The collection removed.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:CollectionRemoved")]
    public class CollectionRemoved
    {
    }
}