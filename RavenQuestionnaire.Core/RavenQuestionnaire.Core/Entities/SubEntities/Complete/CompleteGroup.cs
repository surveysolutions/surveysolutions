using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class CompleteGroup : IGroup<CompleteGroup, CompleteQuestion>
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
            CompleteGroup result;
            if (doc.Propagated)
                result = new PropagatableCompleteGroup()
                             {
                                 PublicKey = doc.PublicKey,
                                 GroupText = doc.GroupText,
                                 Propagated = true
                             };
            result = new CompleteGroup
                         {
                             PublicKey = doc.PublicKey,
                             GroupText = doc.GroupText,
                             Propagated = false
                         };
            result.Questions = doc.Questions.Select(q => (CompleteQuestion) q).ToList();
            result.Groups = doc.Groups.Select(q => (CompleteGroup) q).ToList();
            return result;
        }

        public Guid PublicKey { get; set; }

        public string GroupText { get; set; }

        public bool Propagated { get; set; }

        public List<CompleteQuestion> Questions { get; set; }

        public List<CompleteGroup> Groups { get; set; }

        public virtual bool Add(IComposite c, Guid? parent)
        {
            if (Groups.Any(child => child.Add(c, parent)))
            {
                return true;
            }
            return Questions.Any(child => child.Add(c, parent));
        }

        public bool Remove(IComposite c)
        {
            if (Groups.Any(child => child.Remove(c)))
            {
                return true;
            }
            return Questions.Any(child => child.Remove(c));
        }

        public bool Remove<T>(Guid publicKey) where T : class, IComposite
        {
            if (Groups.Any(child => child.Remove<T>(publicKey)))
            {
                return true;
            }
            return Questions.Any(child => child.Remove<T>(publicKey));
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T) == GetType())
            {
                if (this.PublicKey.Equals(publicKey))
                    return this as T;
            }
            var resultInsideGroups = Groups.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            if (resultInsideGroups != null)
                return resultInsideGroups;
            var resultInsideQuestions = Questions.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            if (resultInsideQuestions != null)
                return resultInsideQuestions;
          /*  foreach (CompleteGroup child in Groups)
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
            }*/
            return null;
        }

      
    }
}
