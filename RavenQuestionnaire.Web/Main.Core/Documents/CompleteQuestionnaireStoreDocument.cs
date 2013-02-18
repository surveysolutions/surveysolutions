﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireStoreDocument.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the CompleteQuestionnaireStoreDocument type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Documents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;

    using Main.Core.AbstractFactories;
    using Main.Core.Entities.Composite;
    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.DenormalizerStorage;

    using Newtonsoft.Json;

    /// <summary>
    /// The complete questionnaire store document.
    /// </summary>
    [SmartDenormalizer]
    public class CompleteQuestionnaireStoreDocument : ICompleteQuestionnaireDocument
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
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireStoreDocument"/> class.
        /// </summary>
        public CompleteQuestionnaireStoreDocument()
        {
            this.CreationDate = DateTime.Now;
            this.LastEntryDate = DateTime.Now;

            //// this.PublicKey = Guid.NewGuid();
            this.Children = new List<IComposite>();
            this.StatusChangeComments = new List<ChangeStatusDocument>();
            
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets Status Change Comments.
        /// </summary>
        public List<ChangeStatusDocument> StatusChangeComments { get; set; }

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
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the enable state calculated.
        /// </summary>
        public DateTime EnableStateCalculated { get; set; }

        /// <summary>
        /// Gets or sets the forcing propagation public key.
        /// </summary>
        public Guid? ForcingPropagationPublicKey
        {
            get
            {
                return null;
            }

            private set
            {
                
            }
        }

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
        /// Gets or sets the parent.
        /// </summary>
        [JsonIgnore]
        public IComposite Parent { get; set; }

        /// <summary>
        /// Gets or sets the propagated.
        /// </summary>
        [XmlIgnore]
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

        public bool HasVisibleItemsForScope(QuestionScope questionScope)
        {
            return true;
        }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets the wrapped questions.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<CompleteQuestionWrapper> WrappedQuestions
        {
            get
            {
                return this.QuestionHash.WrapedQuestions;
            }

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
            return this.QuestionHash.GetQuestion(publicKey, propagationKey);
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
            return this.QuestionHash.GetQuestionWrapper(publicKey, propagationKey);
        }

        /// <summary>
        /// The get featured questions.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<ICompleteQuestion> GetFeaturedQuestions()
        {
            return this.QuestionHash.GetFeaturedQuestions();
        }

        /// <summary>
        /// Gets the questions.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<ICompleteQuestion> Questions
        {
            get
            {
                return this.QuestionHash.Questions;
            }
        }

        /// <summary>
        /// Gets or sets the question hash.
        /// </summary>
        [JsonIgnore]
        public GroupHash QuestionHash
        {
            get
            {
                return this.questionHash ?? (this.questionHash = new GroupHash(this));
            }

            set
            {
                this.questionHash = value;
            }
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
        public static explicit operator CompleteQuestionnaireStoreDocument(QuestionnaireDocument doc)
        {
            var result = new CompleteQuestionnaireStoreDocument
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

            /*   foreach (IQuestion question in doc.Questions)
               {
                   result.Questions.Add(new CompleteQuestionFactory().ConvertToCompleteQuestion(question));
               }
               foreach (IGroup group in doc.Groups)
               {
                   result.Groups.Add(new CompleteGroupFactory().ConvertToCompleteGroup(group));
               }**/
            return result;
        }

        /// <summary>
        /// The op_ explicit.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <returns>
        /// </returns>
        public static explicit operator CompleteQuestionnaireStoreDocument(CompleteQuestionnaireDocument doc)
        {
            var result = new CompleteQuestionnaireStoreDocument
                {
                    PublicKey = doc.PublicKey, 
                    TemplateId = doc.TemplateId, 
                    Title = doc.Title, 
                    Triggers = doc.Triggers, 
                    ConditionExpression = doc.ConditionExpression, 
                    CreationDate = doc.CreationDate, 
                    LastEntryDate = doc.LastEntryDate, 
                    Status = doc.Status, 
                    Creator = doc.Creator, 
                    Responsible = doc.Responsible, 
                    StatusChangeComment = doc.StatusChangeComment, 
                    PropagationPublicKey = doc.PropagationPublicKey, 
                    Description = doc.Description 
               };

            result.StatusChangeComments.Add(new ChangeStatusDocument
                    {
                        Status = doc.Status, 
                        Responsible = doc.Creator,
                        ChangeDate = doc.CreationDate // not sure it's correct
                    });
            

            result.Children.AddRange(doc.Children.ToList());
            return result;
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

        ////  public List<IObserver<CompositeInfo>> Observers { get; set; }

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
                itemToadd.Children.Add(c);
                this.QuestionHash.AddGroup(group);
                return;
            }

            /*if (c is ICompleteGroup && ((ICompleteGroup)c).PropagationPublicKey.HasValue && !parent.HasValue)
            {
                if (this.Children.Count(g => g.PublicKey.Equals(c.PublicKey)) > 0)
                {
                    c.Parent = this;
                    this.Children.Add(c);
                    return;
                }
            }

            foreach (IComposite completeGroup in this.Children)
            {
                try
                {
                    //// c.Parent = completeGroup;
                    completeGroup.Add(c, parent, null);
                    this.QuestionHash.AddGroup(c as ICompleteGroup);
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
        /// Type T.
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

        /*/// <summary>
        /// The remove.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <exception cref="CompositeException">
        /// Raises CompositeException.
        /// </exception>
        public void Remove(IComposite c)
        {
            this.RemoveInt(c);
            this.QuestionHash.RemoveGroup(c as ICompleteGroup);
        }*/

        /*/// <summary>
        /// The remove int.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <exception cref="CompositeException">
        /// </exception>
        private void RemoveInt(IComposite c)
        {
            var propogate = c as ICompleteGroup;
            if (propogate != null && propogate.PropagationPublicKey.HasValue)
            {
                bool isremoved = false;
                List<IComposite> propagatedGroups =
                    this.Children.Where(
                        g =>
                        g.PublicKey.Equals(propogate.PublicKey)
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

            foreach (IComposite completeGroup in this.Children)
            {
                try
                {
                    completeGroup.Remove(c);
                    return;
                }
                catch (CompositeException)
                {
                }
            }

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
        public void Remove(Guid publicKey, Guid? propagationKey)
        {
            IComposite forRemove = this.Children.FirstOrDefault(g => g.PublicKey.Equals(publicKey));
            if (forRemove != null && forRemove is ICompleteGroup && ((ICompleteGroup)forRemove).PropagationPublicKey.HasValue)
            {
                this.Children.Remove(forRemove);
                return;
            }

            foreach (IComposite completeGroup in this.Children)
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
            this.QuestionHash.RemoveGroup(itemToDelete);
        }

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
        /// <exception cref="NotImplementedException">
        /// </exception>
        public IComposite Clone()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}