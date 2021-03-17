using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.Infrastructure.EventBus;

namespace WB.Tests.Unit
{
    internal static class ShouldExtensions
    {
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

        [DebuggerStepThrough]
        public static void ShouldContainEvents<TEvent>(this EventContext eventContext, int count)
        {
            eventContext.Events.Where(e => e.Payload is TEvent).Should().HaveCount(count);
        }

        [DebuggerStepThrough]
        public static void ShouldContainEvent<TEvent>(this EventContext eventContext, Func<TEvent, bool> condition = null)
            where TEvent : IEvent
        {
            eventContext
                .Events
                .Select(@event => @event.Payload.GetType())
                .Should().Contain(typeof(TEvent));

            if (condition != null)
            {
                eventContext
                    .Events
                    .Where(@event => @event.Payload is TEvent)
                    .Select(@event => (TEvent) @event.Payload)
                    .Should().Contain(payload => condition.Invoke(payload));
            }
        }

        [DebuggerStepThrough]
        public static void ShouldNotContainEvent<TEvent>(this EventContext eventContext, Func<TEvent, bool> condition = null)
            where TEvent : IEvent
        {
            if (condition == null)
            {
                eventContext.Events.Should().NotContain(@event
                    => @event.Payload is TEvent);
            }
            else
            {
                eventContext.Events.Should().NotContain(@event
                    => @event.Payload is TEvent
                        && condition.Invoke((TEvent)@event.Payload));
            }
        }

        [DebuggerStepThrough]
        public static void ShouldContainGroup(this QuestionnaireDocument questionnaireDocument, Expression<Func<IGroup, bool>> condition)
        {
            questionnaireDocument.GetAllGroups().Cast<IGroup>().Should().Contain(condition);
        }

        [DebuggerStepThrough]
        public static void ShouldContainWarning(
            this IEnumerable<QuestionnaireVerificationMessage> verificationMessages, string code)
        {
            verificationMessages.Should().Contain(message
                => message.MessageLevel == VerificationMessageLevel.Warning
                && message.Code == code);
        }

        [DebuggerStepThrough]
        public static void ShouldNotContainMessage(
            this IEnumerable<QuestionnaireVerificationMessage> verificationMessages, string code)
        {
            verificationMessages.Should().NotContain(message
                => message.Code == code);
        }

        [DebuggerStepThrough]
        public static void AssertExactlyOneRowMatch(this object[][] data, params object[] row)
        {
            // Assert.That(this.report.Data, Has.One.EqualTo(row)); doesn't work as expected
            var match = new List<Object[]>();

            foreach (var dataRow in data)
            {
                if (dataRow.SequenceEqual(row))
                    match.Add(dataRow);
            }

            Assert.That(match, Has.Exactly(1).Items);
        }

        //[DebuggerStepThrough]
        public static void AssertEqualInAnyOrder(this object[][] data, params object[][] rows)
        {
            Assert.That(data, Has.Length.EqualTo(rows.Length));

            foreach (var item in data)
            {
                AssertExactlyOneRowMatch(rows, item);
            }
        }


        [DebuggerStepThrough]
        public static void AssertEqual(this object[][] data, params object[][] rows)
        {
            Assert.That(data, Has.Length.EqualTo(rows.Length));

            foreach (var item in data.Zip(rows, (d,r) => (d,r)))
            {
                Assert.That(item.Item1, Is.EqualTo(item.Item2));
            }
        }
    }
}
