using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.View;
using WB.UI.Designer.Views.Questionnaire;

namespace WB.UI.Designer.Utils
{
    public class ExpressionReplacer : IExpressionReplacer
    {
        /// <summary>
        /// The view repository.
        /// </summary>
        private readonly IViewRepository _viewRepository;

        public ExpressionReplacer(IViewRepository viewRepository)
        {
            this._viewRepository = viewRepository;
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
            foreach (var pair in this.LoadMap(questionnaireKey).StataMap)
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
            foreach (var pair in this.LoadMap(questionnaireKey).StataMap)
            {
                var key = pair.Key.ToString();
                if (string.IsNullOrWhiteSpace(key) || map.ContainsKey(key))
                    continue;
                map.Add(key, pair.Value);
            }
            return MakeSubstitutions(expression, map);
        }

        private QuestionnaireStataMapView LoadMap(Guid questionnaireKey)
        {
            return this._viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireStataMapView>(new QuestionnaireViewInputModel(questionnaireKey));
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