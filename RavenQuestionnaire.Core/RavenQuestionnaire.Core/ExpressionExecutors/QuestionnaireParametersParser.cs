using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NCalc;
using NCalc.Domain;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    public class QuestionnaireParametersParser : IExpressionExecutor<Questionnaire, IList<Question>>
    {
        public IList<Question> Execute(Questionnaire entity, string expression)
        {
            IList<Question> result = new List<Question>();
            if (string.IsNullOrEmpty(expression))
                return result;
            var e = new Expression(expression);

            e.EvaluateParameter += (name, args) =>
                                       {
                                           var question =
                                               entity.GetAllQuestions().FirstOrDefault(
                                                   q => q.PublicKey.ToString().Equals(name));
                                           if (question == null)
                                               throw new ArgumentOutOfRangeException(
                                                   string.Format("Parameter {0} is invalid", name));
                                           result.Add(question);
                                           args.Result = "0";
                                       };
            e.Evaluate(new EvaluationTesterVisitor(e.Options));
            return result;
        }
    }
}
