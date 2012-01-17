using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public interface IGroup: IComposite
    {
        Guid PublicKey { get; set; }
        string GroupText { get; set; }
        bool Propagated { get; set; }
    }

    public interface IGroup<TGroup, TQuestion> : IGroup
        where TQuestion : IQuestion
        where TGroup : IGroup
    {
        List<TQuestion> Questions { get; set; }
        List<TGroup> Groups { get; set; }
    }
    public class Group : IGroup<Group, Question>
    {
        public Group()
        {
            this.PublicKey = Guid.NewGuid();
            this.Questions= new List<Question>();
            this.Groups = new List<Group>();
        }
        public Group(string text):this()
        {
            this.GroupText = text;
        }

        public Guid PublicKey { get; set; }
        public string GroupText { get; set; }
        public bool Propagated { get; set; }
        public List<Question> Questions  { get; set; }
        public List<Group> Groups { get; set; }
        public void Update(string groupText)
        {
            this.GroupText = groupText;
        }
        public bool Add(IComposite c, Guid? parent)
        {
            if (!parent.HasValue || parent.Value == PublicKey)
            {
                Group group = c as Group;
                if (group != null)
                {
                    Groups.Add(group);
                    return true;
                }
                Question question = c as Question;
                if (question != null)
                {
                    Questions.Add(question);
                    return true;
                }
                if (!parent.HasValue)
                    return false;
            }
            foreach (Group child in Groups)
            {
                if (child.Add(c, parent))
                    return true;
            }
            foreach (Question child in Questions)
            {
                if (child.Add(c, parent))
                    return true;
            }
            return false;
        }

        public bool Remove(IComposite c)
        {
            foreach (Group child in Groups)
            {
                if (child == c)
                {
                    Groups.Remove(child);
                    return true;
                }
                if (child.Remove(c))
                    return true;
            }
            foreach (Question child in Questions)
            {
                if (child == c)
                {
                    Questions.Remove(child);
                    return true;
                }
                if (child.Remove(c))
                    return true;
            }
            return false;
        }
        public bool Remove<T>(Guid publicKey) where T : class, IComposite
        {
            foreach (Group child in Groups)
            {
                if (child.PublicKey == publicKey)
                {
                    Groups.Remove(child);
                    return true;
                }
                if (child.Remove<T>(publicKey))
                    return true;
            }
            foreach (Question child in Questions)
            {
                if (child.PublicKey == publicKey)
                {
                    Questions.Remove(child);
                    return true;
                }
                if (child.Remove<T>(publicKey))
                    return true;
            }
            return false;
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
