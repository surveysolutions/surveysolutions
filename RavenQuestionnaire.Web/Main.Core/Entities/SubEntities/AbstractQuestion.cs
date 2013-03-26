// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractQuestion.cs" company="The World Bank">
//   2012
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
            this.Answers = new List<IAnswer>();
            this.ConditionalDependentGroups = new List<Guid>();
            this.ConditionalDependentQuestions = new List<Guid>();
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
        /// Gets or sets the answers.
        /// </summary>
        public List<IAnswer> Answers { get; set; }

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
        public List<IComposite> Children
        {
            get
            {
                return new List<IComposite>(0);
            }

            set
            {
                // do nothing
            }
        }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets the conditional dependent groups.
        /// </summary>
        public List<Guid> ConditionalDependentGroups { get; set; }

        /// <summary>
        /// Gets or sets the conditional dependent questions.
        /// </summary>
        public List<Guid> ConditionalDependentQuestions { get; set; }

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
        /// Gets or sets the parent.
        /// </summary>
        private IComposite parent;

        public IComposite GetParent()
        {
            return parent;
        }

        public void SetParent(IComposite parent)
        {
            this.parent = parent;
        }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the question scope.
        /// </summary>
        public QuestionScope QuestionScope { get; set; }

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

        /*/// <summary>
        /// Gets or sets Triggers.
        /// </summary>
        public List<Guid> Triggers { get; set; }*/
        #region Public Methods and Operators

        /// <summary>
        /// The add answer.
        /// </summary>
        /// <param name="answer">
        /// The answer.
        /// </param>
        public abstract void AddAnswer(IAnswer answer);

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
        /// The clone.
        /// </summary>
        /// <returns>
        /// The <see cref="IComposite"/>.
        /// </returns>
        public virtual IComposite Clone()
        {
            var question = this.MemberwiseClone() as IQuestion;

            question.SetParent(null);

            if (this.Cards != null)
            {
                question.Cards = new List<Image>(this.Cards); // assuming that cards are structures 
            }

            if (this.ConditionalDependentGroups != null)
            {
                question.ConditionalDependentGroups = new List<Guid>(this.ConditionalDependentGroups);
            }

            if (this.ConditionalDependentQuestions != null)
            {
                question.ConditionalDependentQuestions = new List<Guid>(this.ConditionalDependentQuestions);
            }

            // handle reference part
            question.Answers = new List<IAnswer>();
            foreach (IAnswer answer in this.Answers)
            {
                question.Answers.Add(answer.Clone());
            }

            return question;
        }

        /// <summary>
        /// The connect childs with parent.
        /// </summary>
        public void ConnectChildsWithParent()
        {
            //// do nothing
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