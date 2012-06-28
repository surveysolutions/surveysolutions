using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Statistics
{
    public class CompleteQuestionnaireStatisticView
    {
        public CompleteQuestionnaireStatisticView(CompleteQuestionnaireDocument doc)
        {
            this.Id = doc.Id;
            this.Title = doc.Title;
            this.StartDate = doc.CreationDate;
            this.EndDate = doc.CloseDate;
            this.CompleteQuestionnaireId = doc.Id;
            
            Creator = doc.Creator;
            Status = doc.Status;
            HandleQuestionTree(doc);
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string CompleteQuestionnaireId { get; set; }
        public int TotalQuestionCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public UserLight Creator { get; set; }
        public IList<QuestionStatisticView> AnsweredQuestions { get; set; }
        public IList<QuestionStatisticView> InvalidQuestions { get; set; }
        public IList<QuestionStatisticView> FeaturedQuestions { get; set; }
        public SurveyStatus Status { get; set; }

        protected void HandleQuestionTree(CompleteQuestionnaireDocument target)
        {
            this.InvalidQuestions=new List<QuestionStatisticView>();
            this.AnsweredQuestions = new List<QuestionStatisticView>();
            this.FeaturedQuestions = new List<QuestionStatisticView>();
            Queue<ICompleteGroup> nodes = new Queue<ICompleteGroup>(new List<ICompleteGroup>() { target });
            Queue<Guid> keys = new Queue<Guid>();
            keys.Enqueue(target.PublicKey);
            this.TotalQuestionCount = 0;
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
            CalculateApproximateAnswerTime(this.AnsweredQuestions);
        }
        protected void ProccessQuestions(IEnumerable<ICompleteQuestion> questions, Guid gropPublicKey, Guid screenPublicKey)
        {
            foreach (ICompleteQuestion completeQuestion in questions)
            {
                var statItem = new QuestionStatisticView(completeQuestion, gropPublicKey, screenPublicKey);
                if (completeQuestion.Featured)
                    this.FeaturedQuestions.Add(statItem);
                if (!completeQuestion.Valid)
                    this.InvalidQuestions.Add(statItem);
                if (completeQuestion.Answer != null)
                    this.AnsweredQuestions.Add(statItem);
                this.TotalQuestionCount++;
            }
        }
        protected void CalculateApproximateAnswerTime(IList<QuestionStatisticView> list)
        {
            //todo optimizaton. current order by by could be optimized manualy
            var unansweredList = list.Where(q => !q.AnswerDate.HasValue).ToList();
            list = list.Where(q => q.AnswerDate.HasValue).OrderBy(q => q.AnswerDate).ToList();
            if (list.Count > 0)
            {
                list[0].ApproximateTime = list[0].AnswerDate -
                                          this.StartDate;
                for (int i = 1; i < list.Count; i++)
                {
                    list[i].ApproximateTime = list[i].AnswerDate - list[i - 1].AnswerDate;
                }
            }
            list.Union(unansweredList);
        }
    }
}
