// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteAnswer.cs" company="">
//   
// </copyright>
// <summary>
//   The CompleteAnswer interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.SubEntities.Complete
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.Composite;

    using Newtonsoft.Json;

    /// <summary>
    /// The complete answer.
    /// </summary>
    public class CompleteAnswer : ICompleteAnswer
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteAnswer"/> class.
        /// </summary>
        public CompleteAnswer()
        {
            this.PublicKey = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteAnswer"/> class.
        /// </summary>
        /// <param name="answer">
        /// The answer.
        /// </param>
        public CompleteAnswer(IAnswer answer)
            : this()
        {
            this.AnswerText = answer.AnswerText;
            this.AnswerType = answer.AnswerType;
            this.AnswerValue = answer.AnswerValue;
            this.AnswerImage = answer.AnswerImage;
            this.Mandatory = answer.Mandatory;
            this.PublicKey = answer.PublicKey;
            this.Selected = false;

            /*  this.PublicKey = answer.PublicKey;
            this.QuestionPublicKey = questionPublicKey;*/
            //// this.CustomAnswer = answer.AnswerText;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteAnswer"/> class.
        /// </summary>
        /// <param name="answer">
        /// The answer.
        /// </param>
        /// <param name="propogationPublicKey">
        /// The propogation public key.
        /// </param>
        public CompleteAnswer(ICompleteAnswer answer, Guid? propogationPublicKey)
            : this(answer)
        {
            this.Selected = answer.Selected;
            this.PropogationPublicKey = propogationPublicKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the answer image.
        /// </summary>
        public string AnswerImage { get; set; }

        /// <summary>
        /// Gets or sets the answer text.
        /// </summary>
        public string AnswerText { get; set; }

        /// <summary>
        /// Gets or sets the answer type.
        /// </summary>
        public AnswerType AnswerType { get; set; }

        /// <summary>
        /// Gets or sets the answer value.
        /// </summary>
        public object AnswerValue { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<IComposite> Children
        {
            get
            {
                return new List<IComposite>();
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether mandatory.
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// Gets or sets the name collection.
        /// </summary>
        public string NameCollection { get; set; }

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
        /// Gets or sets a value indicating whether selected.
        /// </summary>
        public bool Selected { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The op_ explicit.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <returns>
        /// </returns>
        public static explicit operator CompleteAnswer(Answer doc)
        {
            return new CompleteAnswer
                {
                    PublicKey = doc.PublicKey, 
                    AnswerText = doc.AnswerText, 
                    AnswerValue = doc.AnswerValue, 
                    Mandatory = doc.Mandatory, 
                    AnswerType = doc.AnswerType, 
                    AnswerImage = doc.AnswerImage
                };
        }

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
        public void Add(IComposite c, Guid? parent)
        {
            throw new NotImplementedException();

            var answer = c as CompleteAnswer;
            if (answer == null)
            {
                throw new CompositeException("answer wasn't found");
            }

            if (answer.PublicKey == this.PublicKey &&
                ((!answer.PropogationPublicKey.HasValue && !this.PropogationPublicKey.HasValue)
                 || answer.PropogationPublicKey == this.PropogationPublicKey))
            {
                this.Set(answer.AnswerValue);
                return;
            }

            throw new CompositeException("answer wasn't found");
        }*/

        /*/// <summary>
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
            if (!typeof(T).IsAssignableFrom(this.GetType()))
            {
                return null;
            }

            if (publicKey == this.PublicKey)
            {
                return this as T;
            }

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
            if (!typeof(T).IsAssignableFrom(this.GetType()))
            {
                return new T[0];
            }

            if (condition(this as T))
            {
                return new[] { this as T };
            }

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
            if (!typeof(T).IsAssignableFrom(this.GetType()))
            {
                return null;
            }

            if (condition(this as T))
            {
                return this as T;
            }

            return null;
        }*/

        /*/// <summary>
        /// The remove.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <exception cref="CompositeException">
        /// </exception>
        public void Remove(IComposite c)
        {
            if (c.PublicKey == this.PublicKey)
            {
                this.Reset();
                return;
            }

            throw new CompositeException("answer wasn't found");
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
            if (publicKey == this.PublicKey)
            {
                this.Reset();
                return;
            }

            throw new CompositeException("answer wasn't found");
        }*/

        #endregion

        #region Methods

        /// <summary>
        /// The reset.
        /// </summary>
        protected void Reset()
        {
            this.Selected = false;

            // if (this.AnswerType == AnswerType.Text)
            // this.AnswerValue = null;
        }

        /// <summary>
        /// The set.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        protected void Set(object text)
        {
            this.Selected = true;

            // if (this.AnswerType == AnswerType.Text)
            // this.AnswerValue = text;
        }

        #endregion
    }
}