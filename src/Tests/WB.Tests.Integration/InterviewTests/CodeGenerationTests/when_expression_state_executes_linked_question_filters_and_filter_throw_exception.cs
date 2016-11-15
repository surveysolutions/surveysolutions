using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Tests.Integration.InterviewTests.CodeGenerationTests
{
    internal class when_expression_state_executes_linked_question_filters_and_filter_throw_exception : CodeGenerationTestsContext
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

                QuestionnaireDocument questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    children: new[]
                    {
                        Create.Chapter(children: new IComposite[]
                        {
                            Create.Roster(id: rosterId, variable: "fixed_roster",
                                rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                                obsoleteFixedTitles: new string[] {"1", "2", "3"}),
                            Create.SingleOptionQuestion(questionId: questionId, variable: "a",
                                linkedToRosterId: rosterId, linkedFilterExpression: "((string)null).Length>0"),

                        })
                    });
                IInterviewExpressionStateV7 state = GetInterviewExpressionState(questionnaireDocument, version: 13) as IInterviewExpressionStateV7;
                state.AddRoster(rosterId, new decimal[0], 1, null);

                var filterResults = state.ProcessLinkedQuestionFilters();
                return new InvokeResults()
                {
                    CountOfOptions = filterResults.LinkedQuestionOptions[questionId].Count()
                };
            });

        It should_result_contain_0_available_option = () =>
             results.CountOfOptions.ShouldEqual(0);

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
            public int CountOfOptions { get; set; }
        }
    }
}