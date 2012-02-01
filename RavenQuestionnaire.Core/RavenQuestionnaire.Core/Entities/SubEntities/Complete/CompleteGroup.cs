using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Iterators;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class CompleteGroup : ICompleteGroup<CompleteGroup, CompleteQuestion>,IComposite
    {
        public CompleteGroup()
        {
            Questions = new List<CompleteQuestion>();
            Groups = new List<CompleteGroup>();
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
                             Propagated = doc.Propagated
                         };
            result.Questions = doc.Questions.Select(q => (CompleteQuestion) q).ToList();
            result.Groups = doc.Groups.Select(q => (CompleteGroup) q).ToList();
            return result;
        }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public bool Propagated { get; set; }

        public List<CompleteQuestion> Questions { get; set; }

        public List<CompleteGroup> Groups { get; set; }

        public Iterator<CompleteAnswer> AnswerIterator
        {
            get { return new QuestionnaireAnswerIterator(this); }
        }
       // private IIteratorContainer iteratorContainer;

        public virtual void Add(IComposite c, Guid? parent)
        {
            if (!parent.HasValue || parent.Value == PublicKey)
            {
                CompleteGroup propogate = c as CompleteGroup;
                if (propogate != null && propogate.Propagated)
                {
                    var group = Groups.FirstOrDefault(g => g.PublicKey.Equals(propogate.PublicKey));
                    if (group != null)
                    {
                        Groups.Add(new PropagatableCompleteGroup(propogate, Guid.NewGuid()));
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
            foreach (CompleteQuestion completeQuestion in Questions)
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
                if (Groups.RemoveAll(
                    g =>
                    g.PublicKey.Equals(propogate.PublicKey) && g is IPropogate &&
                    ((IPropogate) g).PropogationPublicKey.Equals(propogate.PropogationPublicKey)) > 0)
                    return;
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
            foreach (CompleteQuestion completeQuestion in Questions)
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
                if (Groups.RemoveAll(
                    g =>
                    g.PublicKey.Equals(publicKey)) > 0)
                    return;
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
            foreach (CompleteQuestion completeQuestion in Questions)
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
