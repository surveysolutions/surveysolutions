using System;
using System.Linq;
using System.Reflection;
using MigraDocCore.DocumentObjectModel;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Configs;
using WB.Core.BoundedContexts.Headquarters.PdfInterview;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
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
    public void when_writing_footer_should_truncate_long_title_and_use_center_and_right_tab_stops()
    {
        const int pageWidth = 595;
        const int leftMargin = 37;
        const int rightMargin = 33;
        var longTitle = new string('T', 120);
        var expectedTitle = string.Concat(new string('T', 79), "…");
        var interviewKey = new InterviewKey(2895);

        var generator = new PdfInterviewGenerator(
            Mock.Of<IQuestionnaireStorage>(),
            Mock.Of<IStatefulInterviewRepository>(),
            Mock.Of<IImageFileStorage>(),
            Mock.Of<IAttachmentContentService>(),
            Mock.Of<IOptions<GoogleMapsConfig>>(),
            Mock.Of<IOptions<HeadquartersConfig>>(c => c.Value == new HeadquartersConfig()),
            Mock.Of<IAuthorizedUser>());

        var questionnaire = Mock.Of<IQuestionnaire>(q => q.Title == longTitle);
        var interview = Mock.Of<IStatefulInterview>(i => i.GetInterviewKey() == interviewKey);
        var document = new Document();
        var section = document.AddSection();
        section.PageSetup.PageWidth = MigraDocCore.DocumentObjectModel.Unit.FromPoint(pageWidth);
        section.PageSetup.LeftMargin = MigraDocCore.DocumentObjectModel.Unit.FromPoint(leftMargin);
        section.PageSetup.RightMargin = MigraDocCore.DocumentObjectModel.Unit.FromPoint(rightMargin);

        var writeFooterMethod = typeof(PdfInterviewGenerator)
            .GetMethod("WriteFooterToAllPages", BindingFlags.Instance | BindingFlags.NonPublic);

        writeFooterMethod!.Invoke(generator, new object[] { document, questionnaire, interview });

        var footerParagraph = section.Footers.Primary.Elements.OfType<Paragraph>().Single();
        var footerText = string.Concat(footerParagraph.Elements.OfType<Text>().Select(x => x.Content));
        var contentWidth = pageWidth - leftMargin - rightMargin;

        Assert.Multiple(() =>
        {
            Assert.That(footerParagraph.Format.TabStops.Count, Is.EqualTo(2));
            Assert.That(footerParagraph.Format.TabStops[0].Position.Point, Is.EqualTo(contentWidth / 2d));
            Assert.That(footerParagraph.Format.TabStops[0].Alignment, Is.EqualTo(TabAlignment.Center));
            Assert.That(footerParagraph.Format.TabStops[1].Position.Point, Is.EqualTo(contentWidth));
            Assert.That(footerParagraph.Format.TabStops[1].Alignment, Is.EqualTo(TabAlignment.Right));
            Assert.That(footerText, Does.Contain(expectedTitle));
            Assert.That(footerText, Does.Contain(interviewKey.ToString()));
        });
    }
}
