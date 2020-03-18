using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    public class when_creating_temporary_interview : InterviewTestsContext
    {
        [Test]
        public void should_create_interview_that_can_accept_answers()
        {
            using (var eventContext = new EventContext())
            {
                var questionnaireId = Guid.Parse("10000000000000000000000000000000");
                var interviewId = Id.g1;
                var userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var questionnaireVersion = 18;
                
                var questionnaireRepository = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                    => _.Version == questionnaireVersion);

                var command = Create.Command.CreateTemporaryInterview(interviewId, userId, Create.Entity.QuestionnaireIdentity(questionnaireId, questionnaireVersion));

                var interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);

                //Act
                interview.CreateTemporaryInterview(command);

                // Assert
                eventContext.ShouldContainEvent<InterviewCreated>(e => e.AssignmentId == null && e.QuestionnaireId == questionnaireId && e.QuestionnaireVersion == questionnaireVersion);
                eventContext.ShouldContainEvent<SupervisorAssigned>(e => e.SupervisorId == userId);
                eventContext.ShouldContainEvent<InterviewStatusChanged>(e => e.Status == InterviewStatus.InterviewerAssigned);
            }
        }

        [Test]
        public void should_throw_exception_if_cant_create_interview()
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            var interviewId = Id.g1;
            var userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaireVersion = 18;
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(questionnaireId, questionnaireVersion);

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.FixedRoster(fixedTitles: CreateFakeFixedTitles(60), children: new IComposite[]
                {
                    Create.Entity.FixedRoster(fixedTitles: CreateFakeFixedTitles(60), children: new IComposite[]
                    {
                        Create.Entity.FixedRoster(fixedTitles: CreateFakeFixedTitles(60), children: new IComposite[]
                        {
                            Create.Entity.TextQuestion()
                        })
                    })
                })
            });

            var questionnaireRepository = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity, questionnaireDocument);

            var command = Create.Command.CreateTemporaryInterview(interviewId, userId, questionnaireIdentity);

            var interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);

            //Act
            var result = Assert.Catch(() => interview.CreateTemporaryInterview(command));

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetType(), Is.EqualTo(typeof(InterviewException)));
            Assert.That(((InterviewException)result).ExceptionType, Is.EqualTo(InterviewDomainExceptionType.InterviewSizeLimitReached));
        }

        private static FixedRosterTitle[] CreateFakeFixedTitles(int count)
        {
            return Enumerable.Range(1, count).Select(i => new FixedRosterTitle(i, i.ToString())).ToArray();
        }
    }
}
