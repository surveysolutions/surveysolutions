using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Serialization;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.Observers;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class CompleteGroup : ICompleteGroup<ICompleteGroup, ICompleteQuestion>, IComposite
    {
        public CompleteGroup()
        {
            this.Questions = new List<ICompleteQuestion>();
            this.Groups = new List<ICompleteGroup>();
            this.PublicKey = Guid.NewGuid();
            this.Triggers = new List<Guid>();
            this.observers=new List<IObserver<CompositeEventArgs>>();
            //   this.iteratorContainer = new IteratorContainer();
        }

        public CompleteGroup(string name)
            : this()
        {
            this.Title = name;
        }

        public static explicit operator CompleteGroup(Group doc)
        {
            CompleteGroup result = new CompleteGroup(null)
                                       {
                                           PublicKey = doc.PublicKey,
                                           Title = doc.Title,
                                           Propagated = doc.Propagated,
                                           Triggers = doc.Triggers
                                       };

            foreach (IQuestion question in doc.Questions)
            {
                result.Questions.Add(new CompleteQuestionFactory().ConvertToCompleteQuestion(question));
            }
            foreach (IGroup group in doc.Groups)
            {
                result.Groups.Add(new CompleteGroupFactory().ConvertToCompleteGroup(group));
            }
            return result;
        }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public bool IsValid { get; set; }

        public Propagate Propagated { get; set; }

        public List<Guid> Triggers { get; set; }

        public List<ICompleteQuestion> Questions
        {
            get { return questions; }
            set
            {
                questions = value;
                foreach (ICompleteQuestion completeQuestion in questions)
                {
                    this.OnAdded(new CompositeAddedEventArgs(completeQuestion));
                }
            }
        }

        private List<ICompleteQuestion> questions;

        public List<ICompleteGroup> Groups
        {
            get { return groups; }
            set
            {
                groups = value;
                foreach (ICompleteGroup completeGroup in groups)
                {
                    this.OnAdded(new CompositeAddedEventArgs(completeGroup));
                }
            }
        }

        private List<ICompleteGroup> groups;
        // private IIteratorContainer iteratorContainer;

        public virtual void Add(IComposite c, Guid? parent)
        {
            if (!parent.HasValue || parent.Value == PublicKey)
            {
                PropagatableCompleteGroup propogateGroup = c as PropagatableCompleteGroup;
                if (propogateGroup != null)
                {
                    var group = Groups.FirstOrDefault(g => g.PublicKey.Equals(propogateGroup.PublicKey));
                    if (group != null)
                    {
                        Groups.Add(propogateGroup);
                        OnAdded(new CompositeAddedEventArgs(propogateGroup));
                        return;
                    }
                }
            }

            foreach (CompleteGroup completeGroup in Groups)
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
            IPropogate propogated = c as IPropogate;
            if (propogated != null && !(this is IPropogate))
                throw new CompositeException();
            foreach (ICompleteQuestion completeQuestion in Questions)
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

        public virtual void Remove(IComposite c)
        {

            PropagatableCompleteGroup propogate = c as PropagatableCompleteGroup;
            if (propogate != null)
            {
                bool isremoved = false;
                var propagatedGroups = this.Groups.Where(
                     g =>
                     g.PublicKey.Equals(propogate.PublicKey) && g is IPropogate &&
                     ((IPropogate)g).PropogationPublicKey.Equals(propogate.PropogationPublicKey)).ToList();
                foreach (PropagatableCompleteGroup propagatableCompleteGroup in propagatedGroups)
                {
                    Groups.Remove(propagatableCompleteGroup);
                    OnRemoved(new CompositeRemovedEventArgs(propagatableCompleteGroup));
                    isremoved = true;
                }
                if(isremoved)
                return;
                /* if (Groups.RemoveAll(
                     g =>
                     g.PublicKey.Equals(propogate.PublicKey) && g is IPropogate &&
                     ((IPropogate)g).PropogationPublicKey.Equals(propogate.PropogationPublicKey)) > 0)
                 {
                     OnRemoved(new CompositeRemovedEventArgs(null));
                     return;

                 }*/
            }
            foreach (CompleteGroup completeGroup in Groups)
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
            if (c is IPropogate && !(this is IPropogate))
                throw new CompositeException();
            foreach (ICompleteQuestion completeQuestion in Questions)
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

        public virtual void Remove<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T) == typeof(PropagatableCompleteGroup))
            {
                var forRemove = Groups.FirstOrDefault(g => g.PublicKey.Equals(publicKey));
                if (forRemove!=null)
                {
                    Groups.Remove(forRemove);
                    OnRemoved(new CompositeRemovedEventArgs(forRemove));
                    return;
                }
            }
            foreach (CompleteGroup completeGroup in Groups)
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
            foreach (ICompleteQuestion completeQuestion in Questions)
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

        public virtual T Find<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T).IsAssignableFrom(GetType()))
            {
                if (this.PublicKey.Equals(publicKey))
                    return this as T;
            }
            var resultInsideGroups =
                Groups.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            if (resultInsideGroups != null)
                return resultInsideGroups;
            var resultInsideQuestions =
                Questions.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            if (resultInsideQuestions != null)
                return resultInsideQuestions;
            return null;
        }

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return
             Questions.Where(a => a is T && condition(a as T)).Select(a => a as T).Union(
                 Groups.Where(a => a is T && condition(a as T)).Select(a => a as T)).Union(
                     Questions.SelectMany(q => q.Find<T>(condition))).Union(
                         Groups.SelectMany(g => g.Find<T>(condition)));
        }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return ((Questions.Where(a => a is T && condition(a as T)).Select(a => a as T).FirstOrDefault() ??
                 Groups.Where(a => a is T && condition(a as T)).Select(a => a as T).FirstOrDefault()) ??
                Questions.SelectMany(q => q.Find<T>(condition)).FirstOrDefault()) ??
               Groups.SelectMany(g => g.Find<T>(condition)).FirstOrDefault();
        }

        protected void OnAdded(CompositeAddedEventArgs e)
        {
            foreach (IObserver<CompositeEventArgs> observer in observers)
            {
                e.AddedComposite.Subscribe(observer);
                observer.OnNext(e);
            }
        }
        protected void OnRemoved(CompositeRemovedEventArgs e)
        {
            foreach (IObserver<CompositeEventArgs> observer in observers)
            {
                observer.OnNext(e);
            }
        }

        #region Implementation of IObservable<out CompositeEventArgs>

        public IDisposable Subscribe(IObserver<CompositeEventArgs> observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
            foreach (ICompleteQuestion completeQuestion in Questions)
            {
                completeQuestion.Subscribe(observer);
            }
            foreach (ICompleteGroup completeGroup in Groups)
            {
                completeGroup.Subscribe(observer);
            }
            return new Unsubscriber<CompositeEventArgs>(observers, observer);
        }
        private List<IObserver<CompositeEventArgs>> observers;

        #endregion
    }
}
