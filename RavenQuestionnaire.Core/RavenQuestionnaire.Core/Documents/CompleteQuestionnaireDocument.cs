using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Documents
{
    public interface ICompleteQuestionnaireDocument: IQuestionnaireDocument, ICompleteGroup
    {
         UserLight Creator { get; set; }
         string TemplateId { get; set; }
         SurveyStatus Status { set; get; }
         UserLight Responsible { get; set; }
    }


    public class CompleteQuestionnaireDocument : ICompleteQuestionnaireDocument
    {
        public CompleteQuestionnaireDocument()
        {
            this.CreationDate = DateTime.Now;
            this.LastEntryDate = DateTime.Now;
            this.PublicKey = Guid.NewGuid();
            this.compositeobservers = new List<IObserver<CompositeEventArgs>>();
            this.Children = new List<IComposite>();
        }
        public static explicit operator CompleteQuestionnaireDocument(QuestionnaireDocument doc)
        {
            CompleteQuestionnaireDocument result = new CompleteQuestionnaireDocument
            {
                TemplateId = doc.Id,
                Title = doc.Title,
                Triggers = doc.Triggers
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
       
        public UserLight Creator { get; set; }

        public string TemplateId { get; set; }

        public SurveyStatus Status { set; get; }

        public UserLight Responsible { get; set; }

        public string StatusChangeComment { get; set; }

        #region Implementation of IQuestionnaireDocument


        public string Id { get; set; }

        public string Title { get; set; }

        public bool IsValid { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastEntryDate { get; set; }

        public DateTime? OpenDate { get; set; }

        public DateTime? CloseDate { get; set; }

        public Guid PublicKey { get; set; }
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
            if (c is PropagatableCompleteGroup && !parent.HasValue)
            {
                if (this.Children.Count(g => g.PublicKey.Equals(c.PublicKey)) > 0)
                {
                    this.Children.Add(c);
                    OnAdded(new CompositeAddedEventArgs(c));
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
            PropagatableCompleteGroup propogate = c as PropagatableCompleteGroup;
            if (propogate != null)
            {
                bool isremoved = false;
                var propagatedGroups = this.Children.Where(
                    g =>
                    g.PublicKey.Equals(propogate.PublicKey) && g is IPropogate &&
                    ((IPropogate)g).PropogationPublicKey.Equals(propogate.PropogationPublicKey)).ToList();
                foreach (PropagatableCompleteGroup propagatableCompleteGroup in propagatedGroups)
                {
                    Children.Remove(propagatableCompleteGroup);
                    OnRemoved(new CompositeRemovedEventArgs(propagatableCompleteGroup));
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
                if (forRemove!=null && forRemove is PropagatableCompleteGroup)
                {
                    this.Children.Remove(forRemove);
                    OnRemoved(new CompositeRemovedEventArgs(forRemove));
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
            get { throw new NotImplementedException(); }
        }

        protected void OnAdded(CompositeAddedEventArgs e)
        {
            foreach (IObserver<CompositeEventArgs> observer in compositeobservers)
            {
                e.AddedComposite.Subscribe(observer);
                observer.OnNext(e);
            }
        }
        protected void OnRemoved(CompositeRemovedEventArgs e)
        {
            foreach (IObserver<CompositeEventArgs> observer in compositeobservers)
            {
                observer.OnNext(e);
            }
        }

        #region Implementation of IObservable<out CompositeEventArgs>

        public IDisposable Subscribe(IObserver<CompositeEventArgs> observer)
        {
            if (!compositeobservers.Contains(observer))
                compositeobservers.Add(observer);
            foreach (IComposite completeQuestion in Children)
            {
                completeQuestion.Subscribe(observer);
            }
            return new Unsubscriber<CompositeEventArgs>(compositeobservers, observer);
        }
        private List<IObserver<CompositeEventArgs>> compositeobservers;
        #endregion
        #endregion
    }
}
