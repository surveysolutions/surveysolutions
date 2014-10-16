using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_migrating_expressions_to_csharp_and_group_B_and_rosted_D_both_have_enablement_conditions : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            Setup.SimpleQuestionnaireDocumentUpgraderToMockedServiceLocator();

            Setup.InstanceToMockedServiceLocator(Mock.Of<INCalcToCSharpConverter>(_
                => _.Convert("NCalc EC B", Moq.It.IsAny<Dictionary<string, string>>()) == "C# EC B"
                && _.Convert("NCalc EC D", Moq.It.IsAny<Dictionary<string, string>>()) == "C# EC D"));

            questionnaire = Create.Questionnaire(Create.QuestionnaireDocument(children: new[]
            {
                Create.Chapter(children: new[]
                {
                    Create.Group(enablementCondition: null),
                    Create.Group(groupId: groupBId, enablementCondition: "NCalc EC B"),
                    Create.Roster(enablementCondition: null),
                    Create.Roster(rosterId: rosterDId, enablementCondition: "NCalc EC D"),
                    Create.Roster(enablementCondition: null),
                }),
            }));

            eventContext = Create.EventContext();
        };

        Because of = () =>
            questionnaire.MigrateExpressionsToCSharp();

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_2_GroupUpdated_events = () =>
            eventContext.ShouldContainEvents<GroupUpdated>(count: 2);

        It should_raise_GroupUpdated_event_for_group_B_with_enablement_condition_converted_to_csharp = () =>
            eventContext.ShouldContainEvent<GroupUpdated>(@event
                => @event.GroupPublicKey == groupBId
                && @event.ConditionExpression == "C# EC B");

        It should_raise_GroupUpdated_event_for_roster_D_with_enablement_condition_converted_to_csharp = () =>
            eventContext.ShouldContainEvent<GroupUpdated>(@event
                => @event.GroupPublicKey == rosterDId
                && @event.ConditionExpression == "C# EC D");

        It should_raise_ExpressionsMigratedToCSharp_event = () =>
            eventContext.ShouldContainEvent<ExpressionsMigratedToCSharp>();

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid groupBId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
        private static Guid rosterDId = Guid.Parse("dddddddddddddddddddddddddddddddd");
    }
}