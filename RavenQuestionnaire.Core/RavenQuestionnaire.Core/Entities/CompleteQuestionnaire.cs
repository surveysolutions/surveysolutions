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
        public Iterator<CompleteQuestion> QuestionIterator
        {
            get { return iteratorContainer.Create<CompleteQuestionnaireDocument, CompleteQuestion>(this.innerDocument); }
        }
        #region Implementation of IComposite

        public virtual bool Add(IComposite c, Guid? parent)
        {
            if (!parent.HasValue)
            {
                CompleteGroup propogate = c as CompleteGroup;
                if (propogate != null && propogate.Propagated && innerDocument.Groups.FirstOrDefault(g => g.PublicKey.Equals(propogate.PublicKey)) != null)
                {
                    innerDocument.Groups.Add(new PropagatableCompleteGroup(propogate, Guid.NewGuid()));
                    return true;
                }
            }
            if (innerDocument.Groups.Any(child => child.Add(c, parent)))
            {
                return true;
            }
            return innerDocument.Questions.Any(child => child.Add(c, parent));
        }

        public bool Remove(IComposite c)
        {
            PropagatableCompleteGroup propogate = c as PropagatableCompleteGroup;
            if (propogate != null)
            {
                innerDocument.Groups.RemoveAll(
                    g =>
                    g.PublicKey.Equals(propogate.PublicKey) && g is IPropogate &&
                    ((IPropogate)g).PropogationPublicKey.Equals(propogate.PropogationPublicKey));
            }
            if (innerDocument.Groups.Any(child => child.Remove(c)))
            {
                return true;
            }
            return innerDocument.Questions.Any(child => child.Remove(c));
        }

        public bool Remove<T>(Guid publicKey) where T : class, IComposite
        {
            if (innerDocument.Groups.Any(child => child.Remove<T>(publicKey)))
            {
                return true;
            }
            return innerDocument.Questions.Any(child => child.Remove<T>(publicKey));
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
