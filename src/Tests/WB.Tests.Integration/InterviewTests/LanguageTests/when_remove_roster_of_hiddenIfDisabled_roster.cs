using AppDomainToolkit;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    public class when_remove_roster_of_hiddenIfDisabled_roster : InterviewTestsContext
    {
        [OneTimeSetUp]
        public void Context()
        {
            appDomainContext = AppDomainContext.Create();
            events = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();
                
                userId = Id.gF;

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(rosterSource, variable: "rosterSource"),

                    Create.Entity.Roster(rosterGroupId,
                        rosterSizeQuestionId: rosterSource,
                        rosterSizeSourceType: RosterSizeSourceType.Question,
                        enablementCondition: "rosterSource > 10",
                        children: new IComposite[]
                        {
                            Create.Entity.NumericIntegerQuestion(nestedQuestion)
                        }, 
                        hideIfDisabled: true)
                });
                
                interview = SetupInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, rosterSource, RosterVector.Empty, DateTime.Now, 1);

                using (eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, rosterSource, RosterVector.Empty, DateTime.Now, 0);
                    return eventContext.Events.Select(e => e.Payload).ToList();
                }
            });
        }

        private List<IEvent> events;

        [OneTimeTearDown]
        public void ClenupStuff()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void
            should_raise_RosterInstancesRemoved_event() =>
            this.events.ShouldContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterGroupId
                                                    && instance.RosterInstanceId == 0
                                                    && instance.OuterRosterVector.Length == 0));

        [Test]
        public void should_not_raise_RosterInstancesAdded_event() =>
            this.events.ShouldNotContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterGroupId));

        [Test]
        public void should_not_raise_GroupsEnabled_event() =>
            this.events.ShouldNotContainEvent<GroupsEnabled>(@event
                => @event.Groups.Any(g => g == Create.Identity(rosterGroupId, 0)));

        [Test]
        public void should_not_raise_QuestionsEnabled_event() =>
            this.events.ShouldNotContainEvent<QuestionsEnabled>(@event
                => @event.Questions.Any(q => q == Create.Identity(nestedQuestion, 0)));
        
        [Test]
        public void should_expect_only_two_events() =>
            Assert.That(this.events, Has.Count.EqualTo(2));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static readonly Guid rosterGroupId = Id.gA;
        private static readonly Guid rosterSource = Id.g1;
        private static readonly Guid nestedQuestion = Id.g2;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
    }
}