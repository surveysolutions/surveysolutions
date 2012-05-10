using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.Observers;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class CompleteGroup : ICompleteGroup
    {
        public CompleteGroup()
        {
            this.Children = new List<IComposite>();
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

        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public bool IsValid { get; set; }

        public Propagate Propagated { get; set; }

        public List<Guid> Triggers { get; set; }

       
        // private IIteratorContainer iteratorContainer;

        public virtual void Add(IComposite c, Guid? parent)
        {
            if (!parent.HasValue || parent.Value == PublicKey)
            {
                PropagatableCompleteGroup propogateGroup = c as PropagatableCompleteGroup;
                if (propogateGroup != null)
                {
                    var group = Children.FirstOrDefault(g => g.PublicKey.Equals(propogateGroup.PublicKey));
                    if (group != null)
                    {
                        Children.Add(propogateGroup);
                        OnAdded(new CompositeAddedEventArgs(propogateGroup));
                        return;
                    }
                }
            }
            IPropogate propogated = c as IPropogate;
            foreach (IComposite child in this.Children)
            {
                try
                {
                    if (propogated != null && !(child is IPropogate))
                        continue;
                    child.Add(c, parent);
                    return;
                }
                catch (CompositeException)
                {
                }
            }
           /* IPropogate propogated = c as IPropogate;
            if (propogated != null && !(this is IPropogate))
                throw new CompositeException();*/
            //foreach (ICompleteQuestion completeQuestion in Questions)
            //{
            //    try
            //    {
            //        completeQuestion.Add(c, parent);
            //        return;
            //    }
            //    catch (CompositeException)
            //    {
            //    }
            //}
            throw new CompositeException();
        }

        public virtual void Remove(IComposite c)
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
                if(isremoved)
                return;
            }
            IPropogate propogated = c as IPropogate;
            foreach (IComposite child in Children)
            {
                try
                {
                    if (propogated != null && !(child is IPropogate))
                        continue;
                    child.Remove(c);
                    return;
                }
                catch (CompositeException)
                {
                }
            }
            throw new CompositeException();
        }

        public virtual void Remove(Guid publicKey)
        {

            var forRemove = Children.FirstOrDefault(g => g.PublicKey.Equals(publicKey));
            if (forRemove != null && forRemove is PropagatableCompleteGroup)
            {

                Children.Remove(forRemove);
                OnRemoved(new CompositeRemovedEventArgs(forRemove));
                return;
            }

            foreach (CompleteGroup completeGroup in Children)
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

        public virtual T Find<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof (T).IsAssignableFrom(GetType()))
            {
                if (this.PublicKey.Equals(publicKey))
                    return this as T;
            }
            var resultInsideGroups =
                Children.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            if (resultInsideGroups != null)
                return resultInsideGroups;

            return null;
        }

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return
                Children.Where(a => a is T && condition(a as T)).Select(a => a as T).Union(
                    Children.SelectMany(q => q.Find<T>(condition)));
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
            foreach (IComposite completeQuestion in Children)
            {
                completeQuestion.Subscribe(observer);
            }
            return new Unsubscriber<CompositeEventArgs>(observers, observer);
        }
        private List<IObserver<CompositeEventArgs>> observers;

        #endregion
    }
}
