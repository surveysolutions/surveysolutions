// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Answer.cs" company="">
//   
// </copyright>
// <summary>
//   The Answer interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.SubEntities
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.Composite;

    using Newtonsoft.Json;

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
        public object AnswerValue { get; set; }
       
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
    }
}