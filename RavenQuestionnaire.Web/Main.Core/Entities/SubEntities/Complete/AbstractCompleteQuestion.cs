namespace Main.Core.Entities.SubEntities.Complete
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.Composite;

    using Newtonsoft.Json;

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
            this.IsFlaged = false;
            this.Cards = new List<Image>();
            this.AnswerDate = DateTime.Now;
            this.Answers = new List<IAnswer>();
            this.ConditionalDependentQuestions = new List<Guid>();
            this.ConditionalDependentGroups = new List<Guid>();
            this.Comments = new List<CommentDocument>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractCompleteQuestion"/> class.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        protected AbstractCompleteQuestion(string text) : this()
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
        /// Gets or sets the answers.
        /// </summary>
        public List<IAnswer> Answers { get; set; }

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
        public List<IComposite> Children
        {
            get
            {
                return null;
            }

            set
            {
                ////do nothing
                ////throw new NotImplementedException();
            } 
        }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        public string LastComment { get; set; }

        /// <summary>
        /// Gets or sets Comments.
        /// </summary>
        public List<CommentDocument> Comments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsFlaged.
        /// </summary>
        public bool IsFlaged { get; set; }

        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the enable state calculated.
        /// </summary>
        public DateTime EnableStateCalculated { get; set; }

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

        /// <summary>
        /// Gets or sets the propogation public key.
        /// </summary>
        public Guid? PropagationPublicKey { get; set; }

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
        /// Gets or sets the question text.
        /// </summary>
        public string QuestionText { get; set; }

        /// <summary>
        /// Gets or sets the question type.
        /// </summary>
        public QuestionType QuestionType { get; set; }

        /// <summary>
        /// Gets or sets question scope.
        /// </summary>
        public QuestionScope QuestionScope { get; set; }

        /// <summary>
        /// Gets or sets the stata export caption.
        /// </summary>
        public string StataExportCaption { get; set; }

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

        /// <summary>
        /// Gets or sets the conditional dependent questions.
        /// </summary>
        public List<Guid> ConditionalDependentQuestions { get; set; }

        /// <summary>
        /// Gets or sets the conditional dependent groups.
        /// </summary>
        public List<Guid> ConditionalDependentGroups { get; set; }
        
        /// <summary>
        /// Gets or sets the triggers.
        /// </summary>
        public List<Guid> Triggers { get; set; }


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
        public abstract void AddAnswer(IAnswer answer);

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
        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return null;
        }

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
        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return Enumerable.Empty<T>();
        }

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
        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return null;
        }

        /// <summary>
        /// The get answer object.
        /// </summary>
        /// <returns>
        /// The System.Object.
        /// </returns>
        public abstract object GetAnswerObject();

        /// <summary>
        /// The is answered.
        /// </summary>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public abstract bool IsAnswered();

        /// <summary>
        /// The get answer string.
        /// </summary>
        /// <returns>
        /// The System.String.
        /// </returns>
        public abstract string GetAnswerString();
        

        /// <summary>
        /// The connect childs with parent.
        /// </summary>
        public void ConnectChildsWithParent()
        {
            //// do nothing
        }

        /// <summary>
        /// The clone.
        /// </summary>
        /// <returns>
        /// The <see cref="IComposite"/>.
        /// </returns>
        public virtual IComposite Clone()
        {
            var question = this.MemberwiseClone() as ICompleteQuestion;

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
            foreach (var answer in this.Answers)
            {
                var item = answer.Clone();
                question.Answers.Add(answer.Clone());
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
        public abstract void SetAnswer(List<Guid> answer, string answerValue);

        /// <summary>
        /// The set comments.
        /// </summary>
        /// <param name="comments">
        ///   The comments.
        /// </param>
        /// <param name="date">
        ///   The date
        /// </param>
        /// <param name="user">
        /// The user
        /// </param>
        public void SetComments(string comments, DateTime date, UserLight user)
        {
            this.LastComment = comments;
            if (this.Comments == null)
                this.Comments = new List<CommentDocument>();
            if (this.Comments.Count > 0 && this.Comments.Last().Comment == comments)
            {
                return;
            }

            this.Comments.Add(new CommentDocument {CommentDate = date, Comment = comments, User = user});
        }

        #endregion
    }
}