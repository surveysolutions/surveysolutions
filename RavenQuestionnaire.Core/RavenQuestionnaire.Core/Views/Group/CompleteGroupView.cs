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
             expresstionValidator = new CompleteQuestionnaireConditionExecutor(new CompleteQuestionnaireDocument());
            
        }
        public CompleteGroupView(CompleteQuestionnaireDocument doc, RavenQuestionnaire.Core.Entities.SubEntities.Group group)
            : this()
        {
            this.QuestionnaireId = doc.Id;
            this.PublicKey = group.PublicKey;
            this.GroupText = group.GroupText;
            this.CompleteAnswers = doc.CompletedAnswers.ToArray();
            expresstionValidator = new CompleteQuestionnaireConditionExecutor(doc);
            this._questions = ProcessQuestionList(group.Questions, doc.CompletedAnswers, doc.Questionnaire);
            MerdgeAnswersWithResults();
        }
        private readonly CompleteQuestionnaireConditionExecutor expresstionValidator;

        protected CompleteQuestionnaireConditionExecutor ExpresstionValidator
        {
            get
            {
                return expresstionValidator;
            }
        }

        protected CompleteQuestionView[] ProcessQuestionList(
            IList<RavenQuestionnaire.Core.Entities.SubEntities.Question> questions, 
            List<CompleteAnswer> answers, 
            QuestionnaireDocument questionnaire)
        {
            CompleteQuestionView[] result = new CompleteQuestionView[questions.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new CompleteQuestionView(questions[i], questionnaire);
                result[i].Enabled = ExpresstionValidator.Execute(questions[i].ConditionExpression);
                RemoveDisabledAnswers(answers, result[i]);
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
            get { return IdUtil.ParseId(_questionnaireId); }
            set { _questionnaireId = value; }
        }
        private string _questionnaireId;

        public CompleteGroupView[] Groups { get; set; }
        public CompleteQuestionView[] Questions
        {
            get { return _questions; }
        }
        protected CompleteAnswer[] CompleteAnswers { get; set; }
        private CompleteQuestionView[] _questions;
        protected void MerdgeAnswersWithResults()
        {
            foreach (var answer in Questions.SelectMany(q => q.Answers))
            {
                var completeAnswer = CompleteAnswers.FirstOrDefault(a => a.PublicKey.Equals(answer.PublicKey));
                if (completeAnswer != null)
                {
                    answer.Selected = true;
                    answer.CustomAnswer = completeAnswer.CustomAnswer;
                }
            }
        }
    }
}
