// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The abstract question view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.Question
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.Composite;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Views.Answer;
    using RavenQuestionnaire.Core.Views.Card;

    /// <summary>
    /// The abstract question view.
    /// </summary>
    public abstract class AbstractQuestionView : ICompositeView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractQuestionView"/> class.
        /// </summary>
        public bool Capital { get; set; }

        public Propagate ParentGroupType { get; set; }
        public AbstractQuestionView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractQuestionView"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        public AbstractQuestionView(Guid questionnaireId, Guid? groupPublicKey)
            : this()
        {
            this.QuestionnaireKey = questionnaireId;
            this.Parent = groupPublicKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractQuestionView"/> class.
        /// </summary>
        /// <param name="questionnaire">
        /// The questionnaire.
        /// </param>
        /// <param name="doc">
        /// The doc.
        /// </param>
        protected AbstractQuestionView(IQuestionnaireDocument questionnaire, IQuestion doc)
            : this()
        {
            this.PublicKey = doc.PublicKey;
            this.Title = doc.QuestionText;
            this.QuestionType = doc.QuestionType;
            this.QuestionnaireKey = questionnaire.PublicKey;
            this.ConditionExpression = doc.ConditionExpression;
            this.ValidationExpression = doc.ValidationExpression;
            this.ValidationMessage = doc.ValidationMessage;
            this.StataExportCaption = doc.StataExportCaption;
            this.Instructions = doc.Instructions;
            this.Comments = doc.Comments;
            this.AnswerOrder = doc.AnswerOrder;
            this.Featured = doc.Featured;
            this.Capital = doc.Capital;
            this.Mandatory = doc.Mandatory;
            if (doc.Triggers.Count > 0)
            {
                this.TargetGroupKey = doc.Triggers.First();
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the answer order.
        /// </summary>
        public Order AnswerOrder { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<ICompositeView> Children { get; set; }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        [Display(Prompt = "When this question is enabled?")]
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether featured.
        /// </summary>
        public bool Featured { get; set; }
        
         /// <summary>
        /// Gets or sets a value indicating whether capital.
        /// </summary>
        public bool Capital { get; set; }


        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        public int Index { get; set; }

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
        public Guid? Parent { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the question type.
        /// </summary>
        public QuestionType QuestionType { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire key.
        /// </summary>
        public Guid QuestionnaireKey { get; set; }

        /// <summary>
        /// Gets or sets the stata export caption.
        /// </summary>
        public string StataExportCaption { get; set; }

        /// <summary>
        /// Gets or sets the target group key.
        /// </summary>
        public Guid TargetGroupKey { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        [Display(Prompt = "Type question here")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the validation expression.
        /// </summary>
        [Display(Prompt = "When this question is valid?")]
        public string ValidationExpression { get; set; }

        /// <summary>
        /// Gets or sets the validation message.
        /// </summary>
        public string ValidationMessage { get; set; }

        #endregion
    }

    /// <summary>
    /// The abstract question view.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public abstract class AbstractQuestionView<T> : AbstractQuestionView
        where T : AnswerView
    {
        #region Fields

        /// <summary>
        /// The _answers.
        /// </summary>
        private T[] _answers;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractQuestionView{T}"/> class.
        /// </summary>
        public AbstractQuestionView()
        {
            this.Answers = new T[0];
            this.Cards = new CardView[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractQuestionView{T}"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        public AbstractQuestionView(string questionnaireId, Guid? groupPublicKey)
            : this()
        {
            this.QuestionnaireKey = Guid.Parse(questionnaireId);
            this.Parent = groupPublicKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractQuestionView{T}"/> class.
        /// </summary>
        /// <param name="questionnaire">
        /// The questionnaire.
        /// </param>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public AbstractQuestionView(IQuestionnaireDocument questionnaire, IQuestion doc)
            : base(questionnaire, doc)
        {
            this.Answers = new T[0];
            this.Cards = new CardView[0];
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the answers.
        /// </summary>
        public T[] Answers
        {
            get
            {
                return this._answers;
            }

            set
            {
                this._answers = value;
                if (this._answers == null)
                {
                    this._answers = new T[0];
                    return;
                }

                for (int i = 0; i < this._answers.Length; i++)
                {
                    this._answers[i].Index = i + 1;
                }
            }
        }

        /// <summary>
        /// Gets or sets the cards.
        /// </summary>
        public CardView[] Cards { get; set; }

        #endregion
    }

    /// <summary>
    /// The question view.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    /// <typeparam name="TGroup">
    /// </typeparam>
    /// <typeparam name="TQuestion">
    /// </typeparam>
    public abstract class QuestionView<T, TGroup, TQuestion> : AbstractQuestionView<T>
        where T : AnswerView where TQuestion : IQuestion where TGroup : IGroup
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionView{T,TGroup,TQuestion}"/> class.
        /// </summary>
        public QuestionView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionView{T,TGroup,TQuestion}"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        public QuestionView(string questionnaireId, Guid? groupPublicKey)
            : base(questionnaireId, groupPublicKey)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionView{T,TGroup,TQuestion}"/> class.
        /// </summary>
        /// <param name="questionnaire">
        /// The questionnaire.
        /// </param>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public QuestionView(IQuestionnaireDocument questionnaire, TQuestion doc)
            : base(questionnaire, doc)
        {
         
            var parent = GetQuestionGroup(questionnaire, doc.PublicKey);
            this.Parent = parent.PublicKey;
            this.ParentGroupType = (parent as IGroup) != null ? (parent as IGroup).Propagated : Propagate.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionView{T,TGroup,TQuestion}"/> class.
        /// </summary>
        /// <param name="questionnaire">
        /// The questionnaire.
        /// </param>
        /// <param name="doc">
        /// The doc.
        /// </param>
        protected QuestionView(IQuestionnaireDocument questionnaire, IQuestion doc)
            : base(questionnaire, doc)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get question group.
        /// </summary>
        /// <param name="questionnaire">
        /// The questionnaire.
        /// </param>
        /// <param name="questionKey">
        /// The question key.
        /// </param>
        /// <returns>
        /// The System.Nullable`1[T -&gt; System.Guid].
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        {
            if (questionnaire.Children.Any(q => q.PublicKey.Equals(questionKey)))
            {
                return null;
            }

            var group = new Queue<IComposite>();
            foreach (IComposite child in questionnaire.Children)
            {
                group.Enqueue(child);
            }

            while (group.Count != 0)
            {
                IComposite queueItem = group.Dequeue();
                if (queueItem.Children != null)
                {
                    if (queueItem.Children.Any(q => q.PublicKey.Equals(questionKey)))
                    {
                    }

                    foreach (IComposite child in queueItem.Children)
                    {
                        /* var childWithQuestion = child as IGroup<IGroup, TQuestion>;
                         if(childWithQuestion!=null)*/
                        group.Enqueue(child);
                    }
                }
            }

            throw new ArgumentException("group does not exist");
        }

        #endregion
    }

    /// <summary>
    /// The question view.
    /// </summary>
    public class QuestionView : QuestionView<AnswerView, Group, IQuestion>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionView"/> class.
        /// </summary>
        public QuestionView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionView"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        public QuestionView(string questionnaireId, Guid? groupPublicKey)
            : base(questionnaireId, groupPublicKey)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionView"/> class.
        /// </summary>
        /// <param name="questionnaire">
        /// The questionnaire.
        /// </param>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public QuestionView(IQuestionnaireDocument questionnaire, IQuestion doc)
            : base(questionnaire, doc)
        {
            this.Answers =
                doc.Children.Where(a => a is IAnswer).Select(a => new AnswerView(doc.PublicKey, a as IAnswer)).ToArray();
            if (doc.Cards != null)
            {
                this.Cards =
                    doc.Cards.Select(c => new CardView(doc.PublicKey, c)).OrderBy(a => Guid.NewGuid()).ToArray();
            }
        }

        #endregion
    }
}