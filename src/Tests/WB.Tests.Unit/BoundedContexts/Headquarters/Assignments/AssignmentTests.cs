﻿using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    [TestOf(typeof(Assignment))]
    public class AssignmentTests
    {
        [Test]
        public void should_calculate_interviews_needed_quantity_without_deleted_interviews()
        {
            var assignment = Create.Entity.Assignment(quantity: 2);

            assignment.InterviewSummaries.Add(Create.Entity.InterviewSummary());

            Assert.That(assignment.InterviewsNeeded, Is.EqualTo(1));
            Assert.That(assignment.IsCompleted, Is.False);
        }

        [Test]
        public void should_not_complete_assignment_when_it_is_unlimited()
        {
            var assignment = Create.Entity.Assignment(quantity: null);

            assignment.InterviewSummaries.Add(Create.Entity.InterviewSummary());

            Assert.That(assignment.IsCompleted, Is.False);
            Assert.That(assignment.InterviewsNeeded, Is.Null);
        }

        [Test]
        public void should_fill_itself_from_interview()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.gA, 24);
            var questionIdentity = Create.Identity(Id.g1);
            var singleOptionQuestionIdentity = Create.Identity(Id.g2);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: questionIdentity.Id, preFilled: true),
                Create.Entity.SingleOptionQuestion(singleOptionQuestionIdentity.Id, 
                    isPrefilled: true,
                    answers: new List<Answer>
                    {
                        Create.Entity.Answer("one", 1),
                        Create.Entity.Answer("two", 2)
                    })
            });

            var interview = new Mock<IStatefulInterview>();

            interview.SetupGet(x => x.QuestionnaireIdentity)
                .Returns(questionnaireIdentity);

            interview.Setup(x => x.GetQuestion(questionIdentity))
                .Returns(Create.Entity.InterviewTreeQuestion(questionIdentity, answer: "text", questionType: QuestionType.Text));

            interview.Setup(x => x.GetQuestion(singleOptionQuestionIdentity))
                .Returns(Create.Entity.InterviewTreeSingleOptionQuestion(singleOptionQuestionIdentity, answer: 1));

            interview.Setup(x => x.GetAnswerAsString(questionIdentity, null))
                .Returns("text");

            // Act
            var answers = Assignment.GetAnswersFromInterview(interview.Object, Create.Entity.PlainQuestionnaire(questionnaire));

            // Assert
            Assert.That(answers, Has.Count.EqualTo(2));
            Assert.That(answers[0].Answer.ToString(), Is.EqualTo("text"));
            Assert.That(answers[1].Answer.ToString(), Is.EqualTo("1"));
        }
    }
}
