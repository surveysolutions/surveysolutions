// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteAnswer.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The CompleteAnswer interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.SubEntities.Complete
{
    using System;

    using Main.Core.Entities.Composite;

    using Newtonsoft.Json;

    /// <summary>
    /// The complete answer.
    /// </summary>
    public class CompleteAnswer : ICompleteAnswer
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteAnswer"/> class.
        /// </summary>
        public CompleteAnswer()
        {
            this.PublicKey = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteAnswer"/> class.
        /// </summary>
        /// <param name="answer">
        /// The answer.
        /// </param>
        public CompleteAnswer(IAnswer answer)
            : this()
        {
            this.AnswerText = answer.AnswerText;
            this.AnswerType = answer.AnswerType;
            this.AnswerValue = answer.AnswerValue;
            this.AnswerImage = answer.AnswerImage;
            this.Mandatory = answer.Mandatory;
            this.PublicKey = answer.PublicKey;
            this.Selected = false;

            /*  this.PublicKey = answer.PublicKey;
            this.QuestionPublicKey = questionPublicKey;*/
            //// this.CustomAnswer = answer.AnswerText;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteAnswer"/> class.
        /// </summary>
        /// <param name="answer">
        /// The answer.
        /// </param>
        /// <param name="propogationPublicKey">
        /// The propagation public key.
        /// </param>
        public CompleteAnswer(ICompleteAnswer answer, Guid? propogationPublicKey)
            : this(answer)
        {
            this.Selected = answer.Selected;
            this.PropogationPublicKey = propogationPublicKey;
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
        /// Gets or sets the propagation public key.
        /// </summary>
        public Guid? PropogationPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether selected.
        /// </summary>
        public bool Selected { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The op_ explicit.
        /// </summary>
        /// <param name="answer">
        /// The doc.
        /// </param>
        /// <returns>
        /// </returns>
        public static explicit operator CompleteAnswer(Answer answer)
        {
            return new CompleteAnswer
                {
                    PublicKey = answer.PublicKey, 
                    AnswerText = answer.AnswerText, 
                    AnswerValue = answer.AnswerValue, 
                    Mandatory = answer.Mandatory, 
                    AnswerType = answer.AnswerType, 
                    AnswerImage = answer.AnswerImage
                };
        }

        /// <summary>
        /// The clone.
        /// </summary>
        /// <returns>
        /// The <see cref="IAnswer"/>.
        /// </returns>
        public IAnswer Clone()
        {
            CompleteAnswer answer = this.MemberwiseClone() as CompleteAnswer;
            return answer;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The reset.
        /// </summary>
        protected void Reset()
        {
            this.Selected = false;

            // if (this.AnswerType == AnswerType.Text)
            // this.AnswerValue = null;
        }

        /// <summary>
        /// The set.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        protected void Set(object text)
        {
            this.Selected = true;
        }

        #endregion
    }
}