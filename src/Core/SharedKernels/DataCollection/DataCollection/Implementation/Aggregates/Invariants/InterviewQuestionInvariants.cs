using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants
{
    internal class InterviewQuestionInvariants
    {
        private IQuestionOptionsRepository QuestionOptionsRepository => ServiceLocator.Current.GetInstance<IQuestionOptionsRepository>();

        public InterviewQuestionInvariants(Identity questionIdentity, IQuestionnaire questionnaire, InterviewTree interviewTree)
        {
            this.QuestionIdentity = questionIdentity;
            this.Questionnaire = questionnaire;
            this.InterviewTree = interviewTree;
        }

        public Identity QuestionIdentity { get; }
        private IQuestionnaire Questionnaire { get; }
        private InterviewTree InterviewTree { get; }

        private string InterviewId => this.InterviewTree.InterviewId;
        private Guid QuestionId => this.QuestionIdentity.Id;

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

        public InterviewQuestionInvariants RequireOptionExists(decimal value)
        {
            var availableValues = this.Questionnaire.GetOptionForQuestionByOptionValue(this.QuestionId, value);

            if (availableValues == null)
                throw new AnswerNotAcceptedException(
                    $"For question {this.FormatQuestionForException()} was provided selected value {value} as answer. {this.InfoForException}");

            return this;
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

        public InterviewQuestionInvariants RequireAllowedDecimalPlaces(double answer)
        {
            int? countOfDecimalPlacesAllowed = this.Questionnaire.GetCountOfDecimalPlacesAllowedByQuestion(this.QuestionId);
            if (!countOfDecimalPlacesAllowed.HasValue)
                return this;

            var roundedAnswer = Math.Round(answer, countOfDecimalPlacesAllowed.Value);
            if (roundedAnswer != answer)
                throw new AnswerNotAcceptedException(
                    $"Answer '{answer}' for question {this.FormatQuestionForException()}  is incorrect because has more decimal places than allowed by questionnaire. Allowed amount of decimal places is {countOfDecimalPlacesAllowed.Value}. {this.InfoForException}");

            return this;
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

        public InterviewQuestionInvariants RequireRosterSizeAnswerRespectsMaxRosterRowCount(int answer, int maxRosterRowCount)
        {
            if (!this.Questionnaire.ShouldQuestionSpecifyRosterSize(this.QuestionId))
                return this;

            if (answer > maxRosterRowCount)
                throw new AnswerNotAcceptedException(
                    $"Answer '{answer}' for question {this.FormatQuestionForException()} is incorrect because question is used as size of roster and specified answer is greater than {maxRosterRowCount}. {this.InfoForException}");

            return this;
        }

        public InterviewQuestionInvariants RequireQuestionInstanceExists()
        {
            if (this.QuestionIdentity.RosterVector == null)
                throw new InterviewException(
                    $"Roster information for question is missing. " +
                    $"Roster vector cannot be null. " +
                    $"Question ID: {this.QuestionIdentity.Id.FormatGuid()}. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");

            var questions = this.InterviewTree.FindQuestions(this.QuestionIdentity.Id);
            var rosterVectors = questions.Select(question => question.Identity.RosterVector).ToList();

            if (!rosterVectors.Contains(this.QuestionIdentity.RosterVector))
                throw new InterviewException(
                    $"Roster information for question is incorrect. " +
                    $"No questions found for roster vector {this.QuestionIdentity.RosterVector}. " +
                    $"Available roster vectors: {string.Join(", ", rosterVectors)}. " +
                    $"Question ID: {this.QuestionIdentity.Id.FormatGuid()}. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");

            return this;
        }

        public InterviewQuestionInvariants RequireQuestionIsEnabled()
        {
            var question = this.InterviewTree.GetQuestion(this.QuestionIdentity);

            if (question.IsDisabled())
                throw new InterviewException(
                    $"Question {question.FormatForException()} (or it's parent) is disabled " +
                    $"and question's answer cannot be changed. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");

            return this;
        }

        public void RequireLinkedOptionIsAvailable(RosterVector option)
        {
            var question = this.InterviewTree.GetQuestion(this.QuestionIdentity);

            if (!question.AsLinked.Options.Contains(option))
                throw new InterviewException(
                    $"Answer on linked categorical question {question.FormatForException()} cannot be saved. " +
                    $"Specified option {option} is absent. " +
                    $"Available options: {string.Join(", ", question.AsLinked.Options)}. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");
        }

        public InterviewQuestionInvariants RequireLinkedToListOptionIsAvailable(decimal option)
        {
            var question = this.InterviewTree.GetQuestion(this.QuestionIdentity);

            if (!question.AsLinkedToList.Options.Contains(option))
                throw new InterviewException(
                    $"Answer on linked to list question {question.FormatForException()} cannot be saved. " +
                    $"Specified option {option} is absent. " +
                    $"Available options: {string.Join(", ", question.AsLinked.Options)}. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");

            return this;
        }

        public InterviewQuestionInvariants RequireCascadingQuestionAnswerCorrespondsToParentAnswer(decimal answer, QuestionnaireIdentity questionnaireId, Translation translation)
        {
            var question = this.InterviewTree.GetQuestion(this.QuestionIdentity);

            if (!question.IsCascading)
                return this;

            var answerOption = this.QuestionOptionsRepository.GetOptionForQuestionByOptionValue(questionnaireId,
                this.QuestionIdentity.Id, answer, translation);

            if (!answerOption.ParentValue.HasValue)
                throw new QuestionnaireException(
                    $"Answer option has no parent value. Option value: {answer}, Question id: '{this.QuestionIdentity.Id}'.");

            int answerParentValue = answerOption.ParentValue.Value;
            var parentQuestion = question.AsCascading.GetCascadingParentQuestion();

            if (!parentQuestion.IsAnswered)
                return this;

            int actualParentValue = parentQuestion.GetAnswer().SelectedValue;

            if (answerParentValue != actualParentValue)
                throw new AnswerNotAcceptedException(
                    $"For question {question.FormatForException()} was provided " +
                    $"selected value {answer} as answer with parent value {answerParentValue}, " +
                    $"but this do not correspond to the parent answer selected value {actualParentValue}. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");

            return this;
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