// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NumericCompleteQuestion.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The numeric complete question.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Entities.SubEntities.Complete.Question
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The numeric complete question.
    /// </summary>
    public sealed class NumericCompleteQuestion : AbstractCompleteQuestion, INumericQuestion, ICompelteValueQuestion<int?>
    {

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericCompleteQuestion"/> class.
        /// </summary>
        public NumericCompleteQuestion()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericCompleteQuestion"/> class.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public NumericCompleteQuestion(string text)
            : base(text)
        {
        }

        #endregion

        // {
        // get { return new List<IComposite>(); }
        // set { }
        // }
        #region Public Properties

        /// <summary>
        /// Gets or sets the add numeric attr.
        /// </summary>
        public string AddNumericAttr { get; set; }
        
        /// <summary>
        /// Gets or sets the int attr.
        /// </summary>
        public int IntAttr { get; set; }

        #endregion

        #region Public Methods and Operators

        /*/// <summary>
        /// The add.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <param name="parent">
        /// The parent.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override void Add(IComposite c, Guid? parent)
        {
            throw new NotImplementedException();

            /*var question = c as ICompleteQuestion;
            if (question == null || question.PublicKey != this.PublicKey)
                throw new CompositeException();
            this.Answer = question.Answer;
            this.AnswerDate = DateTime.Now;#1#
        }*/

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
        /// The is answered.
        /// </summary>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public override bool IsAnswered()
        {
            return this.Answer != null;
        }

        /// <summary>
        /// The get answer string.
        /// </summary>
        /// <returns>
        /// The System.String.
        /// </returns>
        public override string GetAnswerString()
        {
            return this.Answer.HasValue ? this.Answer.Value.ToString() : string.Empty;
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
            if (string.IsNullOrWhiteSpace(answerValue))
            {
                this.Answer = null;
            }
            else
            {
                int value;
                if (int.TryParse(answerValue.Trim(), out value))
                {
                    this.Answer = value;
                }
            }
        }

        #endregion

        #region Implementation of ICompelteValueQuestion<int>

        public int? Answer { get; set; }

        #endregion
    }
}