using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

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
        Guid PublicKey { get; set; }
        string Title { get; set; }
        Propagate Propagated { get; set; }
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
            this.Questions = new List<IQuestion>();
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
        public List<IQuestion> Questions { get; set; }
        public List<IGroup> Groups { get; set; }
        public void Update(string groupText)
        {
            this.Title = groupText;
        }
        public void Add(IComposite c, Guid? parent)
        {
            if (parent.HasValue && parent.Value == PublicKey)
            {
                IGroup group = c as IGroup;
                if (group != null)
                {
                    Groups.Add(group);
                    return;
                }
                IQuestion question = c as IQuestion;
                if (question != null)
                {
                    Questions.Add(question);
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
                return;
            }
            var question = this.Questions.FirstOrDefault(g => c is IQuestion && g.PublicKey.Equals(((IQuestion)c).PublicKey));
            if (question != null)
            {
                this.Questions.Remove(question);
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
                return;
            }
            var question = this.Questions.FirstOrDefault(g => typeof(IQuestion).IsAssignableFrom(typeof(T)) && g.PublicKey.Equals(publicKey));
            if (question != null)
            {
                this.Questions.Remove(question);
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

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class, IComposite
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
    }
}
