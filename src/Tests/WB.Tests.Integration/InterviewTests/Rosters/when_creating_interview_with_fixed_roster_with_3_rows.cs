﻿using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_creating_interview_on_client_with_fixed_roster_with_3_rows : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
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

                    var command = Create.Command.CreateInterview(Guid.Empty, Guid.NewGuid(), questionnaireIdentity, DateTime.Now,
                        Guid.NewGuid(), null, null, null);
                    interview.CreateInterviewWithPreloadedData(command);

                    return new InvokeResults
                    {
                        SomeRosterWasAdded = eventContext.AnyEvent<RosterInstancesAdded>(x => x.Instances.Any(r => r.GroupId == roster1Id)),
                        CountOfAddedRosters = eventContext.GetSingleEventOrNull<RosterInstancesAdded>()?.Instances.Length ?? 0
                    };
                }
            });

        It should_fire_event_that_some_rosters_were_added = () =>
            results.SomeRosterWasAdded.ShouldBeTrue();

        It should_fire_event_that_3_rosters_were_added = () =>
            results.CountOfAddedRosters.ShouldEqual(3);

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

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