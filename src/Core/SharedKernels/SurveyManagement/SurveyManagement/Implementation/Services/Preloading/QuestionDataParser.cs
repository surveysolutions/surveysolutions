using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading
{
    internal class QuestionDataParser : IQuestionDataParser
    {
        public KeyValuePair<Guid, object>? Parse(string answer, string variableName, QuestionnaireDocument questionnaire)
        {
            if (string.IsNullOrEmpty(answer))
                return null;
            var question = GetQuestionByVariableName(questionnaire, variableName);
            if (question == null)
                return null;
            if (question.LinkedToQuestionId.HasValue)
                return null;

            switch (question.QuestionType)
            {
                case QuestionType.Text:
                case QuestionType.QRBarcode:
                case QuestionType.TextList:
                    return new KeyValuePair<Guid, object>(question.PublicKey, answer);
                case QuestionType.GpsCoordinates:
                    var parsedAnswer = GeoPosition.Parse(answer);
                    if(parsedAnswer==null)
                        break;
                    return new KeyValuePair<Guid, object>(question.PublicKey, parsedAnswer);
                case QuestionType.AutoPropagate:
                    int intValue;
                    if (int.TryParse(answer, out intValue))
                        return new KeyValuePair<Guid, object>(question.PublicKey, intValue);
                    break;

                case QuestionType.Numeric:
                    var numericQuestion = question as INumericQuestion;
                    if (numericQuestion == null)
                        break;
                    // please don't trust R# warning below. if you simplify expression with '?' then answer would be saved as decimal even for integer question
                    if (numericQuestion.IsInteger)
                    {
                        int intNumericValue;
                        if (int.TryParse(answer, out intNumericValue))
                            return new KeyValuePair<Guid, object>(question.PublicKey, intNumericValue);
                    }
                    else
                    {
                        decimal decimalNumericValue;
                        if (decimal.TryParse(answer, out decimalNumericValue))
                            return new KeyValuePair<Guid, object>(question.PublicKey, decimalNumericValue);
                    }
                    break;

                case QuestionType.DateTime:
                    DateTime date;
                    if (!DateTime.TryParse(answer, out date))
                        break;
                    return new KeyValuePair<Guid, object>(question.PublicKey, date);

                case QuestionType.SingleOption:
                    var singleOption = question as SingleQuestion;
                    if (singleOption != null)
                    {
                        decimal answerValue;
                        if (!decimal.TryParse(answer, out answerValue))
                            break;
                        if (!GetAnswerOptionsAsValues(question).Contains(answerValue))
                            break;
                        return new KeyValuePair<Guid, object>(question.PublicKey, answerValue);
                    }
                    break;

                case QuestionType.MultyOption:
                    var multyOption = question as MultyOptionsQuestion;
                    if (multyOption != null)
                    {
                        decimal answerValue;
                        if (!decimal.TryParse(answer, out answerValue))
                            break;
                        if (!GetAnswerOptionsAsValues(question).Contains(answerValue))
                            break;
                        return new KeyValuePair<Guid, object>(question.PublicKey, answerValue);
                    }
                    break;
            }

            return null;
        }

        public KeyValuePair<Guid, object>? BuildAnswerFromStringArray(string[] answers, string variableName, QuestionnaireDocument questionnaire)
        {
            var typedAnswers = new List<object>();

            foreach (var answer in answers)
            {
                var parsedAnswer = Parse(answer, variableName, questionnaire);
                if (!parsedAnswer.HasValue)
                    continue;
                typedAnswers.Add(parsedAnswer.Value.Value);
            }

            if (typedAnswers.Count == 0)
                return null;

            var question = GetQuestionByVariableName(questionnaire, variableName);
            if (question == null)
                return null;

            switch (question.QuestionType)
            {
                case QuestionType.MultyOption:
                    return new KeyValuePair<Guid, object>(question.PublicKey, typedAnswers.Select(a => (decimal)a).ToArray());
                case QuestionType.TextList:
                    return new KeyValuePair<Guid, object>(question.PublicKey,
                        typedAnswers.Select((a, i) => new Tuple<decimal, string>(i + 1, (string)a)).ToArray());
                default:
                    return new KeyValuePair<Guid, object>(question.PublicKey, typedAnswers.First());
            }
        }

        private IQuestion GetQuestionByVariableName(QuestionnaireDocument questionnaire, string variableName)
        {
            return questionnaire.FirstOrDefault<IQuestion>(q => q.StataExportCaption.Equals(variableName, StringComparison.InvariantCultureIgnoreCase));
        }

        private decimal[] GetAnswerOptionsAsValues(IQuestion question)
        {
            return
                question.Answers.Select(answer => this.ParseAnswerOptionValueOrThrow(answer.AnswerValue))
                    .Where(o => o.HasValue)
                    .Select(o => o.Value)
                    .ToArray();
        }

        private decimal? ParseAnswerOptionValueOrThrow(string value)
        {
            decimal parsedValue;

            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedValue))
                return null;

            return parsedValue;
        }
    }
}