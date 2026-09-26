using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api;

/// <summary>
/// Tests for WebInterviewMapper covering critical migration points:
/// - Unanswered questions must never throw (was guaranteed by AutoMapper PreCondition)
/// - Answered questions must produce correct output
/// </summary>
[TestOf(typeof(WebInterviewMapper))]
public class WebInterviewMapperMigrationTests : WebInterviewInterviewEntityFactorySpecification
{
    private Identity gpsQuestionId = Id.Identity1;
    private Identity textListQuestionId = Id.Identity2;
    private Identity integerQuestionId = Id.Identity3;
    private Identity doubleQuestionId = Id.Identity4;
    private Identity dateQuestionId = Id.Identity5;
    private Identity multiOptionQuestionId = Create.Identity(Id.gB);
    private Identity yesNoQuestionId = Create.Identity(Id.gC);
    private Identity textQuestionId = Create.Identity(Id.gD);
    private Identity barcodeQuestionId = Create.Identity(Id.gE);

    protected override QuestionnaireDocument GetDocument()
    {
        return Create.Entity.QuestionnaireDocumentWithOneChapter(Id.gA,
            Create.Entity.GpsCoordinateQuestion(gpsQuestionId.Id),
            Create.Entity.TextListQuestion(textListQuestionId.Id),
            Create.Entity.NumericIntegerQuestion(integerQuestionId.Id),
            Create.Entity.NumericRealQuestion(doubleQuestionId.Id),
            Create.Entity.DateTimeQuestion(dateQuestionId.Id),
            Create.Entity.MultipleOptionsQuestion(multiOptionQuestionId.Id, textAnswers: new[]
            {
                new Answer { AnswerValue = "1", AnswerText = "Option A" },
                new Answer { AnswerValue = "2", AnswerText = "Option B" },
            }),
            Create.Entity.YesNoQuestion(yesNoQuestionId.Id, answers: new[] { 1, 2 }),
            Create.Entity.TextQuestion(textQuestionId.Id),
            Create.Entity.QRBarcodeQuestion(barcodeQuestionId.Id)
        );
    }

    // --- Unanswered question safety (critical migration point) ---

    [Test]
    public void when_GetEntityDetails_for_gps_unanswered_should_return_null_answer()
    {
        var result = Subject.GetEntityDetails(gpsQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewGpsQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Answer, Is.Null);
    }

    [Test]
    public void when_GetEntityDetails_for_textlist_unanswered_should_return_empty_rows()
    {
        var result = Subject.GetEntityDetails(textListQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewTextListQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Rows, Is.Empty);
    }

    [Test]
    public void when_GetEntityDetails_for_integer_unanswered_should_return_null_answer()
    {
        var result = Subject.GetEntityDetails(integerQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewIntegerQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Answer, Is.Null);
    }

    [Test]
    public void when_GetEntityDetails_for_double_unanswered_should_return_null_answer()
    {
        var result = Subject.GetEntityDetails(doubleQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewDoubleQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Answer, Is.Null);
    }

    [Test]
    public void when_GetEntityDetails_for_date_unanswered_should_return_null_answer()
    {
        var result = Subject.GetEntityDetails(dateQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewDateQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Answer, Is.Null);
    }

    [Test]
    public void when_GetEntityDetails_for_text_unanswered_should_return_null_answer()
    {
        var result = Subject.GetEntityDetails(textQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewTextQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Answer, Is.Null);
    }

    [Test]
    public void when_GetEntityDetails_for_barcode_unanswered_should_return_null_answer()
    {
        var result = Subject.GetEntityDetails(barcodeQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewBarcodeQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Answer, Is.Null);
    }

    // --- Answered question correctness ---

    [Test]
    public void when_GetEntityDetails_for_gps_answered_should_return_correct_coordinates()
    {
        CurrentInterview.Apply(Create.Event.GeoLocationQuestionAnswered(gpsQuestionId, 10.5, 20.3));

        var result = Subject.GetEntityDetails(gpsQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewGpsQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Answer, Is.Not.Null);
        Assert.That(result.Answer.Latitude, Is.EqualTo(10.5));
        Assert.That(result.Answer.Longitude, Is.EqualTo(20.3));
    }

