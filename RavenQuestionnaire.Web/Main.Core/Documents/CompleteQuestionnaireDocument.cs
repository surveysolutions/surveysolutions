namespace Main.Core.Documents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    using Main.Core.AbstractFactories;
    using Main.Core.Entities.Composite;
    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The complete questionnaire document.
    /// </summary>
    public class CompleteQuestionnaireDocument : ICompleteQuestionnaireDocument
    {
        #region Fields

        /// <summary>
        /// The triggers.
        /// </summary>
        private readonly List<Guid> triggers = new List<Guid>();

        /// <summary>
        /// The question hash.
        /// </summary>
        private GroupHash questionHash;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireDocument"/> class.
        /// </summary>
        public CompleteQuestionnaireDocument()
        {
            this.CreationDate = DateTime.Now;
            this.LastEntryDate = DateTime.Now;

            //// this.PublicKey = Guid.NewGuid();
            this.Children = new List<IComposite>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<IComposite> Children { get; set; }

        /// <summary>
        /// Gets or sets the close date.
        /// </summary>
        public DateTime? CloseDate { get; set; }

        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the creator.
        /// </summary>
        public UserLight Creator { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the enable state calculated.
        /// </summary>
        public DateTime EnableStateCalculated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the last entry date.
        /// </summary>
        public DateTime LastEntryDate { get; set; }

        /// <summary>
        /// Gets or sets the last visited group.
        /// </summary>
        public VisitedGroup LastVisitedGroup { get; set; }

        /// <summary>
        /// Gets or sets the open date.
        /// </summary>
        public DateTime? OpenDate { get; set; }

        /// <summary>
        /// Gets or sets the created by.
        /// </summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the is public.
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        private IComposite parent;

        /// <summary>
        /// Gets or sets the propagated.
        /// </summary>
        public Propagate Propagated
        {
            get
            {
                return Propagate.None;
            }

            set
            {
            }
        }

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

        public IComposite GetParent()
        {
            return this.parent;
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
        /// Gets the question hash.
        /// </summary>
        private GroupHash GetQuestionHash()
        {
            return this.questionHash ?? (this.questionHash = new GroupHash(this));
        }


        /// <summary>
        /// The get question by key.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="CompleteQuestionWrapper"/>.
        /// </returns>
        public CompleteQuestionWrapper GetQuestionByKey(string key)
        {
            return this.questionHash.GetQuestionByKey(key);
        }

        /// <summary>
        /// The questions.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<ICompleteQuestion> GetQuestions()
        {
            return this.GetQuestionHash().Questions;
        }

        /// <summary>
        /// Gets or sets the responsible.
        /// </summary>
        public UserLight Responsible { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the status change comment.
        /// </summary>
        public string StatusChangeComment { get; set; }

        /// <summary>
        /// Gets or sets the template id.
        /// </summary>
        public Guid TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the triggers.
        /// </summary>
        public List<Guid> Triggers
        {
            get
            {
                return this.triggers;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets the wrapped questions.
        /// </summary>
        public IEnumerable<CompleteQuestionWrapper> WrappedQuestions()
        {
                return this.GetQuestionHash().WrapedQuestions;
        }

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
        /// <exception cref="InvalidOperationException">
        /// Raises InvalidOperationException.
        /// </exception>
        public static explicit operator CompleteQuestionnaireDocument(QuestionnaireDocument doc)
        {
            var result = new CompleteQuestionnaireDocument
                {
                    TemplateId = doc.PublicKey, 
                    Title = doc.Title, 
                    Triggers = doc.Triggers, 
                    ConditionExpression = doc.ConditionExpression, 
                    Description = doc.Description
                };
            foreach (IComposite child in doc.Children)
            {
                var question = child as IQuestion;
                if (question != null)
                {
                    IComposite questionItem = new CompleteQuestionFactory().ConvertToCompleteQuestion(question);
                    questionItem.SetParent(result);
                    result.Children.Add(questionItem);
                    continue;
                }

                var group = child as IGroup;
                if (group != null)
                {
                    IComposite groupItem = new CompleteGroupFactory().ConvertToCompleteGroup(group);
                    groupItem.SetParent(result);
                    result.Children.Add(groupItem);
                    continue;
                }

                throw new InvalidOperationException("Unknown children type.");
            }

            return result;
        }

        ////  public List<IObserver<CompositeInfo>> Observers { get; set; }

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <param name="parentKey">
        /// The parent.
        /// </param>
        /// <param name="parentPropagationKey">
        /// The parent Propagation Key.
        /// </param>
        /// <exception cref="CompositeException">
        /// Raises CompositeException.
        /// </exception>
        public virtual void Add(IComposite c, Guid? parentKey, Guid? parentPropagationKey)
        {
            var group = c as ICompleteGroup;

            if (group == null || !group.PropagationPublicKey.HasValue)
            {
                throw new ArgumentException("Only propagated group can be added.");
            }

            IComposite itemToadd = null;

            if (parentKey == null || this.PublicKey == parentKey)
            {
                itemToadd = this;
            }
            else
            {
                itemToadd = this.Find<ICompleteGroup>(
                    g => g.PublicKey == parentKey && g.PropagationPublicKey == parentPropagationKey).FirstOrDefault();
            }

            if (itemToadd != null)
            {
                c.SetParent(itemToadd);
                itemToadd.Children.Add(c);
                this.GetQuestionHash().AddGroup(group);
                return;
            }

            // legacy
            throw new CompositeException();
        }

        /// <summary>
        /// The clone.
        /// </summary>
        /// <returns>
        /// The <see cref="IComposite"/>.
        /// </returns>
        public IComposite Clone()
        {
            var doc = this.MemberwiseClone() as CompleteQuestionnaireDocument;

            doc.SetParent(null);
            doc.questionHash = null;

            if (this.Triggers != null)
            {
                doc.Triggers = new List<Guid>(this.Triggers);
            }

            if (this.Creator != null)
            {
                doc.Creator = new UserLight(this.Creator.Id, this.Creator.Name);
            }

            if (this.Responsible != null)
            {
                doc.Responsible = new UserLight(this.Responsible.Id, this.Responsible.Name);
            }

            doc.Children = new List<IComposite>();
            foreach (IComposite composite in this.Children)
            {
                var item = composite.Clone();
                item.SetParent(doc);
                doc.Children.Add(composite.Clone());
            }

            return doc;
        }

        /// <summary>
        /// The connect childs with parent.
        /// </summary>
        public void ConnectChildsWithParent()
        {
            foreach (IComposite item in this.Children)
            {
                item.SetParent(this);
                item.ConnectChildsWithParent();
            }
        }

        /// <summary>
        /// The find.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <typeparam name="T">
        /// Type T = class, IComposite.
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            T resultInsideGroups =
                this.Children.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            if (resultInsideGroups != null)
            {
                return resultInsideGroups;
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
        /// Type T.
        /// </typeparam>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; T].
        /// </returns>
        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return
                this.Children.Where(a => a is T && condition(a as T)).Select(a => a as T).Union(
                    this.Children.SelectMany(q => q.Find(condition)));
        }

        /// <summary>
        /// The first or default.
        /// </summary>
        /// <param name="condition">
        /// The condition.
        /// </param>
        /// <typeparam name="T">
        /// Type T.
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return this.Children.Where(a => a is T && condition(a as T)).Select(a => a as T).FirstOrDefault()
                   ?? this.Children.SelectMany(q => q.Find(condition)).FirstOrDefault();
        }

        /// <summary>
        /// The get featured questions.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<ICompleteQuestion> GetFeaturedQuestions()
        {
            return this.GetQuestionHash().GetFeaturedQuestions();
        }

        /// <summary>
        /// The get question.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// The <see cref="ICompleteQuestion"/>.
        /// </returns>
        public ICompleteQuestion GetQuestion(Guid publicKey, Guid? propagationKey)
        {
            return this.GetQuestionHash().GetQuestion(publicKey, propagationKey);
        }

        /// <summary>
        /// The get question wrapper.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// The <see cref="CompleteQuestionWrapper"/>.
        /// </returns>
        public CompleteQuestionWrapper GetQuestionWrapper(Guid publicKey, Guid? propagationKey)
        {
            return this.GetQuestionHash().GetQuestionWrapper(publicKey, propagationKey);
        }

        /// <summary>
        /// Has Visible Items For Scope
        /// </summary>
        /// <param name="questionScope">
        /// The question scope.
        /// </param>
        /// <returns>
        /// True
        /// </returns>
        public bool HasVisibleItemsForScope(QuestionScope questionScope)
        {
            return true;
        }
        
        public void Remove(Guid itemKey, Guid? propagationKey, Guid? parentPublicKey, Guid? parentPropagationKey)
        {
            // only propagate group is allowed to be remove

            if (!parentPublicKey.HasValue)
            {
                throw new ArgumentException("Parent was not set.");
            }

            ICompleteGroup parent = this.Find<ICompleteGroup>(g => g.PublicKey == parentPublicKey && g.PropagationPublicKey == parentPropagationKey).FirstOrDefault();
            
            var itemToDelete =
                parent.Children.Where(i => i.PublicKey == itemKey).Select(a => a as ICompleteGroup).FirstOrDefault(
                    b => b.PropagationPublicKey == propagationKey);


            parent.Children.Remove(itemToDelete);
            this.questionHash.RemoveGroup(itemToDelete);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on deserialized.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            this.ConnectChildsWithParent();
        }
        
        #endregion
    }
}