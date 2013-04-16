// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoPropagateCompleteQuestion.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The auto propagate complete question.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Entities.SubEntities.Complete.Question
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.Composite;

    /// <summary>
    /// The auto propagate complete question.
    /// </summary>
    public sealed class AutoPropagateCompleteQuestion : AbstractCompleteQuestion, IAutoPropagate, ICompelteValueQuestion<int?>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoPropagateCompleteQuestion"/> class.
        /// </summary>
        public AutoPropagateCompleteQuestion():base()
        {
            this.Triggers = new List<Guid>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoPropagateCompleteQuestion"/> class.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public AutoPropagateCompleteQuestion(string text)
            : base(text)
        {
            this.Triggers = new List<Guid>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoPropagateCompleteQuestion"/> class.
        /// </summary>
        /// <param name="template">
        /// The template.
        /// </param>
        public AutoPropagateCompleteQuestion(IAutoPropagate template)
        {
            this.Triggers = template.Triggers;
            this.MaxValue = template.MaxValue;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the target group key.
        /// </summary>
        public Guid TargetGroupKey { get; set; }

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
        /// The clone.
        /// </summary>
        /// <returns>
        /// The <see cref="IComposite"/>.
        /// </returns>
        public override IComposite Clone()
        {
            var question = base.Clone() as AutoPropagateCompleteQuestion;

            if (this.Triggers != null)
            {
                question.Triggers = new List<Guid>(this.Triggers);
            }

            return question;
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
                    if (value > this.MaxValue)
                        throw new ArgumentException("max value is reached");
                    
                    this.Answer = value;
                }
            }
        }

        #endregion

        #region Implementation of ICompelteValueQuestion<int>

        public int? Answer { get; set; }

        #endregion

        #region Implementation of ITriggerable

        ////public List<Guid> Triggers { get; set; }

        #endregion

        #region Implementation of IAutoPropagate

        public int MaxValue { get; set; }

        #endregion
    }
}