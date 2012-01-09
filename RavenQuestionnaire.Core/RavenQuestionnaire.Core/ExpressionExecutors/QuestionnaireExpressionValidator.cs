using System;
using System.Linq;
using NCalc;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    public class QuestionnaireExpressionValidator : IExpressionExecutor<Questionnaire>
    {
        public bool Execute(Questionnaire entity, string expression)
        {
            if (string.IsNullOrEmpty(expression))
                return true;
            var e = new Expression(expression);

            e.EvaluateParameter += (name, args) =>
                                       {
                                           if (!entity.GetAllQuestions().Any(q => q.PublicKey.ToString().Equals(name)))
                                               throw new ArgumentOutOfRangeException(
                                                   string.Format("Parameter {0} is invalid", name));
                                           args.Result = "0";
                                       };
            e.Evaluate();
            return true;
        }
    }
}
