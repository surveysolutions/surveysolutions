﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultyOptionsCompleteQuestion.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The multy options complete question.
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
    /// The multy options complete question.
    /// </summary>
    public sealed class MultyOptionsCompleteQuestion : AbstractCompleteQuestion, IMultyOptionsQuestion
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MultyOptionsCompleteQuestion"/> class.
        /// </summary>
        public MultyOptionsCompleteQuestion()
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

       /* /// <summary>
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
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override void Add(IComposite c, Guid? parent)
        {
            throw new NotImplementedException();

        }*/

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
            IEnumerable<object> answers = this.Answers.Where(c => ((ICompleteAnswer)c).Selected).Select(
                    c => ((ICompleteAnswer)c).AnswerValue ?? ((ICompleteAnswer)c).AnswerText).ToArray();

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
            var items = this.GetAnswerObject() as IEnumerable<object>;
            
            if (items != null && items.ToArray().Any())
            {
                return string.Join(", ", items.Select(a => a.ToString()));
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
                ////multiOption supports only list of answers
                throw new Exception("Parameter: answer");
            }

            foreach (var item in this.Answers)
            {
                (item as ICompleteAnswer).Selected = answer.Contains(item.PublicKey);
            }
            
        }

        #endregion
    }
}