    [Test]
    public void when_GetEntityDetails_for_textlist_answered_should_return_rows()
    {
        CurrentInterview.Apply(Create.Event.TextListQuestionAnswered(
            textListQuestionId.Id,
            answers: new[] { Tuple.Create(1m, "first"), Tuple.Create(2m, "second") }));

        var result = Subject.GetEntityDetails(textListQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewTextListQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Rows, Has.Count.EqualTo(2));
        Assert.That(result.Rows[0].Text, Is.EqualTo("first"));
        Assert.That(result.Rows[1].Text, Is.EqualTo("second"));
    }

    [Test]
    public void when_GetEntityDetails_for_integer_answered_should_return_value()
    {
        CurrentInterview.Apply(Create.Event.NumericIntegerQuestionAnswered(
            questionId: integerQuestionId.Id, answer: 42));

        var result = Subject.GetEntityDetails(integerQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewIntegerQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Answer, Is.EqualTo(42));
    }

    [Test]
    public void when_GetEntityDetails_for_double_answered_should_return_value()
    {
        CurrentInterview.Apply(Create.Event.NumericRealQuestionAnswered(
            identity: doubleQuestionId, answer: 3.14m));

        var result = Subject.GetEntityDetails(doubleQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewDoubleQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Answer, Is.EqualTo((double)3.14m).Within(0.001));
    }

    [Test]
    public void when_GetEntityDetails_for_date_answered_should_return_value()
    {
        var expectedDate = new DateTime(2024, 6, 15);
        CurrentInterview.Apply(Create.Event.DateTimeQuestionAnswered(
            CurrentInterview.Id, dateQuestionId, expectedDate));

        var result = Subject.GetEntityDetails(dateQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewDateQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Answer, Is.EqualTo(expectedDate));
    }

    [Test]
    public void when_GetEntityDetails_for_text_answered_should_return_value()
    {
        CurrentInterview.Apply(Create.Event.TextQuestionAnswered(
            questionId: textQuestionId.Id, answer: "hello world"));

        var result = Subject.GetEntityDetails(textQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewTextQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Answer, Is.EqualTo("hello world"));
    }

    [Test]
    public void when_GetEntityDetails_for_multioption_answered_should_return_selected_values()
    {
        CurrentInterview.Apply(Create.Event.MultipleOptionsQuestionAnswered(
            questionId: multiOptionQuestionId.Id,
            selectedOptions: new decimal[] { 1, 2 }));

        var result = Subject.GetEntityDetails(multiOptionQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewMutliOptionQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Answer, Is.Not.Null);
        Assert.That(result.Answer, Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void when_GetEntityDetails_for_yesno_answered_should_return_options()
    {
        CurrentInterview.Apply(Create.Event.YesNoQuestionAnswered(
            questionId: yesNoQuestionId.Id,
            answeredOptions: new[]
            {
                new AnsweredYesNoOption(1, true),
                new AnsweredYesNoOption(2, false)
            }));

        var result = Subject.GetEntityDetails(yesNoQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewYesNoQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Answer, Is.Not.Null);
        var answeredOption = result.Answer.FirstOrDefault(a => a.Value == 1);
        Assert.That(answeredOption, Is.Not.Null);
        Assert.That(answeredOption.Yes, Is.True);
    }

    // --- Base entity properties ---

    [Test]
    public void when_GetEntityDetails_should_set_IsAnswered_correctly_when_unanswered()
    {
        var result = Subject.GetEntityDetails(integerQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewIntegerQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsAnswered, Is.False);
    }

    [Test]
    public void when_GetEntityDetails_should_set_IsAnswered_correctly_when_answered()
    {
        CurrentInterview.Apply(Create.Event.NumericIntegerQuestionAnswered(
            questionId: integerQuestionId.Id, answer: 7));

        var result = Subject.GetEntityDetails(integerQuestionId.ToString(), CurrentInterview, questionnaire, false)
            as InterviewIntegerQuestion;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsAnswered, Is.True);
    }
}
