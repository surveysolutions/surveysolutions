// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteGroupHeaders.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete group headers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Main.Core.Entities.SubEntities.Complete;

namespace Main.Core.View.Group
{
    /// <summary>
    /// The complete group headers.
    /// </summary>
    public class CompleteGroupHeaders
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteGroupHeaders"/> class.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        public CompleteGroupHeaders(ICompleteGroup group)
        {
            this.PublicKey = group.PublicKey;
            this.GroupText = group.Title;
            this.PropagationKey = group.PropogationPublicKey;
            this.Description = group.Description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteGroupHeaders"/> class.
        /// </summary>
        public CompleteGroupHeaders()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the group text.
        /// </summary>
        public string GroupText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is current.
        /// </summary>
        public bool IsCurrent { get; set; }

        /// <summary>
        /// Gets or sets the propagation key.
        /// </summary>
        public Guid? PropagationKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the totals.
        /// </summary>
        public Counter Totals { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get client id.
        /// </summary>
        /// <param name="prefix">
        /// The prefix.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public virtual string GetClientId(string prefix)
        {
            return string.Format("{0}_{1}", prefix, this.PublicKey);
        }

        #endregion
    }
}