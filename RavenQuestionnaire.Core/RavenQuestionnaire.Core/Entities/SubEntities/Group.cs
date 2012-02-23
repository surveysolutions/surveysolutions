using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Observers;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public enum Propagate
    {
        None,
        Propagated,
        AutoPropagated
    }

    public interface IGroup : IComposite
    {
        string Title { get; set; }
        Propagate Propagated { get; set; }
        List<Guid> Triggers { get; set; }
    }

    public interface IGroup<TGroup, TQuestion> : IGroup
        where TQuestion : IQuestion
        where TGroup : IGroup
    {
        List<TQuestion> Questions { get; set; }
        List<TGroup> Groups { get; set; }
    }
    public class Group : IGroup<IGroup, IQuestion>
    {
        public Group()
        {
            this.PublicKey = Guid.NewGuid();
            this.Observers = new List<IObserver<CompositeEventArgs>>();
            this.Questions = new List<IQuestion>();
            this.Triggers = new List<Guid>();
            this.Groups = new List<IGroup>();
          
        }

        public Group(string text)
            : this()
        {
            this.Title = text;
        }

        public Guid PublicKey { get; set; }
        public string Title { get; set; }
        public Propagate Propagated { get; set; }

        public List<Guid> Triggers { get; set; }

        public List<IQuestion> Questions
        {
            get { return questions; }
            set
            {
                questions = value;
                foreach (IQuestion completeQuestion in questions)
                {
                    this.OnAdded(new CompositeAddedEventArgs(completeQuestion));
                }
            }
        }
        private List<IQuestion> questions;
        public List<IGroup> Groups
        {
            get { return groups; }
            set
            {
                groups = value;
                foreach (IGroup completeGroup in groups)
                {
                    this.OnAdded(new CompositeAddedEventArgs(completeGroup));
                }
            }
        }
        private List<IGroup> groups;
        public void Update(string groupText)
        {
            this.Title = groupText;
        }
        public void Add(IComposite c, Guid? parent)
        {
            if ((parent.HasValue && parent.Value == PublicKey) || !parent.HasValue)
            {
                IGroup group = c as IGroup;
                if (group != null)
                {
                    Groups.Add(group);
                    OnAdded(new CompositeAddedEventArgs(group));
                //    group.Subscribe(this);
                    return;
                }
                IQuestion question = c as IQuestion;
                if (question != null)
                {
                    Questions.Add(question);
                    OnAdded(new CompositeAddedEventArgs(question));
                  //  question.Subscribe(this);
                    return;
                }
            }
            foreach (IGroup child in Groups)
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
            foreach (IQuestion child in Questions)
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

        public void Remove(IComposite c)
        {
            var group = this.Groups.FirstOrDefault(g => c is IGroup && g.PublicKey.Equals(((IGroup)c).PublicKey));
            if (group != null)
            {
                this.Groups.Remove(group);
                OnRemoved(new CompositeRemovedEventArgs(group));
                return;
            }
            var question = this.Questions.FirstOrDefault(g => c is IQuestion && g.PublicKey.Equals(((IQuestion)c).PublicKey));
            if (question != null)
            {
                this.Questions.Remove(question);
                OnRemoved(new CompositeRemovedEventArgs(question));
                return;
            }
            foreach (IGroup child in this.Groups)
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
            foreach (IQuestion child in this.Questions)
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
        public void Remove<T>(Guid publicKey) where T : class, IComposite
        {
            var group = this.Groups.FirstOrDefault(g => typeof(IGroup).IsAssignableFrom(typeof(T)) && g.PublicKey.Equals(publicKey));
            if (group != null)
            {
                this.Groups.Remove(group);
                OnRemoved(new CompositeRemovedEventArgs(group));
                return;
            }
            var question = this.Questions.FirstOrDefault(g => typeof(IQuestion).IsAssignableFrom(typeof(T)) && g.PublicKey.Equals(publicKey));
            if (question != null)
            {
                this.Questions.Remove(question);
                OnRemoved(new CompositeRemovedEventArgs(question));
                return;
            }
            foreach (IGroup child in this.Groups)
            {
                try
                {
                    child.Remove<T>(publicKey);
                    return;
                }
                catch (CompositeException)
                {

                }
            }
            foreach (IQuestion child in this.Questions)
            {
                try
                {
                    child.Remove<T>(publicKey);
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
            foreach (IGroup child in Groups)
            {
                if (child is T && child.PublicKey == publicKey)
                    return child as T;
                T subNodes = child.Find<T>(publicKey);
                if (subNodes != null)
                    return subNodes;
            }
            foreach (IQuestion child in Questions)
            {
                if (child is T && child.PublicKey == publicKey)
                    return child as T;
                T subNodes = child.Find<T>(publicKey);
                if (subNodes != null)
                    return subNodes;
            }
            return null;
        }

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return
                Questions.Where(a => a is T && condition(a as T)).Select(a => a as T).Union(
                    Groups.Where(a => a is T && condition(a as T)).Select(a => a as T)).Union(
                        Questions.SelectMany(q => q.Find<T>(condition))).Union(
                            Groups.SelectMany(g => g.Find<T>(condition)));
            /*  foreach (Group child in Groups)
            {
                if (child is T && condition(this))
                    return child as T;
                T subNodes = child.Find<T>(condition);
                if (subNodes != null)
                    return subNodes;
            }
            foreach (Question child in Questions)
            {
                if (child is T && condition(this))
                    return child as T;
                T subNodes = child.Find<T>(condition);
                if (subNodes != null)
                    return subNodes;
            }
            return null;*/
        }

        protected void OnAdded(CompositeAddedEventArgs e)
        {
            
            foreach (IObserver<CompositeEventArgs> observer in Observers)
            {
                e.AddedComposite.Subscribe(observer);
                observer.OnNext(e);
            }
        }
        protected void OnRemoved(CompositeRemovedEventArgs e)
        {
            foreach (IObserver<CompositeEventArgs> observer in Observers)
            {
                observer.OnNext(e);
            }
        }

        #region Implementation of IObservable<out CompositeEventArgs>

        public IDisposable Subscribe(IObserver<CompositeEventArgs> observer)
        {
            if (!Observers.Contains(observer))
                Observers.Add(observer);
            return new Unsubscriber<CompositeEventArgs>(Observers, observer);
        }

        public List<IObserver<CompositeEventArgs>> Observers { get; set; }

        #endregion

    }
}
