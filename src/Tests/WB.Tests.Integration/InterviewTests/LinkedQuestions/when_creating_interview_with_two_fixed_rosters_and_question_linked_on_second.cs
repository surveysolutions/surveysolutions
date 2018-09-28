using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    [Ignore("Fix in KP-7358")]
    internal class when_creating_interview_with_two_fixed_rosters_and_question_linked_on_second : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();
               
                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
                {
                    Abc.Create.Entity.SingleQuestion(id: linkedToQuestionId, linkedToRosterId: roster2Id, variable: "linked"),
                    Abc.Create.Entity.Roster(rosterId: roster1Id, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, variable: "r1",
                        enablementCondition: "@rowcode == 1",
                        fixedRosterTitles: new[] {IntegrationCreate.FixedTitle(1), IntegrationCreate.FixedTitle(2)},
                        children: new IComposite[]
                        {
                            Abc.Create.Entity.Roster(rosterId: roster2Id, rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                                variable: "r2", 
                                fixedRosterTitles: new[] {IntegrationCreate.FixedTitle(1), IntegrationCreate.FixedTitle(2)})
                        }),
                });

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    var interview = SetupStatefullInterview(questionnaireDocument, useLatestEngine: false);
                    var questionIdentity = Abc.Create.Identity(linkedToQuestionId, RosterVector.Empty);

                    result.LinkedOptionsCount =
                        eventContext.GetSingleEvent<LinkedOptionsChanged>().ChangedLinkedQuestions.First(x => x.QuestionId == questionIdentity).Options.Length;
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_event_has_2_options_for_linked_question () =>
            results.LinkedOptionsCount.Should().Be(2);

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid roster2Id = Guid.Parse("88888888888888888888888888888888");
        private static readonly Guid roster1Id = Guid.Parse("77777777777777777777777777777777");
        private static readonly Guid linkedToQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Serializable]
        internal class InvokeResults
        {
            public int OptionsCountForLinkedToRosterQuestion { get; set; }
            public int LinkedOptionsCount { set; get; }
        }
    }
}
