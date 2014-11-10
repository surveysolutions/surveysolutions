using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
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

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.NumericRealQuestion(rosterSizeQuestionId, "a"),
                    Create.Roster(rosterId, null, children: new IComposite[] {
                        Create.Roster(nestedRosterId, enablementCondition: "a > 0")  
                    })
                );

                var interview = SetupInterview(questionnaireDocument, new List<object>
                {
                    Create.Event.RosterInstancesAdded(new []
                    {
                        Create.AddedRosterInstance(rosterId),
                        Create.AddedRosterInstance(nestedRosterId, new decimal[] { 0 })
                    }),
                    Create.Event.GroupsEnabled(new []
                    {
                        Create.Identity(nestedRosterId)
                    }),
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericRealQuestion(userId, rosterSizeQuestionId, new decimal[] { }, DateTime.Now, 0);

                    return new InvokeResults()
                    {
                        NestedRosterDisabled =
                            HasEvent<GroupsDisabled>(eventContext.Events,
                                where =>
                                    where.Groups.Any(
                                        group =>
                                            group.Id == nestedRosterId && group.RosterVector.Length == 2 &&
                                            group.RosterVector[0] == 0 && group.RosterVector[1] == 0)),
                        NestedRosterEnabled = HasEvent<GroupsEnabled>(eventContext.Events, where =>
                            where.Groups.Any(
                                group =>
                                    group.Id == nestedRosterId && group.RosterVector.Length == 2 &&
                                    group.RosterVector[0] == 0 && group.RosterVector[1] == 0))
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
        private static AppDomainContext appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool NestedRosterDisabled { get; set; }
            public bool NestedRosterEnabled { get; set; }
        }
    }
}