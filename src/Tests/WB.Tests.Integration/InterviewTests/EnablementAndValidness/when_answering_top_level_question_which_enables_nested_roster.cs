using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_top_level_question_which_enables_nested_roster : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var userId = Guid.Parse("11111111111111111111111111111111");

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var nestedRosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.NumericRealQuestion(rosterSizeQuestionId, "a"),
                    Abc.Create.Entity.Roster(rosterId, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedRosterTitles: new []{ IntegrationCreate.FixedTitle(0) }, children: new IComposite[] {
                        Abc.Create.Entity.Roster(nestedRosterId, enablementCondition: "a > 0", rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedRosterTitles: new []{ IntegrationCreate.FixedTitle(0) })  
                    })
                );

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument);
                interview.AnswerNumericRealQuestion(userId, rosterSizeQuestionId, new decimal[] { }, DateTime.Now, 10);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericRealQuestion(userId, rosterSizeQuestionId, new decimal[] { }, DateTime.Now, 0);

                    return new InvokeResults
                    {
                        NestedRosterDisabled = eventContext.AnyEvent<GroupsDisabled>(x => x.Groups
                                .Any(g  => g.Id == nestedRosterId && g.RosterVector.SequenceEqual(Abc.Create.Entity.RosterVector(new[] {0, 0})))),
                        
                        NestedRosterEnabled = eventContext.AnyEvent<GroupsEnabled>(x => x.Groups
                                .Any(g => g.Id == nestedRosterId && g.RosterVector.SequenceEqual(Abc.Create.Entity.RosterVector(new[] {0, 0})))),
                    };
                }
            });
        
        [NUnit.Framework.Test] public void should_disable_nested_roster () =>
            results.NestedRosterDisabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_not_enable_nested_roster () =>
            results.NestedRosterEnabled.Should().BeFalse();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

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
