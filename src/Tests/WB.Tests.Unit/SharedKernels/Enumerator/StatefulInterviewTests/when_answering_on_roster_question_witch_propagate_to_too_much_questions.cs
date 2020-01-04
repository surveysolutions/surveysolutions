using System;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_answering_on_roster_question_witch_propagate_to_too_much_questions : StatefulInterviewTestsContext
    {
        [OneTimeSetUp]
        public void context()
        {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.Group(children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(id: rosterSizeQuestionId),

                    Create.Entity.Roster(rosterSizeQuestionId: rosterSizeQuestionId,
                        children: new IComposite[]
                        {
                            Create.Entity.Roster(variable: "subRoster1",
                                rosterSizeQuestionId: rosterSizeQuestionId,
                                children: new IComposite[]
                                {
                                    Create.Entity.Roster(variable: "subRoster2",
                                        rosterSizeQuestionId: rosterSizeQuestionId,
                                        children: new IComposite[]
                                        {
                                            Create.Entity.TextQuestion(variable: "text1"),
                                            Create.Entity.TextQuestion(variable: "text2"),
                                            Create.Entity.TextQuestion(variable: "text3"),
                                            Create.Entity.TextQuestion(variable: "text4"),
                                            Create.Entity.TextQuestion(variable: "text5")
                                        })
                                })
                        })
                })
            });

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaireDocument);
        }


        [Test]
        public void should_throw_interview_exception()
        {
            var exception = Assert.Throws<InterviewException>(() =>
            {
                interview.AnswerNumericIntegerQuestion(interviewerId, rosterSizeQuestionId, RosterVector.Empty, DateTime.UtcNow, 25);
            });

            Assert.That(exception.ExceptionType, Is.EqualTo(InterviewDomainExceptionType.InterviewSizeLimitReached));
        }

        private StatefulInterview interview;
        private readonly Guid interviewerId = Id.g1;
        private readonly Guid rosterSizeQuestionId = Id.g2;
    }
}
