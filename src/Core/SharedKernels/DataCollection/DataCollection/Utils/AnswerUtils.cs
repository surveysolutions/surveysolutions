using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    public static class AnswerUtils
    {
        public static int TextAnswerMaxLength => 500;

        public static string AnswerToString(object answer, Func<decimal, string> getCategoricalAnswerOptionText = null, bool isTimestamp = false)
        {
            if (answer == null)
                return string.Empty;

            if (answer is string answerAsString)
                return answerAsString;

            if (answer is int answerAsInteger)
                return answerAsInteger.ToString(CultureInfo.InvariantCulture);

            if (answer is DateTime dateTime)
            {
                return isTimestamp 
                    ? dateTime.ToString(DateTimeFormat.DateWithTimeFormat) 
                    : dateTime.ToString(DateTimeFormat.DateFormat);
            }

            if (answer is decimal decimalAnswer)
            {
                if(getCategoricalAnswerOptionText == null)
                    return decimalAnswer.ToString(CultureInfo.InvariantCulture);
                var optionText = getCategoricalAnswerOptionText(decimalAnswer);

                return String.IsNullOrEmpty(optionText)
                    ? decimalAnswer.ToString(CultureInfo.InvariantCulture)
                    : optionText;
            }

            if (answer is decimal[] multiDecimalAnswer)
            {
                return getCategoricalAnswerOptionText == null
                    ? string.Join(", ", multiDecimalAnswer)
                    : string.Join(", ", multiDecimalAnswer.Select(getCategoricalAnswerOptionText));
            }
            if (answer is int[] multiAnswer)
            {
                return getCategoricalAnswerOptionText == null
                    ? string.Join(", ", multiAnswer)
                    : string.Join(", ", multiAnswer.Select(x => getCategoricalAnswerOptionText(x)));
            }

            if (answer is AnsweredYesNoOption[] yesNoAnswer)
            {
                var yesAnsweredValues = yesNoAnswer.Where(a => a.Yes).Select(a => a.OptionValue);

                return getCategoricalAnswerOptionText == null
                    ? string.Join(", ", yesAnsweredValues)
                    : string.Join(", ", yesAnsweredValues.Select(getCategoricalAnswerOptionText));
            }
            if (answer is decimal[][] multiLinkDecimalAnswer)
            {
                return string.Join("|", multiLinkDecimalAnswer.Select(a => string.Join(", ", a)));
            }
            if (answer is int[][] multiLinkAnswer)
            {
                return string.Join("|", multiLinkAnswer.Select(a => string.Join(", ", a)));
            }
            if (answer is GeoPosition geoAnswer)
            {
                return geoAnswer.ToString();
            }

            if (answer is InterviewTextListAnswers textListAnswers)
            {
                return string.Join("|", (textListAnswers).Answers.Select(x => x.Answer));
            }
            if (answer is Tuple<decimal, string>[])
            {
                var answers = answer as Tuple<decimal, string>[];
                var selectedValues = answers.Select(x => x.Item1).ToArray();
                return AnswerToString(selectedValues, answerOptionValue => answers.Single(x => x.Item1 == answerOptionValue).Item2);
            }

            return answer.ToString();
        }

        public static IEnumerable<CategoricalOption> GetCategoricalOptionsFromQuestion(IQuestion question, int? parentQuestionValue, string filter)
        {
            filter = filter ?? string.Empty;

            if (question.Answers.Any(x => x.AnswerCode.HasValue))
            {
                foreach (var answer in question.Answers)
                {
                    if (answer.AnswerText.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) continue;

                    var categoricalOption = new CategoricalOption
                    {
                        Value = Convert.ToInt32(answer.AnswerCode.Value),
                        Title = answer.AnswerText,
                        ParentValue = answer.ParentCode.HasValue ? Convert.ToInt32(answer.AnswerCode.Value) : (int?)null
                    };

                    if (answer.ParentCode == parentQuestionValue)
                        yield return categoricalOption;
                    else if (parentQuestionValue == null)
                        yield return categoricalOption;
                }
            }
            else
            {
                foreach (var answer in question.Answers)
                {
                    var parentOption = string.IsNullOrEmpty(answer.ParentValue)
                        ? (int?)null
                        : Convert.ToInt32(ParseAnswerOptionParentValueOrThrow(answer.ParentValue, question.PublicKey));

                    if (answer.AnswerText.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) continue;

                    var categoricalOption = new CategoricalOption
                    {
                        Value = Convert.ToInt32(ParseAnswerOptionValueOrThrow(answer.AnswerValue, question.PublicKey)),
                        Title = answer.AnswerText,
                        ParentValue = parentOption
                    };

                    if (parentOption == parentQuestionValue)
                        yield return categoricalOption;
                    else if (parentQuestionValue == null)
                        yield return categoricalOption;
                }
            }
        }

        public static CategoricalOption GetOptionForQuestionByOptionText(IQuestion question, string optionText)
        {
            return question.Answers.SingleOrDefault(x => x.AnswerText == optionText).ToCategoricalOption();
        }

        public static CategoricalOption GetOptionForQuestionByOptionValue(IQuestion question, decimal optionValue)
        {
            if (question.QuestionType == QuestionType.Numeric)
            {
                if (!question.Answers.Any())
                    return null;

                return question.Answers.Any(x => x.AnswerCode.HasValue) ?
                    question.Answers.SingleOrDefault(answer => answer.AnswerCode == optionValue).ToCategoricalOption() :
                    question.Answers.SingleOrDefault(answer => optionValue == ParseAnswerOptionValueOrThrow(answer.AnswerValue, question.PublicKey))
                        .ToCategoricalOption();
            }

            if (question.Answers.Any(x => x.AnswerCode.HasValue))
            {
                return question.Answers.Single(answer => answer.AnswerCode == optionValue).ToCategoricalOption();
            }
            else
            {
                return question.Answers.Single(answer => optionValue == ParseAnswerOptionValueOrThrow(answer.AnswerValue, question.PublicKey))
                    .ToCategoricalOption();
            }

        }

        private static decimal ParseAnswerOptionValueOrThrow(string value, Guid questionId)
        {
            decimal parsedValue;

            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedValue))
                throw new QuestionnaireException(string.Format(
                    "Cannot parse answer option value '{0}' as decimal. Question id: '{1}'.",
                    value, questionId));

            return parsedValue;
        }

        private static decimal ParseAnswerOptionParentValueOrThrow(string value, Guid questionId)
        {
            decimal parsedValue;

            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedValue))
                throw new QuestionnaireException(string.Format(
                    "Cannot parse answer option parent value '{0}' as decimal. Question id: '{1}'.",
                    value, questionId));

            return parsedValue;
        }


        public static string GetPictureFileName(string variableName, RosterVector rosterVector) => $"{variableName}__{rosterVector}.jpg";
    }
}
