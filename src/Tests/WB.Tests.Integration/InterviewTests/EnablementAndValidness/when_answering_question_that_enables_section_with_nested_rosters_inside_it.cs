using System;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    [TestFixture]
    internal class when_answering_question_that_enables_section_with_nested_rosters_inside_it : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                Guid userId = Guid.NewGuid();

                var questionnaireDocument = Create.Entity.QuestionnaireDocument(questionnaireId,
                    Create.Entity.Group(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), children: new IComposite[]
                    {
                        Create.Entity.NumericIntegerQuestion(numId, "x1")
                    }),
                    Create.Entity.Group(Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD"), enablementCondition: "x1 == 1", children: new IComposite[]
                    {
                        Create.Entity.TextListQuestion(list1Id, variable: "l1"),
                        Create.Entity.ListRoster(roster1Id, rosterSizeQuestionId: list1Id, variable: "r1", children: new IComposite[]
                        {
                            Create.Entity.TextListQuestion(list2Id, variable: "l2"),
                            Create.Entity.ListRoster(roster2Id, rosterSizeQuestionId: list2Id, variable: "r2", children: new IComposite[]
                            {
                                Create.Entity.TextQuestion(textId, variable: null)
                            })
                        })
                    }));

                var interview = SetupStatefullInterview(questionnaireDocument);
                interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(interview.Id, userId, numId, 1));
                interview.AnswerTextListQuestion(userId, list1Id, RosterVector.Empty, DateTime.Now, new[] { Tuple.Create(1m, "Hello") });
                interview.AnswerTextListQuestion(userId, list2Id, Create.Entity.RosterVector(new[] {1}), DateTime.Now, new[] { Tuple.Create(1m, "World") });
                interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(questionId: numId, answer: 2));
                var invokeResults = new InvokeResults();
                
                using (new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), numId, RosterVector.Empty, DateTime.Now, 1);
                    invokeResults.TopRosterIsEnabled = interview.IsEnabled(Create.Identity(roster1Id, Create.Entity.RosterVector(new[] {1})));
                    invokeResults.NestedRosterIsEnabled = interview.IsEnabled(Create.Identity(roster2Id, Create.Entity.RosterVector(new[] {1, 1})));
                    return invokeResults;
                }
            });

        [NUnit.Framework.Test] public void should_mark_nested_roster_as_enabled () => results.NestedRosterIsEnabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_mark_top_level_roster_as_enabled () => results.TopRosterIsEnabled.Should().BeTrue();


        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

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
        }
    }
}
