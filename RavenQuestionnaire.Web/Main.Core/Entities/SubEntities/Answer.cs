// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Answer.cs" company="The World Bank">
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
    /// The answer.
    /// </summary>
    public class Answer : IAnswer
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Answer"/> class.
        /// </summary>
        public Answer(/*Question owner*/)
        {
            this.PublicKey = Guid.NewGuid();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the answer image.
        /// </summary>
        public string AnswerImage { get; set; }

        /// <summary>
        /// Gets or sets the answer text.
        /// </summary>
        public string AnswerText { get; set; }

        /// <summary>
        /// Gets or sets the answer type.
        /// </summary>
        public AnswerType AnswerType { get; set; }

        /// <summary>
        /// Gets or sets the answer value.
        /// </summary>
        public string AnswerValue { get; set; }
       
        /// <summary>
        /// Gets or sets a value indicating whether mandatory.
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// Gets or sets the name collection.
        /// </summary>
        public string NameCollection { get; set; }
        
        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        #endregion

        #region Implementation of ICloneable

        /// <summary>
        /// The clone.
        /// </summary>
        /// <returns>
        /// The <see cref="IAnswer"/>.
        /// </returns>
        public IAnswer Clone()
        {
            return this.MemberwiseClone() as Answer;
        }

        #endregion
    }
}