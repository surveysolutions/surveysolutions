// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SingleCompleteQuestion.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The single complete question.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Entities.SubEntities.Complete.Question
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using Main.Core.Entities.Composite;

    /// <summary>
    /// The single complete question.
    /// </summary>
    public sealed class SingleCompleteQuestion : AbstractCompleteQuestion, ISingleQuestion
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleCompleteQuestion"/> class.
        /// </summary>
        public SingleCompleteQuestion()
            : base()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleCompleteQuestion"/> class.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public SingleCompleteQuestion(string text)
            : base(text)
        {
           
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the add single attr.
        /// </summary>
        public string AddSingleAttr { get; set; }

        /*/// <summary>
        /// Gets or sets the children.
        /// </summary>
        public override List<IComposite> Children { get; set; }*/

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
        /// <exception cref="CompositeException">
        /// </exception>
        public override void Add(IComposite c, Guid? parent)
        {
            throw new CompositeException();
        }*/

        public override void AddAnswer(IAnswer answer)
        {
            if (answer == null)
            {
                return;
            }

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
            
            return answers.Any() ? answers.First() : null;
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
            var answer = this.Answers.FirstOrDefault(a => ((ICompleteAnswer)a).Selected);
            return answer == null ? string.Empty : answer.AnswerText;
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
        /// <exception cref="CompositeException">
        /// </exception>
        public override void SetAnswer(List<Guid> answer, string answerValue)
        {
            if (answer == null)
            {
                return;
            }

            Guid selecteAnswer = answer.First();
            
            foreach (var item in this.Answers)
            {
                (item as ICompleteAnswer).Selected = selecteAnswer == item.PublicKey;
            }
        }

        #endregion
    }
}