using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_one_roster_refers_another_roster_in_condition : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        protected static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        public void BecauseOf() => result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
        {
            SetUp.MockedServiceLocator();

            Guid fixed1 = Guid.Parse("11111111111111111111111111111111");
            Guid fixed2 = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                Create.Entity.FixedRoster(fixed1, variable:"f1", fixedTitles: new []{Create.Entity.FixedTitle(1), Create.Entity.FixedTitle(2)}),
                Create.Entity.FixedRoster(fixed2, variable:"f2", enablementCondition: "f1.Count() > 2",  fixedTitles: new []{Create.Entity.FixedTitle(3), Create.Entity.FixedTitle(4)}),
            });
            
            using (var eventContext = new EventContext())
            {
                var interview = SetupInterview(questionnaireDocument: questionnaire);

                return new InvokeResults
                {
                    QuestionEnabledEventRaised = eventContext.GetSingleEventOrNull<GroupsDisabled>()?.Groups?.Any(x => x.Id == fixed2) ?? false
                };
            }
        });

        [NUnit.Framework.Test] public void should_raise_group_disabled_event_for_roster__f2 () => 
            result.QuestionEnabledEventRaised.Should().BeTrue();

        private static InvokeResults result;

        [Serializable]
        internal class InvokeResults
        {
            public bool QuestionEnabledEventRaised { get; set; }
        }
    }
}
