using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.Observers;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Entities
{
    public class CompleteQuestionnaire : IEntity<CompleteQuestionnaireDocument>, IComposite//, IPropogate
    {
        private CompleteQuestionnaireDocument innerDocument;
        private IIteratorContainer iteratorContainer;
        private CompositeHandler handler;
        public CompleteQuestionnaireDocument GetInnerDocument()
        {
            return innerDocument;
        }
        public CompleteQuestionnaire(Questionnaire template, UserLight user, SurveyStatus status)
        {
            innerDocument =
                (CompleteQuestionnaireDocument)((IEntity<QuestionnaireDocument>)template).GetInnerDocument();
            innerDocument.Creator = user;
            innerDocument.Status = status;
            innerDocument.Responsible = user;
            handler = new CompositeHandler(innerDocument.Observers, this);
            /* foreach (ICompleteGroup completeGroup in GroupIterator)
             {
                 var group = completeGroup as CompleteGroup;
                 if (group != null)
                     group.Subscribe(this.handler);
             }*/
        }

        public CompleteQuestionnaire(CompleteQuestionnaireDocument document, IIteratorContainer iteratorContainer)
        {
            this.innerDocument = document;
            this.iteratorContainer = iteratorContainer;
            handler = new CompositeHandler(innerDocument.Observers, this);
            /*    foreach (ICompleteGroup completeGroup in GroupIterator)
                {
                    var group = completeGroup as CompleteGroup;
                    if (group != null)
                        group.Subscribe(this.handler);
                }*/
        }

        public string CompleteQuestinnaireId
        {
            get { return innerDocument.Id; }
        }
        /*  public void Subscribe(IComposite target, CompleteGroup group, Actions action)
          {
              group.Subscribe(this.handler);
          }
          public void Unsubscribe(CompleteGroup group)
          {
              group.Unsubscribe();
          }*/
        public void SetStatus(SurveyStatus status)
        {
            innerDocument.Status = status;
        }


        public void SetResponsible(UserLight user)
        {
            innerDocument.Responsible = user;
        }

        public Iterator<ICompleteAnswer> AnswerIterator
        {
            get { return iteratorContainer.Create<ICompleteGroup<ICompleteGroup, ICompleteQuestion>, ICompleteAnswer>(this.innerDocument); }
        }
        public Iterator<ICompleteGroup> GroupIterator
        {
            get { return new HierarchicalGroupIterator(this.innerDocument); }
        }
        /*   public Iterator<CompleteQuestion> QuestionIterator
           {
               get { return iteratorContainer.Create<CompleteQuestionnaireDocument, CompleteQuestion>(this.innerDocument); }
           }*/
        #region Implementation of IComposite

        public virtual void Add(IComposite c, Guid? parent)
        {
            CompleteGroup group = c as CompleteGroup;
            if (group != null && group.Propagated)
            {
                if (!(group is PropagatableCompleteGroup))
                c = new PropagatableCompleteGroup(group, Guid.NewGuid());

                if (!parent.HasValue)
                {

                    if (this.innerDocument.Groups.Count(g => g.PublicKey.Equals(group.PublicKey)) > 0)
                    {
                        this.innerDocument.Groups.Add(c as ICompleteGroup);
                        this.handler.Add(c);
                        return;
                    }
                }
            }
            foreach (CompleteGroup completeGroup in this.innerDocument.Groups)
            {
                try
                {
                    completeGroup.Add(c, parent);
                    this.handler.Add(c);
                    return;
                }
                catch (CompositeException)
                {
                }
            }
            foreach (CompleteQuestion completeQuestion in this.innerDocument.Questions)
            {
                try
                {
                    completeQuestion.Add(c, parent);
                    this.handler.Add(c);
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
                    this.innerDocument.Groups.RemoveAll(
                        g =>
                        g.PublicKey.Equals(propogate.PublicKey) && g is IPropogate &&
                        ((IPropogate)g).PropogationPublicKey.Equals(propogate.PropogationPublicKey)) > 0)
                {
                    this.handler.Remove(c);
                    return;
                }
            }
            foreach (CompleteGroup completeGroup in this.innerDocument.Groups)
            {
                try
                {
                    completeGroup.Remove(c);
                    this.handler.Remove(c);
                    return;
                }
                catch (CompositeException)
                {
                }
            }
            foreach (CompleteQuestion completeQuestion in this.innerDocument.Questions)
            {
                try
                {
                    completeQuestion.Remove(c);
                    this.handler.Remove(c);
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
                if (this.innerDocument.Groups.RemoveAll(
                    g =>
                    g.PublicKey.Equals(publicKey)) > 0)
                {
                    this.handler.Remove<T>(publicKey);
                    return;
                }
            }
            foreach (CompleteGroup completeGroup in this.innerDocument.Groups)
            {
                try
                {
                    completeGroup.Remove<T>(publicKey);
                    this.handler.Remove<T>(publicKey);
                    return;
                }
                catch (CompositeException)
                {
                }
            }
            foreach (CompleteQuestion completeQuestion in this.innerDocument.Questions)
            {
                try
                {
                    completeQuestion.Remove<T>(publicKey);
                    this.handler.Remove<T>(publicKey);
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
            var resultInsideGroups = innerDocument.Groups.Where(a => a is IComposite).Select(answer => (answer as IComposite).Find<T>(publicKey)).FirstOrDefault(result => result != null);
            if (resultInsideGroups != null)
                return resultInsideGroups;
            var resultInsideQuestions = innerDocument.Questions.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            if (resultInsideQuestions != null)
                return resultInsideQuestions;
            return null;
        }

        #endregion


    }
}
