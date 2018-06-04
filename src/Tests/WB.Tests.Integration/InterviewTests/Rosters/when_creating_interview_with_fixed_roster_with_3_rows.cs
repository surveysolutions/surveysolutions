using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_creating_interview_on_client_with_fixed_roster_with_3_rows : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Entity.Roster(roster1Id, fixedRosterTitles: new []
                    {
                        IntegrationCreate.FixedTitle(1),
                        IntegrationCreate.FixedTitle(2),
                        IntegrationCreate.FixedTitle(3)
                    })
                });

                using (var eventContext = new EventContext())
                {
                    var questionnaireIdentity = new QuestionnaireIdentity(questionnaireDocument.PublicKey, 1);

                    var interview = SetupStatefullInterview(questionnaireDocument);

                    var command = Create.Command.CreateInterview(Guid.Empty, Guid.NewGuid(), questionnaireIdentity, 
                        Guid.NewGuid(), null, null, null);
                    interview.CreateInterview(command);

                    return new InvokeResults
                    {
                        SomeRosterWasAdded = eventContext.AnyEvent<RosterInstancesAdded>(x => x.Instances.Any(r => r.GroupId == roster1Id)),
                        CountOfAddedRosters = eventContext.GetSingleEventOrNull<RosterInstancesAdded>()?.Instances.Length ?? 0
                    };
                }
            });

        [NUnit.Framework.Test] public void should_fire_event_that_some_rosters_were_added () =>
            results.SomeRosterWasAdded.Should().BeTrue();

        [NUnit.Framework.Test] public void should_fire_event_that_3_rosters_were_added () =>
            results.CountOfAddedRosters.Should().Be(3);

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid roster1Id = Guid.Parse("77777777777777777777777777777777");

        [Serializable]
        internal class InvokeResults
        {
            public bool SomeRosterWasAdded { get; set; }
            public int CountOfAddedRosters { get; set; }
        }
    }
}
