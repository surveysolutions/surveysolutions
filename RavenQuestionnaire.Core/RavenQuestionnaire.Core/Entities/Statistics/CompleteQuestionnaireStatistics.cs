using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Entities.Statistics
{
    public class CompleteQuestionnaireStatistics : IEntity<CompleteQuestionnaireStatisticDocument>
    {
        #region .ctors

        public CompleteQuestionnaireStatistics(CompleteQuestionnaireStatisticDocument doc)
        {
            this.innerDocument = doc;
        }

        public CompleteQuestionnaireStatistics(CompleteQuestionnaireDocument target)
        {
            this.innerDocument = new CompleteQuestionnaireStatisticDocument();
            this.innerDocument.Id = IdUtil.CreateStatisticId(IdUtil.ParseId(target.Id));
            this.innerDocument.Creator = target.Creator;
            UpdateStatistic(target);
        }

        #endregion

        public void UpdateStatistic(CompleteQuestionnaireDocument target)
        {
            this.innerDocument.CompleteQuestionnaireId = target.Id;
            this.innerDocument.TemplateId = target.TemplateId;
            this.innerDocument.StartDate = target.CreationDate;
            this.innerDocument.EndDate = target.CloseDate;
            this.innerDocument.Title = target.Title;
            this.innerDocument.Status = target.Status;
            this.innerDocument.Creator = target.Creator;

            HandleQuestionTree(target);
         /*   CollectFeturedQuestions(target);
            CollectInvalidQuestions(target);*/
        }

        #region Implementation of IEntity<CompleteQuestionnaireStatisticDocument>

        CompleteQuestionnaireStatisticDocument IEntity<CompleteQuestionnaireStatisticDocument>.GetInnerDocument()
        {
            return innerDocument;
        }
        private CompleteQuestionnaireStatisticDocument innerDocument;

        #endregion

        protected void HandleQuestionTree(CompleteQuestionnaireDocument target)
        {
            this.innerDocument.InvalidQuestions.Clear();
            this.innerDocument.AnsweredQuestions.Clear();
            this.innerDocument.FeturedQuestions.Clear();
            Queue<ICompleteGroup> nodes = new Queue<ICompleteGroup>(new List<ICompleteGroup>() { target });
            Queue<Guid> keys = new Queue<Guid>();
            keys.Enqueue(target.PublicKey);
            this.innerDocument.TotalQuestionCount = 0;
            {
                ICompleteGroup group = nodes.Dequeue();
                var key = keys.Dequeue();
                var groupKey = group.PublicKey;
                if (group.PropogationPublicKey.HasValue)
                {
                    groupKey = group.PropogationPublicKey.Value;
                }
                ProccessQuestions(@group.Children.OfType<ICompleteQuestion>(), groupKey, key);
                foreach (ICompleteGroup subGroup in group.Children.OfType<ICompleteGroup>())
                {
                    nodes.Enqueue(subGroup);
                    keys.Enqueue(subGroup.PublicKey);

                }
            }
            while (nodes.Count > 0)
            {
                ICompleteGroup group = nodes.Dequeue();
                var key = keys.Dequeue();
                var groupKey = group.PublicKey;
                if (group.PropogationPublicKey.HasValue)
                {
                    groupKey = group.PropogationPublicKey.Value;
                }
                ProccessQuestions(group.Children.OfType<ICompleteQuestion>(), groupKey, key);
                foreach (ICompleteGroup subGroup in group.Children.OfType<ICompleteGroup>())
                {
                    nodes.Enqueue(subGroup);
                    keys.Enqueue(key);

                }
            }
            CalculateApproximateAnswerTime(this.innerDocument.AnsweredQuestions);
        }
        protected void ProccessQuestions(IEnumerable<ICompleteQuestion> questions, Guid gropPublicKey, Guid screenPublicKey)
        {
            foreach (ICompleteQuestion completeQuestion in questions)
            {
                var statItem = new QuestionStatisticDocument(completeQuestion, gropPublicKey, screenPublicKey);
                if(completeQuestion.Featured)
                    this.innerDocument.FeturedQuestions.Add(statItem);
                if (!completeQuestion.Valid)
                    this.innerDocument.InvalidQuestions.Add(statItem);
                if (completeQuestion.Answer != null)
                    this.innerDocument.AnsweredQuestions.Add(statItem);
                this.innerDocument.TotalQuestionCount++;
            }
        }

      /*  protected void CollectFeturedQuestions(CompleteQuestionnaireDocument target)
        {
            this.innerDocument.AnsweredQuestions.Clear();
            var questions = target.Find<ICompleteQuestion>(q => q.Featured);

            foreach (ICompleteQuestion completeQuestion in questions)
            {
                this.innerDocument.FeturedQuestions.Add(new QuestionStatisticDocument(completeQuestion));
            }
           // CalculateApproximateAnswerTime(this.innerDocument.AnsweredQuestions);
        }
        protected void CollectAnsweredQuestions(CompleteQuestionnaireDocument target)
        {
            this.innerDocument.AnsweredQuestions.Clear();
            var questions = target.Find<ICompleteQuestion>(q => q.Answers.Any(a => a.Selected));
            
            foreach (ICompleteQuestion completeQuestion in questions)
            {
                this.innerDocument.AnsweredQuestions.Add(new QuestionStatisticDocument(completeQuestion));
            }
            CalculateApproximateAnswerTime(this.innerDocument.AnsweredQuestions);
        }*/
        protected void CalculateApproximateAnswerTime(IList<QuestionStatisticDocument> list)
        {
            //todo optimizaton. current order by by could be optimized manualy
            var unansweredList = list.Where(q => !q.AnswerDate.HasValue).ToList();
            list = list.Where(q=>q.AnswerDate.HasValue).OrderBy(q => q.AnswerDate).ToList();
            if (list.Count > 0)
            {
                list[0].ApproximateTime = list[0].AnswerDate -
                                          this.innerDocument.StartDate;
                for (int i = 1; i < list.Count; i++)
                {
                    list[i].ApproximateTime = list[i].AnswerDate - list[i - 1].AnswerDate;
                }
            }
            list.Union(unansweredList);
        }

      /*  protected void CollectInvalidQuestions(CompleteQuestionnaireDocument target)
        {
            this.innerDocument.InvalidQuestions.Clear();
         
            var questions = target.Find<ICompleteQuestion>(q => !q.Valid);

            foreach (ICompleteQuestion completeQuestion in questions)
            {
                this.innerDocument.InvalidQuestions.Add(new QuestionStatisticDocument(completeQuestion));
            }
          //  CalculateApproximateAnswerTime(this.innerDocument.InvalidQuestions);
        }*/
    }
}
