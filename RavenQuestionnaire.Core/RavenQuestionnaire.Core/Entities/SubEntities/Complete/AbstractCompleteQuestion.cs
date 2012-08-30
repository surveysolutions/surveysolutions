// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractCompleteQuestion.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The abstract complete question.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    using RavenQuestionnaire.Core.Entities.Composite;

    /// <summary>
    /// The abstract complete question.
    /// </summary>
    public abstract class AbstractCompleteQuestion : ICompleteQuestion
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractCompleteQuestion"/> class.
        /// </summary>
        protected AbstractCompleteQuestion()
        {
            this.PublicKey = Guid.NewGuid();
            this.Enabled = true;
            this.Valid = true;
            this.Cards = new List<Image>();
            this.Triggers = new List<Guid>();
            this.AnswerDate = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractCompleteQuestion"/> class.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        protected AbstractCompleteQuestion(string text)
            : this()
        {
            this.QuestionText = text;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the answer date.
        /// </summary>
        public DateTime? AnswerDate { get; set; }

        /// <summary>
        /// Gets or sets the answer order.
        /// </summary>
        public Order AnswerOrder { get; set; }

        /// <summary>
        /// Gets or sets the cards.
        /// </summary>
        public List<Image> Cards { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public abstract List<IComposite> Children { get; set; }

        public bool Capital { get; set; }


        
        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether featured.
        /// </summary>
        public bool Featured { get; set; }

        /// <summary>
        /// Gets or sets the instructions.
        /// </summary>
        public string Instructions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mandatory.
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        [JsonIgnore]
        public IComposite Parent
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets the propogation public key.
        /// </summary>
        public Guid? PropogationPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the question text.
        /// </summary>
        public string QuestionText { get; set; }

        /// <summary>
        /// Gets or sets the question type.
        /// </summary>
        public QuestionType QuestionType { get; set; }

        /// <summary>
        /// Gets or sets the stata export caption.
        /// </summary>
        public string StataExportCaption { get; set; }

        /// <summary>
        /// Gets or sets the triggers.
        /// </summary>
        public List<Guid> Triggers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether valid.
        /// </summary>
        public bool Valid { get; set; }

        /// <summary>
        /// Gets or sets the validation expression.
        /// </summary>
        public string ValidationExpression { get; set; }

        /// <summary>
        /// Gets or sets the validation message.
        /// </summary>
        public string ValidationMessage { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <param name="parent">
        /// The parent.
        /// </param>
        public abstract void Add(IComposite c, Guid? parent);

        /// <summary>
        /// The find.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        public abstract T Find<T>(Guid publicKey) where T : class, IComposite;

        /// <summary>
        /// The find.
        /// </summary>
        /// <param name="condition">
        /// The condition.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; T].
        /// </returns>
        public abstract IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class;

        /// <summary>
        /// The first or default.
        /// </summary>
        /// <param name="condition">
        /// The condition.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        public abstract T FirstOrDefault<T>(Func<T, bool> condition) where T : class;

        /// <summary>
        /// The get answer object.
        /// </summary>
        /// <returns>
        /// The System.Object.
        /// </returns>
        public abstract object GetAnswerObject();

        /// <summary>
        /// The get answer string.
        /// </summary>
        /// <returns>
        /// The System.String.
        /// </returns>
        public abstract string GetAnswerString();

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        public abstract void Remove(IComposite c);

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        public abstract void Remove(Guid publicKey);

        /// <summary>
        /// The set answer.
        /// </summary>
        /// <param name="answer">
        /// The answer.
        /// </param>
        /// <param name="answerValue">
        /// The answer value.
        /// </param>
        public abstract void SetAnswer(List<Guid> answer, string answerValue);

        /// <summary>
        /// The set comments.
        /// </summary>
        /// <param name="comments">
        /// The comments.
        /// </param>
        public void SetComments(string comments)
        {
            this.Comments = comments;
        }

        #endregion
    }
}