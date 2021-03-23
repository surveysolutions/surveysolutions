using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
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
                var noAnsweredValues = yesNoAnswer.Where(a => !a.Yes).Select(a => a.OptionValue);

                var yesAnswers = getCategoricalAnswerOptionText == null
                    ? string.Join(", ", yesAnsweredValues)
                    : string.Join(", ", yesAnsweredValues.Select(getCategoricalAnswerOptionText));
                
                var noAnswers = getCategoricalAnswerOptionText == null
                        ? string.Join(", ", noAnsweredValues)
                        : string.Join(", ", noAnsweredValues.Select(getCategoricalAnswerOptionText));
                
                return $"{yesAnswers}|{noAnswers}";

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

        public static IEnumerable<CategoricalOption> GetCategoricalOptionsFromQuestion(IQuestion question, int? parentQuestionValue, string filter, int[] excludeOptionIds = null)
        {
            filter = filter ?? string.Empty;

            foreach (var answer in question.Answers)
            {
                if (answer.AnswerText.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) continue;

                var categoricalOption = new CategoricalOption
                {
                    Title = answer.AnswerText, 
                    Value = (int)answer.GetParsedValue(),
                    ParentValue = answer.GetParsedParentValue()
                };

                if(excludeOptionIds?.Contains(categoricalOption.Value) ?? false) continue;
                if (categoricalOption.ParentValue == parentQuestionValue || parentQuestionValue == null)
                    yield return categoricalOption;
            }
        }

        public static CategoricalOption GetOptionForQuestionByOptionText(IQuestion question, string optionText)
        {
            return question.Answers.SingleOrDefault(x => x.AnswerText == optionText).ToCategoricalOption();
        }

        public static CategoricalOption GetOptionForQuestionByOptionValue(IQuestion question, decimal optionValue, decimal? parentValue) =>
            question.Answers.Find(answer => answer.GetParsedValue() == optionValue &&
                                            (question.CascadeFromQuestionId.HasValue && answer.GetParsedParentValue() == parentValue ||
                                             !question.CascadeFromQuestionId.HasValue))
                ?.ToCategoricalOption();

        public static string GetPictureFileName(string variableName, RosterVector rosterVector) => $"{variableName}__{rosterVector}.jpg";
    }
}
