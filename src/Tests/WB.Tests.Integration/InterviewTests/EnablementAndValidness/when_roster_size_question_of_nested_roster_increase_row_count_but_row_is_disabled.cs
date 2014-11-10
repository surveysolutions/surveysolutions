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
                
                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.Roster(rosterId, children: new IComposite[]
                        {
                            Create.NumericIntegerQuestion(nestedRosterSizeQuestionId, variable: "a"),
                            Create.Roster(nestedRosterId, rosterSizeSourceType: RosterSizeSourceType.Question, enablementCondition: "a > 1",
                                rosterSizeQuestionId: nestedRosterSizeQuestionId, children: new[]
                                {
                                    Create.Question()
                                })
                        })
                    );

                var interview = SetupInterview(questionnaireDocument, new InterviewPassiveEvent[]
                {
                    Create.Event.RosterInstancesAdded(new[]
                    {
                        Create.AddedRosterInstance(rosterId)
                    })
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, nestedRosterSizeQuestionId, new decimal[] { 0 }, DateTime.Now, 1);

                    return new InvokeResults()
                    {
                        NestedRosterEnabled = HasEvent<GroupsEnabled>(eventContext.Events, where =>
                            where.Groups.Any(
                                group =>
                                    group.Id == nestedRosterId && group.RosterVector.Length == 2 &&
                                    group.RosterVector[0] == 0 && group.RosterVector[1] == 0)),
                        NestedRosterDisabled =
                            HasEvent<GroupsDisabled>(eventContext.Events,
                                where =>
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