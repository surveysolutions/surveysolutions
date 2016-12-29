using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    [TestFixture]
    internal class when_answering_question_that_enables_section_with_nested_rosters_inside_it : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                Guid userId = Guid.NewGuid();

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.Chapter(id: Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), children: new IComposite[]
                    {
                        Create.NumericIntegerQuestion(numId, variable: "x1")
                    }),
                    Create.Chapter(id: Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD"), enablementCondition: "x1 == 1", children: new IComposite[]
                    {
                        Create.ListQuestion(list1Id, variable: "l1"),
                        Create.Roster(roster1Id, rosterSizeQuestionId: list1Id, variable: "r1", rosterSizeSourceType:RosterSizeSourceType.Question, children: new IComposite[]
                        {
                            Create.ListQuestion(list2Id, variable: "l2"),
                            Create.Roster(roster2Id, rosterSizeQuestionId: list2Id, variable: "r2", rosterSizeSourceType:RosterSizeSourceType.Question, children: new IComposite[]
                            {
                                Create.TextQuestion(textId)
                            })
                        })
                    }));

                var interview = SetupStatefullInterview(questionnaireDocument);
                interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestion(numId, answer: 1));
                interview.AnswerTextListQuestion(userId, list1Id, RosterVector.Empty, DateTime.Now, new[] { Tuple.Create(1m, "Hello") });
                interview.AnswerTextListQuestion(userId, list2Id, Create.RosterVector(1), DateTime.Now, new[] { Tuple.Create(1m, "World") });

                var invokeResults = new InvokeResults();
                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestion(numId, answer: 2));
                    invokeResults.SubGroupGotEnablementEvents = eventContext.AnyEvent<GroupsDisabled>(x => x.Groups.Any(y => y.Id == roster2Id));
                }


                using (new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), numId, RosterVector.Empty, DateTime.Now, 1);
                    invokeResults.TopRosterIsEnabled = interview.IsEnabled(Create.Identity(roster1Id, Create.RosterVector(1)));
                    invokeResults.NestedRosterIsEnabled = interview.IsEnabled(Create.Identity(roster2Id, Create.RosterVector(1, 1)));
                    return invokeResults;
                }
            });

        It should_not_raise_enablement_events_for_subgroups = () => results.SubGroupGotEnablementEvents.ShouldBeFalse();

        It should_mark_nested_roster_as_enabled = () => results.NestedRosterIsEnabled.ShouldBeTrue();

        It should_mark_top_level_roster_as_enabled = () => results.TopRosterIsEnabled.ShouldBeTrue();


        private Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static Guid questionnaireId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid roster1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid roster2Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid numId = Guid.Parse("11111111111111111111111111111111");
        private static Guid list1Id = Guid.Parse("22222222222222222222222222222222");
        private static Guid list2Id = Guid.Parse("33333333333333333333333333333333");
        private static Guid textId = Guid.Parse("44444444444444444444444444444444");

        [Serializable]
        internal class InvokeResults
        {
            public bool TopRosterIsEnabled { get; set; }
            public bool NestedRosterIsEnabled { get; set; }
            public bool SubGroupGotEnablementEvents { get; set; }
        }
    }
}