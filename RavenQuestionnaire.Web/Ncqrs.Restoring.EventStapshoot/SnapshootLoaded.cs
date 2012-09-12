// -----------------------------------------------------------------------
// <copyright file="SnapshootLoaded.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;

namespace Ncqrs.Restoring.EventStapshoot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


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
