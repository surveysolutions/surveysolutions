using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.Group
{
    public class CompleteGroupView
    {
        public CompleteGroupView()
        {
            Groups = new CompleteGroupView[0];
            _questions = new CompleteQuestionView[0];
            
        }
        public CompleteGroupView(CompleteQuestionnaireDocument doc, RavenQuestionnaire.Core.Entities.SubEntities.Group group, IExpressionExecutor<CompleteQuestionnaireDocument> executor)
            : this()
        {
            this.completeQuestionnaireDocument = doc;
            this.conditionExecutor = executor;
            this.PublicKey = group.PublicKey;
            this.GroupText = group.GroupText;
            this._questions = ProcessQuestionList(group.Questions);
            MerdgeAnswersWithResults();
        }

        private IExpressionExecutor<CompleteQuestionnaireDocument> conditionExecutor;

        protected CompleteQuestionView[] ProcessQuestionList(
            IList<RavenQuestionnaire.Core.Entities.SubEntities.Question> questions)
        {
            CompleteQuestionView[] result = new CompleteQuestionView[questions.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new CompleteQuestionView(questions[i], this.completeQuestionnaireDocument.Questionnaire);
                result[i].Enabled = this.conditionExecutor.Execute(completeQuestionnaireDocument, questions[i].ConditionExpression);
                RemoveDisabledAnswers(this.completeQuestionnaireDocument.CompletedAnswers, result[i]);
            }
            return result;
        }
        protected void RemoveDisabledAnswers(List<CompleteAnswer> answers, CompleteQuestionView question)
        {
            if (!question.Enabled)
                answers.RemoveAll(a => a.QuestionPublicKey.Equals(question.PublicKey));
        }

        public Guid PublicKey { get; set; }
        public string GroupText { get; set; }
        public Guid? ParentGroup { get; set; }

        public string QuestionnaireId
        {
            get { return IdUtil.ParseId(this.completeQuestionnaireDocument.Id); }
        }
        private string _questionnaireId;

        public CompleteGroupView[] Groups { get; set; }
        public CompleteQuestionView[] Questions
        {
            get { return _questions; }
        }

        protected CompleteQuestionnaireDocument completeQuestionnaireDocument;
        private CompleteQuestionView[] _questions;
        protected void MerdgeAnswersWithResults()
        {
            foreach (var answer in Questions.SelectMany(q => q.Answers))
            {
                var completeAnswer = completeQuestionnaireDocument.CompletedAnswers.FirstOrDefault(a => a.PublicKey.Equals(answer.PublicKey));
                if (completeAnswer != null)
                {
                    answer.Selected = true;
                    answer.CustomAnswer = completeAnswer.CustomAnswer;
                }
            }
        }
    }
}
