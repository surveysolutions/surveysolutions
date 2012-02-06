using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public interface IGroup
    {
        Guid PublicKey { get; set; }
        string Title { get; set; }
        bool Propagated { get; set; }
    }

    public interface IGroup<TGroup, TQuestion> : IGroup
        where TQuestion : IQuestion
        where TGroup : IGroup
    {
        List<TQuestion> Questions { get; set; }
        List<TGroup> Groups { get; set; }
    }
    public class Group : IGroup<IGroup, IQuestion>, IComposite
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
        public bool Propagated { get; set; }
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
                Group group = c as Group;
                if (group != null)
                {
                    Groups.Add(group);
                    return;
                }
                Question question = c as Question;
                if (question != null)
                {
                    Questions.Add(question);
                    return;
                }
            }
            foreach (Group child in Groups)
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
            foreach (Question child in Questions)
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
            var group = this.Groups.FirstOrDefault(g => c is Group && g.PublicKey.Equals(((Group)c).PublicKey));
            if (group != null)
            {
                this.Groups.Remove(group);
                return;
            }
            var question = this.Questions.FirstOrDefault(g => c is Question && g.PublicKey.Equals(((Question)c).PublicKey));
            if (question != null)
            {
                this.Questions.Remove(question);
                return;
            }
            foreach (Group child in this.Groups)
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
            foreach (Question child in this.Questions)
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
            var group = this.Groups.FirstOrDefault(g => typeof(T) == typeof(Group) && g.PublicKey.Equals(publicKey));
            if (group != null)
            {
                this.Groups.Remove(group);
                return;
            }
            var question = this.Questions.FirstOrDefault(g => typeof(T) == typeof(Question) && g.PublicKey.Equals(publicKey));
            if (question != null)
            {
                this.Questions.Remove(question);
                return;
            }
            foreach (Group child in this.Groups)
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
            foreach (Question child in this.Questions)
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
            foreach (Group child in Groups)
            {
                if (child is T && child.PublicKey == publicKey)
                    return child as T;
                T subNodes = child.Find<T>(publicKey);
                if (subNodes != null)
                    return subNodes;
            }
            foreach (Question child in Questions)
            {
                if (child is T && child.PublicKey == publicKey)
                    return child as T;
                T subNodes = child.Find<T>(publicKey);
                if (subNodes != null)
                    return subNodes;
            }
            return null;
        }
    }
}
