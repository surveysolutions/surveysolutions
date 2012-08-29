// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionRemoved.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the CollectionRemoved type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Events.Collection
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The collection removed.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:CollectionRemoved")]
    public class CollectionRemoved
    {
    }
}