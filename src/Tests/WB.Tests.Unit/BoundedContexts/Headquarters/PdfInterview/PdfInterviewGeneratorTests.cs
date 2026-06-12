using System;
using System.IO;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Configs;
using WB.Core.BoundedContexts.Headquarters.PdfInterview;
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
    public void when_generate_interview_pdf_with_large_image_answer_should_succeed_without_exceptions()
    {
        var multimediaQuestionId = Id.g3;
        var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
            Create.Entity.MultimediaQuestion(questionId: multimediaQuestionId));
        var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
        var interview = SetUp.StatefulInterview(questionnaireDocument);
        interview.Apply(new InterviewKeyAssigned(new InterviewKey(2895), DateTimeOffset.Now));
        interview.AnswerPictureQuestion(
            userId: Id.g4,
            questionId: multimediaQuestionId,
            rosterVector: RosterVector.Empty,
            originDate: DateTimeOffset.Now,
            pictureFileName: "large_photo.jpg");

        var questionnaireStorage = Mock.Of<IQuestionnaireStorage>(s =>
            s.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language) == questionnaire);
        var statefulInterviewRepository = Mock.Of<IStatefulInterviewRepository>(r =>
            r.Get(Id.g1.FormatGuid()) == interview);

        // Create a large in-memory image (2000x2000) to simulate an oversized photo upload
        var largeImageBytes = CreateJpegImageBytes(width: 2000, height: 2000);

        var imageFileStorage = Mock.Of<IImageFileStorage>(s =>
            s.GetInterviewBinaryData(interview.Id, "large_photo.jpg") == largeImageBytes);

        var generator = new PdfInterviewGenerator(questionnaireStorage,
            statefulInterviewRepository,
            imageFileStorage,
            Mock.Of<IAttachmentContentService>(),
            Mock.Of<IOptions<GoogleMapsConfig>>(),
            Mock.Of<IOptions<HeadquartersConfig>>(c => c.Value == new HeadquartersConfig()),
            Mock.Of<IAuthorizedUser>());

        var result = generator.Generate(Id.g1, PdfView.Interviewer);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Length, Is.GreaterThan(0));
    }

    private static byte[] CreateJpegImageBytes(int width, int height)
    {
        using var image = new Image<Rgb24>(width, height);
        using var ms = new MemoryStream();
        image.Save(ms, JpegFormat.Instance);
        return ms.ToArray();
    }
}
