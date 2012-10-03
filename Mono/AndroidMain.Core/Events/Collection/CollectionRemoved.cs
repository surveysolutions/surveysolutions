﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionRemoved.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the CollectionRemoved type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Events.Collection
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