using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using Moq;
using NUnit.Framework;
using SkiaSharp;
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

    [Test]
    public void when_shortening_footer_title_should_keep_short_title_as_is()
    {
        Assert.That(PdfInterviewGenerator.ShortenFooterTitle(null), Is.EqualTo(string.Empty));
        Assert.That(PdfInterviewGenerator.ShortenFooterTitle("Household survey"), Is.EqualTo("Household survey"));

        var maxLengthTitle = new string('a', PdfInterviewGenerator.FooterTitleMaxLength);
        Assert.That(PdfInterviewGenerator.ShortenFooterTitle(maxLengthTitle), Is.EqualTo(maxLengthTitle));
    }

    [Test]
    public void when_shortening_long_footer_title_should_cut_it_with_ellipsis()
    {
        var title = new string('a', PdfInterviewGenerator.FooterTitleMaxLength + 1);

        var shortened = PdfInterviewGenerator.ShortenFooterTitle(title);

        Assert.That(shortened, Has.Length.EqualTo(PdfInterviewGenerator.FooterTitleMaxLength));
        Assert.That(shortened, Does.EndWith("a…"));
    }

    [Test]
    public void when_shortening_footer_title_should_not_leave_spaces_before_ellipsis()
    {
        var title = new string('a', 140) + new string(' ', 20) + "tail";

        Assert.That(PdfInterviewGenerator.ShortenFooterTitle(title), Is.EqualTo(new string('a', 140) + "…"));
    }

    [Test]
    public void when_shortening_footer_title_should_not_split_surrogate_pair()
    {
        // the cut position is between two chars of the first emoji
        var title = new string('a', PdfInterviewGenerator.FooterTitleMaxLength - 2) + "😀😀";

        var shortened = PdfInterviewGenerator.ShortenFooterTitle(title);

        Assert.That(shortened, Is.EqualTo(new string('a', PdfInterviewGenerator.FooterTitleMaxLength - 2) + "…"));
        Assert.DoesNotThrow(() => new UTF8Encoding(false, true).GetBytes(shortened));
    }

    [Test]
    public void when_writing_footer_should_use_three_columns_with_fixed_widths_and_aligned_texts()
    {
        var section = CreateSection();

        PdfInterviewGenerator.WriteFooter(section, "Household survey", "00-00-28-95");

        var table = section.Footers.Primary.Elements.OfType<Table>().Single();
        Assert.That(table.Columns.Count, Is.EqualTo(3));
        Assert.That(table.Columns[0].Width.Point, Is.EqualTo(table.Columns[2].Width.Point));
        Assert.That(table.Columns[0].Width.Point + table.Columns[1].Width.Point + table.Columns[2].Width.Point,
            Is.EqualTo(595.276 - 37 - 33).Within(0.01));

        var cells = table.Rows[0].Cells;
        Assert.That(GetParagraph(cells[0]).Format.Alignment, Is.EqualTo(ParagraphAlignment.Left));
        Assert.That(GetParagraph(cells[1]).Format.Alignment, Is.EqualTo(ParagraphAlignment.Center));
        Assert.That(GetParagraph(cells[2]).Format.Alignment, Is.EqualTo(ParagraphAlignment.Right));
        Assert.That(GetText(cells[1]), Is.EqualTo("Household survey"));
        Assert.That(GetText(cells[2]), Is.EqualTo("00-00-28-95"));
    }

    [Test]
    public void when_writing_footer_with_very_long_title_should_shorten_title_and_keep_interview_key()
    {
        var section = CreateSection();

        PdfInterviewGenerator.WriteFooter(section, new string('a', 2000), "00-00-28-95");

        var cells = section.Footers.Primary.Elements.OfType<Table>().Single().Rows[0].Cells;
        Assert.That(GetText(cells[1]), Has.Length.EqualTo(PdfInterviewGenerator.FooterTitleMaxLength));
        Assert.That(GetText(cells[1]), Does.EndWith("…"));
        Assert.That(GetText(cells[2]), Is.EqualTo("00-00-28-95"));
    }

    private static Section CreateSection()
    {
        var document = new Document();
        var section = document.AddSection();
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.LeftMargin = 37;
        section.PageSetup.RightMargin = 33;
        return section;
    }

    private static Paragraph GetParagraph(Cell cell) => cell.Elements.OfType<Paragraph>().Single();

    private static string GetText(Cell cell) => string.Concat(GetParagraph(cell).Elements.OfType<Text>().Select(text => text.Content));

    private static byte[] CreateJpegImageBytes(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        return data.ToArray();
    }
}
