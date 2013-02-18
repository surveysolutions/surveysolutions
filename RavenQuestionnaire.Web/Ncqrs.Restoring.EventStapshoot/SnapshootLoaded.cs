// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SnapshootLoaded.cs" company="">
//   
// </copyright>
// <summary>
//   The snapshoot loaded.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Ncqrs.Restoring.EventStapshoot
{
    using System;

    using Ncqrs.Eventing.Sourcing.Snapshotting;
    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The snapshoot loaded.
    /// </summary>
    [Serializable]
    [EventName("Ncqrs.Restoring.EventStapshoot:SnapshootLoaded")]
    public class SnapshootLoaded
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the template.
        /// </summary>
        public Snapshot Template { get; set; }

        #endregion
    }
}