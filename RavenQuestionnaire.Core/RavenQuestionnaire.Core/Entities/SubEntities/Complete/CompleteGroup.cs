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
using RavenQuestionnaire.Core.Utility;

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
        public CompleteGroup(ICompleteGroup group, Guid propogationPublicKey)
            : this()
        {
            this.Title = group.Title;
            this.Propagated = group.Propagated;
            this.PublicKey = group.PublicKey;

            for (int i = 0; i < group.Children.Count; i++)
            {
                var question = group.Children[i] as ICompleteQuestion;
                if (question != null)
                {
                    var newQuestion = new CompleteQuestionFactory().ConvertToCompleteQuestion(question);
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
                        this.Children.Add((BindedCompleteQuestion)newQuestion);
                    continue;
                    
                }
                var groupChild = group.Children[i] as ICompleteGroup;
                if(groupChild!=null)
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
            if (!parent.HasValue || parent.Value == PublicKey )
            {
                ICompleteGroup propogateGroup = c as ICompleteGroup;
                if (propogateGroup != null && propogateGroup.PropogationPublicKey.HasValue)
                {
                    var group = Children.FirstOrDefault(g => g.PublicKey==propogateGroup.PublicKey);
                    if (group != null)
                    {
                        Children.Add(propogateGroup);
                        OnAdded(new CompositeAddedEventArgs(propogateGroup));
                        return;
                    }
                }
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

        public virtual void Remove(IComposite c)
        {

            ICompleteGroup propogate = c as ICompleteGroup;
            if (propogate != null && propogate.PropogationPublicKey.HasValue)
            {
                bool isremoved = false;
                var propagatedGroups = this.Children.Where(
                     g =>
                     g.PublicKey==propogate.PublicKey && g is ICompleteGroup &&
                     ((ICompleteGroup)g).PropogationPublicKey == propogate.PropogationPublicKey).ToList();
                foreach (ICompleteGroup propagatableCompleteGroup in propagatedGroups)
                {
                    Children.Remove(propagatableCompleteGroup);
                    OnRemoved(new CompositeRemovedEventArgs(propagatableCompleteGroup));
                    isremoved = true;
                }
                if(isremoved)
                return;
            }
            foreach (IComposite child in Children)
            {
                try
                {
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
            if (forRemove != null && forRemove is ICompleteGroup && ((ICompleteGroup)forRemove).PropogationPublicKey.HasValue)
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

        public Guid? PropogationPublicKey { get; set; }
    }
}
