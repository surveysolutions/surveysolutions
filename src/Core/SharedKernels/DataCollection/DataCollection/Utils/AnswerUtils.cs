using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    public static class AnswerUtils
    {
        public static string AnswerToString(object answer, Func<decimal, string> getCategoricalAnswerOptionText = null)
        {
            if (answer == null)
                return string.Empty;

            if (answer is string)
                return (string) answer;

            if (answer is int)
                return ((int) answer).ToString(CultureInfo.InvariantCulture);

            if (answer is DateTime)
                return ((DateTime)answer).ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern, CultureInfo.InvariantCulture);

            if (answer is decimal)
            {
                var decimalAnswer = (decimal) answer;

                return getCategoricalAnswerOptionText == null
                    ? decimalAnswer.ToString(CultureInfo.InvariantCulture)
                    : getCategoricalAnswerOptionText(decimalAnswer);
            }

            if (answer is decimal[])
            {
                var multiAnswer = (decimal[]) answer;

                return getCategoricalAnswerOptionText == null
                    ? string.Join(", ", multiAnswer)
                    : string.Join(", ", multiAnswer.Select(getCategoricalAnswerOptionText));
            }
            if (answer is AnsweredYesNoOption[])
            {
                var yesNoAnswer = (AnsweredYesNoOption[])answer;
                var yesAnsweredValues = yesNoAnswer.Where(a => a.Yes).Select(a => a.OptionValue);

                return getCategoricalAnswerOptionText == null
                    ? string.Join(", ", yesAnsweredValues)
                    : string.Join(", ", yesAnsweredValues.Select(getCategoricalAnswerOptionText));
            }
            if (answer is decimal[][])
            {
                var multiLinkAnswer = (decimal[][])answer;
                return string.Join("|", multiLinkAnswer.Select(a => string.Join(", ", a)));
            }
            if (answer is GeoPosition)
            {
                return ((GeoPosition) answer).ToString();
            }

            if (answer is GeoLocationPoint)
            {
                var geoAnswer = answer as GeoLocationPoint;
                return string.Format(CultureInfo.InvariantCulture, "[{0};{1}]", geoAnswer.Latitude, geoAnswer.Longitude);
            }
            if (answer is InterviewTextListAnswers)
            {
                return string.Join("|", ((InterviewTextListAnswers) answer).Answers.Select(x => x.Answer));
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
                    if (answer.AnswerText.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 &&
                        answer.ParentCode == parentQuestionValue)
                        yield return
                            new CategoricalOption()
                            {
                                Value = Convert.ToInt32(answer.AnswerCode.Value),
                                Title = answer.AnswerText,
                                ParentValue =
                                    answer.ParentCode.HasValue ? Convert.ToInt32(answer.AnswerCode.Value) : (int?)null
                            };
                }
            }
            else
            {
                foreach (var answer in question.Answers)
                {
                    var parentOption = string.IsNullOrEmpty(answer.ParentValue)
                        ? (int?)null
                        : Convert.ToInt32(ParseAnswerOptionParentValueOrThrow(answer.ParentValue, question.PublicKey));

                    if (answer.AnswerText.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 &&
                        parentOption == parentQuestionValue)
                        yield return
                            new CategoricalOption()
                            {
                                Value = Convert.ToInt32(ParseAnswerOptionValueOrThrow(answer.AnswerValue, question.PublicKey)),
                                Title = answer.AnswerText,
                                ParentValue = parentOption
                            };
                }
            }
        }

        public static CategoricalOption GetOptionForQuestionByOptionText(IQuestion question, string optionText)
        {
            return question.Answers.SingleOrDefault(x => x.AnswerText == optionText).ToCategoricalOption();
        }

        public static CategoricalOption GetOptionForQuestionByOptionValue(IQuestion question, decimal optionValue)
        {
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
    }
}
