// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BindedCompleteQuestion.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The binded complete question.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Entities.SubEntities.Complete
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.Composite;

    using Newtonsoft.Json;

    /// <summary>
    /// The binded complete question.
    /// </summary>
    public class BindedCompleteQuestion : ICompleteQuestion, IBinded
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BindedCompleteQuestion"/> class.
        /// </summary>
        public BindedCompleteQuestion()
        {
            this.PublicKey = Guid.NewGuid();
            this.Children = new List<IComposite>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindedCompleteQuestion"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="template">
        /// The template.
        /// </param>
        public BindedCompleteQuestion(Guid publicKey, IBinded template)
        {
            this.PublicKey = publicKey;
            this.ParentPublicKey = template.ParentPublicKey;
        }

        #endregion

        /*
        public BindedCompleteQuestion(ICompleteQuestion template)
        {
            this.ParentPublicKey = template.PublicKey;
        }*/
        #region Public Properties

        /// <summary>
        /// Gets or sets the answer.
        /// </summary>
        public object Answer { get; set; }

        /// <summary>
        /// Gets or sets the answer date.
        /// </summary>
        public DateTime? AnswerDate { get; set; }

        /// <summary>
        /// Gets or sets the answer order.
        /// </summary>
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
        public List<Image> Cards { get; set; }

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
        public bool Enabled
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
        /// Gets or sets the propogation public key.
        /// </summary>
        public Guid? PropagationPublicKey
        {
            get
            {
                return null;
            }

            set
            {
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
        /// Gets or sets a value indicating whether valid.
        /// </summary>
        public bool Valid
        {
            get
            {
                return true;
            }

            set
            {
            }
        }

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
        /// The op_ explicit.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <returns>
        /// </returns>
        public static explicit operator BindedCompleteQuestion(BindedQuestion doc)
        {
            var result = new BindedCompleteQuestion { PublicKey = doc.PublicKey, ParentPublicKey = doc.ParentPublicKey };
            return result;
        }

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
        /// The copy.
        /// </summary>
        /// <param name="template">
        /// The template.
        /// </param>
        public void Copy(ICompleteQuestion template)
        {
            this.Children = template.Children;
            this.Answer = template.GetAnswerObject();
            this.QuestionText = template.QuestionText;
            this.QuestionType = template.QuestionType;
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
        /// The get answer object.
        /// </summary>
        /// <returns>
        /// The System.Object.
        /// </returns>
        public object GetAnswerObject()
        {
            return this.Answer;
        }

        /// <summary>
        /// The is answered.
        /// </summary>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public bool IsAnswered()
        {
            return this.Answer != null;
        }

        /// <summary>
        /// The get answer string.
        /// </summary>
        /// <returns>
        /// The System.String.
        /// </returns>
        public string GetAnswerString()
        {
            return this.Answer == null ? string.Empty : this.Answer.ToString();
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

        /// <summary>
        /// The set answer.
        /// </summary>
        /// <param name="answer">
        /// The answer.
        /// </param>
        /// <param name="answerValue">
        /// The answer value.
        /// </param>
        public void SetAnswer(List<Guid> answer, string answerValue)
        {
        }

        /// <summary>
        /// The set comments.
        /// </summary>
        /// <param name="comments">
        /// The comments.
        /// </param>
        public void SetComments(string comments)
        {
        }

        #endregion
    }
}