#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Options;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.BarCodes;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PdfSharpCore.Utils;
using WB.Core.BoundedContexts.Headquarters.Configs;
using WB.Core.BoundedContexts.Headquarters.PdfInterview.PdfWriters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Sanitizer;
using PdfInterviewRes = WB.Core.BoundedContexts.Headquarters.Resources.PdfInterview;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview
{
    public class PdfInterviewGenerator : IPdfInterviewGenerator
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IAttachmentContentService attachmentContentService;
        private readonly IOptions<GoogleMapsConfig> googleMapsConfig;
        private readonly IOptions<HeadquartersConfig> headquartersConfig;
        private readonly IAuthorizedUser authorizedUser;


        public PdfInterviewGenerator(IQuestionnaireStorage questionnaireStorage,
            IStatefulInterviewRepository statefulInterviewRepository,
            IImageFileStorage imageFileStorage,
            IAttachmentContentService attachmentContentService,
            IOptions<GoogleMapsConfig> googleMapsConfig,
            IOptions<HeadquartersConfig> headquartersConfig,
            IAuthorizedUser authorizedUser)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.imageFileStorage = imageFileStorage;
            this.attachmentContentService = attachmentContentService;
            this.googleMapsConfig = googleMapsConfig;
            this.headquartersConfig = headquartersConfig;
            this.authorizedUser = authorizedUser;
        }

        static PdfInterviewGenerator()
        {
            IFontResolver pdfInterviewFontResolver = new PdfInterviewFontResolver();
            GlobalFontSettings.FontResolver = pdfInterviewFontResolver;
            
            ImageSource.ImageSourceImpl = new ImageSharpImageSource<SixLabors.ImageSharp.PixelFormats.Rgba32>();
        }

        public Stream? Generate(Guid interviewId)
        {
            var pdfView = !authorizedUser.IsAuthenticated || authorizedUser.IsInterviewer
                ? PdfView.Interviewer
                : PdfView.Review;
            return Generate(interviewId, pdfView);
        }
        
        public Stream? Generate(Guid interviewId, PdfView pdfView)
        {
            var interview = statefulInterviewRepository.Get(interviewId.FormatGuid());
            if (interview == null)
                return null;

            var questionnaire = questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            if (questionnaire == null)
                return null;

            var nodes = GetAllInterviewNodes(interview, pdfView).ToList();
            var identifyedEntities = questionnaire.GetPrefilledEntities().Select(id => new Identity(id, RosterVector.Empty)).ToList();
           
            PdfDocument pdfDocument = new PdfDocument();
            Document document = new Document();
            new DefinePdfStyles().Define(document);
            var firstPageSection = document.AddSection();
            firstPageSection.PageSetup.PageFormat = PageFormat.A4;
            new InterviewHeaderPdfWriter(nodes, questionnaire, interview, headquartersConfig).Write(firstPageSection);
            DrawDoubleLines(firstPageSection);

            if (identifyedEntities.Count > 0)
            {
                WritePrefilledData(identifyedEntities, questionnaire, interview, document);
                //DrawDoubleLines(firstPageSection);
                document.AddSection().PageSetup.PageFormat = PageFormat.A4;
            }

            var tableOfContents = WriteTableOfContents(document.LastSection);
            nodes.RemoveAll(node => identifyedEntities.Contains(node) || node.Id == questionnaire.CoverPageSectionId);
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

        private IEnumerable<Identity> GetAllInterviewNodes(IStatefulInterview interview, PdfView pdfView)
        {
            if (pdfView == PdfView.Interviewer)
            {
                foreach (var entity in interview.GetUnderlyingInterviewerEntities().Where(interview.IsEnabled))
                    yield return entity;
                
                yield break;
            }
            
            var enabledSectionIds = interview.GetEnabledSections().Select(x => x.Identity);

            foreach (var enabledSectionId in enabledSectionIds)
            {
                var interviewEntities = interview.GetUnderlyingEntitiesForReviewRecursive(enabledSectionId);
                
                foreach (var interviewEntity in interviewEntities.Where(interview.IsEnabled))
                    yield return interviewEntity;
            }
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
                    new QuestionPdfWriter(question, interview, questionnaire, imageFileStorage, googleMapsConfig, attachmentContentService)
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

                if (questionnaire.IsVariable(node.Id))
                {
                    var variable = interview.GetVariable(node);
                    new VariablePdfWriter(variable, interview, questionnaire)
                        .Write(section.AddParagraph());
                    section.AddParagraph();
                    continue;
                }

                throw new ArgumentException("Unknown prefilled tree node type for entity " + node);
            }
        }

        private void WriteInterviewData(List<Identity> nodes, IQuestionnaire questionnaire, IStatefulInterview interview,
            Document document, Paragraph tableOfContents)
        {
            Table? table = null;
            DateTimeOffset? prevDateTime = null;

            foreach (Identity node in nodes)
            {
                if (questionnaire.IsQuestion(node.Id))
                {
                    var row = table?.AddRow() ?? throw new ArgumentException("Table should be created before");
                    var question = interview.GetQuestion(node);
                    if (question.AnswerTime.HasValue)
                    {
                        if (question.AnswerTime != null && prevDateTime?.Date != question.AnswerTime.Value.Date)
                            WriteQuestionDate(row[0].AddParagraph(), question);
                        WriteQuestionTime(row[1].AddParagraph(), question);
                        prevDateTime = question.AnswerTime;
                    }

                    new QuestionPdfWriter(question, interview, questionnaire, imageFileStorage, googleMapsConfig, attachmentContentService)
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
                    var row = table?.AddRow() ?? throw new ArgumentException("Table should be created before");
                        
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
                    new RosterPdfWriter(roster, questionnaire).Write(document.LastSection.AddParagraph());
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
                section.PageSetup.BottomMargin = Unit.FromPoint(37);
                
                section.PageSetup.FooterDistance = Unit.FromPoint(16);
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
            XFont fontBarcode = new XFont(DefinePdfStyles.DefaultFonts, 14, XFontStyle.Regular);
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
                //section.Footers.Primary.Format. SpaceBefore = Unit.FromPoint(10);
                section.Footers.Primary.Format.LeftIndent = Unit.FromPoint(0);
                section.Footers.Primary.Format.RightIndent = Unit.FromPoint(0);
                section.Footers.Primary.Format.Borders.Top = new Border()
                {
                    Style = BorderStyle.DashLargeGap,
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
            hyperlink.AddTab();
            hyperlink.AddText(title);
            paragraph.AddLineBreak();
        }

        private static Table GenerateQuestionsTable(Section section)
        {
            var table = section.AddTable();
            table.AddColumn(Unit.FromPoint(49));
            table.AddColumn(Unit.FromPoint(60));
            table.AddColumn(Unit.FromPoint(416));

            table.Rows.HeightRule = RowHeightRule.Auto;
            return table;
        }

        private void WriteQuestionTime(Paragraph paragraph, InterviewTreeQuestion question)
        {
            if (question.AnswerTime.HasValue)
            {
                paragraph.AddFormattedText(question.AnswerTime.Value.ToString(DateTimeFormat.TimeWithTimezoneFormat),
                    PdfStyles.QuestionAnswerTime);
            }
        }
        
        private void WriteQuestionDate(Paragraph paragraph, InterviewTreeQuestion question)
        {
            if (question.AnswerTime.HasValue)
            {
                paragraph.AddFormattedText(question.AnswerTime.Value.ToString(DateTimeFormat.DateFormat),
                    PdfStyles.QuestionAnswerDate);
            }
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
    }
}
