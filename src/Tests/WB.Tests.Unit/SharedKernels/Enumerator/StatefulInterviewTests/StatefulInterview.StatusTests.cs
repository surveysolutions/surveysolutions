using System;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal partial class StatefulInterviewTests
    {
        [Test]
        public void When_getting_status_for_interviewer_for_empty_interview()
        {
            var chapterId = Id.gA;
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: chapterId, 
                children: new IComposite[] {
                    Create.Entity.NumericIntegerQuestion(Id.g1, scope: QuestionScope.Interviewer, variable: "q1"),
                    Create.Entity.NumericIntegerQuestion(Id.g2, scope: QuestionScope.Supervisor, variable: "q2"),
                    Create.Entity.NumericIntegerQuestion(Id.g3, scope: QuestionScope.Hidden, variable: "q3"),
                    Create.Entity.NumericIntegerQuestion(Id.g4, isPrefilled: true, variable: "q4"),
                    Create.Entity.Group(Id.g5),
                    Create.Entity.Variable(Id.g6, variableName: "var1")
                }
            );

            var interview = SetUp.StatefulInterview(questionnaire);

            var status = interview.GetInterviewSimpleStatus(false);

            Assert.That(status.Status, Is.EqualTo(GroupStatus.NotStarted));
            Assert.That(status.SimpleStatus, Is.EqualTo(SimpleGroupStatus.Other));
            Assert.That(status.ActiveQuestionCount, Is.EqualTo(2));
            Assert.That(status.AnsweredQuestionsCount, Is.EqualTo(0));
        }
        [Test]
        public void When_getting_status_for_supervisor_for_empty_interview()
        {
            var chapterId = Id.gA;
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: chapterId, 
                children: new IComposite[] {
                    Create.Entity.NumericIntegerQuestion(Id.g1, scope: QuestionScope.Interviewer, variable: "q1"),
                    Create.Entity.NumericIntegerQuestion(Id.g2, scope: QuestionScope.Supervisor, variable: "q2"),
                    Create.Entity.NumericIntegerQuestion(Id.g3, scope: QuestionScope.Hidden, variable: "q3"),
                    Create.Entity.NumericIntegerQuestion(Id.g4, isPrefilled: true, variable: "q4"),
                    Create.Entity.Group(Id.g5),
                    Create.Entity.Variable(Id.g6, variableName: "var1")
                }
            );

            var interview = SetUp.StatefulInterview(questionnaire);

            var status = interview.GetInterviewSimpleStatus(true);

            Assert.That(status.Status, Is.EqualTo(GroupStatus.NotStarted));
            Assert.That(status.SimpleStatus, Is.EqualTo(SimpleGroupStatus.Other));
            Assert.That(status.ActiveQuestionCount, Is.EqualTo(3));
            Assert.That(status.AnsweredQuestionsCount, Is.EqualTo(0));
        }

        [Test]
        public void When_getting_status_for_interviewer_for_completed_interview()
        {
            var userId = Id.gF;
            var chapterId = Id.gA;
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: chapterId, 
                children: new IComposite[] {
                    Create.Entity.NumericIntegerQuestion(Id.g1, scope: QuestionScope.Interviewer, variable: "q1"),
                    Create.Entity.NumericIntegerQuestion(Id.g2, scope: QuestionScope.Supervisor, variable: "q2"),
                    Create.Entity.NumericIntegerQuestion(Id.g3, scope: QuestionScope.Hidden, variable: "q3"),
                    Create.Entity.NumericIntegerQuestion(Id.g4, isPrefilled: true, variable: "q4"),
                    Create.Entity.Group(Id.g5),
                    Create.Entity.Variable(Id.g6, variableName: "var1")
                }
            );

            var interview = SetUp.StatefulInterview(questionnaire);

            interview.AnswerNumericIntegerQuestion(userId, Id.g4, RosterVector.Empty, DateTime.Now, 42);
            interview.AnswerNumericIntegerQuestion(userId, Id.g1, RosterVector.Empty, DateTime.Now, 21);

            var status = interview.GetInterviewSimpleStatus(false);

            Assert.That(status.Status, Is.EqualTo(GroupStatus.Completed));
            Assert.That(status.SimpleStatus, Is.EqualTo(SimpleGroupStatus.Completed));
            Assert.That(status.ActiveQuestionCount, Is.EqualTo(2));
            Assert.That(status.AnsweredQuestionsCount, Is.EqualTo(2));
        }

        [Test]
        public void When_getting_status_for_supervisor_for_completed_interview()
        {
            var userId = Id.gF;
            var chapterId = Id.gA;
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: chapterId, 
                children: new IComposite[] {
                    Create.Entity.NumericIntegerQuestion(Id.g1, scope: QuestionScope.Interviewer, variable: "q1"),
                    Create.Entity.NumericIntegerQuestion(Id.g2, scope: QuestionScope.Supervisor, variable: "q2"),
                    Create.Entity.NumericIntegerQuestion(Id.g3, scope: QuestionScope.Hidden, variable: "q3"),
                    Create.Entity.NumericIntegerQuestion(Id.g4, isPrefilled: true, variable: "q4"),
                    Create.Entity.Group(Id.g5),
                    Create.Entity.Variable(Id.g6, variableName: "var1")
                }
            );

            var interview = SetUp.StatefulInterview(questionnaire);

            interview.AnswerNumericIntegerQuestion(userId, Id.g4, RosterVector.Empty, DateTime.Now, 42);
            interview.AnswerNumericIntegerQuestion(userId, Id.g1, RosterVector.Empty, DateTime.Now, 21);

            interview.AnswerNumericIntegerQuestion(userId, Id.g2, RosterVector.Empty, DateTime.Now, 21);

            var status = interview.GetInterviewSimpleStatus(true);

            Assert.That(status.Status, Is.EqualTo(GroupStatus.Completed));
            Assert.That(status.SimpleStatus, Is.EqualTo(SimpleGroupStatus.Completed));
            Assert.That(status.ActiveQuestionCount, Is.EqualTo(3));
            Assert.That(status.AnsweredQuestionsCount, Is.EqualTo(3));
        }
        
        [Test]
        public void When_getting_status_for_interviewer_for_invalid_interview()
        {
            var userId = Id.gF;
            var chapterId = Id.gA;
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: chapterId, 
                children: new IComposite[] {
                    Create.Entity.NumericIntegerQuestion(Id.g1, scope: QuestionScope.Interviewer, variable: "q1"),
                    Create.Entity.NumericIntegerQuestion(Id.g2, scope: QuestionScope.Supervisor, variable: "q2"),
                    Create.Entity.NumericIntegerQuestion(Id.g3, scope: QuestionScope.Hidden, variable: "q3"),
                    Create.Entity.NumericIntegerQuestion(Id.g4, isPrefilled: true, variable: "q4"),
                    Create.Entity.Group(Id.g5),
                    Create.Entity.Variable(Id.g6, variableName: "var1"),

                    Create.Entity.StaticText(Id.g7)
                }
            );

            var interview = SetUp.StatefulInterview(questionnaire);

            interview.AnswerNumericIntegerQuestion(userId, Id.g4, RosterVector.Empty, DateTime.Now, 42);
            interview.Apply(Create.Event.StaticTextsDeclaredInvalid(new Identity(Id.g7, RosterVector.Empty) ));

            var status = interview.GetInterviewSimpleStatus(false);

            Assert.That(status.Status, Is.EqualTo(GroupStatus.StartedInvalid));
            Assert.That(status.SimpleStatus, Is.EqualTo(SimpleGroupStatus.Invalid));
            Assert.That(status.ActiveQuestionCount, Is.EqualTo(2));
            Assert.That(status.AnsweredQuestionsCount, Is.EqualTo(1));
        }


        [Test]
        public void When_getting_status_for_supervisor_for_invalid_interview()
        {
            var userId = Id.gF;
            var chapterId = Id.gA;
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: chapterId, 
                children: new IComposite[] {
                    Create.Entity.NumericIntegerQuestion(Id.g1, scope: QuestionScope.Interviewer, variable: "q1"),
                    Create.Entity.NumericIntegerQuestion(Id.g2, scope: QuestionScope.Supervisor, variable: "q2"),
                    Create.Entity.NumericIntegerQuestion(Id.g3, scope: QuestionScope.Hidden, variable: "q3"),
                    Create.Entity.NumericIntegerQuestion(Id.g4, isPrefilled: true, variable: "q4"),
                    Create.Entity.Group(Id.g5),
                    Create.Entity.Variable(Id.g6, variableName: "var1"),

                    Create.Entity.StaticText(Id.g7)
                }
            );

            var interview = SetUp.StatefulInterview(questionnaire);

            interview.AnswerNumericIntegerQuestion(userId, Id.g4, RosterVector.Empty, DateTime.Now, 42);
            interview.AnswerNumericIntegerQuestion(userId, Id.g1, RosterVector.Empty, DateTime.Now, 21);

            interview.AnswerNumericIntegerQuestion(userId, Id.g2, RosterVector.Empty, DateTime.Now, 21);
            interview.Apply(Create.Event.StaticTextsDeclaredInvalid(new Identity(Id.g7, RosterVector.Empty) ));

            var status = interview.GetInterviewSimpleStatus(true);

            Assert.That(status.Status, Is.EqualTo(GroupStatus.CompletedInvalid));
            Assert.That(status.SimpleStatus, Is.EqualTo(SimpleGroupStatus.Invalid));
            Assert.That(status.ActiveQuestionCount, Is.EqualTo(3));
            Assert.That(status.AnsweredQuestionsCount, Is.EqualTo(3));
        }
    }
}
