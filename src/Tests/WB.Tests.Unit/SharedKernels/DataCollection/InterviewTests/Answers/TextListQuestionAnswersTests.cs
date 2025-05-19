using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Answers;

public class TextListQuestionAnswersTests: InterviewTestsContext
{
    readonly Guid questionId = Id.g1;
    readonly Guid userId = Id.gA;
    private StatefulInterview interview;

    [SetUp]
    public void setup_interview_with_text_list_question()
    {
        var variableName = "text_list__variable";

        var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
            Create.Entity.TextListQuestion(questionId, variable: variableName));

        interview = Create.AggregateRoot.StatefulInterview(shouldBeInitialized: false,
            questionnaire: questionnaire);

        var command = Create.Command.CreateInterview(
            questionnaire.PublicKey, 1,
            null, new List<InterviewAnswer>(),
            userId);

        interview.CreateInterview(command);
    }

    [Test]
    public void when_answer_non_empty_text_list_answers_should_allow()
    {
        using (EventContext eventContext = new EventContext())
        {
            Tuple<decimal, string>[] newAnswer = Create.Entity.ListAnswer(1, 2).ToTupleArray();

            interview.AnswerTextListQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, newAnswer);

            eventContext.ShouldContainEvent<TextListQuestionAnswered>(ev => ev.Answers.Length == 2);
        }
    }

    [Test]
    public void when_answer_with_one_empty_answer_with_spaces_in_text_list_answer_should_throw_exception()
    {
        Tuple<decimal, string>[] newAnswer = Create.Entity.ListAnswer(1, 2, 3).ToTupleArray();
        newAnswer[1] = new Tuple<decimal, string>(2, "       ");

        var exception = NUnit.Framework.Assert.Throws<InterviewException>(() => 
            interview.AnswerTextListQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, newAnswer)
        );
        
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("String values should be not empty or whitespaces for question"));
    }

    [Test]
    public void when_answer_with_all_empty_answer_with_spaces_in_text_list_answer_should_throw_exception()
    {
        Tuple<decimal, string>[] newAnswer =
        [
            Tuple.Create(1m, "       "),
            Tuple.Create(2m, "         "),
            Tuple.Create(3m, "           ")
        ];

        var exception = NUnit.Framework.Assert.Throws<InterviewException>(() => 
            interview.AnswerTextListQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, newAnswer)
        );
        
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("String values should be not empty or whitespaces for question"));
    }
}
