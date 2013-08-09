namespace Main.Core.Events.Collection
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The collection item removed.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:CollectionItemRemoved")]
    public class CollectionItemRemoved
    {
    }
}