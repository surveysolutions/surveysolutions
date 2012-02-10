using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Serialization;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.Observers;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class CompleteGroup : ICompleteGroup<ICompleteGroup, ICompleteQuestion>, IComposite
    {
        public CompleteGroup()
        {
            Questions = new List<ICompleteQuestion>();
            Groups = new List<ICompleteGroup>();
            this.PublicKey = Guid.NewGuid();
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
            result.Questions =
               doc.Questions.Select(q => new CompleteQuestionFactory().ConvertToCompleteQuestion(q)).ToList();
            result.Groups =
                doc.Groups.Select(q => new CompleteGroupFactory().ConvertToCompleteGroup(q)).ToList();
            return result;
        }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public Propagate Propagated { get; set; }
        
        public List<ICompleteQuestion> Questions { get; set; }

        public List<ICompleteGroup> Groups { get; set; }
        [XmlIgnore]
        public Iterator<ICompleteAnswer> AnswerIterator
        {
            get { return new QuestionnaireAnswerIterator(this); }
        }
        // private IIteratorContainer iteratorContainer;

        public virtual void Add(IComposite c, Guid? parent)
        {
            if (!parent.HasValue || parent.Value == PublicKey)
            {
                PropagatableCompleteGroup propogateGroup = c as PropagatableCompleteGroup;
                if (propogateGroup != null)
                {
                    var group = Groups.FirstOrDefault(g => g.PublicKey.Equals(propogateGroup.PublicKey));
                    if (group != null)
                    {
                        Groups.Add(propogateGroup);
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
            foreach (ICompleteQuestion completeQuestion in Questions)
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
                    ((IPropogate)g).PropogationPublicKey.Equals(propogate.PropogationPublicKey)) > 0)
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
            foreach (ICompleteQuestion completeQuestion in Questions)
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
            foreach (ICompleteQuestion completeQuestion in Questions)
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
            if (typeof(T) == GetType())
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

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class, IComposite
        {
            return
             Questions.Where(a => a is T && condition(a as T)).Select(a => a as T).Union(
                 Groups.Where(a => a is T && condition(a as T)).Select(a => a as T)).Union(
                     Questions.SelectMany(q => q.Find<T>(condition))).Union(
                         Groups.SelectMany(g => g.Find<T>(condition)));
        }
    }
}
