using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Observers;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Documents
{
    public interface IQuestionnaireDocument : IGroup
    {
        string Id { get; set; }
        DateTime CreationDate { get; set; }
        DateTime LastEntryDate { get; set; }
        DateTime? OpenDate { get; set; }
        DateTime? CloseDate { get; set; }
        List<IObserver<CompositeInfo>> Observers { get; set; }
    }

    public interface IQuestionnaireDocument<TGroup, TQuestion> : IQuestionnaireDocument, IGroup<TGroup, TQuestion>
        where TQuestion : IQuestion
        where TGroup : IGroup
    {
    }

    public class QuestionnaireDocument : IQuestionnaireDocument<IGroup, IQuestion>
    {
        public QuestionnaireDocument()
        {
            CreationDate = DateTime.Now;
            LastEntryDate = DateTime.Now;
            Questions = new List<IQuestion>();
            Groups = new List<IGroup>();
            Observers = new List<IObserver<CompositeInfo>>();
            FlowGraph = null;
        }

        public string Id { get; set; }
       
        public string Title { get; set; }
        [JsonIgnore]
        public Guid PublicKey { get; set; }
        [JsonIgnore]
        public Propagate Propagated
        {
            get { return Propagate.None; }
            set {  }
        }

        public Guid? ForcingPropagationPublicKey
        {
            get { return null; }
            set {  }
        }

        public DateTime CreationDate
        { get; set; }
        public DateTime LastEntryDate
        { get; set; }

        public DateTime? OpenDate
        { get; set; }

        public DateTime? CloseDate { get; set; }

        public List<IQuestion> Questions { get; set; }
        public List<IGroup> Groups { get; set; }
        public FlowGraph FlowGraph { get; set; }
        public List<IObserver<CompositeInfo>> Observers { get; set; }

        #region Implementation of IComposite
        public void Add(IComposite c, Guid? parent)
        {
            if (!parent.HasValue)
            {
                IGroup group = c as IGroup;
                if (group != null)
                {
                    this.Groups.Add(group);
                    return;
                }
                IQuestion question = c as IQuestion;
                if (question != null)
                {
                    this.Questions.Add(question);
                    return;
                }
            }
            foreach (IGroup child in this.Groups)
            {
                try
                {
                    child.Add(c, parent);
                    return;
                }
                catch (CompositeException)
                {
                }
                /* if (child.Add(c, parent))
                     return true;*/
            }
            foreach (IQuestion child in this.Questions)
            {
                try
                {
                    child.Add(c, parent);
                    return;
                }
                catch (CompositeException)
                {
                }
                /*  if (child.Add(c, parent))
                      return true;*/
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
        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return
             Questions.Where(a => a is T && condition(a as T)).Select(a => a as T).Union(
                 Groups.Where(a => a is T && condition(a as T)).Select(a => a as T)).Union(
                     Questions.SelectMany(q => q.Find<T>(condition))).Union(
                         Groups.SelectMany(g => g.Find<T>(condition)));

            /*   foreach (Group child in innerDocument.Groups)
               {
                   if (child is T && condition(child))
                       return child as T;
                   T subNodes = child.Find<T>(condition);
                   if (subNodes != null)
                       return subNodes;
               }
               foreach (Question child in innerDocument.Questions)
               {
                   if (child is T && condition(child))
                       return child as T;
                   T subNodes = child.Find<T>(condition);
                   if (subNodes != null)
                       return subNodes;
               }
               return null;*/
        }

        #endregion
    }
}
