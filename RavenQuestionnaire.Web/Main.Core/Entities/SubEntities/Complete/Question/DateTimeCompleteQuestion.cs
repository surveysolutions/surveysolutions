using System.Threading;

namespace Main.Core.Entities.SubEntities.Complete.Question
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Main.Core.Entities.Composite;

    /// <summary>
    /// The date time complete question.
    /// </summary>
    public sealed class DateTimeCompleteQuestion : AbstractCompleteQuestion, IDateTimeQuestion, ICompelteValueQuestion<DateTime?>
    {
        #region Fields


        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeCompleteQuestion"/> class.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public DateTimeCompleteQuestion(string text)
            : base(text)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeCompleteQuestion"/> class.
        /// </summary>
        public DateTimeCompleteQuestion()
            : base()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the add date time attr.
        /// </summary>
        public string AddDateTimeAttr { get; set; }
        
        /// <summary>
        /// Gets or sets the date time attr.
        /// </summary>
        public DateTime DateTimeAttr { get; set; }

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
            return this.Answer.HasValue
                       ? Convert.ToString(this.Answer.Value, CultureInfo.InvariantCulture)
                       : string.Empty;
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
            DateTime date;
            if (DateTime.TryParse(answerValue, out date))
                this.Answer = date;
        }

        #endregion

        #region Implementation of ICompelteValueQuestion<DateTime>

        public DateTime? Answer { get; set; }

        #endregion
    }
}