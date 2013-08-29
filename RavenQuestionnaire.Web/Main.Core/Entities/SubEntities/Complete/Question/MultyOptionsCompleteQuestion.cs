using Main.Core.Domain.Exceptions;

namespace Main.Core.Entities.SubEntities.Complete.Question
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using Main.Core.Entities.Composite;

    /// <summary>
    /// The multy options complete question.
    /// </summary>
    public sealed class MultyOptionsCompleteQuestion : AbstractCompleteQuestion, IMultyOptionsQuestion
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MultyOptionsCompleteQuestion"/> class.
        /// </summary>
        public MultyOptionsCompleteQuestion()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultyOptionsCompleteQuestion"/> class.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public MultyOptionsCompleteQuestion(string text)
            : base(text)
        {

        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the add multy attr.
        /// </summary>
        public string AddMultyAttr { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void AddAnswer(IAnswer answer)
        {
            if (answer == null)
            {
                return;
            }

            // AddAnswer(answer);
            if (this.Answers.Any(a => a.PublicKey.Equals(answer.PublicKey)))
            {
                throw new DuplicateNameException("answer with current publick key already exist");
            }

            this.Answers.Add(answer);
        }

        /// <summary>
        /// The get answer object.
        /// </summary>
        /// <returns>
        /// The System.Object.
        /// </returns>
        public override object GetAnswerObject()
        {
            IEnumerable<object> answers = 
                this.Answers.Where(c => ((ICompleteAnswer)c).Selected).Select(c => c.AnswerValue ?? c.AnswerText).ToArray();

            return answers.Any() ? answers : null;
        }

        /// <summary>
        /// The is answered.
        /// </summary>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public override bool IsAnswered()
        {
            return this.Answers.Any(c => ((ICompleteAnswer)c).Selected);
        }

        /// <summary>
        /// The get answer string.
        /// </summary>
        /// <returns>
        /// The System.String.
        /// </returns>
        public override string GetAnswerString()
        {
            var items = this.Answers.Where(a => ((ICompleteAnswer)a).Selected).ToArray();

            if (items.Any())
            {
                return string.Join(", ", items.Select(a => a.AnswerType == AnswerType.Image ? a.AnswerImage : a.AnswerText.ToString()));
            }

            return string.Empty;
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
        /// <exception cref="Exception">
        /// </exception>
        /// <exception cref="CompositeException">
        /// </exception>
        public override void SetAnswer(List<Guid> answer, string answerValue)
        {
            if (answer == null)
            {
            }

            // iterates over all items to set on/off current state
            foreach (var item in this.Answers)
            {
                (item as ICompleteAnswer).Selected = answer.Contains(item.PublicKey);
            }

        }

        public override void ThrowDomainExceptionIfAnswerInvalid(List<Guid> answerKeys, string answerValue)
        {
            if (answerKeys == null)
            {
                return;
            }
            foreach (var item in answerKeys)
            {
                if (this.Answers.All(a => a.PublicKey != item))
                    throw new InterviewException(string.Format("value {0} is absent", item));
            }
        }

        #endregion
    }
}