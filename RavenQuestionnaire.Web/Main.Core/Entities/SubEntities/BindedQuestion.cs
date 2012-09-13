// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BindedQuestion.cs" company="">
//   
// </copyright>
// <summary>
//   The binded question.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.SubEntities
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities.Complete;

    using Newtonsoft.Json;

    /// <summary>
    /// The binded question.
    /// </summary>
    public class BindedQuestion : IQuestion, IBinded
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BindedQuestion"/> class.
        /// </summary>
        public BindedQuestion()
        {
            this.PublicKey = Guid.NewGuid();
            this.Children = new List<IComposite>();
        }

        #endregion

        // public BindedQuestion(IQuestion template)
        // {
        // this.ParentPublicKey=template.PublicKey;
        // }
        #region Public Properties

        /// <summary>
        /// Gets or sets the answer order.
        /// </summary>
        [JsonIgnore]
        public Order AnswerOrder { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        public Dictionary<string, object> Attributes
        {
            get
            {
                return new Dictionary<string, object>(0);
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether capital.
        /// </summary>
        public bool Capital { get; set; }

        /// <summary>
        /// Gets or sets the cards.
        /// </summary>
        [JsonIgnore]
        public List<Image> Cards { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        [JsonIgnore]
        public List<IComposite> Children { get; set; }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        [JsonIgnore]
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        [JsonIgnore]
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether featured.
        /// </summary>
        public bool Featured
        {
            get
            {
                return false;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the instructions.
        /// </summary>
        [JsonIgnore]
        public string Instructions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mandatory.
        /// </summary>
        public bool Mandatory
        {
            get
            {
                return false;
            }

            set
            {
            }
        }

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
        /// Gets or sets the parent public key.
        /// </summary>
        public Guid ParentPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the question text.
        /// </summary>
        [JsonIgnore]
        public string QuestionText { get; set; }

        /// <summary>
        /// Gets or sets the question type.
        /// </summary>
        [JsonIgnore]
        public QuestionType QuestionType { get; set; }

        /// <summary>
        /// Gets or sets the stata export caption.
        /// </summary>
        [JsonIgnore]
        public string StataExportCaption { get; set; }

        /// <summary>
        /// Gets or sets the triggers.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        [JsonIgnore]
        public List<Guid> Triggers
        {
            get
            {
                return new List<Guid>(0);
            }

            set
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Gets or sets the validation expression.
        /// </summary>
        [JsonIgnore]
        public string ValidationExpression { get; set; }

        /// <summary>
        /// Gets or sets the validation message.
        /// </summary>
        [JsonIgnore]
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
        /// <exception cref="CompositeException">
        /// </exception>
        public void Add(IComposite c, Guid? parent)
        {
            throw new CompositeException();
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
            return new T[0];
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
        /// The remove.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <exception cref="CompositeException">
        /// </exception>
        public void Remove(IComposite c)
        {
            throw new CompositeException();
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <exception cref="CompositeException">
        /// </exception>
        public void Remove(Guid publicKey)
        {
            throw new CompositeException();
        }

        #endregion
    }
}