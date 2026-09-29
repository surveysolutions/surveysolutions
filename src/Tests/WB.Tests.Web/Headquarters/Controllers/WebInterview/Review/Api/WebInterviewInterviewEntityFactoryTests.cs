using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api;

public class WebInterviewInterviewEntityFactoryTests : WebInterviewInterviewEntityFactorySpecification
{
    private Identity intQuestionId = Id.Identity1;
    private Identity singleOptionQuestionId = Id.Identity2;
    private Identity textListQuestionId = Id.Identity3;
    private Identity linkedToListQuestionId = Id.Identity4;
    private Identity doubleQuestionId = Id.Identity5;

    protected override QuestionnaireDocument GetDocument()
    {
        return Create.Entity.QuestionnaireDocumentWithOneChapter(Id.gA,
            Create.Entity.SingleQuestion(singleOptionQuestionId.Id, options: new List<Answer>()
            {
                new Answer(){AnswerValue = "1", AnswerText = "1"},
                new Answer(){AnswerValue = "2", AnswerText = "2"},
            }),
            Create.Entity.NumericIntegerQuestion(intQuestionId.Id),
            Create.Entity.TextListQuestion(textListQuestionId.Id),
            Create.Entity.SingleQuestion(linkedToListQuestionId.Id, linkedToQuestionId: textListQuestionId.Id),
            Create.Entity.NumericQuestion(doubleQuestionId.Id)
        );
    }

    [Test]
    public void when_GetEntityDetails_for_single_option_unanswered_question()
    {
        // act
        var singleOptionQuestion = Subject.GetEntityDetails(singleOptionQuestionId.ToString(), CurrentInterview, questionnaire, true)
            as InterviewSingleOptionQuestion;
        
        Assert.That(singleOptionQuestion, Is.Not.Null);
        Assert.That(singleOptionQuestion.Answer, Is.Null);
    }
    
    [Test]
    public void when_GetEntityDetails_for_text_list_unanswered_question()
    {
        // act
        var listQuestion = Subject.GetEntityDetails(textListQuestionId.ToString(), CurrentInterview, questionnaire, true)
            as InterviewTextListQuestion;
        
        Assert.That(listQuestion, Is.Not.Null);
        Assert.That(listQuestion.Rows, Is.Empty);
    }
    
    [Test]
    public void when_GetEntityDetails_for_single_option_linked_to_question_unanswered_question()
    {
        // act
        var linkedToListQuestion = Subject.GetEntityDetails(linkedToListQuestionId.ToString(), CurrentInterview, questionnaire, true)
            as InterviewSingleOptionQuestion;
        
        Assert.That(linkedToListQuestion, Is.Not.Null);
        Assert.That(linkedToListQuestion.Answer, Is.Null);
    }
    
    [Test]
    public void when_GetEntityDetails_for_integer_unanswered_question()
    {
        // act
        var interviewIntegerQuestion = Subject.GetEntityDetails(intQuestionId.ToString(), CurrentInterview, questionnaire, true)
            as InterviewIntegerQuestion;
        
        Assert.That(interviewIntegerQuestion, Is.Not.Null);
        Assert.That(interviewIntegerQuestion.Answer, Is.Null);
    }
    
    [Test]
    public void when_GetEntityDetails_for_double_unanswered_question()
    {
        // act
        var doubleQuestion = Subject.GetEntityDetails(doubleQuestionId.ToString(), CurrentInterview, questionnaire, true)
            as InterviewDoubleQuestion;
        
        Assert.That(doubleQuestion, Is.Not.Null);
        Assert.That(doubleQuestion.Answer, Is.Null);
    }

    [Test]
    public void when_GetEntityDetails_for_integer_answered_question_should_return_answer_value()
    {
        CurrentInterview.Apply(Create.Event.NumericIntegerQuestionAnswered(questionId: intQuestionId.Id, answer: 42));

        var result = Subject.GetEntityDetails(intQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewIntegerQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Answer, Is.EqualTo(42));
        Assert.That(result.IsAnswered, Is.True);
    }

    [Test]
    public void when_GetEntityDetails_for_double_answered_question_should_return_answer_value()
    {
        CurrentInterview.Apply(Create.Event.NumericRealQuestionAnswered(identity: doubleQuestionId, answer: 3.14m));

        var result = Subject.GetEntityDetails(doubleQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewDoubleQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Answer, Is.EqualTo((double)3.14m).Within(0.0001));
        Assert.That(result.IsAnswered, Is.True);
    }

    [Test]
    public void when_GetEntityDetails_for_single_option_answered_question_should_return_selected_value()
    {
        CurrentInterview.Apply(Create.Event.SingleOptionQuestionAnswered(singleOptionQuestionId.Id, singleOptionQuestionId.RosterVector, 2));

        var result = Subject.GetEntityDetails(singleOptionQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewSingleOptionQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Answer, Is.EqualTo(2));
    }

    [Test]
    public void when_GetEntityDetails_for_text_list_answered_question_should_return_rows()
    {
        CurrentInterview.Apply(Create.Event.TextListQuestionAnswered(
            questionId: textListQuestionId.Id,
            answers: new[] { new Tuple<decimal, string>(1, "Item one"), new Tuple<decimal, string>(2, "Item two") }));

        var result = Subject.GetEntityDetails(textListQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewTextListQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Rows, Has.Count.EqualTo(2));
        Assert.That(result.Rows[0].Text, Is.EqualTo("Item one"));
        Assert.That(result.Rows[1].Text, Is.EqualTo("Item two"));
    }

    [Test]
    public void when_GetEntityDetails_for_text_list_should_default_max_answers_count_to_200()
    {
        var result = Subject.GetEntityDetails(textListQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewTextListQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.MaxAnswersCount, Is.EqualTo(200));
    }

    [Test]
    public void when_GetEntityDetails_for_question_should_map_id()
    {
        var result = Subject.GetEntityDetails(intQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewIntegerQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(intQuestionId.ToString()));
    }

    [Test]
    public void when_GetEntityDetails_with_includeVariableName_should_populate_Name()
    {
        var result = Subject.GetEntityDetails(intQuestionId.ToString(), CurrentInterview, questionnaire, false, includeVariableName: true)
            as InterviewIntegerQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void when_GetEntityDetails_without_includeVariableName_Name_should_be_null()
    {
        var result = Subject.GetEntityDetails(intQuestionId.ToString(), CurrentInterview, questionnaire, false, includeVariableName: false)
            as InterviewIntegerQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.Null);
    }
}
