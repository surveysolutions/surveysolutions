using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Entities.Extensions;
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
            Queue<ICompleteGroup<ICompleteGroup, ICompleteQuestion>> nodes =
                new Queue<ICompleteGroup<ICompleteGroup, ICompleteQuestion>>(
                    new List<ICompleteGroup<ICompleteGroup, ICompleteQuestion>>() {target});
            this.innerDocument.TotalQuestionCount = 0;
            while (nodes.Count > 0)
            {
                ICompleteGroup<ICompleteGroup, ICompleteQuestion> group = nodes.Dequeue();
                ProccessQuestions(group.Questions);
                foreach (ICompleteGroup subGroup in group.Groups)
                {
                    ICompleteGroup<ICompleteGroup, ICompleteQuestion> groupWithQuestions =
                        subGroup as ICompleteGroup<ICompleteGroup, ICompleteQuestion>;
                    if (groupWithQuestions != null)
                        nodes.Enqueue(groupWithQuestions);
                }
            }
            CalculateApproximateAnswerTime(this.innerDocument.AnsweredQuestions);
        }
        protected void ProccessQuestions(IEnumerable<ICompleteQuestion> questions)
        {
            foreach (ICompleteQuestion completeQuestion in questions)
            {
                var statItem = new QuestionStatisticDocument(completeQuestion);
                if(completeQuestion.Featured)
                    this.innerDocument.FeturedQuestions.Add(statItem);
                if (!completeQuestion.Valid)
                    this.innerDocument.InvalidQuestions.Add(statItem);
                ICompleteQuestion<ICompleteAnswer> withAnswers = completeQuestion as ICompleteQuestion<ICompleteAnswer>;
                if (withAnswers != null && withAnswers.Answers.Any(a => a.Selected))
                    this.innerDocument.AnsweredQuestions.Add(statItem);
                this.innerDocument.TotalQuestionCount++;
            }
        }

      /*  protected void CollectFeturedQuestions(CompleteQuestionnaireDocument target)
        {
            this.innerDocument.AnsweredQuestions.Clear();
            var questions = target.Find<ICompleteQuestion<ICompleteAnswer>>(q => q.Featured);

            foreach (ICompleteQuestion<ICompleteAnswer> completeQuestion in questions)
            {
                this.innerDocument.FeturedQuestions.Add(new QuestionStatisticDocument(completeQuestion));
            }
           // CalculateApproximateAnswerTime(this.innerDocument.AnsweredQuestions);
        }
        protected void CollectAnsweredQuestions(CompleteQuestionnaireDocument target)
        {
            this.innerDocument.AnsweredQuestions.Clear();
            var questions = target.Find<ICompleteQuestion<ICompleteAnswer>>(q => q.Answers.Any(a => a.Selected));
            
            foreach (ICompleteQuestion<ICompleteAnswer> completeQuestion in questions)
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
         
            var questions = target.Find<ICompleteQuestion<ICompleteAnswer>>(q => !q.Valid);

            foreach (ICompleteQuestion<ICompleteAnswer> completeQuestion in questions)
            {
                this.innerDocument.InvalidQuestions.Add(new QuestionStatisticDocument(completeQuestion));
            }
          //  CalculateApproximateAnswerTime(this.innerDocument.InvalidQuestions);
        }*/
    }
}
