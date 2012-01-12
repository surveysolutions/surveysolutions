using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class CompleteGroup: IGroup<CompleteGroup, CompleteQuestion>
    {
        public CompleteGroup()
        {
            Questions= new List<CompleteQuestion>();
            Groups = new List<CompleteGroup>();
        }
        public CompleteGroup(string name):this()
        {
            this.GroupText = name;
        }
        public static explicit operator CompleteGroup(Group doc)
        {
            CompleteGroup result = new CompleteGroup
            {
                PublicKey = doc.PublicKey,
                GroupText = doc.GroupText
            };
            result.Questions = doc.Questions.Select(q => (CompleteQuestion) q).ToList();
            result.Groups = doc.Groups.Select(q => (CompleteGroup)q).ToList();
            return result;
        }
        public Guid PublicKey { get; set; }

        public string GroupText { get; set; }

        public List<CompleteQuestion> Questions { get; set; }
        public List<CompleteGroup> Groups { get; set; }


        public new bool Add(IComposite c, Guid? parent)
        {
           /* if (!parent.HasValue || parent.Value == PublicKey)
            {
                CompleteGroup group = c as CompleteGroup;
                if (group != null)
                {
                    Groups.Add(group);
                    return true;
                }
                CompleteQuestion question = c as CompleteQuestion;
                if (question != null)
                {
                    Questions.Add(question);
                    return true;
                }
                if (!parent.HasValue)
                    return false;
            }*/
            foreach (CompleteGroup child in Groups)
            {
                if (child.Add(c, parent))
                    return true;
            }
            foreach (CompleteQuestion child in Questions)
            {
                if (child.Add(c, parent))
                    return true;
            }
            return false;
        }

        public new bool Remove(IComposite c)
        {
            foreach (CompleteGroup child in Groups)
            {
              /*  if (child == c)
                {
                    Groups.Remove(child);
                    return true;
                }*/
                if (child.Remove(c))
                    return true;
            }
            foreach (CompleteQuestion child in Questions)
            {
                if (child == c)
                {
                    child.Answers.ForEach(a => a.Reset());
                    return true;
                }
                if (child.Remove(c))
                    return true;
            }
            return false;
        }
        public new bool Remove<T>(Guid publicKey) where T : class, IComposite
        {
            foreach (CompleteGroup child in Groups)
            {
               /* if (child.PublicKey == publicKey)
                {
                    Groups.Remove(child);
                    return true;
                }*/
                if (child.Remove<T>(publicKey))
                    return true;
            }
            foreach (CompleteQuestion child in Questions)
            {
                if (child.PublicKey == publicKey)
                {
                    child.Answers.ForEach(a => a.Reset());
                    return true;
                }
                if (child.Remove<T>(publicKey))
                    return true;
            }
            return false;
        }

        public new T Find<T>(Guid publicKey) where T : class, IComposite
        {
            foreach (CompleteGroup child in Groups)
            {
                if (child is T && child.PublicKey == publicKey)
                    return child as T;
                T subNodes = child.Find<T>(publicKey);
                if (subNodes != null)
                    return subNodes;
            }
            foreach (CompleteQuestion child in Questions)
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
