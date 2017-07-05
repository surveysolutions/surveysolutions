using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_one_roster_refers_another_roster_in_condition : in_standalone_app_domain
    {
        Because of = () => result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
        {
            Setup.MockedServiceLocator();

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

        It should_raise_group_disabled_event_for_roster__f2 = () => 
            result.QuestionEnabledEventRaised.ShouldBeTrue();

        private static InvokeResults result;

        [Serializable]
        internal class InvokeResults
        {
            public bool QuestionEnabledEventRaised { get; set; }
        }
    }
}