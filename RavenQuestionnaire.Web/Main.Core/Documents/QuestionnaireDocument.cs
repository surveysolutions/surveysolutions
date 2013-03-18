// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireDocument.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The QuestionnaireDocument interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Documents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities;
    using Main.DenormalizerStorage;

#warning if MONODROID is bad. should use abstract logger (ILogger?) which implementation will be different in different apps
#if MONODROID
    using AndroidLogger;
#else
    using NLog;
#endif

    using Newtonsoft.Json;

    /// <summary>
    /// The questionnaire document.
    /// </summary>
    [SmartDenormalizer]
    public class QuestionnaireDocument : IQuestionnaireDocument
    {
#if MONODROID
        private static readonly ILog Logger = LogManager.GetLogger(typeof(QuestionnaireDocument));
#else
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
#endif

        #region Fields

        /// <summary>
        /// The triggers.
        /// </summary>
        private readonly List<Guid> triggers = new List<Guid>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireDocument"/> class.
        /// </summary>
        public QuestionnaireDocument()
        {
            this.CreationDate = DateTime.Now;
            this.LastEntryDate = DateTime.Now;
            this.PublicKey = Guid.NewGuid();
            this.Children = new List<IComposite>();
            this.ConditionExpression = string.Empty;
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

        // public string Id { get; set; }

        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the last entry date.
        /// </summary>
        public DateTime LastEntryDate { get; set; }

        /// <summary>
        /// Gets or sets the open date.
        /// </summary>
        public DateTime? OpenDate { get; set; }

        /// <summary>
        /// Gets or sets deleted document flag
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the created by.
        /// </summary>
        public Guid? CreatedBy { get; set; }

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
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

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
        public void Add(IComposite c, Guid? parent, Guid? parentPropagationKey)
        {
            if (!parent.HasValue || this.PublicKey == parent)
            {
                ////add to the root
                c.SetParent(this);
                this.Children.Add(c);
                return;
            }
            
            var group = this.Find<Group>(parent.Value);
            if (@group != null)
            {
                @group.Children.Add(c);
                return;
            }

            //// leave legacy for awhile
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
            foreach (IComposite child in this.Children)
            {
                if (child is T && child.PublicKey == publicKey)
                {
                    return child as T;
                }

                var subNodes = child.Find<T>(publicKey);
                if (subNodes != null)
                {
                    return subNodes;
                }
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
        /// The remove.
        /// </summary>
        /// <param name="itemKey">
        /// The item key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <param name="parentPublicKey">
        /// The parent public key.
        /// </param>
        /// <param name="parentPropagationKey">
        /// The parent propagation key.
        /// </param>
        public void Remove(Guid itemKey, Guid? propagationKey, Guid? parentPublicKey, Guid? parentPropagationKey)
        {
            // we could delete group from the root of Questionnaire
            if (parentPublicKey == null || parentPublicKey == Guid.Empty || this.PublicKey == parentPublicKey)
            {
                this.Children.RemoveAll(i => i.PublicKey == itemKey);
            }
            else
            {
                IGroup parent = this.Find<IGroup>(g => g.PublicKey == parentPublicKey).FirstOrDefault();
                if (parent != null)
                {
                    parent.Children.RemoveAll(i => i.PublicKey == itemKey);
                }
            }
        }

        public void RemoveGroup(Guid groupId)
        {
            var groupParent = this.GetParentOfGroup(groupId);

            if (groupParent != null)
            {
                RemoveChildGroupBySpecifiedId(groupParent, groupId);
            }
            else
            {
                Logger.Warn(string.Format("Failed to remove group '{0}' because it's parent is not found.", groupId));
            }
        }

        private static void RemoveChildGroupBySpecifiedId(IComposite container, Guid groupId)
        {
            container.Children.RemoveAll(child => IsGroupWithSpecifiedId(child, groupId));
        }

        private IComposite GetParentOfGroup(Guid groupId)
        {
            if (ContainsChildGroupWithSpecifiedId(this, groupId))
                return this;

            return this.Find<IGroup>(
                group => ContainsChildGroupWithSpecifiedId(group, groupId)).SingleOrDefault();
        }

        private static bool ContainsChildGroupWithSpecifiedId(IComposite container, Guid groupId)
        {
            return container.Children.Any(child => IsGroupWithSpecifiedId(child, groupId));
        }

        private static bool IsGroupWithSpecifiedId(IComposite child, Guid groupId)
        {
            return child is IGroup && ((IGroup)child).PublicKey == groupId;
        }

        /// <summary>
        /// The connect childs with parent.
        /// </summary>
        public void ConnectChildsWithParent()
        {
            foreach (var item in this.Children)
            {
                item.SetParent(this);
                item.ConnectChildsWithParent();
            }
        }

        /// <summary>
        /// The clone.
        /// </summary>
        /// <returns>
        /// The <see cref="IComposite"/>.
        /// </returns>
        public IComposite Clone()
        {
            var doc = this.MemberwiseClone() as QuestionnaireDocument;

            doc.Triggers = new List<Guid>(this.Triggers);
            doc.SetParent(null);

            doc.Children = new List<IComposite>();
            foreach (var composite in this.Children)
            {
                doc.Children.Add(composite.Clone());
            }

            return doc;
        }
        
        #endregion
    }
}