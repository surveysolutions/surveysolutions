using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.SharedKernels.DataCollection.V9;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Integration.InterviewTests.CodeGenerationTests
{
    internal class when_generating_models_with_variable_usage_in_nested_roster : CodeGenerationTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
                Guid variableId = Guid.Parse("11111111111111111111111111111112");
                Guid questionId = Guid.Parse("21111111111111111111111111111112");
                Guid rosterId = Guid.Parse("cccccccccccccccccccccccccccccccc");

                AssemblyContext.SetupServiceLocator();

                var assetsTitles = new[]
                {
                    Create.FixedRosterTitle(1, "TV"),
                    Create.FixedRosterTitle(2, "Microwave"),
                    Create.FixedRosterTitle(3, "Cleaner")
                };

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
                {
                    Create.Chapter(children: new IComposite[]
                    {
                        Create.NumericIntegerQuestion(id: questionId, variable: "q1"),
                        Create.Variable(id: variableId, type: VariableType.Boolean, variableName: "num",
                            expression: "q1>5"),
                        Create.Roster(rosterId, variable: "assets",
                            rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedRosterTitles: assetsTitles,
                            enablementCondition: "num.GetValueOrDefault()")
                    }),
                });
                IInterviewExpressionStateV9 state =
                    GetInterviewExpressionState(questionnaireDocument, version: 15) as
                        IInterviewExpressionStateV9;
                state.AddRoster(rosterId, new decimal[0], 1, null);
                state.AddRoster(rosterId, new decimal[0], 2, null);
                state.SaveAllCurrentStatesAsPrevious();
                var enablementConditions = state.ProcessEnablementConditions();

                return new InvokeResults()
                {
                    CoundOfDisabledGroups = enablementConditions.GroupsToBeDisabled.Count
                };
            });

        It should_disable_2_roster_groups = () =>
            results.CoundOfDisabledGroups.ShouldEqual(2);

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResults results;

        [Serializable]
        public class InvokeResults
        {
            public int CoundOfDisabledGroups { get; set; }
        }
    }
}