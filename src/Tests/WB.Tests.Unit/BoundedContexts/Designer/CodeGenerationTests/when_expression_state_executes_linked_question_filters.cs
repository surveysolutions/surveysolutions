using System;
using System.Collections.Generic;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Tests.Unit.BoundedContexts.Designer.CodeGenerationTests
{
    internal class when_expression_state_executes_linked_question_filters : CodeGenerationTestsContext
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
                IInterviewExpressionStateV7 state = GetInterviewExpressionState(questionnaireDocument,version: new Version(13, 0, 0)) as IInterviewExpressionStateV7;
                state.AddRoster(rosterId, new decimal[0], 1, null);
                state.AddRoster(rosterId, new decimal[0], 2, null);
                state.AddRoster(rosterId, new decimal[0], 3, null);

                return state.ProcessLinkedQuestionFilters();
            });

        It should_return_3_filters = () =>
            results.Count.ShouldEqual(3);

        It should_result_of_first_filter_be_false = () =>
            results[0].Enabled.ShouldBeFalse();

        It should_result_of_second_filter_be_true = () =>
            results[1].Enabled.ShouldBeTrue();

        It should_result_of_third_filter_be_true = () =>
           results[2].Enabled.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static AppDomainContext appDomainContext;
        private static List<LinkedQuestionFilterResult> results;

        [Serializable]
        public class InvokeResults
        {
            public int ValidQuestionsCount { get; set; }
            public int InvalidQuestionsCount { get; set; }
        }
    }
}