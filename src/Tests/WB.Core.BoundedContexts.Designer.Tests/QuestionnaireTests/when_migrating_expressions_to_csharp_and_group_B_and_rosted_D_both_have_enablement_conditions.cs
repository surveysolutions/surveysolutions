using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
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

        It should_raise_TemplateImported_event = () =>
            eventContext.ShouldContainEvent<TemplateImported>();

        It should_set_group_B_enablement_condition_to_converted_to_csharp = () =>
            eventContext.GetSingleEvent<TemplateImported>().Source.Find<IGroup>(groupBId)
                .ConditionExpression.ShouldEqual("C# EC B");

        It should_set_roster_D_enablement_condition_to_converted_to_csharp = () =>
            eventContext.GetSingleEvent<TemplateImported>().Source.Find<IGroup>(rosterDId)
                .ConditionExpression.ShouldEqual("C# EC D");

        It should_raise_ExpressionsMigratedToCSharp_event = () =>
            eventContext.ShouldContainEvent<ExpressionsMigratedToCSharp>();

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid groupBId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
        private static Guid rosterDId = Guid.Parse("dddddddddddddddddddddddddddddddd");
    }
}