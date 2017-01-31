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

        public InterviewQuestionInvariants RequireQuestion(QuestionType? questionType = null)
        {
            this.RequireQuestionExists();

            if (questionType.HasValue)
            {
                this.RequireQuestionType(questionType.Value);
            }

            return this;
        }

        private void RequireQuestionExists()
        {
            if (!this.Questionnaire.HasQuestion(this.QuestionId))
                throw new InterviewException(
                    $"Question is missing. {this.InfoForException}");
        }

        private void RequireQuestionType(QuestionType expectedQuestionType)
        {
            QuestionType actualQuestionType = this.Questionnaire.GetQuestionType(this.QuestionId);

            if (expectedQuestionType != actualQuestionType)
                throw new AnswerNotAcceptedException(
                    $"Question {this.FormatQuestionForException()} has type {actualQuestionType}. " +
                    $"But following type was expected: {expectedQuestionType}. " +
                    this.InfoForException);
        }

        public InterviewQuestionInvariants RequireNumericRealQuestion()
        {
            if (this.Questionnaire.IsQuestionInteger(this.QuestionId))
                throw new AnswerNotAcceptedException(
                    $"Question {this.FormatQuestionForException()} doesn't support answer of type real. {this.InfoForException}");

            return this;
        }

        public InterviewQuestionInvariants RequireNumericIntegerQuestion()
        {
            if (!this.Questionnaire.IsQuestionInteger(this.QuestionId))
                throw new AnswerNotAcceptedException(
                    $"Question {this.FormatQuestionForException()} doesn't support answer of type integer. {this.InfoForException}");

            return this;
        }

        public void RequireMaxAnswersCountLimit(Tuple<decimal, string>[] answers, int? maxAnswersCountLimit)
        {
            if (maxAnswersCountLimit.HasValue && answers.Length > maxAnswersCountLimit.Value)
                throw new InterviewException(
                    $"Answers exceed MaxAnswerCount limit {maxAnswersCountLimit.Value} for question {this.FormatQuestionForException()}. {this.InfoForException}");
        }

        public void RequireNotEmptyTexts(Tuple<decimal, string>[] answers)
        {
            if (answers.Any(x => string.IsNullOrWhiteSpace(x.Item2)))
                throw new InterviewException(
                    $"String values should be not empty or whitespaces for question {this.FormatQuestionForException()}. {this.InfoForException}");
        }

        public void RequireUniqueValues(Tuple<decimal, string>[] answers)
        {
            var decimals = answers.Select(x => x.Item1).Distinct().ToArray();

            if (answers.Length > decimals.Length)
                throw new InterviewException(
                    $"Decimal values should be unique for question {this.FormatQuestionForException()}. {this.InfoForException}");
        }

        public void RequireOptionExists(decimal value)
        {
            var availableValues = this.Questionnaire.GetOptionForQuestionByOptionValue(this.QuestionId, value);

            if (availableValues == null)
                throw new AnswerNotAcceptedException(
                    $"For question {this.FormatQuestionForException()} was provided selected value {value} as answer. {this.InfoForException}");
        }

        public void RequireOptionsExist(IReadOnlyCollection<int> values)
        {
            IEnumerable<decimal> availableValues = this.Questionnaire.GetMultiSelectAnswerOptionsAsValues(this.QuestionId);

            bool someValueIsNotOneOfAvailable = values.Any(value => !availableValues.Contains(value));
            if (someValueIsNotOneOfAvailable)
                throw new AnswerNotAcceptedException(
                    $"For question {this.FormatQuestionForException()} were provided selected values {JoinUsingCommas(values)} as answer. But only following values are allowed: {JoinUsingCommas(availableValues)}. {this.InfoForException}");
        }

        public void RequireMaxAnswersCountLimit(int answersCount)
        {
            int? maxSelectedOptions = this.Questionnaire.GetMaxSelectedAnswerOptions(this.QuestionId);

            if (maxSelectedOptions.HasValue && maxSelectedOptions > 0 && answersCount > maxSelectedOptions)
                throw new AnswerNotAcceptedException(
                    $"For question {this.FormatQuestionForException()} number of answers is greater than the maximum number of selected answers. {this.InfoForException}");
        }

        public void RequireAllowedDecimalPlaces(double answer)
        {
            int? countOfDecimalPlacesAllowed = this.Questionnaire.GetCountOfDecimalPlacesAllowedByQuestion(this.QuestionId);
            if (!countOfDecimalPlacesAllowed.HasValue)
                return;

            var roundedAnswer = Math.Round(answer, countOfDecimalPlacesAllowed.Value);
            if (roundedAnswer != answer)
                throw new AnswerNotAcceptedException(
                    $"Answer '{answer}' for question {this.FormatQuestionForException()}  is incorrect because has more decimal places than allowed by questionnaire. Allowed amount of decimal places is {countOfDecimalPlacesAllowed.Value}. {this.InfoForException}");
        }

        public InterviewQuestionInvariants RequireRosterSizeAnswerNotNegative(int answer)
        {
            if (!this.Questionnaire.ShouldQuestionSpecifyRosterSize(this.QuestionId))
                return this;

            if (answer < 0)
                throw new AnswerNotAcceptedException(
                    $"Answer '{answer}' for question {this.FormatQuestionForException()} is incorrect because question is used as size of roster and specified answer is negative. {this.InfoForException}");

            return this;
        }

        public void RequireRosterSizeAnswerRespectsMaxRosterRowCount(int answer, int maxRosterRowCount)
        {
            if (!this.Questionnaire.ShouldQuestionSpecifyRosterSize(this.QuestionId))
                return;

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