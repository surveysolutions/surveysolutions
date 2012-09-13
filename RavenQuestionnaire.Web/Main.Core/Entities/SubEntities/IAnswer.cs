// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAnswer.cs" company="">
//   
// </copyright>
// <summary>
//   The Answer interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Entities.SubEntities
{
    using Main.Core.Entities.Composite;

    /// <summary>
    /// The Answer interface.
    /// </summary>
    public interface IAnswer : IComposite
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

        #endregion
    }
}