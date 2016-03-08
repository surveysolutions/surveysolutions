using System;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Tests.Unit.BoundedContexts.Designer.CodeGenerationTests
{
    internal class when_expression_state_executes_linked_question_filters_for_the_second_time_and_option_get_enabled : CodeGenerationTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
                Guid questionId = Guid.Parse("11111111111111111111111111111112");
                Guid rosterId = Guid.Parse("21111111111111111111111111111112");

                AssemblyContext.SetupServiceLocator();

                QuestionnaireDocument questionnaireDocument = Create.QuestionnaireDocument(questionnaireId, children: new[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.Roster(rosterId: rosterId, variable: "fixed_roster", rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedTitles: new string[] {"1", "2", "3"}),
                    Create.SingleOptionQuestion(questionId: questionId, variable: "a", linkedToRosterId: rosterId,linkedFilterExpression: "@rowcode>1" ),

                })
            });
                IInterviewExpressionStateV7 state = GetInterviewExpressionState(questionnaireDocument, version: new Version(13, 0, 0)) as IInterviewExpressionStateV7;
                state.AddRoster(rosterId, new decimal[0], 1, null);
                state.AddRoster(rosterId, new decimal[0], 2, null);
                state.AddRoster(rosterId, new decimal[0], 3, null);
                
                state.EnableLinkedQuestionOptions(new[] { new Identity(questionId, new decimal[] { 1 }), new Identity(questionId, new decimal[] { 2 }), new Identity(questionId, new decimal[] { 3 }) });
                state.SaveAllCurrentStatesAsPrevious();
                var filterResults = state.ProcessLinkedQuestionFilters();
                return new InvokeResults()
                {
                    CountOfEnabledOptions = filterResults.OptionsDeclaredEnabled.Count,
                    CountOfDisabledOptions = filterResults.OptionsDeclaredDisabled.Count
                };
            });

        It should_result_contain_1_disabled_option = () =>
            results.CountOfDisabledOptions.ShouldEqual(1);

        It should_result_contain_0_enabled_option = () =>
             results.CountOfEnabledOptions.ShouldEqual(0);

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static AppDomainContext appDomainContext;
        private static InvokeResults results;

        [Serializable]
        public class InvokeResults
        {
            public int CountOfEnabledOptions { get; set; }
            public int CountOfDisabledOptions { get; set; }
        }
    }
}