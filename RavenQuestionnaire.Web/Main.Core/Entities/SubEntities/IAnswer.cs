namespace Main.Core.Entities.SubEntities
{
    using System;

    /// <summary>
    /// The Answer interface.
    /// </summary>
    #warning: remove this interface, as there are no need to have an interface for a value object
    public interface IAnswer // : ICloneable
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
        string AnswerValue { get; set; }

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

        /// <summary>
        /// The clone.
        /// </summary>
        /// <returns>
        /// The <see cref="IAnswer"/>.
        /// </returns>
        IAnswer Clone();
    }
}