// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAnswer.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The Answer interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Entities.SubEntities
{
    using System;

    /// <summary>
    /// The Answer interface.
    /// </summary>
    public interface IAnswer ////: IComposite
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the answer image.
        /// </summary>
        string AnswerImage { get; set; }

        /// <summary>
        /// Gets or sets the answer text.
        /// </summary>
        string AnswerText { get; set; }

        /// <summary>
        /// Gets or sets the answer type.
        /// </summary>
        AnswerType AnswerType { get; set; }

        /// <summary>
        /// Gets or sets the answer value.
        /// </summary>
        object AnswerValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mandatory.
        /// </summary>
        bool Mandatory { get; set; }

        /// <summary>
        /// Gets or sets the name collection.
        /// </summary>
        string NameCollection { get; set; }
        
        /// <summary>
        /// Gets the public key.
        /// </summary>
        Guid PublicKey { get; }

        #endregion
    }
}