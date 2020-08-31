#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Options;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;
using NHibernate.Criterion;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.BarCodes;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PdfSharpCore.Utils;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Sanitizer;
using WB.UI.Headquarters.Configs;
using WB.UI.Headquarters.PdfInterview.PdfWriters;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Services.Impl;
using Color = MigraDocCore.DocumentObjectModel.Color;
using Font = MigraDocCore.DocumentObjectModel.Font;
using PdfInterviewRes = WB.Core.BoundedContexts.Headquarters.Resources.PdfInterview;

namespace WB.UI.Headquarters.PdfInterview
{
    public class PdfInterviewGenerator : IPdfInterviewGenerator
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IAttachmentContentService attachmentContentService;
        private readonly IOptions<GoogleMapsConfig> googleMapsConfig;
        private readonly IOptions<HeadquartersConfig> headquartersConfig;

        
        public PdfInterviewGenerator(IQuestionnaireStorage questionnaireStorage,
            IStatefulInterviewRepository statefulInterviewRepository,
            IImageFileStorage imageFileStorage,
            IAttachmentContentService attachmentContentService,
            IOptions<GoogleMapsConfig> googleMapsConfig,
            IOptions<HeadquartersConfig> headquartersConfig)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.imageFileStorage = imageFileStorage;
            this.attachmentContentService = attachmentContentService;
            this.googleMapsConfig = googleMapsConfig;
            this.headquartersConfig = headquartersConfig;
        }

        static PdfInterviewGenerator()
        {
            IFontResolver pdfInterviewFontResolver = new PdfInterviewFontResolver();
            GlobalFontSettings.FontResolver = pdfInterviewFontResolver;
            
            ImageSource.ImageSourceImpl = new ImageSharpImageSource<SixLabors.ImageSharp.PixelFormats.Rgba32>();
        }

        public Stream Generate(Guid interviewId, IPrincipal user)
        {
            var interview = statefulInterviewRepository.Get(interviewId.FormatGuid());
            if (interview == null)
                return null;

            var questionnaire = questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            if (questionnaire == null)
                return null;

            var nodes = GetAllInterviewNodes(interview, user).ToList();
            var identifyedEntities = questionnaire.GetPrefilledEntities().Select(id => new Identity(id, RosterVector.Empty)).ToList();
           
            PdfDocument pdfDocument = new PdfDocument();
            Document document = new Document();
            new DefinePdfStyles().Define(document);
            var firstPageSection = document.AddSection();
            firstPageSection.PageSetup.PageFormat = PageFormat.A4;
            WritePdfInterviewHeader(firstPageSection, nodes, questionnaire, interview);
            DrawDoubleLines(firstPageSection);

            if (identifyedEntities.Count > 0)
            {
                WritePrefilledData(identifyedEntities, questionnaire, interview, document);
                DrawDoubleLines(firstPageSection);
            }

            var tableOfContents = WriteTableOfContents(firstPageSection);
            nodes.RemoveAll(node => identifyedEntities.Contains(node));
            WriteInterviewData(nodes, questionnaire, interview, document, tableOfContents);

            SetPagesMargins(document);
            WriteFooterToAllPages(document, questionnaire, interview);
            
            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true);
            renderer.Document = document;
            renderer.PdfDocument = pdfDocument;
            renderer.RenderDocument();

            RenderBarCode(pdfDocument.Pages[0], interview);

            var memoryStream = new MemoryStream();
            renderer.PdfDocument.Save(memoryStream);
            return memoryStream;
        }

        private void DrawDoubleLines(Section section)
        {
            var table = section.AddTable();
            table.AddColumn(Unit.FromPoint(525));
            var row = table.AddRow();
            row.Format.Font.Size = Unit.FromPoint(1);
            row.Borders.Bottom = new Border() { Style = BorderStyle.Single, Width = Unit.FromPoint(1)};
            row.Borders.Top = new Border() { Style = BorderStyle.Single, Width = Unit.FromPoint(1)};
        }

        private void WritePrefilledData(List<Identity> nodes,
            IQuestionnaire questionnaire,
            IStatefulInterview interview,
            Document document)
        {
            var coverSectionTitle = questionnaire.IsCoverPageSupported
                ? interview.GetGroup(new Identity(questionnaire.CoverPageSectionId, RosterVector.Empty)).Title.Text.RemoveHtmlTags()
                : PdfInterviewRes.Cover;

            var section = document.LastSection;
            section.AddParagraph();
            var sectionTitleParagraph = section.AddParagraph();
            sectionTitleParagraph.Style = PdfStyles.SectionHeader;
            sectionTitleParagraph.AddBookmark(coverSectionTitle);
            sectionTitleParagraph.AddWrappedText(coverSectionTitle);

            foreach (Identity node in nodes)
            {
                if (questionnaire.IsQuestion(node.Id))
                {
                    var question = interview.GetQuestion(node);
                    new QuestionPdfWriter(question, interview, imageFileStorage, googleMapsConfig)
                        .Write(section.AddParagraph());
                    if (question.FailedErrors != null && question.FailedErrors.Any())
                        new ErrorsPdfWriter(question).Write(section.AddParagraph());
                    if (question.FailedWarnings != null && question.FailedWarnings.Any())
                        new WarningsPdfWriter(question).Write(section.AddParagraph());
                    if (question.AnswerComments != null && question.AnswerComments.Any())
                        new CommentsPdfWriter(question).Write(section.AddParagraph());
                    section.AddParagraph();
                    continue;
                }

                if (questionnaire.IsStaticText(node.Id))
                {
                    var staticText = interview.GetStaticText(node);
                    new StaticTextPdfWriter(staticText, interview, questionnaire, attachmentContentService)
                        .Write(section.AddParagraph());
                    if (staticText.FailedErrors != null && staticText.FailedErrors.Any())
                        new ErrorsPdfWriter(staticText).Write(section.AddParagraph());
                    if (staticText.FailedWarnings != null && staticText.FailedWarnings.Any())
                        new WarningsPdfWriter(staticText).Write(section.AddParagraph());
                    section.AddParagraph();
                    continue;
                }

                throw new ArgumentException("Unknown prefilled tree node type for entity " + node);
            }
        }

        private void WriteInterviewData(List<Identity> nodes, IQuestionnaire questionnaire, IStatefulInterview interview,
            Document document, Paragraph tableOfContents)
        {
            Table table = null;
            DateTimeOffset? prevDateTime = null;

            foreach (Identity node in nodes)
            {
                if (questionnaire.IsQuestion(node.Id))
                {
                    var row = table.AddRow();
                    var question = interview.GetQuestion(node);
                    if (question.AnswerTime.HasValue)
                    {
                        if (question.AnswerTime != null && prevDateTime?.Date != question.AnswerTime.Value.Date)
                            WriteQuestionDate(row[0].AddParagraph(), question);
                        WriteQuestionTime(row[1].AddParagraph(), question);
                        prevDateTime = question.AnswerTime;
                    }

                    new QuestionPdfWriter(question, interview, imageFileStorage, googleMapsConfig)
                        .Write(row[2].AddParagraph());

                    if (question.FailedErrors != null && question.FailedErrors.Any())
                        new ErrorsPdfWriter(question).Write(row[2].AddParagraph());
                    if (question.FailedWarnings != null && question.FailedWarnings.Any())
                        new WarningsPdfWriter(question).Write(row[2].AddParagraph());
                    if (question.AnswerComments != null && question.AnswerComments.Any())
                        new CommentsPdfWriter(question).Write(row[2].AddParagraph());
                    row[2].AddParagraph();

                    continue;
                }

                if (questionnaire.IsStaticText(node.Id))
                {
                    var row = table.AddRow();
                        
                    var staticText = interview.GetStaticText(node);
                    new StaticTextPdfWriter(staticText, interview, questionnaire, attachmentContentService)
                        .Write(row[2].AddParagraph());
                    if (staticText.FailedErrors != null && staticText.FailedErrors.Any())
                        new ErrorsPdfWriter(staticText).Write(row[2].AddParagraph());
                    if (staticText.FailedWarnings != null && staticText.FailedWarnings.Any())
                        new WarningsPdfWriter(staticText).Write(row[2].AddParagraph());
                    row[2].AddParagraph();
                    
                    continue;
                }

                if (questionnaire.IsSubSection(node.Id))
                {
                    var group = interview.GetGroup(node);
                    if (@group is InterviewTreeSection interviewTreeSection)
                    {
                        var section = document.AddSection();
                        section.PageSetup.PageFormat = PageFormat.A4;

                        new SectionPdfWriter(interviewTreeSection)
                            .Write(section.AddParagraph());
                        table = GenerateQuestionsTable(document.LastSection);
                        WritePageOfContentRecord(tableOfContents, interviewTreeSection.Title.Text);
                    }
                    else
                    {
                        new GroupPdfWriter(group).Write(document.LastSection.AddParagraph());
                        table = GenerateQuestionsTable(document.LastSection);
                    }

                    continue;
                }

                if (questionnaire.IsRosterGroup(node.Id))
                {
                    var roster = interview.GetRoster(node);
                    new GroupPdfWriter(roster).Write(document.LastSection.AddParagraph());
                    table = GenerateQuestionsTable(document.LastSection);
                    continue;
                }

                if (questionnaire.IsVariable(node.Id))
                    continue;

                throw new ArgumentException("Unknown tree node type for entity " + node);
            }
        }

        private void SetPagesMargins(Document document)
        {
            foreach (Section section in document.Sections)
            {
                section.PageSetup.PageFormat = PageFormat.A4;
                
                section.PageSetup.LeftMargin = Unit.FromPoint(37);
                section.PageSetup.RightMargin = Unit.FromPoint(33);
                section.PageSetup.TopMargin = Unit.FromPoint(31);
                section.PageSetup.BottomMargin = Unit.FromPoint(31);
            }
        }

        private void RenderBarCode(PdfPage page, IStatefulInterview interview)
        {
            var interviewKey = interview.GetInterviewKey().ToString();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            BarCode barcode = BarCode.FromType(CodeType.Code3of9Standard, interviewKey);
            barcode.TextLocation = new TextLocation();
            barcode.Text = interviewKey;
            barcode.StartChar = Convert.ToChar("*");
            barcode.EndChar = Convert.ToChar("*");
            barcode.Direction = CodeDirection.LeftToRight;
            XFont fontBarcode = new XFont("Arial", 14, XFontStyle.Regular);
            var position = new XPoint(Convert.ToDouble(37), Convert.ToDouble(0));
            XSize size = new XSize(Convert.ToDouble(149), Convert.ToDouble(53));
            barcode.Size = size;
            gfx.DrawBarCode(barcode, XBrushes.Black, fontBarcode, position);
        }

        private Paragraph WriteTableOfContents(Section section)
        {
            var paragraph = section.AddParagraph();
            paragraph.Style = PdfStyles.SectionHeader;
            paragraph.AddLineBreak();
            paragraph.AddText(PdfInterviewRes.TableOfContent);

            var tableOfContent = section.AddParagraph();
            tableOfContent.Style = PdfStyles.TableOfContent;
            return tableOfContent;
        }

        private void WriteFooterToAllPages(Document document, IQuestionnaire questionnaire, IStatefulInterview interview)
        {
            foreach (Section section in document.Sections)
            {
                //section.Footers.Primary.Format.SpaceAfter = Unit.FromPoint(10);
                section.Footers.Primary.Format.SpaceBefore = Unit.FromPoint(10);
                section.Footers.Primary.Format.LeftIndent = Unit.FromPoint(0);
                section.Footers.Primary.Format.RightIndent = Unit.FromPoint(0);
                section.Footers.Primary.Format.Borders.Top = new Border()
                {
                    Style = BorderStyle.Dot,
                    Width = Unit.FromPoint(1)
                };
                
                Paragraph leftFooter = section.Footers.Primary.AddParagraph();
                leftFooter.AddPageField();
                leftFooter.AddText(PdfInterviewRes.PageOf);
                leftFooter.AddNumPagesField();
                leftFooter.Format.Font.Size = Unit.FromPoint(6);
                leftFooter.Format.Alignment = ParagraphAlignment.Left;            
                
                Paragraph centerFooter = section.Footers.Primary.AddParagraph();
                centerFooter.AddText(questionnaire.Title);
                centerFooter.Format.Font.Size = Unit.FromPoint(6);
                centerFooter.Format.Alignment = ParagraphAlignment.Center;            

                Paragraph rightFooter = section.Footers.Primary.AddParagraph();
                rightFooter.AddText(interview.GetInterviewKey().ToString());
                rightFooter.Format.Font.Size = Unit.FromPoint(6);
                rightFooter.Format.Alignment = ParagraphAlignment.Right;            
            }
        }

        private void WritePageOfContentRecord(Paragraph paragraph, string text)
        {
            var title = text.RemoveHtmlTags();
            Hyperlink hyperlink = paragraph.AddHyperlink(title);
            hyperlink.AddPageRefField(title);
            hyperlink.AddText($"\t{title}");
            paragraph.AddLineBreak();
        }

        private static IEnumerable<Identity> GetAllInterviewNodes(IStatefulInterview interview, IPrincipal user)
        {
            var enabledSectionIds = interview.GetEnabledSections().Select(x => x.Identity);

            foreach (var enabledSectionId in enabledSectionIds)
            {
                var interviewEntities = user.IsInRole(UserRoles.Interviewer.ToString())
                    ? interview.GetUnderlyingInterviewerEntities(enabledSectionId)
                    : interview.GetUnderlyingEntitiesForReviewRecursive(enabledSectionId);
                
                foreach (var interviewEntity in interviewEntities.Where(interview.IsEnabled))
                    yield return interviewEntity;
            }
        }

        private void WritePdfInterviewHeader(Section section, List<Identity> nodes, IQuestionnaire questionnaire, IStatefulInterview interview)
        {
            var questions = nodes.Where(node => questionnaire.IsQuestion(node.Id)).ToList();
            int answeredQuestions = questions.Count(node => interview.WasAnswered(node)); 
            int unansweredQuestions = questions.Count(node => !interview.WasAnswered(node) && interview.IsEnabled(node)); 
            int sectionsCount = questionnaire.GetAllSections().Count(nodeId => interview.IsEnabled(new Identity(nodeId, RosterVector.Empty))); 
            int errorsCount = nodes.Count(node => !interview.IsEntityValid(node));
            int warningsCount = nodes.Count(node => !interview.IsEntityPlausible(node));
            int commentedCount = questions.Count(node => interview.GetQuestionComments(node).Any());

            var textFrame = section.AddTextFrame();
            textFrame.RelativeVertical = RelativeVertical.Page;
            textFrame.RelativeHorizontal = RelativeHorizontal.Page;
            textFrame.Top = TopPosition.Parse("31pt");
            textFrame.Width = Unit.FromPoint(590);
            textFrame.Height = Unit.FromPoint(40);

            var headerTable = textFrame.AddTable();
            headerTable.AddColumn(Unit.FromPoint(205));
            headerTable.AddColumn(Unit.FromPoint(22));
            headerTable.AddColumn(Unit.FromPoint(120));
            headerTable.AddColumn(Unit.FromPoint(221));
            var headerRow = headerTable.AddRow();


            var logoContent = GetEmbeddedResource("pdf_logo.png");
            ImageSource.IImageSource logoImageSource = ImageSource.FromStream("logo.png", () => logoContent);
            var image = headerRow[1].AddImage(logoImageSource);
            image.Width = Unit.FromPoint(14);
            image.Height = Unit.FromPoint(35);
            image.LockAspectRatio = true;

            var surveySolutions = headerRow[2].AddParagraph();
            surveySolutions.AddFormattedText(PdfInterviewRes.SurveySolutions, isBold: true, size: Unit.FromPoint(10));
            surveySolutions.AddLineBreak();
            surveySolutions.AddFormattedText(PdfInterviewRes.InterviewTranscript, size: Unit.FromPoint(10));
            

            if (!string.IsNullOrEmpty(headquartersConfig.Value.BaseUrl))
            {
                var leftTopText = headerRow[3].AddParagraph();
                leftTopText.Format.Alignment = ParagraphAlignment.Right;
                leftTopText.Format.Font.Size = Unit.FromPoint(7);
                leftTopText.AddText(string.Format(PdfInterviewRes.GeneratedBy, headquartersConfig.Value.BaseUrl));
                leftTopText.AddLineBreak();
                leftTopText.AddText(string.Format(PdfInterviewRes.GeneratedAt, DateTime.UtcNow.ToString(PdfDateTimeFormats.DateTimeFormat)));
            }

            Table table = section.AddTable();
            table.AddColumn(Unit.FromPoint(175)).LeftPadding = Unit.FromPoint(0);
            table.AddColumn(Unit.FromPoint(175)).LeftPadding = Unit.FromPoint(15);
            table.AddColumn(Unit.FromPoint(175)).LeftPadding = Unit.FromPoint(15);

            table.AddRow().TopPadding = Unit.FromPoint(40);
            var row = table.AddRow();

            row[0].AddParagraphFormattedText(Common.InterviewKey, PdfStyles.HeaderLineTitle);

            var interviewKeyValue = row[0].AddParagraph();
            interviewKeyValue.Format.Font.Size = Unit.FromPoint(18); 
            interviewKeyValue.AddText(interview.GetInterviewKey().ToString());

            if (interview.StartedDate.HasValue)
            {
                var startedTitle = row[0].AddParagraph();
                startedTitle.Style = PdfStyles.HeaderLineTitle;
                startedTitle.Format.SpaceBefore = Unit.FromPoint(10);
                startedTitle.AddFormattedText(PdfInterviewRes.Started, PdfStyles.HeaderLineTitle);

                var startedValue = row[0].AddParagraph();
                startedValue.AddFormattedText(interview.StartedDate.Value.ToString(PdfDateTimeFormats.DateFormat), PdfStyles.HeaderLineDate);
                startedValue.AddSpace(1);
                startedValue.AddFormattedText(interview.StartedDate.Value.ToString(PdfDateTimeFormats.TimeFormat), PdfStyles.HeaderLineTime);
            }

            if (interview.CompletedDate.HasValue)
            {
                var completedTitle = row[0].AddParagraph();
                completedTitle.Style = PdfStyles.HeaderLineTitle;
                completedTitle.Format.SpaceBefore = Unit.FromPoint(10);
                completedTitle.AddFormattedText(PdfInterviewRes.Completed, PdfStyles.HeaderLineTitle);

                var completedValue = row[0].AddParagraph();
                completedValue.AddFormattedText(interview.CompletedDate.Value.ToString(PdfDateTimeFormats.DateFormat), PdfStyles.HeaderLineDate);
                completedValue.AddSpace(1);
                completedValue.AddFormattedText(interview.CompletedDate.Value.ToString(PdfDateTimeFormats.TimeFormat), PdfStyles.HeaderLineTime);
            }
            
            row[1].AddParagraphFormattedText(Common.Questionnaire, PdfStyles.HeaderLineTitle);
            var questionnaireTitle = row[1].AddParagraph();
            questionnaireTitle.Format.SpaceBefore = Unit.FromPoint(4);
            questionnaireTitle.AddFormattedText(questionnaire.Title, isBold: true, size: Unit.FromPoint(12));
            var questionnaireVersion = row[1].AddParagraph();
            questionnaireVersion.Format.SpaceBefore = Unit.FromPoint(4);
            questionnaireVersion.AddFormattedText(String.Format(PdfInterviewRes.Version, questionnaire.Version), 
                size: Unit.FromPoint(7));

            row[2].AddParagraphFormattedText(PdfInterviewRes.InterviewVolume, PdfStyles.HeaderLineTitle);
            var interviewStats = row[2].AddParagraph();
            interviewStats.Format.SpaceBefore = Unit.FromPoint(4);
            interviewStats.Format.Font.Size = Unit.FromPoint(9);
            //interviewStats.Format.LineSpacing = Unit.FromPoint(5);
            interviewStats.Format.LineSpacingRule = LineSpacingRule.OnePtFive;
            interviewStats.AddFormattedText(string.Format(PdfInterviewRes.InterviewStatisticsSections, sectionsCount), isBold: true);
            interviewStats.AddLineBreak();
            interviewStats.AddFormattedText(string.Format(PdfInterviewRes.InterviewStatisticsAnswered, answeredQuestions), isBold: true);
            interviewStats.AddLineBreak();
            interviewStats.AddFormattedText(string.Format(PdfInterviewRes.InterviewStatisticsUnanswered, unansweredQuestions));
            interviewStats.AddLineBreak();
            interviewStats.AddFormattedText(string.Format(PdfInterviewRes.InterviewStatisticsWarnings, warningsCount))
                .Color = warningsCount > 0 ? new Color(255, 138, 0) : new Color(182, 182, 182);
            interviewStats.AddLineBreak();
            interviewStats.AddFormattedText(string.Format(PdfInterviewRes.InterviewStatisticsErrors, errorsCount))
                .Color = errorsCount > 0 ? new Color(231, 73, 36) : new Color(33, 150, 83);
            interviewStats.AddLineBreak();
            interviewStats.AddFormattedText(string.Format(PdfInterviewRes.InterviewStatisticsCommented, commentedCount))
                .Color = commentedCount > 0 ? new Color(71, 27, 195) : new Color(182, 182, 182);

            row[0].Borders.Right = new Border() { Style = BorderStyle.Single, Width = Unit.FromPoint(1) };
            row[2].Borders.Left = new Border() { Style = BorderStyle.Single, Width = Unit.FromPoint(1) };
            table.AddRow();
        }
        
        private static Stream? GetEmbeddedResource(String filename)
        {
            return typeof(PdfInterviewGenerator).Assembly
                .GetManifestResourceStream($"WB.UI.Headquarters.Content.images.{filename}");
        }

        private static Table GenerateQuestionsTable(Section section)
        {
            var table = section.AddTable();
            table.AddColumn(Unit.FromPoint(49));
            table.AddColumn(Unit.FromPoint(60));
            table.AddColumn(Unit.FromPoint(416));
            return table;
        }

        private void WriteQuestionTime(Paragraph paragraph, InterviewTreeQuestion question)
        {
            if (question.AnswerTime.HasValue)
            {
                paragraph.AddFormattedText(question.AnswerTime.Value.ToString(PdfDateTimeFormats.TimeFormat),
                    PdfStyles.QuestionAnswerTime);
            }
        }
        
        private void WriteQuestionDate(Paragraph paragraph, InterviewTreeQuestion question)
        {
            if (question.AnswerTime.HasValue)
            {
                paragraph.AddFormattedText(question.AnswerTime.Value.ToString(PdfDateTimeFormats.DateFormat),
                    PdfStyles.QuestionAnswerDate);
            }
        }
    }
}
