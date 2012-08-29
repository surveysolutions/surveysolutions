// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteGroup.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete group.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    using RavenQuestionnaire.Core.AbstractFactories;
    using RavenQuestionnaire.Core.Entities.Composite;

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
        /// The propogation public key.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public CompleteGroup(ICompleteGroup group, Guid propogationPublicKey)
            : this()
        {
            this.Title = group.Title;
            this.Propagated = group.Propagated;
            this.PublicKey = group.PublicKey;
            this.ConditionExpression = group.ConditionExpression;

            for (int i = 0; i < group.Children.Count; i++)
            {
                var question = group.Children[i] as ICompleteQuestion;
                if (question != null)
                {
                    ICompleteQuestion newQuestion = new CompleteQuestionFactory().ConvertToCompleteQuestion(question);
                    newQuestion.PropogationPublicKey = propogationPublicKey;
                    if (!(newQuestion is IBinded))
                    {
                        foreach (ICompleteAnswer completeAnswer in newQuestion.Children)
                        {
                            completeAnswer.PropogationPublicKey = propogationPublicKey;
                        }

                        this.Children.Add(newQuestion);
                    }
                    else
                    {
                        this.Children.Add(newQuestion);
                    }

                    continue;
                }

                var groupChild = group.Children[i] as ICompleteGroup;
                if (groupChild != null)
                {
                    this.Children.Add(new CompleteGroup(groupChild, propogationPublicKey));
                    continue;
                }

                throw new InvalidOperationException("uncnown children type");
            }

            /* for (int i = 0; i < groupWithQuestion.Groups.Count; i++)
                {
                    this.Groups.Add(new PropagatableCompleteGroup(groupWithQuestion.Groups[i], propogationPublicKey));
                   
                }*/
            this.PropogationPublicKey = propogationPublicKey;
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
        /// Gets or sets a value indicating whether is valid.
        /// </summary>
        public bool IsValid { get; set; }

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
        /// Gets or sets the propagated.
        /// </summary>
        public Propagate Propagated { get; set; }

        /// <summary>
        /// Gets or sets the propogation public key.
        /// </summary>
        public Guid? PropogationPublicKey { get; set; }

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
                    ConditionExpression = doc.ConditionExpression
                };

            /* foreach (IComposite question in doc.Children)
            {
              //  result.Questions.Add(new CompleteQuestionFactory().ConvertToCompleteQuestion(question));
                throw new NotImplementedException();
            }
            foreach (IGroup group in doc.Groups)
            {
                result.Groups.Add(new CompleteGroupFactory().ConvertToCompleteGroup(group));
            }*/
            foreach (IComposite child in doc.Children)
            {
                var question = child as IQuestion;
                if (question != null)
                {
                    result.Children.Add(new CompleteQuestionFactory().ConvertToCompleteQuestion(question));
                    continue;
                }

                var group = child as IGroup;
                if (group != null)
                {
                    result.Children.Add(new CompleteGroupFactory().ConvertToCompleteGroup(group));
                    continue;
                }

                throw new InvalidOperationException("unknown children type");
            }

            return result;
        }

        // private IIteratorContainer iteratorContainer;

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
        public virtual void Add(IComposite c, Guid? parent)
        {
            if (!parent.HasValue || parent.Value == this.PublicKey)
            {
                var propogateGroup = c as ICompleteGroup;
                if (propogateGroup != null && propogateGroup.PropogationPublicKey.HasValue)
                {
                    IComposite group = this.Children.FirstOrDefault(g => g.PublicKey == propogateGroup.PublicKey);
                    if (group != null)
                    {
                        this.Children.Add(propogateGroup);
                        return;
                    }
                }
            }

            /* foreach (IComposite child in this.Children)
            {
                try
                {
                    child.Add(c, parent);
                    return;
                }
                catch (CompositeException)
                {
                }
            }*/
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
        public virtual T Find<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T).IsAssignableFrom(this.GetType()))
            {
                if (this.PublicKey.Equals(publicKey))
                {
                    return this as T;
                }
            }

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
        /// <param name="c">
        /// The c.
        /// </param>
        /// <exception cref="CompositeException">
        /// </exception>
        public virtual void Remove(IComposite c)
        {
            var propogate = c as ICompleteGroup;
            if (propogate != null && propogate.PropogationPublicKey.HasValue)
            {
                bool isremoved = false;
                List<IComposite> propagatedGroups =
                    this.Children.Where(
                        g =>
                        g.PublicKey == propogate.PublicKey && g is ICompleteGroup
                        && ((ICompleteGroup)g).PropogationPublicKey == propogate.PropogationPublicKey).ToList();
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
            }*/
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
        public virtual void Remove(Guid publicKey)
        {
            IComposite forRemove = this.Children.FirstOrDefault(g => g.PublicKey.Equals(publicKey));
            if (forRemove != null && forRemove is ICompleteGroup
                && ((ICompleteGroup)forRemove).PropogationPublicKey.HasValue)
            {
                this.Children.Remove(forRemove);
                return;
            }

            foreach (CompleteGroup completeGroup in this.Children)
            {
                try
                {
                    completeGroup.Remove(publicKey);
                    return;
                }
                catch (CompositeException)
                {
                }
            }

            throw new CompositeException();
        }

        #endregion
    }
}