using System;
using Microsoft.Extensions.Options;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.IO;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Configs;
using WB.Core.BoundedContexts.Headquarters.PdfInterview;
using WB.Core.BoundedContexts.Headquarters.PdfInterview.PdfWriters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.PdfInterview;

[TestOf(typeof(PdfInterviewGenerator))]
public class PdfInterviewGeneratorTests
{
    [Test]
    public void when_generate_interview_pdf_should_without_exceptions()
    {
        var questionnaireDocument = Create.Entity.QuestionnaireDocument(Id.g2, "Q Title", new[]
        {
            Create.Entity.TextQuestion()
        });
        var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
        var interview = SetUp.StatefulInterview(questionnaireDocument);
        interview.Apply(new InterviewKeyAssigned(new InterviewKey(2895), DateTimeOffset.Now));

        var questionnaireStorage = Mock.Of<IQuestionnaireStorage>(s => 
            s.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language) == questionnaire);

        var statefulInterviewRepository = Mock.Of<IStatefulInterviewRepository>(r => 
            r.Get(Id.g1.FormatGuid()) == interview);
        
        var generator = new PdfInterviewGenerator(questionnaireStorage,
            statefulInterviewRepository,
            Mock.Of<IImageFileStorage>(),
            Mock.Of<IAttachmentContentService>(),
            Mock.Of<IOptions<GoogleMapsConfig>>(),
            Mock.Of<IOptions<HeadquartersConfig>>(c => c.Value == new HeadquartersConfig()),
            Mock.Of<IAuthorizedUser>());

        var generate = generator.Generate(Id.g1, PdfView.Interviewer);
        
        Assert.That(generate, Is.Not.Null);
    }

    [Test]
    public void when_writing_area_question_with_coordinates_should_contain_swapped_and_rounded_values()
    {
        var questionId = Id.g1;
        // Coordinates stored as "longitude,latitude" pairs (GIS X,Y order)
        const string coordinates = "10.123456,20.654321;11.222333,21.444555";

        var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
            id: Id.g2,
            children: new[] { Create.Entity.GeographyQuestion(id: questionId) });
        var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
        var interview = SetUp.StatefulInterview(questionnaireDocument);
        interview.Apply(new AreaQuestionAnswered(
            Id.g3, questionId, RosterVector.Empty, DateTimeOffset.Now,
            "POLYGON(...)", null, 123.456, 45.678, coordinates, null, 2, null, null));

        var question = interview.GetQuestion(Identity.Create(questionId, RosterVector.Empty));

        var writer = new QuestionPdfWriter(
            question, interview, questionnaire,
            Mock.Of<IImageFileStorage>(),
            Mock.Of<IOptions<GoogleMapsConfig>>(),
            Mock.Of<IAttachmentContentService>());

        var document = new Document();
        var section = document.AddSection();
        var paragraph = section.AddParagraph();
        writer.Write(paragraph);

        var ddl = DdlWriter.WriteToString(document);

        Assert.Multiple(() =>
        {
            // Latitude (parts[1]) shown before longitude (parts[0]), 6 decimal places, no comma between them
            Assert.That(ddl, Does.Contain("20.654321"), "Latitude of first point should appear");
            Assert.That(ddl, Does.Contain("10.123456"), "Longitude of first point should appear");
            Assert.That(ddl, Does.Contain("21.444555"), "Latitude of second point should appear");
            Assert.That(ddl, Does.Contain("11.222333"), "Longitude of second point should appear");
            Assert.That(ddl, Does.Not.Contain("20.654321, 10.123456"), "Comma-separated lat,lon format should not be used");
            // Area and length with 3 decimal places (values < 1000 avoid thousands-separator formatting)
            Assert.That(ddl, Does.Contain("123.456"), "Area should use 3 decimal places");
            Assert.That(ddl, Does.Contain("45.678"), "Length should use 3 decimal places");
        });
    }
}
