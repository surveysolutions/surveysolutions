namespace Main.Core.ExpressionExecutors
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;

    using NCalc;
    using NCalc.Domain;

    /// <summary>
    /// The questionnaire parameters parser.
    /// </summary>
    public class QuestionnaireParametersParser : IExpressionExecutor<QuestionnaireDocument, IList<IQuestion>>
    {
        #region Public Methods and Operators

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Main.Core.Entities.SubEntities.IQuestion].
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public IList<IQuestion> Execute(QuestionnaireDocument entity, string expression)
        {
            IList<IQuestion> result = new List<IQuestion>();
            if (string.IsNullOrEmpty(expression))
            {
                return result;
            }

            var expressionEntity = new Expression(expression);

            expressionEntity.EvaluateParameter += (name, args) =>
                {
                    IQuestion question = entity.FirstOrDefault<IQuestion>(q => q.PublicKey.ToString().Equals(name));
                    if (question == null)
                    {
                        throw new ArgumentOutOfRangeException(string.Format("Parameter {0} is invalid", name));
                    }

                    result.Add(question);
                    args.Result = "0";
                };
            expressionEntity.Evaluate(new EvaluationTesterVisitor(expressionEntity.Options));
            return result;
        }

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.List`1[T -&gt; System.Guid].
        /// </returns>
        public List<Guid> Execute(string expression)
        {
            var result = new List<Guid>();
            if (string.IsNullOrEmpty(expression))
            {
                return result;
            }

            var expressionEntity = new Expression(expression);

            expressionEntity.EvaluateParameter += (name, args) =>
                {
                    try
                    {
                        result.Add(Guid.Parse(name));
                    }
                    catch (FormatException)
                    {
                        // ignore invalid parameters
                    }

                    args.Result = "0";
                };
            try
            {
                expressionEntity.Evaluate(new EvaluationTesterVisitor(expressionEntity.Options));
            }
            catch (EvaluationException)
            {
                // ignore invalid expression
            }

            return result;
        }

        #endregion
    }
}