// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractQuestion.cs" company="">
//   
// </copyright>
// <summary>
//   The abstract question.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.SubEntities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.Composite;
    using Main.Core.ExpressionExecutors;

    using Newtonsoft.Json;

    /// <summary>
    /// The abstract question.
    /// </summary>
    public abstract class AbstractQuestion : IQuestion
    {

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractQuestion"/> class.
        /// </summary>
        protected AbstractQuestion()
        {
            // PublicKey = Guid.NewGuid();
            this.Cards = new List<Image>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractQuestion"/> class.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        protected AbstractQuestion(string text)
            : this()
        {
            this.QuestionText = text;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the answer order.
        /// </summary>
        public Order AnswerOrder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether capital.
        /// </summary>
        public bool Capital { get; set; }

        /// <summary>
        /// Gets or sets the cards.
        /// </summary>
        public List<Image> Cards { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public abstract List<IComposite> Children { get; set; }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        public string ConditionExpression { get; set; }

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
        /// The add card.
        /// </summary>
        /// <param name="card">
        /// The card.
        /// </param>
        public void AddCard(Image card)
        {
            if (this.Cards == null)
            {
                this.Cards = new List<Image>();
            }

            this.Cards.Add(card);
        }

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
        /// The remove card.
        /// </summary>
        /// <param name="imageKey">
        /// The image key.
        /// </param>
        /// <returns>
        /// The Main.Core.Entities.SubEntities.Image.
        /// </returns>
        public Image RemoveCard(Guid imageKey)
        {
            if (this.Cards == null)
            {
                this.Cards = new List<Image>();
            }

            Image card = this.Cards.Single(c => c.PublicKey == imageKey);
            this.Cards.Remove(card);
            return card;
        }

        /// <summary>
        /// The update card.
        /// </summary>
        /// <param name="imageKey">
        /// The image key.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="desc">
        /// The desc.
        /// </param>
        public void UpdateCard(Guid imageKey, string title, string desc)
        {
            if (this.Cards == null)
            {
                this.Cards = new List<Image>();
            }

            Image card = this.Cards.Single(c => c.PublicKey == imageKey);

            card.Title = title;
            card.Description = desc;
        }

        #endregion
    }
}