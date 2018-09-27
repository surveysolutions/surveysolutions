using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_answering_question_that_disables_group_with_nested_roster_inside_it : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.Roster(parentRosterId, variable: "parent", rosterSizeSourceType: RosterSizeSourceType.FixedTitles, 
                        fixedRosterTitles: new [] { IntegrationCreate.FixedTitle(1, "Roster 1"), IntegrationCreate.FixedTitle(2, "Roster 2") }, children: new IComposite[]
                        {
                            Abc.Create.Entity.NumericIntegerQuestion(q1Id, variable: "q1"),
                            Abc.Create.Entity.Group(groupId, enablementCondition: "q1 == 1", children: new IComposite[]
                            {
                                Abc.Create.Entity.Roster(
                                    rosterId: rosterId,
                                    rosterSizeSourceType: RosterSizeSourceType.Question,
                                    rosterSizeQuestionId: q1Id,
                                    variable: "r",
                                    children: new IComposite[]
                                    {
                                        Abc.Create.Entity.NumericIntegerQuestion(q2Id, variable: "q2"),
                                    })
                            })
                        })
                    );

                var result = new InvokeResults();

                var interview = SetupInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, q1Id, Abc.Create.Entity.RosterVector(new[] {1}), DateTime.Now, 1);
                interview.AnswerNumericIntegerQuestion(userId, q2Id, Abc.Create.Entity.RosterVector(new[] {1, 0}), DateTime.Now, 1);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, q1Id, Abc.Create.Entity.RosterVector(new[] {1}), DateTime.Now, 2);

                    result.QuestionsQ2Disabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == q2Id));
                    result.RosterDisabled = eventContext.AnyEvent<GroupsDisabled>(x => x.Groups.Any(g => g.Id == rosterId));
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_declare_question_q2_as_disabled () =>
            results.QuestionsQ2Disabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_declare_nested_roster_as_disabled () =>
            results.RosterDisabled.Should().BeTrue();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid questionnaireId = Guid.Parse("77778888000000000000000000000000");
        private static readonly Guid q1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid q2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid rosterId = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid groupId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid parentRosterId = Guid.Parse("55555555555555555555555555555555");

        [Serializable]
        internal class InvokeResults
        {
            public bool QuestionsQ2Disabled { get; set; }
            public bool RosterDisabled { get; set; }
        }
    }
}
