using System;
using System.Collections.Generic;
using System.Linq;
using NCalc;
using NCalc.Domain;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    public class QuestionnaireParametersParser : IExpressionExecutor<Questionnaire, IList<IQuestion>>
    {
        public IList<IQuestion> Execute(Questionnaire entity, string expression)
        {
            IList<IQuestion> result = new List<IQuestion>();
            if (string.IsNullOrEmpty(expression))
                return result;
            var expressionEntity = new Expression(expression);

            expressionEntity.EvaluateParameter += (name, args) =>
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
            expressionEntity.Evaluate(new EvaluationTesterVisitor(expressionEntity.Options));
            return result;
        }
    }
}
