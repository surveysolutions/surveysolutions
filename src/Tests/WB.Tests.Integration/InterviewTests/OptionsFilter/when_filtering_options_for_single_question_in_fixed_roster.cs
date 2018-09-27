using System;
using System.Collections.Generic;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.OptionsFilter
{
    internal class when_filtering_options_for_single_question_in_fixed_roster : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                List<Answer> options = new List<Answer>
                {
                    Create.Entity.Option(1, "Option 1"),
                    Create.Entity.Option(2, "Option 2"),
                    Create.Entity.Option(11, "Option 11"),
                    Create.Entity.Option(12, "Option 12")
                };

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.FixedRoster(rosterId, variable: "parent", fixedTitles: new [] { IntegrationCreate.FixedTitle(1, "Roster 1"), IntegrationCreate.FixedTitle(2, "Roster 2") }, children: new IComposite[]
                    {
                        Create.Entity.SingleQuestion(q1Id, "q1", optionsFilter: "@optioncode < 10", options: options)
                    })
                );

                var interview = SetupInterview(questionnaireDocument);

                results = new InvokeResults
                {
                    CountOfFilteredOptions = interview.GetFirstTopFilteredOptionsForQuestion(Create.Identity(q1Id, Create.Entity.RosterVector(new[] {1})), null, "").Count
                };
                return results;
            });

        [NUnit.Framework.Test] public void should_return_2_options () =>
            results.CountOfFilteredOptions.Should().Be(2);

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid q1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Serializable]
        internal class InvokeResults
        {
            public int CountOfFilteredOptions{ get; set; }
        }
    }
}
