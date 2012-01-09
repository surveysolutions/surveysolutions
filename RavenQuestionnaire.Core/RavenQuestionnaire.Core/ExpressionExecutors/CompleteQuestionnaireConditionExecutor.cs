using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NCalc;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    public class CompleteQuestionnaireConditionExecutor : IExpressionExecutor<CompleteQuestionnaireDocument>
    {
        public CompleteQuestionnaireConditionExecutor(CompleteQuestionnaireDocument questionnaire)
        {
            this.questionnaire = questionnaire;
        }

        private readonly CompleteQuestionnaireDocument questionnaire;
        public CompleteQuestionnaireDocument Entity
        {
            get { return this.questionnaire; }
        }

        public bool Execute(string condition)
        {
            if (string.IsNullOrEmpty(condition))
                return true;
            var e = new Expression(condition);
            foreach (var answer in this.questionnaire.CompletedAnswers)
            {
                e.Parameters[answer.QuestionPublicKey.ToString()] = answer.CustomAnswer;
            }

            bool result = false;
            try
            {
                result = (bool)e.Evaluate();
            }
            catch (Exception)
            {
            }
            return result;
        }
    }
}
