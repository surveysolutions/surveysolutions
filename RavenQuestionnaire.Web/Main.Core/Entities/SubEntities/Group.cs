namespace Main.Core.Entities.SubEntities
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Main.Core.Entities.Composite;

    using Newtonsoft.Json;

    /// <summary>
    /// The group.
    /// </summary>
    [DebuggerDisplay("Group {PublicKey}")]
    public class Group : IGroup
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Group"/> class.
        /// </summary>
        public Group()
        {
            this.PublicKey = Guid.NewGuid();
            this.Children = new List<IComposite>();
            this.Triggers = new List<Guid>();
            this.ConditionExpression = string.Empty;
            this.Description = string.Empty;
            this.Enabled = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Group"/> class.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public Group(string text)
            : this()
        {
            this.Title = text;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<IComposite> Children { get; set; }

        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        private IComposite parent;
        
        /// <summary>
        /// Gets or sets the propagated.
        /// </summary>
        public Propagate Propagated { get; set; }

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
        /// Gets or sets the triggers.
        /// </summary>
        public List<Guid> Triggers { get; set; }

        #endregion

        #region Public Methods and Operators

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
        /// The insert.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <param name="afterItem">
        /// The after item.
        /// </param>
        /// <exception cref="CompositeException">
        /// </exception>
        public void Insert(IComposite c, Guid? afterItem)
        {
            try
            {
                int index = this.Children.FindIndex(0, this.Children.Count, x => x.PublicKey == afterItem);
                if (index != -1)
                {
                    this.Children.Insert(index + 1, c);
                    return;
                }
                
                this.Children.Insert(0, c);
                return;
            }
            catch (CompositeException)
            {
            }

            throw new CompositeException();
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
            var newGroup = new Group
                {
                    ConditionExpression = this.ConditionExpression,
                    Description = this.Description,
                    Enabled = this.Enabled,
                    Propagated = this.Propagated,
                    PublicKey = this.PublicKey,
                    Title = this.Title,
                    Triggers = new List<Guid>(this.Triggers)
                };

            foreach (var composite in this.Children)
            {
                newGroup.Children.Add(composite.Clone());
            }

            return newGroup;
        }

        /// <summary>
        /// The update.
        /// </summary>
        /// <param name="groupText">
        /// The group text.
        /// </param>
        public void Update(string groupText)
        {
            this.Title = groupText;
        }

        #endregion

        public override string ToString()
        {
            return string.Format("Group {{{0}}} '{1}'", this.PublicKey, this.Title ?? "<untitled>");
        }
    }
}