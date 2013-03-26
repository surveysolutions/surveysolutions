// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The abstract question view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View.Answer;
using Main.Core.View.Card;

namespace Main.Core.View.Question
{

    /// <summary>
    /// The abstract question view.
    /// </summary>
    public abstract class AbstractQuestionView : ICompositeView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractQuestionView"/> class.
        /// </summary>
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
            this.Title = doc.QuestionText.Replace(System.Environment.NewLine, " ");
            this.QuestionType = doc.QuestionType;
            this.QuestionScope = doc.QuestionScope;
            this.QuestionnaireKey = questionnaire.PublicKey;
            this.ConditionExpression = doc.ConditionExpression;
            this.ValidationExpression = doc.ValidationExpression;
            this.ValidationMessage = doc.ValidationMessage;
            this.StataExportCaption = doc.StataExportCaption;
            this.Instructions = doc.Instructions;
            ////this.Comments = doc.Comments;
            this.AnswerOrder = doc.AnswerOrder;
            this.Featured = doc.Featured;
            this.Mandatory = doc.Mandatory;
            this.Capital = doc.Capital;
            var autoQuestion = doc as IAutoPropagate;

            if (autoQuestion != null && autoQuestion.Triggers.Any())
            {
                this.Triggers = autoQuestion.Triggers;
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
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether featured.
        /// </summary>
        public bool Featured { get; set; }

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
        /// Gets or sets a value indicating whether Capital.
        /// </summary>
        public bool Capital { get; set; }

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
        /// Gets or sets question scope.
        /// </summary>
        public QuestionScope QuestionScope { get; set; }

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
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the validation expression.
        /// </summary>
        public string ValidationExpression { get; set; }

        /// <summary>
        /// Gets or sets the validation message.
        /// </summary>
        public string ValidationMessage { get; set; }

        /// <summary>
        /// Gets or sets Triggers.
        /// </summary>
        public List<Guid> Triggers { get; set; }

        /// <summary>
        /// Gets or sets MaxValue
        /// </summary>
        public int MaxValue { get; set; }

        /// <summary>
        /// Gets or sets Groups.
        /// </summary>
        public Dictionary<string, Guid> Groups { get; set; }

        /// <summary>
        /// Gets or sets parent group title.
        /// </summary>
        public string GroupTitle { get; set; }

        /// <summary>
        /// Gets or sets parent group is propagated.
        /// </summary>
        public bool IsPropagated { get; set; }

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
            this.Triggers = new List<Guid>();
            this.Groups = new Dictionary<string, Guid>();
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
            this.Triggers = new List<Guid>();
            this.Groups = new Dictionary<string, Guid>();
            var parent = this.GetQuestionGroup(questionnaire, doc.PublicKey);
            this.Parent = parent.PublicKey;
            this.GroupTitle = parent.Title;
            this.IsPropagated = parent.Propagated != Propagate.None;
        }



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
        /// Parent group
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        protected IGroup GetQuestionGroup(IQuestionnaireDocument questionnaire, Guid questionKey)
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
                        return queueItem as IGroup;
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
        where T : AnswerView
        where TQuestion : IQuestion
        where TGroup : IGroup
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

        protected Dictionary<string, Guid> LoadGroups(IQuestionnaireDocument questionnaire, Guid? questionPublicKey, Guid? groupPublicKey)
        {
            try
            {
                var excludedGroups = new List<Guid>();
                if (questionPublicKey != null)
                    groupPublicKey = this.GetQuestionGroup(questionnaire, questionPublicKey.Value).PublicKey;
                if (groupPublicKey != null)
                {
                    excludedGroups.Add(groupPublicKey.Value);
                }

                var groups = new Dictionary<string, Guid>();
                if (questionnaire != null)
                {
                    foreach (var group in questionnaire.Children.Where(t => t is IGroup))
                    {
                        this.SelectAll(group, groups, excludedGroups);
                    }
                }

                return groups;
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Select all groups
        /// </summary>
        /// <param name="currentGroup">
        /// The current group.
        /// </param>
        /// <param name="groups">
        /// The groups.
        /// </param>
        /// <param name="excludedGroups">
        /// The excluded Groups.
        /// </param>
        private void SelectAll(IComposite currentGroup, Dictionary<string, Guid> groups, List<Guid> excludedGroups)
        {
            var s = excludedGroups.Where(t => t == currentGroup.PublicKey).FirstOrDefault();
            if (excludedGroups.Count > 0 && s == Guid.Empty
                && (currentGroup as IGroup).Propagated == Propagate.AutoPropagated)
            {
                groups.Add(
                    string.Format("{0}-{1}", (currentGroup as IGroup).Title, currentGroup.PublicKey),
                    currentGroup.PublicKey);
            }

            if (currentGroup.Children.Where(t => t is IGroup).Count() > 0)
            {
                foreach (var childGroup in currentGroup.Children.Where(t => t is IGroup))
                {
                    this.SelectAll(childGroup, groups, excludedGroups);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// The question view.
    /// </summary>
    public class QuestionView : QuestionView<AnswerView, IGroup, IQuestion>
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

        public QuestionView(IQuestionnaireDocument questionnaire, Guid? groupPublicKey)
        {
            this.Groups = this.LoadGroups(questionnaire, null, groupPublicKey);
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
                doc.Answers.Where(a => a is IAnswer).Select(a => new AnswerView(doc.PublicKey, a as IAnswer)).ToArray();
            if (doc.Cards != null)
            {
                this.Cards =
                    doc.Cards.Select(c => new CardView(doc.PublicKey, c)).OrderBy(a => Guid.NewGuid()).ToArray();
            }


            var autoQuestion = doc as IAutoPropagate;
            if (autoQuestion != null)
            {
                this.MaxValue = autoQuestion.MaxValue;
                if (autoQuestion.Triggers != null)
                {
                    this.Triggers = autoQuestion.Triggers.ToList();
                }
            }

            this.Groups = this.LoadGroups(questionnaire, doc.PublicKey, null);
        }

        #endregion
    }
}