using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class CompleteGroup : ICompleteGroup<CompleteGroup, CompleteQuestion>
    {
        public CompleteGroup()
        {
            Questions = new List<CompleteQuestion>();
            Groups = new List<CompleteGroup>();
        }

        public CompleteGroup(string name)
            : this()
        {
            this.GroupText = name;
        }

        public static explicit operator CompleteGroup(Group doc)
        {
            /*CompleteGroup result;
            if (doc.Propagated)
                result = new PropagatableCompleteGroup()
                             {
                                 PublicKey = doc.PublicKey,
                                 GroupText = doc.GroupText,
                                 Propagated = true
                             };*/
            CompleteGroup result = new CompleteGroup
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
            IPropogate propogated = c as IPropogate;
            if (propogated != null && !(this is IPropogate))
                return false;
            if (!parent.HasValue || parent.Value == PublicKey)
            {
                CompleteGroup propogate = c as CompleteGroup;
                if (propogate != null && propogate.Propagated)
                {
                    Groups.Add(new PropagatableCompleteGroup(propogate, Guid.NewGuid()));
                    return true;
                }
            }
            if (Groups.Any(child => child.Add(c, parent)))
            {
                return true;
            }
            return Questions.Any(child => child.Add(c, parent));
        }

        public virtual bool Remove(IComposite c)
        {
            PropagatableCompleteGroup propogate = c as PropagatableCompleteGroup;
            if (propogate != null)
            {
                Groups.RemoveAll(
                    g =>
                    g.PublicKey.Equals(propogate.PublicKey) &&
                    ((IPropogate) g).PropogationPublicKey.Equals(propogate.PropogationPublicKey));
            }
            if (Groups.Any(child => child.Remove(c)))
            {
                return true;
            }
            return Questions.Any(child => child.Remove(c));
        }

        public virtual bool Remove<T>(Guid publicKey) where T : class, IComposite
        {
            if(typeof(T)== typeof(PropagatableCompleteGroup))
            {
                Groups.RemoveAll(
                   g =>
                   g.PublicKey.Equals(publicKey));
            }
            if (Groups.Any(child => child.Remove<T>(publicKey)))
            {
                return true;
            }
            return Questions.Any(child => child.Remove<T>(publicKey));
        }

        public virtual T Find<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof (T) == GetType())
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

    }
}
