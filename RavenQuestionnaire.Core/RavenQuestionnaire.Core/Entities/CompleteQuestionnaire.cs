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
    public class CompleteQuestionnaire : IEntity<CompleteQuestionnaireDocument>, IComposite
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

       /* public void ClearAnswers()
        {
            this.GetAllQuestions().ForEach(q => q.Reset());
        }
        public void AddAnswer(Guid publicKey, string customText)
        {
            if (publicKey == Guid.Empty)
                return;

            var templateAnswer = this.Find<CompleteAnswer>(publicKey);

            if (templateAnswer == null)
                throw new InvalidOperationException("Answer with current public key doesn't exist in question list.");
            templateAnswer.GiveAnAnswer(customText);
        }
        public void ChangeAnswer(CompleteAnswer answer)
        {
            var question = this.Find<CompleteQuestion>(answer.QuestionPublicKey);

            if (question == null)
                throw new InvalidOperationException("Question does not exist in questionnaire");
            question.Reset();
            question.SetAnswer(answer.PublicKey, answer.CustomAnswer);
        }

        public void RemoveAnswerOnQuestion(Guid questionPublicKey)
        {
            var question = this.Find<CompleteQuestion>(questionPublicKey);
            question.ClearAnswers();
        }*/

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

        public bool Add(IComposite c, Guid? parent)
        {
           /* if (!parent.HasValue)
            {
                CompleteGroup group = c as CompleteGroup;
                if (group != null)
                {
                    innerDocument.Groups.Add(group);
                    return true;
                }
                CompleteQuestion question = c as CompleteQuestion;
                if (question != null)
                {
                    innerDocument.Questions.Add(question);
                    return true;
                }
            }*/
            foreach (CompleteGroup child in innerDocument.Groups)
            {
                if (child.Add(c, parent))
                    return true;
            }
            foreach (CompleteQuestion child in innerDocument.Questions)
            {
                if (child.Add(c, parent))
                    return true;
            }
            return false;
        }

        public bool Remove(IComposite c)
        {
            foreach (CompleteGroup child in innerDocument.Groups)
            {
               /* if (child == c)
                {
                    innerDocument.Groups.Remove(child);
                    return true;
                }*/
                if (child.Remove(c))
                    return true;
            }
            foreach (CompleteQuestion child in innerDocument.Questions)
            {
                if (child == c)
                {
                    child.Answers.ForEach(a => a.Reset());
                    return true;
                }
                if (child.Remove(c))
                    return true;
            }
            return false;
        }

        public bool Remove<T>(Guid publicKey) where T : class, IComposite
        {
            foreach (CompleteGroup child in innerDocument.Groups)
            {
               /* if (child.PublicKey == publicKey)
                {
                    innerDocument.Groups.Remove(child);
                    return true;
                }*/
                if (child.Remove<T>(publicKey))
                    return true;
            }
            foreach (CompleteQuestion child in innerDocument.Questions)
            {
                if (child.PublicKey == publicKey)
                {
                    child.Answers.ForEach(a => a.Reset());
                    return true;
                }
                if (child.Remove<T>(publicKey))
                    return true;
            }
            return false;
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            foreach (CompleteGroup child in innerDocument.Groups)
            {
                if (child is T && child.PublicKey == publicKey)
                    return child as T;
                T subNodes = child.Find<T>(publicKey);
                if (subNodes != null)
                    return subNodes;
            }
            foreach (CompleteQuestion child in innerDocument.Questions)
            {
                if (child is T && child.PublicKey == publicKey)
                    return child as T;
                T subNodes = child.Find<T>(publicKey);
                if (subNodes != null)
                    return subNodes;
            }
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
    }
}
