using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.Infrastructure.EventBus;
using WB.Tests.Unit.SharedKernels.DataCollection.InterviewExpressionStateTests;

namespace WB.Tests.Unit
{
    internal static class ShouldExtensions
    {
        [DebuggerStepThrough]
        public static void ShouldMatchMethodInfo(this MetodInfo left, MetodInfo right)
        {
            left.Name.ShouldEqual(right.Name);
            left.ReturnType.ShouldEqual(right.ReturnType);
            left.ParamsType.ShouldContainOnly(right.ParamsType);
        }

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public static void ShouldContainEvents<TEvent>(this EventContext eventContext, int count)
        {
            eventContext.Events.Count(e => e.Payload is TEvent).ShouldEqual(count);
        }

        [DebuggerStepThrough]
        public static void ShouldContainEvent<TEvent>(this EventContext eventContext, Func<TEvent, bool> condition = null)
            where TEvent : IEvent
        {
            if (condition == null)
            {
                eventContext
                    .Events
                    .Select(@event => @event.Payload.GetType().Name)
                    .ShouldContain(typeof(TEvent).Name);
            }
            else
            {
                eventContext.Events.ShouldContain(@event
                    => @event.Payload is TEvent
                        && condition.Invoke((TEvent)@event.Payload));
            }
        }

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public static void ShouldContainGroup(this QuestionnaireDocument questionnaireDocument, Expression<Func<IGroup, bool>> condition)
        {
            questionnaireDocument.GetAllGroups().ShouldContain(condition);
        }

        [DebuggerStepThrough]
        public static void ShouldContainWarning(
            this IEnumerable<QuestionnaireVerificationMessage> verificationMessages, string code)
        {
            verificationMessages.ShouldContain(message
                => message.MessageLevel == VerificationMessageLevel.Warning
                && message.Code == code);
        }

        [DebuggerStepThrough]
        public static void ShouldNotContainMessage(
            this IEnumerable<QuestionnaireVerificationMessage> verificationMessages, string code)
        {
            verificationMessages.ShouldNotContain(message
                => message.Code == code);
        }
    }
}
