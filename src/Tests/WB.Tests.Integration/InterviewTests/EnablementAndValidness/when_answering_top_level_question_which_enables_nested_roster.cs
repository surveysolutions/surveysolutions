using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_top_level_question_which_enables_nested_roster : InterviewTestsContext
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

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var nestedRosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.NumericRealQuestion(rosterSizeQuestionId, "a"),
                    Create.Roster(rosterId, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedRosterTitles: new []{ Create.FixedRosterTitle(0) }, children: new IComposite[] {
                        Create.Roster(nestedRosterId, enablementCondition: "a > 0", rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedRosterTitles: new []{ Create.FixedRosterTitle(0) })  
                    })
                );

                var interview = SetupInterview(questionnaireDocument);
                interview.AnswerNumericRealQuestion(userId, rosterSizeQuestionId, new decimal[] { }, DateTime.Now, 10);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericRealQuestion(userId, rosterSizeQuestionId, new decimal[] { }, DateTime.Now, 0);

                    return new InvokeResults
                    {
                        NestedRosterDisabled = eventContext.AnyEvent<GroupsDisabled>(x => x.Groups
                                .Any(g  => g.Id == nestedRosterId && g.RosterVector.SequenceEqual(Create.RosterVector(0, 0)))),
                        
                        NestedRosterEnabled = eventContext.AnyEvent<GroupsEnabled>(x => x.Groups
                                .Any(g => g.Id == nestedRosterId && g.RosterVector.SequenceEqual(Create.RosterVector(0, 0)))),
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