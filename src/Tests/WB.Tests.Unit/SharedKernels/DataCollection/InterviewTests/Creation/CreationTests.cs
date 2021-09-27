using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Creation
{
    [TestOf(typeof(Interview))]
    internal class CreationTests
    {
        [Test]
        public void should_be_able_to_put_answers_to_questions_to_interview()
        {
            Guid q1 = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            var identity = Create.Identity(q1);
            var textQuestionAnswer = "text";

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(q1, variable: "txt", scope: QuestionScope.Interviewer));

            var interview = SetUp.StatefulInterview(questionnaire, false);
            var answers = new List<InterviewAnswer>();
            answers.Add(
                new InterviewAnswer
                {
                    Identity = identity,
                    Answer = Create.Entity.TextQuestionAnswer(textQuestionAnswer)
                });


            // Act
            interview.CreateInterview(
                Create.Command.CreateInterview(answers: answers));

            // Assert
            Assert.That(interview.GetTextQuestion(identity).GetAnswer().Value, Is.EqualTo(textQuestionAnswer));
            Assert.That(interview.IsReadOnlyQuestion(identity), Is.False,
                "Interviewer scoped questions should not be marked as readonly");
        }

        [Test]
        public void should_not_allow_create_created_interview()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter();

            var interview = SetUp.StatefulInterview(questionnaire, false);

            // Act
            interview.CreateInterview(Create.Command.CreateInterview(interviewId: Id.gA));
            TestDelegate act = () => interview.CreateInterview(Create.Command.CreateInterview(interviewId: Id.gA));

            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<InterviewException>()
                                             .With.Property(nameof(InterviewException.ExceptionType)).EqualTo(InterviewDomainExceptionType.DuplicateCreationCommand));
        }

        [Test]
        public void should_be_able_to_create_interview_with_answered_question_in_roster()
        {
            Guid q1 = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            Guid rosterId = Guid.Parse("cccccccccccccccccccccccccccccccc");

            var identity = Create.Identity(q1, 1);
            var textQuestionAnswer = "text";

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.Roster(
                    rosterId,
                    rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                    fixedRosterTitles: new[] {Create.Entity.FixedTitle(1, "one")},
                    children: new List<IComposite>
                    {
                        Create.Entity.TextQuestion(q1, variable: "txt")
                    })
            );

            var interview = SetUp.StatefulInterview(questionnaire, false);
            var answers = new List<InterviewAnswer>
            {
                Create.Entity.InterviewAnswer(identity, Create.Entity.TextQuestionAnswer(textQuestionAnswer))
            };

            // Act
            interview.CreateInterview(
                Create.Command.CreateInterview(answers: answers));

            // Assert
            Assert.That(interview.GetTextQuestion(identity).GetAnswer().Value, Is.EqualTo(textQuestionAnswer));
        }
    }
}
