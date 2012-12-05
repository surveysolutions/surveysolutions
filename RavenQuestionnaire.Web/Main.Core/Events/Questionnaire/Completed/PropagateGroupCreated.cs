﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropagateGroupCreated.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the PropagateGroupCreated type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Events.Questionnaire.Completed
{
    using System;

    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The propagate group created.
    /// </summary>
    [Serializable]
    public class PropagateGroupCreated
    {
        /// <summary>
        /// Gets or sets the propagation key.
        /// </summary>
        public Guid? ParentPropagationKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid ParentPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        public CompleteGroup Group { get; set; }
    }
}
