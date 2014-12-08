using System;
using System.Linq;
using System.Linq.Expressions;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Tests.Unit.SharedKernels.DataCollection.InterviewExpressionStateTests;

namespace WB.Tests.Unit
{
    internal static class ShouldExtensions
    {
        public static void ShouldMatchMethodInfo(this MetodInfo left, MetodInfo right)
        {
            left.Name.ShouldEqual(right.Name);
            left.ReturnType.ShouldEqual(right.ReturnType);
            left.ParamsType.ShouldContainOnly(right.ParamsType);
        }

        public static void ShouldContainValues(this QuestionTemplateModel question,
            Guid id,
            string variableName,
            bool isMandatory,
            string conditions,
            string validations,
            QuestionType questionType,
            string generatedIdName,
            string generatedTypeName,
            string generatedMemberName,
            string generatedStateName,
            string rosterScopeName,
            string generatedValidationsMethodName,
            string generatedMandatoryMethodName,
            string generatedConditionsMethodName)
        {
            question.Id.ShouldEqual(id);
            question.VariableName.ShouldEqual(variableName);
            question.IsMandatory.ShouldEqual(isMandatory);
            question.Conditions.ShouldEqual(conditions);
            question.Validations.ShouldEqual(validations);
            question.QuestionType.ShouldEqual(questionType);
            question.GeneratedIdName.ShouldEqual(generatedIdName);
            question.GeneratedTypeName.ShouldEqual(generatedTypeName);
            question.GeneratedMemberName.ShouldEqual(generatedMemberName);
            question.GeneratedStateName.ShouldEqual(generatedStateName);
            question.RosterScopeName.ShouldEqual(rosterScopeName);
            question.GeneratedValidationsMethodName.ShouldEqual(generatedValidationsMethodName);
            question.GeneratedMandatoryMethodName.ShouldEqual(generatedMandatoryMethodName);
            question.GeneratedConditionsMethodName.ShouldEqual(generatedConditionsMethodName);
        }

        public static void ShouldContainEvents<TEvent>(this EventContext eventContext, int count)
        {
            eventContext.Events.Count(e => e.Payload is TEvent).ShouldEqual(count);
        }

        public static void ShouldContainEvent<TEvent>(this EventContext eventContext, Func<TEvent, bool> condition = null)
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

        public static void ShouldContainGroup(this QuestionnaireDocument questionnaireDocument, Expression<Func<IGroup, bool>> condition)
        {
            questionnaireDocument.GetAllGroups().ShouldContain(condition);
        }
    }
}
