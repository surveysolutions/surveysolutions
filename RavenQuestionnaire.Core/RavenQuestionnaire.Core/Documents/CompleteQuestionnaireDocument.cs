using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Observers;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Documents
{
    public interface ICompleteQuestionnaireDocument<TGroup, TQuestion> : IQuestionnaireDocument<TGroup, TQuestion>, ICompleteGroup<TGroup, TQuestion>
        where TQuestion : ICompleteQuestion
        where TGroup : ICompleteGroup
    {
         UserLight Creator { get; set; }

         string TemplateId { get; set; }

         SurveyStatus Status { set; get; }

         UserLight Responsible { get; set; }

         string StatusChangeComment { get; set; }
    }


    public class CompleteQuestionnaireDocument : ICompleteQuestionnaireDocument<ICompleteGroup, ICompleteQuestion>
    {
        public CompleteQuestionnaireDocument()
        {
            CreationDate = DateTime.Now;
            LastEntryDate = DateTime.Now;
            Questions = new List<ICompleteQuestion>();
            Groups = new List<ICompleteGroup>();
            Observers = new List<IObserver<CompositeInfo>>();
        }
        public static explicit operator CompleteQuestionnaireDocument(QuestionnaireDocument doc)
        {
            CompleteQuestionnaireDocument result = new CompleteQuestionnaireDocument
            {
                TemplateId = doc.Id,
                Title = doc.Title
            };
            result.Questions =
                doc.Questions.Select(q => new CompleteQuestionFactory().ConvertToCompleteQuestion(q)).ToList();
            result.Groups =
                doc.Groups.Select(q => new CompleteGroupFactory().ConvertToCompleteGroup(q)).ToList();
            result.Observers = doc.Observers;
            return result;
        }
        public UserLight Creator { get; set; }

        public string TemplateId { get; set; }

        public SurveyStatus Status { set; get; }

        public UserLight Responsible { get; set; }

        public string StatusChangeComment { get; set; }

        #region Implementation of IQuestionnaireDocument

        public List<ICompleteQuestion> Questions { get; set; }

        public List<ICompleteGroup> Groups { get; set; }

        public string Id { get; set; }

        public string Title { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastEntryDate { get; set; }

        public DateTime? OpenDate { get; set; }

        public DateTime? CloseDate { get; set; }

        [XmlIgnore]
        public Guid PublicKey { get; set; }
        [XmlIgnore]
        public bool Propagated
        {
            get { return false; }
            set { }
        }

        public Guid? ForcingPropagationPublicKey
        {
            get { return null; }
            set {  }
        }
        public List<IObserver<CompositeInfo>> Observers { get; set; }
        #endregion

        #region Implementation of IComposite

        public virtual void Add(IComposite c, Guid? parent)
        {
            ICompleteGroup group = c as ICompleteGroup;
            /*  if (group != null && group.Propagated)
              {
                  if (!(group is PropagatableCompleteGroup))
                      c = new PropagatableCompleteGroup(group, Guid.NewGuid());
  */
            if (group != null && group is IPropogate && !parent.HasValue)
            {
                if (this.Groups.Count(g => g.PublicKey.Equals(group.PublicKey)) > 0)
                {
                    this.Groups.Add(group);
                    return;
                }
            }
            //      }
            foreach (CompleteGroup completeGroup in this.Groups)
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
            foreach (CompleteQuestion completeQuestion in this.Questions)
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

        public void Remove(IComposite c)
        {
            PropagatableCompleteGroup propogate = c as PropagatableCompleteGroup;
            if (propogate != null)
            {
                if (
                    this.Groups.RemoveAll(
                        g =>
                        g.PublicKey.Equals(propogate.PublicKey) && g is IPropogate &&
                        ((IPropogate)g).PropogationPublicKey.Equals(propogate.PropogationPublicKey)) > 0)
                {
                    return;
                }
            }
            foreach (CompleteGroup completeGroup in this.Groups)
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
            foreach (CompleteQuestion completeQuestion in this.Questions)
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

        public void Remove<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T) == typeof(PropagatableCompleteGroup))
            {
                if (this.Groups.RemoveAll(
                    g =>
                    g.PublicKey.Equals(publicKey)) > 0)
                {
                    return;
                }
            }
            foreach (CompleteGroup completeGroup in this.Groups)
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
            foreach (CompleteQuestion completeQuestion in this.Questions)
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

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            var resultInsideGroups = this.Groups.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            if (resultInsideGroups != null)
                return resultInsideGroups;
            var resultInsideQuestions = this.Questions.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            if (resultInsideQuestions != null)
                return resultInsideQuestions;
            return null;
        }
        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class, IComposite
        {
            return
              this.Questions.Where(a => a is T && condition(a as T)).Select(a => a as T).Union(
                  this.Groups.Where(a => a is T && condition(a as T)).Select(a => a as T)).Union(
                      this.Questions.SelectMany(q => q.Find<T>(condition))).Union(
                          this.Groups.SelectMany(g => g.Find<T>(condition)));

        }

        #endregion
    }
}
