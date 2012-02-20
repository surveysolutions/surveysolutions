#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Iterators;

#endregion

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class CompleteGroup : ICompleteGroup<ICompleteGroup, ICompleteQuestion>, IComposite
    {
        public CompleteGroup()
        {
            Questions = new List<ICompleteQuestion>();
            Groups = new List<ICompleteGroup>();
            PublicKey = Guid.NewGuid();
            //   this.iteratorContainer = new IteratorContainer();
        }

        public CompleteGroup(string name)
            : this()
        {
            Title = name;
        }

        [XmlIgnore]
        public Iterator<ICompleteAnswer> AnswerIterator
        {
            get { return new QuestionnaireAnswerIterator(this); }
        }

        #region ICompleteGroup<ICompleteGroup,ICompleteQuestion> Members

        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public Propagate Propagated { get; set; }

        public List<ICompleteQuestion> Questions { get; set; }

        public List<ICompleteGroup> Groups { get; set; }

        // private IIteratorContainer iteratorContainer;

        public virtual void Add(IComposite c, Guid? parent)
        {
            if (!parent.HasValue || parent.Value == PublicKey)
            {
                var propogateGroup = c as PropagatableCompleteGroup;
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
            var propogated = c as IPropogate;
            if (propogated != null && !(this is IPropogate))
                throw new CompositeException();
            foreach (var completeQuestion in Questions)
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
            var propogate = c as PropagatableCompleteGroup;
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
            foreach (var completeQuestion in Questions)
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
            if (typeof (T) == typeof (PropagatableCompleteGroup))
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
            foreach (var completeQuestion in Questions)
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
                if (PublicKey.Equals(publicKey))
                    return this as T;
            }
            var resultInsideGroups =
                Groups.Where(a => a is IComposite).Select(answer => (answer).Find<T>(publicKey)).FirstOrDefault(
                    result => result != null);
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
                        Questions.SelectMany(q => q.Find(condition))).Union(
                            Groups.Where(g => g is IComposite).SelectMany(g => (g).Find(condition)));

            /*   if (typeof(T) == GetType())
            {
                if (condition(this))
                    return this as T;
            }
            var resultInsideGroups =
                Groups.Where(a => a is IComposite).Select(answer => (answer as IComposite).Find<T>(condition)).FirstOrDefault(result => result != null);
            if (resultInsideGroups != null)
                return resultInsideGroups;
            var resultInsideQuestions =
                Questions.Select(answer => answer.Find<T>(condition)).FirstOrDefault(result => result != null);
            if (resultInsideQuestions != null)
                return resultInsideQuestions;
            return null;*/
        }

        #endregion

        public static explicit operator CompleteGroup(Group doc)
        {
            var result = new CompleteGroup(null)
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
    }
}