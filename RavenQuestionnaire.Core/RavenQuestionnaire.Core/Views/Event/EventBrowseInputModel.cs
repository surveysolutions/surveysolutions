// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventBrowseInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The event browse input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.Event
{
    using System;

    /// <summary>
    /// The event browse input model.
    /// </summary>
    public class EventBrowseInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBrowseInputModel"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        public EventBrowseInputModel(Guid? publicKey)
        {
            this.PublickKey = publicKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the publick key.
        /// </summary>
        public Guid? PublickKey { get; private set; }

        #endregion
    }
}