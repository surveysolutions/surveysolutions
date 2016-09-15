using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_roster_size_question_of_nested_roster_increase_row_count_but_row_is_disabled : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var userId = Guid.Parse("11111111111111111111111111111111");

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");;
                var nestedRosterSizeQuestionId = Guid.Parse("22222222222222222222222222222222");
                var rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var nestedRosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Roster(rosterId, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedTitles: new[] { "1" },
                        children: new IComposite[]
                        {
                            Create.NumericIntegerQuestion(nestedRosterSizeQuestionId, variable: "a"),
                            Create.Roster(nestedRosterId, rosterSizeSourceType: RosterSizeSourceType.Question, enablementCondition: "a > 1",
                                rosterSizeQuestionId: nestedRosterSizeQuestionId, children: new[]
                                {
                                    Create.Question()
                                })
                        })
                    );

                var interview = SetupInterview(questionnaireDocument);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, nestedRosterSizeQuestionId, new decimal[] { 0 }, DateTime.Now, 1);

                    return new InvokeResults()
                    {
                        NestedRosterEnabled = eventContext.AnyEvent<GroupsEnabled>(e => e.Groups.Any(q => q.Id == nestedRosterId)),
                        NestedRosterDisabled = eventContext.AnyEvent<GroupsDisabled>(e => e.Groups.Any(q => q.Id == nestedRosterId))
                    };
                }
            });
        
        It should_disable_nested_roster = () =>
            results.NestedRosterDisabled.ShouldBeTrue();

        It should_not_enable_nested_roster = () =>
            results.NestedRosterEnabled.ShouldBeFalse();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool NestedRosterDisabled { get; set; }
            public bool NestedRosterEnabled { get; set; }
        }
    }
}