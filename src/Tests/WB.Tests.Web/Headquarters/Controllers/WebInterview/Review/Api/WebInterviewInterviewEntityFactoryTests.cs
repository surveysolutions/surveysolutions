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
}
