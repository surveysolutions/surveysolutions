// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireDocument.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the ICompleteQuestionnaireDocument type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Documents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;

    using Newtonsoft.Json;

    using RavenQuestionnaire.Core.AbstractFactories;
    using RavenQuestionnaire.Core.Entities.Composite;
    using RavenQuestionnaire.Core.Entities.Extensions;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The CompleteQuestionnaireDocument interface.
    /// </summary>
    public interface ICompleteQuestionnaireDocument : IQuestionnaireDocument, ICompleteGroup
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the creator.
        /// </summary>
        UserLight Creator { get; set; }

        /// <summary>
        /// Gets or sets the question hash.
        /// </summary>
        GroupHash QuestionHash { get; set; }

        /// <summary>
        /// Gets or sets the responsible.
        /// </summary>
        UserLight Responsible { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        SurveyStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the template id.
        /// </summary>
        Guid TemplateId { get; set; }

        #endregion
    }

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
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        public bool Enabled { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether is valid.
        /// </summary>
        public bool IsValid { get; set; }

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
        public Guid? PropogationPublicKey
        {
            get
            {
                return null;
            }

            set
            {
               
            }
        }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the question hash.
        /// </summary>
        [JsonIgnore]
        public GroupHash QuestionHash
        {
            get
            {
                if (this.questionHash == null)
                {
                    this.questionHash = new GroupHash(this);
                }

                return this.questionHash;
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
        public static explicit operator CompleteQuestionnaireDocument(QuestionnaireDocument doc)
        {
            var result = new CompleteQuestionnaireDocument
                {
                    TemplateId = doc.PublicKey, 
                    Title = doc.Title, 
                    Triggers = doc.Triggers, 
                    ConditionExpression = doc.ConditionExpression
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
        public virtual void Add(IComposite c, Guid? parent)
        {
            if (c is ICompleteGroup && ((ICompleteGroup)c).PropogationPublicKey.HasValue && !parent.HasValue)
            {
                if (this.Children.Count(g => g.PublicKey.Equals(c.PublicKey)) > 0)
                {
                    this.Children.Add(c);
                    return;
                }
            }

            // }
            foreach (IComposite completeGroup in this.Children)
            {
                try
                {
                    completeGroup.Add(c, parent);
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
            var propogate = c as ICompleteGroup;
            if (propogate != null && propogate.PropogationPublicKey.HasValue)
            {
                bool isremoved = false;
                List<IComposite> propagatedGroups =
                    this.Children.Where(
                        g =>
                        g.PublicKey.Equals(propogate.PublicKey)
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
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <exception cref="CompositeException">
        /// Raises CompositeException.
        /// </exception>
        public void Remove(Guid publicKey)
        {
            IComposite forRemove = this.Children.FirstOrDefault(g => g.PublicKey.Equals(publicKey));
            if (forRemove != null && forRemove is ICompleteGroup
                && ((ICompleteGroup)forRemove).PropogationPublicKey.HasValue)
            {
                this.Children.Remove(forRemove);
                return;
            }

            foreach (IComposite completeGroup in this.Children)
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