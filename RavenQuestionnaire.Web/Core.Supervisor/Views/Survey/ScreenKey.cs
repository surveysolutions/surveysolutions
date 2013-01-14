// -----------------------------------------------------------------------
// <copyright file="ScreenKey.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Core.Supervisor.Views.Survey
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ScreenKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenKey"/> class.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        public ScreenKey(ICompleteGroup group)
        {
            this.Propagated = group.Propagated;
            this.PublicKey = group.PublicKey;
            this.PropagationKey = group.PropagationPublicKey.HasValue ? group.PropagationPublicKey.Value : Guid.Empty;
        }

        /// <summary>
        /// Gets a value indicating whether isPropagated.
        /// </summary>
        public bool IsPropagated
        { 
            get
            {
                return this.Propagated != Propagate.None;
            }
        }

        /// <summary>
        /// Gets a value indicating whether isTemplate.
        /// </summary>
        public bool IsTemplate
        {
            get
            {
                return this.IsPropagated && this.PropagationKey == Guid.Empty;
            }
        }


        /// <summary>
        /// Gets or sets the propagated.
        /// </summary>
        public Propagate Propagated { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PropagationKey { get; set; }
    }
}
