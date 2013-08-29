namespace Main.Core.Entities.SubEntities.Complete.Question
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The text complete question.
    /// </summary>
    public sealed class TextCompleteQuestion : AbstractCompleteQuestion, 
                                               ITextCompleteQuestion, 
                                               ICompelteValueQuestion<string>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TextCompleteQuestion"/> class.
        /// </summary>
        public TextCompleteQuestion()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextCompleteQuestion"/> class.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public TextCompleteQuestion(string text)
            : base(text)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the add text attr.
        /// </summary>
        public string AddTextAttr { get; set; }

        /// <summary>
        /// Gets or sets the answer.
        /// </summary>
        public string Answer { get; set; }

        #endregion
        
        #region Public Methods and Operators

        /// <summary>
        /// The add answer.
        /// </summary>
        /// <param name="answer">
        /// The answer.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override void AddAnswer(IAnswer answer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get answer object.
        /// </summary>
        /// <returns>
        /// The System.Object.
        /// </returns>
        public override object GetAnswerObject()
        {
            return this.Answer;
        }

        /// <summary>
        /// The get answer string.
        /// </summary>
        /// <returns>
        /// The System.String.
        /// </returns>
        public override string GetAnswerString()
        {
            return this.Answer ?? string.Empty;
        }

        /// <summary>
        /// The is answered.
        /// </summary>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public override bool IsAnswered()
        {
            return !string.IsNullOrWhiteSpace(this.Answer);
        }

        /// <summary>
        /// The set answer.
        /// </summary>
        /// <param name="answer">
        /// The answer.
        /// </param>
        /// <param name="answerValue">
        /// The answer value.
        /// </param>
        public override void SetAnswer(List<Guid> answer, string answerValue)
        {
            this.Answer = answerValue;
        }

        public override void ThrowDomainExceptionIfAnswerInvalid(List<Guid> answerKeys, string answerValue)
        {
            
        }

        #endregion
    }
}