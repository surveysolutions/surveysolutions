using System;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
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
    public void when_generate_interview_pdf_with_image_in_unsupported_format_should_without_exceptions()
    {
        var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
            Create.Entity.MultimediaQuestion(questionId: Id.g3, variable: "photo"));
        var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
        var interview = SetUp.StatefulInterview(questionnaireDocument);
        interview.Apply(new InterviewKeyAssigned(new InterviewKey(2895), DateTimeOffset.Now));
        interview.AnswerPictureQuestion(Id.gA, Id.g3, RosterVector.Empty, DateTimeOffset.Now, "image.heic");

        var questionnaireStorage = Mock.Of<IQuestionnaireStorage>(s =>
            s.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language) == questionnaire);

        var statefulInterviewRepository = Mock.Of<IStatefulInterviewRepository>(r =>
            r.Get(Id.g1.FormatGuid()) == interview);

        var imageFileStorage = Mock.Of<IImageFileStorage>(s =>
            s.GetInterviewBinaryData(interview.Id, "image.heic") == new byte[] { 1, 234, 21, 0, 54, 1, 66, 78 });

        var generator = new PdfInterviewGenerator(questionnaireStorage,
            statefulInterviewRepository,
            imageFileStorage,
            Mock.Of<IAttachmentContentService>(),
            Mock.Of<IOptions<GoogleMapsConfig>>(),
            Mock.Of<IOptions<HeadquartersConfig>>(c => c.Value == new HeadquartersConfig()),
            Mock.Of<IAuthorizedUser>());

        var generate = generator.Generate(Id.g1, PdfView.Interviewer);

        Assert.That(generate, Is.Not.Null);
    }
}
