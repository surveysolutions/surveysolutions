using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.OptionsFilter
{
    internal class when_filtering_options_for_multi_question_in_fixed_roster : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Roster(rosterId, variable: "parent", rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                        fixedTitles: new[] { Create.FixedTitle(1, "Roster 1"), Create.FixedTitle(2, "Roster 2") }, children: new IComposite[]
                        {
                            Create.MultyOptionsQuestion(q1Id, variable: "q1", optionsFilter: "@optioncode < 10")
                        })
                    );

                ILatestInterviewExpressionState interviewState = GetInterviewExpressionState(questionnaireDocument);

                var interview = SetupInterview(questionnaireDocument, precompiledState: interviewState);

                IEnumerable<CategoricalOption> options = new List<CategoricalOption>()
                {
                    Create.CategoricalOption(1, "Option 1"),
                    Create.CategoricalOption(2, "Option 2"),
                    Create.CategoricalOption(11, "Option 11"),
                    Create.CategoricalOption(12, "Option 12"),
                };

                results = new InvokeResults
                {
                    CountOfFilteredOptions = interviewState.FilterOptionsForQuestion(Create.Identity(q1Id, Create.RosterVector(1)), options).Count()
                };
                return results;
            });

        It should_return_2_options = () =>
            results.CountOfFilteredOptions.ShouldEqual(2);

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid q1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Serializable]
        internal class InvokeResults
        {
            public int CountOfFilteredOptions { get; set; }
        }
    }
}