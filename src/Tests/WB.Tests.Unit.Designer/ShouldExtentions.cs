using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.Infrastructure.EventBus;

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
            question.Id.ShouldEqual(id);
            question.VariableName.ShouldEqual(variableName);
            question.Condition.ShouldEqual(conditions);

            //question.ValidationExpressions.FirstOrDefault().ValidationExpression.ShouldEqual(validations);

            question.IdName.ShouldEqual(generatedIdName);
            question.TypeName.ShouldEqual(generatedTypeName);
            question.MemberName.ShouldEqual(generatedMemberName);
            question.StateName.ShouldEqual(generatedStateName);
            question.RosterScopeName.ShouldEqual(rosterScopeName);
            //question.ValidationExpressions.FirstOrDefault().ValidationMethodName.ShouldEqual(generatedValidationsMethodName);
            question.ConditionMethodName.ShouldEqual(generatedConditionsMethodName);
        }

        public static void ShouldContainEvent<TEvent>(this EventContext eventContext, Func<TEvent, bool> condition = null)
            where TEvent : IEvent
        {
            if (condition == null)
            {
                eventContext.Events.ShouldContain(@event
                    => @event.Payload is TEvent);
            }
            else
            {
                eventContext.Events.ShouldContain(@event
                    => @event.Payload is TEvent
                        && condition.Invoke((TEvent)@event.Payload));
            }
        }

        public static void ShouldNotContainEvent<TEvent>(this EventContext eventContext, Func<TEvent, bool> condition = null)
            where TEvent : IEvent
        {
            if (condition == null)
            {
                eventContext.Events.ShouldNotContain(@event
                    => @event.Payload is TEvent);
            }
            else
            {
                eventContext.Events.ShouldNotContain(@event
                    => @event.Payload is TEvent
                        && condition.Invoke((TEvent)@event.Payload));
            }
        }

        public static void ShouldContainWarning(
            this IEnumerable<QuestionnaireVerificationMessage> verificationMessages, string code, string warningMessage =null)
        {
            verificationMessages.ShouldContain(message
                => message.MessageLevel == VerificationMessageLevel.Warning
                   && message.Code == code && (warningMessage == null || message.Message == warningMessage));
        }

        public static void ShouldContainError(
            this IEnumerable<QuestionnaireVerificationMessage> verificationMessages, string code)
        {
            verificationMessages.ShouldContain(message
                => message.MessageLevel == VerificationMessageLevel.General
                && message.Code == code);
        }

        public static void ShouldNotContain(
            this IEnumerable<QuestionnaireVerificationMessage> verificationMessages, string code)
        {
            verificationMessages.ShouldNotContain(message => message.Code == code);
        }

        public static void ShouldContainCritical(
            this IEnumerable<QuestionnaireVerificationMessage> verificationMessages, string code)
        {
            verificationMessages.ShouldContain(message
                => message.MessageLevel == VerificationMessageLevel.Critical
                && message.Code == code);
        }

        public static void ShouldNotContainMessage(
            this IEnumerable<QuestionnaireVerificationMessage> verificationMessages, string code)
        {
            verificationMessages.ShouldNotContain(message
                => message.Code == code);
        }
    }
}
