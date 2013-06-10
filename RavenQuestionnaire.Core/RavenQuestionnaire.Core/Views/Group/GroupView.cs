// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroupView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The abstract group view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View;
using Main.Core.View.Question;

namespace RavenQuestionnaire.Core.Views.Group
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The abstract group view.
    /// </summary>
    public abstract class AbstractGroupView : ICompositeView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractGroupView"/> class.
        /// </summary>
        public AbstractGroupView()
        {
            this.Children = new List<ICompositeView>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractGroupView"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="parentGroup">
        /// The parent group.
        /// </param>
        public AbstractGroupView(string questionnaireId, Guid? parentGroup)
        {
            this.QuestionnaireKey = Guid.Parse(questionnaireId);
            this.Parent = parentGroup;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractGroupView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        protected AbstractGroupView(IQuestionnaireDocument doc, IGroup group)
        {
            if (group.Triggers != null)
            {
                this.Trigger = group.Triggers.Count > 0 ? group.Triggers[0].ToString() : null;
            }
            else
            {
                this.Trigger = null;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<ICompositeView> Children { get; set; }

        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        public Guid? Parent { get; set; }

        /// <summary>
        /// Gets or sets the propagated.
        /// </summary>
        public Propagate Propagated { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire key.
        /// </summary>
        public Guid QuestionnaireKey { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        public ICompositeView ParentView { get; private set; }

        /// <summary>
        /// Gets or sets the parent group title.
        /// </summary>
        public string ParentGroupTitle { get; set; }

        /// <summary>
        /// Gets or sets the trigger.
        /// </summary>
        public string Trigger { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        #endregion
    }

    /// <summary>
    /// The abstract group view.
    /// </summary>
    /// <typeparam name="TGroup">
    /// </typeparam>
    /// <typeparam name="TQuestion">
    /// </typeparam>
    public abstract class AbstractGroupView<TGroup, TQuestion> : AbstractGroupView
        where TGroup : AbstractGroupView where TQuestion : AbstractQuestionView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractGroupView{TGroup,TQuestion}"/> class.
        /// </summary>
        public AbstractGroupView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractGroupView{TGroup,TQuestion}"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="parentGroup">
        /// The parent group.
        /// </param>
        public AbstractGroupView(Guid questionnaireId, Guid? parentGroup)
        {
            this.QuestionnaireKey = questionnaireId;
            this.Parent = parentGroup;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractGroupView{TGroup,TQuestion}"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        protected AbstractGroupView(IQuestionnaireDocument doc, IGroup group)
        {
            this.QuestionnaireKey = doc.PublicKey;
            this.PublicKey = group.PublicKey;
            this.Title = group.Title;
            this.Propagated = group.Propagated;
            if (group.Triggers != null)
            {
                this.Trigger = group.Triggers.Count > 0 ? group.Triggers[0].ToString() : null;
            }
            else
            {
                this.Trigger = null;
            }
        }

        #endregion
    }

    /// <summary>
    /// The group view.
    /// </summary>
    /// <typeparam name="TGroupView">
    /// </typeparam>
    /// <typeparam name="TQuestionView">
    /// </typeparam>
    /// <typeparam name="TGroup">
    /// </typeparam>
    /// <typeparam name="TQuestion">
    /// </typeparam>
    public abstract class GroupView<TGroupView, TQuestionView, TGroup, TQuestion> :
        AbstractGroupView<TGroupView, TQuestionView>
        where TGroupView : AbstractGroupView
        where TQuestionView : AbstractQuestionView
        where TQuestion : IQuestion
        where TGroup : IGroup
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupView{TGroupView,TQuestionView,TGroup,TQuestion}"/> class.
        /// </summary>
        public GroupView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupView{TGroupView,TQuestionView,TGroup,TQuestion}"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="parentGroup">
        /// The parent group.
        /// </param>
        public GroupView(Guid questionnaireId, Guid? parentGroup)
            : base(questionnaireId, parentGroup)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupView{TGroupView,TQuestionView,TGroup,TQuestion}"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        public GroupView(IQuestionnaireDocument doc, TGroup group)
            : base(doc, group)
        {
            var parentGroup = this.GetGroupParent(doc, group) as IGroup;
            if (parentGroup != null)
            {
                this.Parent = parentGroup.PublicKey;
                this.ParentGroupTitle = parentGroup.Title;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get group parent.
        /// </summary>
        /// <param name="questionnaire">
        /// The questionnaire.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <returns>
        /// The System.Nullable`1[T -&gt; System.Guid].
        /// </returns>
        protected IComposite GetGroupParent(IQuestionnaireDocument questionnaire, TGroup group)
        {
            if (questionnaire.Children.Any(q => q.PublicKey.Equals(group.PublicKey)))
            {
                return null;
            }

            var groups = new Queue<IComposite>();
            foreach (IComposite child in questionnaire.Children)
            {
                groups.Enqueue(child);
            }

            while (groups.Count != 0)
            {
                IComposite queueItem = groups.Dequeue();
                if (queueItem == null)
                {
                    continue;
                }

                if (queueItem.Children != null && queueItem.Children.Any(q => q.PublicKey.Equals(group.PublicKey)))
                {
                    return queueItem;
                }

                if (queueItem.Children != null)
                {
                    foreach (IComposite child in queueItem.Children)
                    {
                        groups.Enqueue(child);
                    }
                }
            }

            return null;
        }

        #endregion
    }

    /// <summary>
    /// The group view.
    /// </summary>
    public class GroupView : GroupView<GroupView, QuestionView, IGroup, IQuestion>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupView"/> class.
        /// </summary>
        public GroupView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupView"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="parentGroup">
        /// The parent group.
        /// </param>
        public GroupView(Guid questionnaireId, Guid? parentGroup)
            : base(questionnaireId, parentGroup)
        {
            this.QuestionnaireKey = questionnaireId;
            this.Parent = parentGroup;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        public GroupView(IQuestionnaireDocument doc, IGroup group)
            : base(doc, group)
        {
            foreach (IComposite composite in group.Children)
            {
                if ((composite as IQuestion) != null)
                {
                    var q = composite as IQuestion;
                    List<IQuestion> r = group.Children.OfType<IQuestion>().ToList();
                    this.Children.Add(new QuestionView(doc, q) { Index = r.IndexOf(q) + 1 });
                }
                else
                {
                    var g = composite as IGroup;
                    this.Children.Add(new GroupView(doc, g));
                }
            }
            
            this.ConditionExpression = group.ConditionExpression;
            this.Description = group.Description;
        }

        #endregion
    }
}