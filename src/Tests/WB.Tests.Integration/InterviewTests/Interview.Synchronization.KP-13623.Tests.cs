using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
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
    internal partial class InterviewTests 
    {
        [Test]
        public void KP_13623_Conditions_correct_play_on_SynchronizeInterviewEvents()
        {
            var userId = Id.g1;

            var questionnaireId = Guid.NewGuid();

            var q1 = Id.g2;
            var q2 = Id.g3;
            var q11 = Id.g4;
            var section = Id.gA;
            var q3 = Id.g5;

            using (var appDomainContext = AppDomainContext.Create())
            {
                SetUp.MockedServiceLocator();

                Answer Answer(int value) => Create.Entity.Answer(value.ToString(), value);

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.SingleOptionQuestion(q1, variable: "q1", answers: new List<Answer>
                    {
                        Answer(1), Answer(2)
                    }),
                    Create.Entity.SingleOptionQuestion(q11, variable: "q11", answers: new List<Answer>
                    {
                        Answer(1), Answer(2), Answer(3)
                    }),

                    Create.Entity.TextQuestion(q2, variable: "q2"),

                    Create.Entity.Group(section, "Sub", "sub", "q1 == 1 && IsAnswered(q11)", false, new[]
                    {
                        Create.Entity.TextQuestion(q3, variable: "q3")
                    }));

                var interview = SetupStatefullInterview(appDomainContext.AssemblyLoadContext, questionnaireDocument,
                    new List<object>());
                interview.AssignInterviewer(userId, userId, DateTime.UtcNow);

                interview.AnswerSingleOptionQuestion(userId, q1, RosterVector.Empty, DateTimeOffset.Now, 2);
                interview.AnswerSingleOptionQuestion(userId, q11, RosterVector.Empty, DateTimeOffset.Now, 2);

                interview.SynchronizeInterviewEvents(Create.Command.SynchronizeInterviewEventsCommand(interview.Id,
                    userId, interview.QuestionnaireIdentity.QuestionnaireId, interview.QuestionnaireIdentity.Version,
                    new IEvent[]
                    {
                        Create.Event.SingleOptionQuestionAnswered(q1, RosterVector.Empty, 1, userId),

                    }));

                using (var eventContext = new EventContext())
                {
                    interview.AnswerTextQuestion(Guid.NewGuid(), q2, RosterVector.Empty, DateTimeOffset.Now, "ff");

                    var disableEvent = GetFirstEventByType<GroupsDisabled>(eventContext.Events);
                    Assert.That(disableEvent, Is.Null);
                }
            }
        }
    }
}
