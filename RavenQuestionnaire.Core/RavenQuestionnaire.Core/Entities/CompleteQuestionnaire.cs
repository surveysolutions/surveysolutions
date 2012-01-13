using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Entities
{
    public class CompleteQuestionnaire : IEntity<CompleteQuestionnaireDocument>, IComposite//, IPropogate
    {
        private CompleteQuestionnaireDocument innerDocument;
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

        public CompleteQuestionnaire(CompleteQuestionnaireDocument document)
        {
            this.innerDocument = document;
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
        public IEnumerable<CompleteAnswer> GetAllAnswers()
        {
            List<CompleteAnswer> result= new List<CompleteAnswer>();
            foreach (CompleteQuestion completeQuestion in GetAllQuestions())
            {
                foreach (CompleteAnswer completeAnswer in completeQuestion.Answers)
                {
                    completeAnswer.QuestionPublicKey = completeQuestion.PublicKey;
                    if (completeAnswer.Selected)
                        result.Add(completeAnswer);
                }
            }
            return result;
            //  return GetAllQuestions().SelectMany(q => q.Answers).Where(a => a.Selected);
        }
        public IList<CompleteGroup> GetAllGroups()
        {
            return innerDocument.Groups;
        }
        
        #region Implementation of IComposite

        public virtual bool Add(IComposite c, Guid? parent)
        {
            if (innerDocument.Groups.Any(child => child.Add(c, parent)))
            {
                return true;
            }
            return innerDocument.Questions.Any(child => child.Add(c, parent));
        }

        public bool Remove(IComposite c)
        {
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
        public List<CompleteQuestion> GetRootQuestions()
        {
            return innerDocument.Questions;
        }

        public List<CompleteQuestion> GetAllQuestions()
        {
            List<CompleteQuestion> result = new List<CompleteQuestion>();
            result.AddRange(innerDocument.Questions);
            Queue<CompleteGroup> groups = new Queue<CompleteGroup>();
            foreach (var child in innerDocument.Groups)
            {
                groups.Enqueue(child);
            }
            while (groups.Count != 0)
            {
                var queueItem = groups.Dequeue();
                result.AddRange(queueItem.Questions);
                foreach (var child in queueItem.Groups)
                {
                    groups.Enqueue(child);
                }
            }
            return result;
        }

      /*  #region Implementation of IPropogate

        public void Propogate(Guid childGroupPublicKey)
        {
            var group = this.innerDocument.Groups.FirstOrDefault(g => g.PublicKey.Equals(childGroupPublicKey));
            if (group == null)
                throw new ArgumentException("Propogated group can't be founded");
            this.innerDocument.Groups.Add(group);
        }

        public void RemovePropogated(Guid childGroupPublicKey)
        {
            var group = this.innerDocument.Groups.FirstOrDefault(g => g.PublicKey.Equals(childGroupPublicKey));
            if (group == null)
                throw new ArgumentException("Removed group can't be founded");
            this.innerDocument.Groups.Remove(group);
        }

        #endregion*/
    }
}
