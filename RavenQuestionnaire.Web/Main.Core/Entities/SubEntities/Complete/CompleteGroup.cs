// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteGroup.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete group.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Entities.SubEntities.Complete
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.AbstractFactories;
    using Main.Core.Entities.Composite;

    using Newtonsoft.Json;

    /// <summary>
    /// The complete group.
    /// </summary>
    public class CompleteGroup : ICompleteGroup
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteGroup"/> class.
        /// </summary>
        public CompleteGroup()
        {
            this.Children = new List<IComposite>();
            this.PublicKey = Guid.NewGuid();
            this.Triggers = new List<Guid>();
            this.Enabled = true;
            // this.iteratorContainer = new IteratorContainer();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteGroup"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public CompleteGroup(string name)
            : this()
        {
            this.Title = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteGroup"/> class.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="propogationPublicKey">
        /// The propagation public key.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public CompleteGroup(ICompleteGroup group, Guid? propogationPublicKey)
            : this()
        {
            this.Title = group.Title;
            this.Propagated = group.Propagated;
            this.PublicKey = group.PublicKey;
            this.ConditionExpression = group.ConditionExpression;
            this.Description = group.Description;
            this.PropagationPublicKey = propogationPublicKey;

            for (int i = 0; i < group.Children.Count; i++)
            {
                var question = group.Children[i] as ICompleteQuestion;
                if (question != null)
                {
                    ICompleteQuestion newQuestion = new CompleteQuestionFactory().ConvertToCompleteQuestion(question);
                    newQuestion.PropagationPublicKey = propogationPublicKey;

                    /*foreach (ICompleteAnswer completeAnswer in newQuestion.Answers)
                    {
                        completeAnswer.PropogationPublicKey = propogationPublicKey;
                    }*/

                    newQuestion.Parent = this;
                    this.Children.Add(newQuestion);
                    continue;
                }

                var groupChild = group.Children[i] as ICompleteGroup;
                if (groupChild != null)
                {
                    IComposite groupItem = new CompleteGroup(groupChild, propogationPublicKey);
                    groupItem.Parent = this;
                    this.Children.Add(group);
                    continue;
                }

                throw new InvalidOperationException("Unknown child type.");
            }
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
        /// Gets or sets the enable state calculated.
        /// </summary>
        public DateTime EnableStateCalculated { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        [JsonIgnore]
        public IComposite Parent { get; set; }

        /// <summary>
        /// Gets or sets the propagated.
        /// </summary>
        public Propagate Propagated { get; set; }

        /// <summary>
        /// Gets or sets the propogation public key.
        /// </summary>
        public Guid? PropagationPublicKey { get; set; }

        /// <summary>
        /// False if group is empty or has hidden items, true overwise
        /// </summary>
        /// <param name="questionScope">
        /// The question scope.
        /// </param>
        /// <returns>
        /// False or true
        /// </returns>
        public bool HasVisibleItemsForScope(QuestionScope questionScope)
        {
            var count = this.Children.OfType<ICompleteGroup>().Where(g => g.HasVisibleItemsForScope(questionScope)).Count()
                + this.Children.OfType<ICompleteQuestion>().Where(q => q.QuestionScope <= questionScope).Count();
            return count != 0 || this.Children.Count == 0;
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
        /// The op_ explicit.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public static explicit operator CompleteGroup(Group doc)
        {
            var result = new CompleteGroup(null)
                {
                    PublicKey = doc.PublicKey, 
                    Title = doc.Title, 
                    Propagated = doc.Propagated, 
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
                    questionItem.Parent = result;
                    result.Children.Add(questionItem);
                    continue;
                }

                var group = child as IGroup;
                if (group != null)
                {
                    IComposite groupItem = new CompleteGroupFactory().ConvertToCompleteGroup(group);
                    groupItem.Parent = result;
                    result.Children.Add(groupItem);
                    continue;
                }

                throw new InvalidOperationException("Unknown child type.");
            }

            return result;
        }

        // private IIteratorContainer iteratorContainer;

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
        public virtual void Add(IComposite c, Guid? parent, Guid? parentPropagationKey)
        {
            if (!parent.HasValue || parent.Value == this.PublicKey)
            {
                var propogateGroup = c as ICompleteGroup;
                if (propogateGroup != null && propogateGroup.PropagationPublicKey.HasValue)
                {
                    IComposite group = this.Children.FirstOrDefault(g => g.PublicKey == propogateGroup.PublicKey);
                    if (group != null)
                    {
                        if (
                            !this.Children.OfType<ICompleteGroup>().Any(
                                g =>
                                g.PublicKey == propogateGroup.PublicKey &&
                                g.PropagationPublicKey == propogateGroup.PropagationPublicKey))
                        {
                            propogateGroup.Parent = this;
                            this.Children.Add(propogateGroup);
                        }

                        return;
                    }
                }
            }

            throw new CompositeException();
        }*/

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
        public virtual T Find<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T).IsAssignableFrom(this.GetType()))
            {
                if (this.PublicKey.Equals(publicKey))
                {
                    return this as T;
                }
            }

            T resultInsideGroups = this.Children.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
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

        /*/// <summary>
        /// The remove.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <exception cref="CompositeException">
        /// </exception>
        public virtual void Remove(IComposite c)
        {
            var propogate = c as ICompleteGroup;
            if (propogate != null && propogate.PropagationPublicKey.HasValue)
            {
                bool isremoved = false;
                List<IComposite> propagatedGroups =
                    this.Children.Where(
                        g =>
                        g.PublicKey == propogate.PublicKey && g is ICompleteGroup
                        && ((ICompleteGroup)g).PropagationPublicKey == propogate.PropagationPublicKey).ToList();
                foreach (ICompleteGroup propagatableCompleteGroup in propagatedGroups)
                {
                    this.Children.Remove(propagatableCompleteGroup);
                    isremoved = true;
                }

                if (isremoved)
                {
                    return;
                }
            }

            /*foreach (IComposite child in Children)
            {
                try
                {
                    child.Remove(c);
                    return;
                }
                catch (CompositeException)
                {
                }
            }#1#
            throw new CompositeException();
        }*/
        
        /*/// <summary>
        /// The remove.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        public virtual void Remove(Guid publicKey, Guid? propagationKey)
        {
            IComposite forRemove = this.Children.FirstOrDefault(g => g.PublicKey.Equals(publicKey));
            if (forRemove != null && forRemove is ICompleteGroup
                && ((ICompleteGroup)forRemove).PropagationPublicKey.HasValue)
            {
                this.Children.Remove(forRemove);
                return;
            }

            foreach (CompleteGroup completeGroup in this.Children)
            {
                try
                {
                    completeGroup.Remove(publicKey, null);
                    return;
                }
                catch (CompositeException)
                {
                }
            }

            throw new CompositeException();
        }*/

        /// <summary>
        /// The connect childs with parent.
        /// </summary>
        public void ConnectChildsWithParent()
        {
            foreach (var item in this.Children)
            {
                item.Parent = this;
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
            var group = this.MemberwiseClone() as CompleteGroup;

            if (this.Triggers != null)
            {
                this.Triggers = new List<Guid>(this.Triggers);
            }

            group.Children = new List<IComposite>();
            foreach (var composite in this.Children)
            {
                var item = composite.Clone();
                item.Parent = group;
                group.Children.Add(composite.Clone());
            }

            return group;
        }

        #endregion
    }
}