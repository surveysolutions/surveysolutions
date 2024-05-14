using System;
using System.Collections.Generic;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal partial class StatefulInterviewTests
    {
        [Test]
        public void When_compliete_interview_with_yes_no_protected_question_on_tablet_Should_not_exists_new_protected_events()
        {
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: 
                Create.Entity.YesNoQuestion(questionId, variable: "var1", answers: new []{ 1,2,3,4,5})
                );

            var interview = Create.AggregateRoot.StatefulInterview(Guid.Empty, questionnaire: questionnaire, shouldBeInitialized:false);

            var answersToIdentifyingQuestions = new List<InterviewAnswer>
            {
                new InterviewAnswer
                {
                    Identity = Create.Identity(questionId),
                    Answer = Create.Entity.YesNoAnswer(new []
                    {
                        Create.Entity.AnsweredYesNoOption(1, true),
                        Create.Entity.AnsweredYesNoOption(2, false),
                        Create.Entity.AnsweredYesNoOption(5, true),
                    })
                }
            };

            var protectedAnswers = new List<string>() { "var1" };
            var command = Create.Command.CreateInterview(interview.Id, answers: answersToIdentifyingQuestions, assignmentId: 1,
                protectedAnswers: protectedAnswers);
            interview.CreateInterview(command);
            interview.AssignInterviewer(userId, userId, DateTime.UtcNow);
            var answerCommand = Create.Command.AnswerYesNoQuestion(userId, questionId, RosterVector.Empty, answeredOptions: new []
            {
                Create.Entity.AnsweredYesNoOption(1, true),
                Create.Entity.AnsweredYesNoOption(2, false),
                Create.Entity.AnsweredYesNoOption(3, true),
                Create.Entity.AnsweredYesNoOption(4, false),
                Create.Entity.AnsweredYesNoOption(5, true),
            });
            interview.AnswerYesNoQuestion(answerCommand);

            using (var eventContext = new EventContext())
            { 
                //act
                interview.Complete(userId, "comment", DateTime.UtcNow, null);

                eventContext.ShouldNotContainEvent<AnswersMarkedAsProtected>();
            }
        }

        [Test]
        public void When_compliete_interview_with_integer_protected_question_on_tablet_Should_not_exists_new_protected_events()
        {
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: 
                Create.Entity.NumericIntegerQuestion(questionId, variable: "var1")
                );

            var interview = Create.AggregateRoot.StatefulInterview(Guid.Empty, questionnaire: questionnaire, shouldBeInitialized: false);

            var answersToIdentifyingQuestions = new List<InterviewAnswer>
            {
                new InterviewAnswer
                {
                    Identity = Create.Identity(questionId),
                    Answer = Create.Entity.NumericIntegerAnswer(5)
                }
            };

            var protectedAnswers = new List<string>() { "var1" };
            var command = Create.Command.CreateInterview(interview.Id, answers: answersToIdentifyingQuestions, assignmentId: 1,
                protectedAnswers: protectedAnswers);
            interview.CreateInterview(command);
            interview.AssignInterviewer(userId, userId, DateTime.UtcNow);
            interview.AnswerNumericIntegerQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, 55);

            using (var eventContext = new EventContext())
            {
                //act
                interview.Complete(userId, "comment", DateTime.UtcNow, null);

                eventContext.ShouldNotContainEvent<AnswersMarkedAsProtected>();
            }
        }

        [Test]
        public void When_compliete_interview_with_multi_protected_question_on_tablet_Should_not_exists_new_protected_events()
        {
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: 
                Create.Entity.MultipleOptionsQuestion(questionId, variable: "var1", answers: new []{ 1,2,3,4,5})
                );

            var interview = Create.AggregateRoot.StatefulInterview(Guid.Empty, questionnaire: questionnaire, shouldBeInitialized: false);

            var answersToIdentifyingQuestions = new List<InterviewAnswer>
            {
                new InterviewAnswer
                {
                    Identity = Create.Identity(questionId),
                    Answer = Create.Entity.MultiOptionAnswer(1,2,5)
                }
            };

            var protectedAnswers = new List<string>() { "var1" };
            var command = Create.Command.CreateInterview(interview.Id, answers: answersToIdentifyingQuestions, assignmentId: 1,
                protectedAnswers: protectedAnswers);
            interview.CreateInterview(command);
            interview.AssignInterviewer(userId, userId, DateTime.UtcNow);
            interview.AnswerMultipleOptionsQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, new []{1,2,3,4,5});

            using (var eventContext = new EventContext())
            {
                //act
                interview.Complete(userId, "comment", DateTime.UtcNow, null);

                eventContext.ShouldNotContainEvent<AnswersMarkedAsProtected>();
            }
        }

        [Test]
        public void When_compliete_interview_with_text_list_protected_question_on_tablet_Should_not_exists_new_protected_events()
        {
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: 
                Create.Entity.TextListQuestion(questionId, variable: "var1")
                );

            var interview = Create.AggregateRoot.StatefulInterview(Guid.Empty, questionnaire: questionnaire, shouldBeInitialized: false);

            var answersToIdentifyingQuestions = new List<InterviewAnswer>
            {
                new InterviewAnswer
                {
                    Identity = Create.Identity(questionId),
                    Answer = Create.Entity.ListAnswer(1,2)
                }
            };

            var protectedAnswers = new List<string>() { "var1" };
            var command = Create.Command.CreateInterview(interview.Id, answers: answersToIdentifyingQuestions, assignmentId: 1,
                protectedAnswers: protectedAnswers);
            interview.CreateInterview(command);
            interview.AssignInterviewer(userId, userId, DateTime.UtcNow);
            interview.AnswerTextListQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, new []
            {
                new Tuple<decimal, string>(1, "answer #1"), 
                new Tuple<decimal, string>(2, "answer #2"), 
                new Tuple<decimal, string>(3, "answer #3"), 
            });

            using (var eventContext = new EventContext())
            {
                //act
                interview.Complete(userId, "comment", DateTime.UtcNow, null);

                eventContext.ShouldNotContainEvent<AnswersMarkedAsProtected>();
            }
        }
    }
}
