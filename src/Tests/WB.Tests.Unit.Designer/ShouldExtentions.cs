using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer
{
    internal static class ShouldExtensions
    {
        public static void ShouldContainValues(this QuestionTemplateModel question,
            Guid id,
            string variableName,
            string conditions,
            string validations,
            QuestionType questionType,
            string generatedIdName,
            string generatedTypeName,
            string generatedMemberName,
            string generatedStateName,
            string rosterScopeName,
            string generatedValidationsMethodName,
            string generatedConditionsMethodName)
        {
            question.Id.Should().Be(id);
            question.VariableName.Should().Be(variableName);
            question.Condition.Should().Be(conditions);
            question.IdName.Should().Be(generatedIdName);
            question.TypeName.Should().Be(generatedTypeName);
            question.MemberName.Should().Be(generatedMemberName);
            question.StateName.Should().Be(generatedStateName);
            question.RosterScopeName.Should().Be(rosterScopeName);
            question.ConditionMethodName.Should().Be(generatedConditionsMethodName);
        }

        public static void ShouldContainWarning(this IEnumerable<QuestionnaireVerificationMessage> messages, string code, string message = null)
        {
            if (message == null)
            {
                messages
                    .Where(m => m.MessageLevel == VerificationMessageLevel.Warning)
                    .Select(m => m.Code)
                    .Should().Contain(code);
            }
            else
            {
                messages.Should().Contain(m
                    => m.MessageLevel == VerificationMessageLevel.Warning
                    && m.Code == code
                    && m.Message == message);
            }
        }

        public static void ShouldContainError(this IEnumerable<QuestionnaireVerificationMessage> messages, string code)
            => messages
                .Where(m => m.MessageLevel == VerificationMessageLevel.General)
                .Select(m => m.Code)
                .Should().Contain(code);

        public static void ShouldContainCritical(
            this IEnumerable<QuestionnaireVerificationMessage> verificationMessages, string code)
        {
            verificationMessages.Should().Contain(message
                => message.MessageLevel == VerificationMessageLevel.Critical
                && message.Code == code);
        }

        public static void ShouldNotContainWarning(this IEnumerable<QuestionnaireVerificationMessage> messages, string code)
        {
            var warnings = messages
                .Where(m => m.MessageLevel == VerificationMessageLevel.Warning)
                .Where(m => m.Code == code)
                .ToList();

            if (warnings.Any())
                throw new Exception(
                    $"Contains one or more warnings {code} but shouldn't:{Environment.NewLine}{FormatForAssertion(warnings)}");
        }

        public static void ShouldNotContainError(this IEnumerable<QuestionnaireVerificationMessage> messages, string code)
        {
            var errors = messages
                .Where(m => m.MessageLevel == VerificationMessageLevel.General)
                .Where(m => m.Code == code)
                .ToList();

            if (errors.Any())
                throw new Exception(
                    $"Contains one or more errors {code} but shouldn't:{Environment.NewLine}{FormatForAssertion(errors)}");
        }

        public static void ShouldNotContain(this IEnumerable<QuestionnaireVerificationMessage> verificationMessages, string code)
        {
            var messages = verificationMessages
                .Where(m => m.Code == code)
                .ToList();

            if (messages.Any())
                throw new Exception(
                    $"Contains one or more message {code} but shouldn't:{Environment.NewLine}{FormatForAssertion(messages)}");
        }

        public static void ShouldNotContainMessage(this IEnumerable<QuestionnaireVerificationMessage> verificationMessages, string code)
            => verificationMessages.ShouldNotContain(code);

        private static string FormatForAssertion(IEnumerable<QuestionnaireVerificationMessage> warnings)
            => string.Join(Environment.NewLine, warnings.Select(FormatForAssertion));

        private static string FormatForAssertion(QuestionnaireVerificationMessage message, int index)
            => $"{index + 1}. {message.MessageLevel} {message.Code}{Environment.NewLine}{FormatForAssertion(message.References)}";

        private static string FormatForAssertion(IEnumerable<QuestionnaireEntityReference> references)
            => string.Join(Environment.NewLine, references.Select(FormatForAssertion));

        private static string FormatForAssertion(QuestionnaireEntityReference reference)
            => $"  {reference.Type} {reference.ItemId}";
    }
}
