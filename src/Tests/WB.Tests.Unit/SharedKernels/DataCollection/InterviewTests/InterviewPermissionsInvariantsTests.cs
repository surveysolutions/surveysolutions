using System;
using System.Linq.Expressions;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests;

public class InterviewPermissionsInvariantsTests : InterviewTestsContext
{
    [Test]
    public void when_answer_interviewer_question_after_complete_interview_should_throw_exception()
    {
        //arrange
        var questionId = Id.g1;
        var interviewerId = Id.g2;

        var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
            Create.Entity.TextQuestion(questionId));
        var interview = SetUp.StatefulInterview(questionnaire);
        interview.Complete(interviewerId, "complete", DateTimeOffset.Now, null);
        
        //act
        var exception = Assert.Catch<InterviewException>(() =>
            interview.AnswerTextQuestion(interviewerId, questionId, RosterVector.Empty, DateTimeOffset.UtcNow, "test")
        );
        
        //assert
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Interviewer question cannot be edited on completed interview"));
        Assert.That(interview.GetQuestion(Identity.Create(questionId, RosterVector.Empty)).IsAnswered(), Is.False);
    }
}
