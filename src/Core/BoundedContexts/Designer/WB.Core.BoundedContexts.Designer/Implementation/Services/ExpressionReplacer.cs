using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class ExpressionReplacer : IExpressionReplacer
    {
        private readonly QuestionnaireStataMapView variableMap;

        public ExpressionReplacer(IQuestionnaireDocument questionnaire)
        {
            var questionnaireDocument = questionnaire as QuestionnaireDocument;
            if (questionnaireDocument != null)
                this.variableMap = new QuestionnaireStataMapView(questionnaireDocument);
        }

        /// <summary>
        /// Replaces all occurences of stata captions in expression with public keys (guids) 
        /// </summary>
        /// <param name="expression">
        /// Condition or validation expression to encode
        /// </param>
        /// <param name="questionnaireKey">
        /// Questionnaire public key
        /// </param>
        /// <returns>
        /// Encoded expression with public keys instead of stata captions
        /// </returns>
        public string ReplaceStataCaptionsWithGuids(string expression, Guid questionnaireKey)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return expression;
            var map = new Dictionary<string, string>();
            foreach (var pair in this.variableMap.StataMap)
            {
                if (string.IsNullOrWhiteSpace(pair.Value) || map.ContainsKey(pair.Value))
                    continue;
                map.Add(pair.Value, pair.Key.ToString());
            }
            return MakeSubstitutions(expression, map);
        }

        /// <summary>
        /// Replaces all occurences question public keys in expression with stata caption
        /// </summary>
        /// <param name="expression">
        ///  Condition or validation expression to decode
        /// </param>
        /// <param name="questionnaireKey">
        /// Decode expression with stata captions instead of public keys
        /// </param>
        /// <returns></returns>
        public string ReplaceGuidsWithStataCaptions(string expression, Guid questionnaireKey)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return expression;
            var map = new Dictionary<string, string>();
            foreach (var pair in this.variableMap.StataMap)
            {
                var key = pair.Key.ToString();
                if (string.IsNullOrWhiteSpace(key) || map.ContainsKey(key))
                    continue;
                map.Add(key, pair.Value);
            }
            return MakeSubstitutions(expression, map);
        }

        private static string MakeSubstitutions(string expression, IEnumerable<KeyValuePair<string, string>> map)
        {
            foreach (var pair in map)
            {
                expression = expression.Replace(string.Format("[{0}]", pair.Key), string.Format("[{0}]", pair.Value));
            }
            return expression;
        }
    }
}