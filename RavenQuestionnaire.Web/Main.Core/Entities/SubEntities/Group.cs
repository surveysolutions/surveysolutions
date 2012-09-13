// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Group.cs" company="">
//   
// </copyright>
// <summary>
//   The propagate.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.SubEntities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.Composite;

    using Newtonsoft.Json;

    /// <summary>
    /// The group.
    /// </summary>
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
        /// Gets or sets the parent group.
        /// </summary>
        [JsonIgnore]
        public IComposite ParentGroup { get; set; }

        /// <summary>
        /// Gets or sets the propagated.
        /// </summary>
        public Propagate Propagated { get; set; }

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
            if ((parent.HasValue && parent.Value == this.PublicKey) || !parent.HasValue)
            {
                this.Children.Add(c);
                return;
            }

            foreach (IComposite child in this.Children)
            {
                try
                {
                    child.Add(c, parent);
                    return;
                }
                catch (CompositeException)
                {
                }
            }

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
                else
                {
                    this.Children.Add(c);
                    return;
                }
            }
            catch (CompositeException)
            {
            }

            throw new CompositeException();
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        public void Remove(IComposite c)
        {
            this.Remove(c.PublicKey);
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
            IComposite group = this.Children.FirstOrDefault(g => g.PublicKey.Equals(publicKey));
            if (group != null)
            {
                this.Children.Remove(group);
                return;
            }

            foreach (IComposite child in this.Children)
            {
                try
                {
                    child.Remove(publicKey);
                    return;
                }
                catch (CompositeException)
                {
                }
            }

            throw new CompositeException();
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
    }
}