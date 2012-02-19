using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml.Serialization;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.Observers;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Documents
{
    public interface ICompleteQuestionnaireDocument<TGroup, TQuestion> : IQuestionnaireDocument<TGroup, TQuestion>, ICompleteGroup<TGroup, TQuestion>
        where TQuestion : ICompleteQuestion
        where TGroup : ICompleteGroup
    {
         UserLight Creator { get; set; }

         string TemplateId { get; set; }

         SurveyStatus Status { set; get; }

         UserLight Responsible { get; set; }

         string StatusChangeComment { get; set; }
    }


    public class CompleteQuestionnaireDocument : ICompleteQuestionnaireDocument<ICompleteGroup, ICompleteQuestion>
    {
        public CompleteQuestionnaireDocument()
        {
            this.CreationDate = DateTime.Now;
            this.LastEntryDate = DateTime.Now;

            this.compositeobservers = new List<IObserver<CompositeEventArgs>>();
            this.Questions = new ObservableCollectionS<ICompleteQuestion>();
         

            this.Groups=new ObservableCollectionS<ICompleteGroup>();
           
            
            this.Observers = new List<IObserver<CompositeInfo>>();
            
            SubscribeBindedQuestions();
        }
        public static explicit operator CompleteQuestionnaireDocument(QuestionnaireDocument doc)
        {
            CompleteQuestionnaireDocument result = new CompleteQuestionnaireDocument
            {
                TemplateId = doc.Id,
                Title = doc.Title
            };
            foreach (IQuestion question in doc.Questions)
            {
                result.Questions.Add(new CompleteQuestionFactory().ConvertToCompleteQuestion(question));
            }
            foreach (IGroup group in doc.Groups)
            {
                result.Groups.Add(new CompleteGroupFactory().ConvertToCompleteGroup(group));
            }
            result.Observers = doc.Observers;
            return result;
        }


        protected void SubscribeBindedQuestions()
        {
            var addAnswers = from q in this.GetAllAnswerAddedEvents()
                             let question =
                                 ((CompositeAddedEventArgs) q.ParentEvent).AddedComposite as
                                 ICompleteQuestion
                             let binded =
                                 this.GetAllBindedQuestions(question.PublicKey)
                             where binded.Any()
                             select q;
            addAnswers
                .Subscribe(Observer.Create<CompositeAddedEventArgs>(
                    BindQuestion));
        }

        protected void BindQuestion(CompositeAddedEventArgs e)
        {
            var template = ((CompositeAddedEventArgs) e.ParentEvent).AddedComposite as ICompleteQuestion;

            if (template == null)
                return;
            var propagatedTemplate = template as IPropogate;
            IEnumerable<BindedCompleteQuestion> binded;
            if (propagatedTemplate == null)
            {
                binded =
                    this.GetAllBindedQuestions(template.PublicKey);
            }
            else
            {
                binded = this.GetPropagatedGroupsByKey(propagatedTemplate.PropogationPublicKey).SelectMany(
                    pg => pg.GetAllBindedQuestions(template.PublicKey));
            }
            foreach (BindedCompleteQuestion bindedCompleteQuestion in binded)
            {
                bindedCompleteQuestion.Copy(template);
            }

        }

        public UserLight Creator { get; set; }

        public string TemplateId { get; set; }

        public SurveyStatus Status { set; get; }

        public UserLight Responsible { get; set; }

        public string StatusChangeComment { get; set; }

        #region Implementation of IQuestionnaireDocument

        public ObservableCollectionS<ICompleteQuestion> Questions
        {
            get { return questions; }
            set
            {
                questions = value;
                questions.GetObservableAddedValues().Subscribe(q => this.OnAdded(new CompositeAddedEventArgs(q)));
                questions.GetObservableRemovedValues().Subscribe(
                    q => OnRemoved(new CompositeRemovedEventArgs(null)));
                foreach (ICompleteQuestion completeQuestion in questions)
                {
                    this.OnAdded(new CompositeAddedEventArgs(completeQuestion));
                }
            }
        }

        private ObservableCollectionS<ICompleteQuestion> questions;

        public ObservableCollectionS<ICompleteGroup> Groups
        {
            get { return groups; }
            set
            {
                groups = value;
                this.Groups.GetObservableAddedValues().Subscribe(g => this.OnAdded(new CompositeAddedEventArgs(g)));
                this.Groups.GetObservableRemovedValues().Subscribe(
                    q => OnRemoved(new CompositeRemovedEventArgs(null)));
                foreach (ICompleteGroup completeGroup in groups)
                {
                    this.OnAdded(new CompositeAddedEventArgs(completeGroup));
                }
            }
        }

        private ObservableCollectionS<ICompleteGroup> groups;

        public string Id { get; set; }

        public string Title { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastEntryDate { get; set; }

        public DateTime? OpenDate { get; set; }

        public DateTime? CloseDate { get; set; }

        [XmlIgnore]
        public Guid PublicKey { get; set; }
        [XmlIgnore]
        public Propagate Propagated
        {
            get { return Propagate.None; }
            set { }
        }

        public Guid? ForcingPropagationPublicKey
        {
            get { return null; }
            set {  }
        }
        public List<IObserver<CompositeInfo>> Observers { get; set; }
        #endregion

        #region Implementation of IComposite

        public virtual void Add(IComposite c, Guid? parent)
        {
            ICompleteGroup group = c as ICompleteGroup;
            /*  if (group != null && group.Propagated)
              {
                  if (!(group is PropagatableCompleteGroup))
                      c = new PropagatableCompleteGroup(group, Guid.NewGuid());
  */
            if (group != null && group is IPropogate && !parent.HasValue)
            {
                if (this.Groups.Count(g => g.PublicKey.Equals(group.PublicKey)) > 0)
                {
                    this.Groups.Add(group);
                    OnAdded(new CompositeAddedEventArgs(group));
                    return;
                }
            }
            //      }
            foreach (CompleteGroup completeGroup in this.Groups)
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
            foreach (CompleteQuestion completeQuestion in this.Questions)
            {
                try
                {
                    completeQuestion.Add(c, parent);
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
                if (
               this.Groups.RemoveAll(g => g.PublicKey.Equals(propogate.PublicKey) && g is IPropogate &&
                     ((IPropogate)g).PropogationPublicKey.Equals(propogate.PropogationPublicKey)) > 0)
                {
                    OnRemoved(new CompositeRemovedEventArgs(null));
                    return;
                }

                /*bool deleted = false;
                var toRemove = this.Groups.Where(g =>
                                                 g.PublicKey.Equals(propogate.PublicKey) && g is IPropogate &&
                                                 ((IPropogate) g).PropogationPublicKey.Equals(
                                                     propogate.PropogationPublicKey));
                foreach (ICompleteGroup completeGroup in toRemove)
                {
                    Groups.Remove(completeGroup);
                    OnRemoved(new CompositeRemovedEventArgs(completeGroup));
                    deleted = true;
                }
                if(deleted)
                    return;*/
            }
            foreach (CompleteGroup completeGroup in this.Groups)
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
            foreach (CompleteQuestion completeQuestion in this.Questions)
            {
                try
                {
                    completeQuestion.Remove(c);
                    return;
                }
                catch (CompositeException)
                {
                }
            }
            throw new CompositeException();
        }

        public void Remove<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T) == typeof(PropagatableCompleteGroup))
            {
                var forRemove = this.Groups.FirstOrDefault(g => g.PublicKey.Equals(publicKey));
                if (forRemove!=null)
                {
                    this.Groups.Remove(forRemove);
                    OnRemoved(new CompositeRemovedEventArgs(forRemove));
                    return;
                }
            }
            foreach (CompleteGroup completeGroup in this.Groups)
            {
                try
                {
                    completeGroup.Remove<T>(publicKey);
                    return;
                }
                catch (CompositeException)
                {
                }
            }
            foreach (CompleteQuestion completeQuestion in this.Questions)
            {
                try
                {
                    completeQuestion.Remove<T>(publicKey);
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
            var resultInsideGroups = this.Groups.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            if (resultInsideGroups != null)
                return resultInsideGroups;
            var resultInsideQuestions = this.Questions.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            if (resultInsideQuestions != null)
                return resultInsideQuestions;
            return null;
        }
        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return
              this.Questions.Where(a => a is T && condition(a as T)).Select(a => a as T).Union(
                  this.Groups.Where(a => a is T && condition(a as T)).Select(a => a as T)).Union(
                      this.Questions.SelectMany(q => q.Find<T>(condition))).Union(
                          this.Groups.SelectMany(g => g.Find<T>(condition)));

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
            foreach (ICompleteQuestion completeQuestion in Questions)
            {
                completeQuestion.Subscribe(observer);
            }
            foreach (ICompleteGroup completeGroup in Groups)
            {
                completeGroup.Subscribe(observer);
            }
            return new Unsubscriber<CompositeEventArgs>(compositeobservers, observer);
        }
        private List<IObserver<CompositeEventArgs>> compositeobservers;
        #endregion
        #endregion
    }
}
