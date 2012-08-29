// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventViewInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The event view input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.Event
{
    using System;

    /// <summary>
    /// The event view input model.
    /// </summary>
    public class EventViewInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EventViewInputModel"/> class.
        /// </summary>
        /// <param name="clientPublicKey">
        /// The client public key.
        /// </param>
        public EventViewInputModel(Guid clientPublicKey)
        {
            this.ClientPublickKey = clientPublicKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the client publick key.
        /// </summary>
        public Guid ClientPublickKey { get; private set; }

        #endregion
    }
}