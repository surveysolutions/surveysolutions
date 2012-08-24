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

namespace RavenQuestionnaire.Core.Documents
{
    public class CompleteQuestionnaireStoreDocument : ICompleteQuestionnaireDocument
    {
        public CompleteQuestionnaireStoreDocument()
        {
            this.CreationDate = DateTime.Now;
            this.LastEntryDate = DateTime.Now;
           // this.PublicKey = Guid.NewGuid();
            this.Children = new List<IComposite>();
          
        }

        public CompleteQuestionnaireStoreDocument(CompleteQuestionnaireDocument doc) : base()
        {
            foreach (IComposite child in doc.Children)
            {
                var question = child as IQuestion;
                if (question != null)
                {
                    this.Children.Add(question);
                    continue;
                }
                var group = child as IGroup;
                if (group != null)
                {
                    this.Children.Add(group);
                    continue;
                }
                throw new InvalidOperationException("unknown children type");
            }

        }

        public CompleteQuestionnaireStoreDocument(QuestionnaireDocument doc)
        {
            CompleteQuestionnaireStoreDocument result = new CompleteQuestionnaireStoreDocument
            {
                TemplateId = doc.Id,
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
        }


        public VisitedGroup LastVisitedGroup { get; set; }

        public UserLight Creator { get; set; }

        public string TemplateId { get; set; }

        public SurveyStatus Status { set; get; }

        public UserLight Responsible { get; set; }

        private GroupHash questionHash;
        [JsonIgnore]
        public GroupHash QuestionHash
        {
            get
            {
                if (questionHash == null)
                    questionHash = new GroupHash(this);
                return questionHash;
            }
            set { questionHash = value; }
        }

        public string StatusChangeComment { get; set; }

        #region Implementation of IQuestionnaireDocument


        public string Id { get; set; }

        public string Title { get; set; }

        public bool IsValid { get; set; }

        public string ConditionExpression { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastEntryDate { get; set; }

        public DateTime? OpenDate { get; set; }

        public DateTime? CloseDate { get; set; }

        public Guid PublicKey
        {
            get { return publicKey; }
            set
            {
                publicKey = value;
                this.Id = value.ToString();
            }
        }

        private Guid publicKey;
        [XmlIgnore]
        public Propagate Propagated
        {
            get { return Propagate.None; }
            set { }
        }

        public List<Guid> Triggers
        {
            get { return triggers; }
            set { }
        }

        private List<Guid> triggers = new List<Guid>();
        public Guid? ForcingPropagationPublicKey
        {
            get { return null; }
            set {  }
        }
      //  public List<IObserver<CompositeInfo>> Observers { get; set; }
        #endregion

        #region Implementation of IComposite

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
            //      }
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

        public void Remove(IComposite c)
        {
            ICompleteGroup propogate = c as ICompleteGroup;
            if (propogate != null && propogate.PropogationPublicKey.HasValue)
            {
                bool isremoved = false;
                var propagatedGroups = this.Children.Where(
                    g =>
                    g.PublicKey.Equals(propogate.PublicKey)  &&
                    ((ICompleteGroup)g).PropogationPublicKey==propogate.PropogationPublicKey).ToList();
                foreach (ICompleteGroup propagatableCompleteGroup in propagatedGroups)
                {
                    Children.Remove(propagatableCompleteGroup);
                    isremoved = true;
                }
                if (isremoved)
                    return;

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

        public void Remove(Guid publicKey)
        {
            
                var forRemove = this.Children.FirstOrDefault(g => g.PublicKey.Equals(publicKey));
                if (forRemove!=null && forRemove is ICompleteGroup &&((ICompleteGroup)forRemove).PropogationPublicKey.HasValue)
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

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            var resultInsideGroups = this.Children.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            if (resultInsideGroups != null)
                return resultInsideGroups;
            
            return null;
        }
        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return
              this.Children.Where(a => a is T && condition(a as T)).Select(a => a as T).Union(
                      this.Children.SelectMany(q => q.Find<T>(condition)));

        }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return Children.Where(a => a is T && condition(a as T)).Select(a => a as T).FirstOrDefault() ??
                   Children.SelectMany(q => q.Find<T>(condition)).FirstOrDefault();
        }

        public List<IComposite> Children { get; set; }


        [JsonIgnore]
        public IComposite Parent
        {
            get; set;
        }
        #endregion

        public Guid? PropogationPublicKey
        {
            get { return null; }
            set {}
        }

        public bool Enabled { get; set; }
    }
}
