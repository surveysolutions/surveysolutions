using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests
{
    [TestOf(typeof(Interview))]
    internal partial class InterviewTests : InterviewTestsContext
    {
        [Test]
        public void when_synchronizing_interview_events_and_questions_from_list_roster_instance_dont_have_events_by_disabled_questions_then_expression_processor_should_disable_it_in_tree()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var numericId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var textListId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            var rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var textQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.NumericIntegerQuestion(numericId, variable: "i"),
                    Abc.Create.Entity.TextListQuestion(textListId),
                    Abc.Create.Entity.Roster(rosterId, variable: "r", rosterSizeQuestionId: textListId, children: new[]
                    {
                        Abc.Create.Entity.TextQuestion(textQuestionId, "i != 5")
                    })
                );

                var interview = SetupStatefullInterview(questionnaireDocument, new List<object>());
                interview.AssignInterviewer(userId, userId, DateTime.UtcNow);

                using (var eventContext = new EventContext())
                {

                    interview.SynchronizeInterviewEvents(Create.Command.SynchronizeInterviewEventsCommand(interview.Id,
                        userId, interview.QuestionnaireIdentity.QuestionnaireId, interview.QuestionnaireIdentity.Version,
                        new IEvent[]
                        {
                            Create.Event.NumericIntegerQuestionAnswered(numericId, RosterVector.Empty, 5, userId),
                            Create.Event.TextListQuestionAnswered(textListId, RosterVector.Empty,
                                new[] {new Tuple<decimal, string>(1, "1")},
                                DateTime.UtcNow)
                        }));

                    return new
                    {
                        DisabledEvent = GetFirstEventByType<QuestionsDisabled>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.DisabledEvent, Is.Not.Null);
            Assert.That(results.DisabledEvent.Questions.Length, Is.EqualTo(1));
            Assert.That(results.DisabledEvent.Questions.Select(e => e.Id).ToArray(),
                Is.EquivalentTo(new[] { textQuestionId }));

            appDomainContext.Dispose();
            appDomainContext = null;
        }
    }
}
