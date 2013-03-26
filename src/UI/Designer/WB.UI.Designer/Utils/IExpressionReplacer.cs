namespace WB.UI.Designer.Utils
{
    using System;

    public interface IExpressionReplacer 
    {
        /// <summary>
        /// Replaces all occurrences of stata captions in expression with public keys (guids) 
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
        string ReplaceStataCaptionsWithGuids(string expression, Guid questionnaireKey);

        /// <summary>
        /// Replaces all occurrences question public keys in expression with stata caption
        /// </summary>
        /// <param name="expression">
        ///  Condition or validation expression to decode
        /// </param>
        /// <param name="questionnaireKey">
        /// Decode expression with stata captions instead of public keys
        /// </param>
        /// <returns></returns>
        string ReplaceGuidsWithStataCaptions(string expression, Guid questionnaireKey);
    }
}