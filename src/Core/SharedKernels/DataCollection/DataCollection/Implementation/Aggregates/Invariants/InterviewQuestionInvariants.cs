using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants
{
    internal class InterviewQuestionInvariants
    {
        public InterviewQuestionInvariants(string interviewId, Guid questionId, IQuestionnaire questionnaire)
        {
            this.InterviewId = interviewId;
            this.QuestionId = questionId;
            this.Questionnaire = questionnaire;
        }

        private string InterviewId { get; }
        private Guid QuestionId { get; }
        private IQuestionnaire Questionnaire { get; }

        private string InfoForException => $"Question ID: {this.QuestionId.FormatGuid()}. Interview ID: {this.InterviewId}.";

        public void RequireQuestionExists()
        {
            if (!this.Questionnaire.HasQuestion(this.QuestionId))
                throw new InterviewException(
                    $"Question is missing. {this.InfoForException}");
        }

        public void RequireQuestionType(params QuestionType[] expectedQuestionTypes)
        {
            QuestionType actualQuestionType = this.Questionnaire.GetQuestionType(this.QuestionId);

            if (!expectedQuestionTypes.Contains(actualQuestionType))
                throw new AnswerNotAcceptedException(
                    $"Question {this.FormatQuestionForException()} has type {actualQuestionType}. " +
                    $"But one of the following types was expected: {JoinUsingCommas(expectedQuestionTypes)}. " +
                    this.InfoForException);
        }

        public void RequireNumericRealQuestion()
        {
            if (this.Questionnaire.IsQuestionInteger(this.QuestionId))
                throw new AnswerNotAcceptedException(
                    $"Question {this.FormatQuestionForException()} doesn't support answer of type real. {this.InfoForException}");
        }

        public void RequireNumericIntegerQuestion()
        {
            if (!this.Questionnaire.IsQuestionInteger(this.QuestionId))
                throw new AnswerNotAcceptedException(
                    $"Question {this.FormatQuestionForException()} doesn't support answer of type integer. {this.InfoForException}");
        }

        public void ThrowIfAnswersExceedsMaxAnswerCountLimit(Tuple<decimal, string>[] answers, int? maxAnswersCountLimit)
        {
            if (maxAnswersCountLimit.HasValue && answers.Length > maxAnswersCountLimit.Value)
                throw new InterviewException(
                    $"Answers exceeds MaxAnswerCount limit for question {this.FormatQuestionForException()}. {this.InfoForException}");
        }

        public void ThrowIfStringValueAreEmptyOrWhitespaces(Tuple<decimal, string>[] answers)
        {
            if (answers.Any(x => string.IsNullOrWhiteSpace(x.Item2)))
                throw new InterviewException(
                    $"String values should be not empty or whitespaces for question {this.FormatQuestionForException()}. {this.InfoForException}");
        }

        public void ThrowIfDecimalValuesAreNotUnique(Tuple<decimal, string>[] answers)
        {
            var decimals = answers.Select(x => x.Item1).Distinct().ToArray();

            if (answers.Length > decimals.Length)
                throw new InterviewException(
                    $"Decimal values should be unique for question {this.FormatQuestionForException()}. {this.InfoForException}");
        }

        public void ThrowIfValueIsNotOneOfAvailableOptions(decimal value)
        {
            var availableValues = this.Questionnaire.GetOptionForQuestionByOptionValue(this.QuestionId, value);

            if (availableValues == null)
                throw new AnswerNotAcceptedException(
                    $"For question {this.FormatQuestionForException()} was provided selected value {value} as answer. {this.InfoForException}");
        }

        public void ThrowIfSomeValuesAreNotFromAvailableOptions(IReadOnlyCollection<int> values)
        {
            IEnumerable<decimal> availableValues = this.Questionnaire.GetMultiSelectAnswerOptionsAsValues(this.QuestionId);

            bool someValueIsNotOneOfAvailable = values.Any(value => !availableValues.Contains(value));
            if (someValueIsNotOneOfAvailable)
                throw new AnswerNotAcceptedException(
                    $"For question {this.FormatQuestionForException()} were provided selected values {JoinUsingCommas(values)} as answer. But only following values are allowed: {JoinUsingCommas(availableValues)}. {this.InfoForException}");
        }

        public void ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(int answersCount)
        {
            int? maxSelectedOptions = this.Questionnaire.GetMaxSelectedAnswerOptions(this.QuestionId);

            if (maxSelectedOptions.HasValue && maxSelectedOptions > 0 && answersCount > maxSelectedOptions)
                throw new AnswerNotAcceptedException(
                    $"For question {this.FormatQuestionForException()} number of answers is greater than the maximum number of selected answers. {this.InfoForException}");
        }

        public void ThrowIfAnswerHasMoreDecimalPlacesThenAccepted(double answer)
        {
            int? countOfDecimalPlacesAllowed = this.Questionnaire.GetCountOfDecimalPlacesAllowedByQuestion(this.QuestionId);
            if (!countOfDecimalPlacesAllowed.HasValue)
                return;

            var roundedAnswer = Math.Round(answer, countOfDecimalPlacesAllowed.Value);
            if (roundedAnswer != answer)
                throw new AnswerNotAcceptedException(
                    $"Answer '{answer}' for question {this.FormatQuestionForException()}  is incorrect because has more decimal places than allowed by questionnaire. Allowed amount of decimal places is {countOfDecimalPlacesAllowed.Value}. {this.InfoForException}");
        }

        public void ThrowIfRosterSizeAnswerIsNegativeOrGreaterThenMaxRosterRowCount(int answer)
        {
            if (answer < 0)
                throw new AnswerNotAcceptedException(
                    $"Answer '{answer}' for question {this.FormatQuestionForException()} is incorrect because question is used as size of roster and specified answer is negative. {this.InfoForException}");
        }

        public void ThrowIfRosterSizeAnswerIsGreaterThenMaxRosterRowCount(int answer, int maxRosterRowCount)
        {
            if (answer > maxRosterRowCount)
                throw new AnswerNotAcceptedException(
                    $"Answer '{answer}' for question {this.FormatQuestionForException()} is incorrect because question is used as size of roster and specified answer is greater than {maxRosterRowCount}. {this.InfoForException}");
        }

        private string FormatQuestionForException()
            => $"'{this.GetQuestionTitleForException()} [{this.GetQuestionVariableNameForException()}]'";

        private string GetQuestionTitleForException()
            => this.Questionnaire.HasQuestion(this.QuestionId)
                ? this.Questionnaire.GetQuestionTitle(this.QuestionId) ?? "<<NO TITLE>>"
                : "<<MISSING>>";

        private string GetQuestionVariableNameForException()
            => this.Questionnaire.HasQuestion(this.QuestionId)
                ? this.Questionnaire.GetQuestionVariableName(this.QuestionId) ?? "<<NO VARIABLE>>"
                : "<<MISSING>>";

        private static string JoinUsingCommas(IEnumerable<QuestionType> values)
            => JoinUsingCommas(values.Select(value => value.ToString()));

        private static string JoinUsingCommas(IEnumerable<decimal> values)
            => JoinUsingCommas(values.Select(value => value.ToString(CultureInfo.InvariantCulture)));

        private static string JoinUsingCommas(IEnumerable<int> values)
            => JoinUsingCommas(values.Select(value => value.ToString(CultureInfo.InvariantCulture)));

        private static string JoinUsingCommas(IEnumerable<string> values) => string.Join(", ", values);
    }
}