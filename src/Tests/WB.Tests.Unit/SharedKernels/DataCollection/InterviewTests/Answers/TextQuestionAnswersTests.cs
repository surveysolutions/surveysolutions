using System;
using System.Collections.Generic;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Answers;

public class TextQuestionAnswersTests : InterviewTestsContext
{
    readonly Guid questionId = Id.g1;
    readonly Guid userId = Id.gA;
    private StatefulInterview interview;

    [SetUp]
    public void setup_interview_with_text_question()
    {
        var variableName = "text_variable";

        var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
            Create.Entity.TextQuestion(questionId, variable: variableName));

        interview = Create.AggregateRoot.StatefulInterview(shouldBeInitialized: false,
            questionnaire: questionnaire);
        
        var command = Create.Command.CreateInterview(
            questionnaire.PublicKey, 1,
            null, new List<InterviewAnswer>(),
            userId);

        interview.CreateInterview(command);
    }

    [Test]
    public void When_answer_non_empty_text_answer_should_allow()
    {
        using (EventContext eventContext = new EventContext())
        {
            interview.AnswerTextQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, "new answer");

            eventContext.ShouldContainEvent<TextQuestionAnswered>();
        }
    }

    [Test]
    public void When_answer_with_spaces_in_text_answer_should_remove_answer()
    {
        interview.AnswerTextQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, "value");
        
        using (EventContext eventContext = new EventContext())
        {
            interview.AnswerTextQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, "      ");

            eventContext.ShouldNotContainEvent<TextQuestionAnswered>();
            eventContext.ShouldContainEvent<AnswersRemoved>();
        }
    }
}
