using System;
using System.Linq;
using NCalc;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    public class QuestionnaireExpressionValidator : IExpressionExecutor<Questionnaire>
    {
        public QuestionnaireExpressionValidator(Questionnaire questionnaire)
        {
            this.questionnaire = questionnaire;
        }

        private readonly Questionnaire questionnaire;
        public Questionnaire Entity
        {
            get { return questionnaire; }
        }

        public bool Execute(string expression)
        {
            if (string.IsNullOrEmpty(expression))
                return true;
            var e = new Expression(expression);

            e.EvaluateParameter += new EvaluateParameterHandler(e_EvaluateParameter);
            e.Evaluate();
            return true;
        }
        private void e_EvaluateParameter(string name, ParameterArgs args)
        {
            if (!this.questionnaire.GetAllQuestions().Any(q => q.PublicKey.ToString().Equals(name)))
                throw new ArgumentOutOfRangeException(string.Format("Parameter {0} is invalid", name));
            args.Result = "0";
        }
    }
}
