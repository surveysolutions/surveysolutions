using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Entities
{
    public class CompleteQuestionnaire : IEntity<CompleteQuestionnaireDocument>, IComposite//, IPropogate
    {
        private CompleteQuestionnaireDocument innerDocument;
        private IIteratorContainer iteratorContainer;
        public CompleteQuestionnaireDocument GetInnerDocument()
        {
            return innerDocument;
        }
        public CompleteQuestionnaire(Questionnaire template, UserLight user, SurveyStatus status)
        {
            innerDocument = (CompleteQuestionnaireDocument)((IEntity<QuestionnaireDocument>) template).GetInnerDocument();
            innerDocument.Creator = user;
            innerDocument.Status = status;
            innerDocument.Responsible = user;
        }

        public CompleteQuestionnaire(CompleteQuestionnaireDocument document, IIteratorContainer iteratorContainer)
        {
            this.innerDocument = document;
            this.iteratorContainer = iteratorContainer;
        }

        public string CompleteQuestinnaireId
        {
            get { return innerDocument.Id; }
        }

        public void SetStatus(SurveyStatus status)
        {
            innerDocument.Status = status;
        }


        public void SetResponsible(UserLight user)
        {
            innerDocument.Responsible = user;
        }

        public Iterator<CompleteAnswer> AnswerIterator
        {
            get { return iteratorContainer.Create<CompleteQuestionnaireDocument, CompleteAnswer>(this.innerDocument); }
        }
        public Iterator<CompleteGroup> GroupIterator
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
            if (!parent.HasValue)
            {
                CompleteGroup propogate = c as CompleteGroup;
                if (propogate != null && propogate.Propagated)
                {
                    var group = this.innerDocument.Groups.FirstOrDefault(g => g.PublicKey.Equals(propogate.PublicKey));
                    if (group != null)
                    {
                        var newGroup = new PropagatableCompleteGroup(propogate, Guid.NewGuid());
                        this.innerDocument.Groups.Add(newGroup);
                        
                        return;
                    }
                }
            }
            foreach (CompleteGroup completeGroup in this.innerDocument.Groups)
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
            foreach (CompleteQuestion completeQuestion in this.innerDocument.Questions)
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
                    this.innerDocument.Groups.RemoveAll(
                        g =>
                        g.PublicKey.Equals(propogate.PublicKey) && g is IPropogate &&
                        ((IPropogate) g).PropogationPublicKey.Equals(propogate.PropogationPublicKey)) > 0)
                    return;
            }
            foreach (CompleteGroup completeGroup in this.innerDocument.Groups)
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
            foreach (CompleteQuestion completeQuestion in this.innerDocument.Questions)
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
                if (this.innerDocument.Groups.RemoveAll(
                    g =>
                    g.PublicKey.Equals(publicKey)) > 0)
                    return;
            }
            foreach (CompleteGroup completeGroup in this.innerDocument.Groups)
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
            foreach (CompleteQuestion completeQuestion in this.innerDocument.Questions)
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
            var resultInsideGroups = innerDocument.Groups.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
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
