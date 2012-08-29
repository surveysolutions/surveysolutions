// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionItemRemoved.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The collection item removed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Events.Collection
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