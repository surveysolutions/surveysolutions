using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using It = Machine.Specifications.It;

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

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.Roster(id: roster1Id, fixedTitles: new []
                    {
                        Create.FixedTitle(1),
                        Create.FixedTitle(2),
                        Create.FixedTitle(3),
                    })
                });

                using (var eventContext = new EventContext())
                {
                    var questionnaireIdentity = new QuestionnaireIdentity(questionnaireDocument.PublicKey, 1);

                    ILatestInterviewExpressionState expressionState = GetInterviewExpressionState(questionnaireDocument);

                    var interview = new StatefulInterview(
                        Create.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity, questionnaireDocument),
                        Stub<IInterviewExpressionStatePrototypeProvider>.Returning(expressionState),
                        Create.SubstitionTextFactory());

                    interview.CreateInterviewOnClient(questionnaireIdentity, Guid.NewGuid(), DateTime.Now, Guid.NewGuid());

                    return new InvokeResults
                    {
                        SomeRosterWasAdded = eventContext.AnyEvent<RosterInstancesAdded>(x => x.Instances.Any(r => r.GroupId == roster1Id)),
                        CountOfAddedRosters = eventContext.GetSingleEventOrNull<RosterInstancesAdded>()?.Instances.Length ?? 0,
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