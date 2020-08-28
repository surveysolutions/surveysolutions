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

        private const string DateTimeFormat = "yyyy-MM-dd HH:mm zzz";
        private const string TimeFormat = "HH:mm zzz";
        private const string DateFormat = "yyyy-MM-dd";
        
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
            var questions = nodes.Where(node => questionnaire.IsQuestion(node.Id)).ToList();
            int answeredQuestions = questions.Count(node => interview.WasAnswered(node)); 
            int unansweredQuestions = questions.Count(node => !interview.WasAnswered(node) && interview.IsEnabled(node)); 
            int sectionsCount = questionnaire.GetAllSections().Count(nodeId => interview.IsEnabled(new Identity(nodeId, RosterVector.Empty))); 
            int errorsCount = nodes.Count(node => !interview.IsEntityValid(node));
            int warningsCount = nodes.Count(node => !interview.IsEntityPlausible(node));
            int commentedCount = questions.Count(node => interview.GetQuestionComments(node).Any());
            nodes.RemoveAll(node => identifyedEntities.Contains(node));
           
            PdfDocument pdfDocument = new PdfDocument();
            Document document = new Document();
            DefineStyles(document);
            var firstPageSection = document.AddSection();
            firstPageSection.PageSetup.PageFormat = PageFormat.A4;
            WritePdfInterviewHeader(firstPageSection, questionnaire, interview, 
                answeredQuestions, unansweredQuestions, sectionsCount, errorsCount, warningsCount, commentedCount);
            DrawDoubleLines(firstPageSection);
            WritePrefilledData(identifyedEntities, questionnaire, interview, document);
            DrawDoubleLines(firstPageSection);
            var tableOfContents = WriteTableOfContents(firstPageSection);
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
            row.Format.Font.Size = "1pt";
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
                    WriteQuestionData(section.AddParagraph(), question, interview);
                    if (question.FailedErrors != null && question.FailedErrors.Any())
                        WriteErrorsData(section.AddParagraph(), question, interview);
                    if (question.FailedWarnings != null && question.FailedWarnings.Any())
                        WriteWarningsData(section.AddParagraph(), question, interview);
                    if (question.AnswerComments != null && question.AnswerComments.Any())
                        WriteCommentsData(section.AddParagraph(), question, interview);
                    section.AddParagraph();
                    continue;
                }

                if (questionnaire.IsStaticText(node.Id))
                {
                    var staticText = interview.GetStaticText(node);
                    WriteStaticTextData(section.AddParagraph(), staticText, interview, questionnaire);
                    if (staticText.FailedErrors != null && staticText.FailedErrors.Any())
                        WriteErrorsData(section.AddParagraph(), staticText, interview);
                    if (staticText.FailedWarnings != null && staticText.FailedWarnings.Any())
                        WriteWarningsData(section.AddParagraph(), staticText, interview);
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

            foreach (Identity node in nodes)
            {
                if (questionnaire.IsQuestion(node.Id))
                {
                    var row = table.AddRow();
                    var question = interview.GetQuestion(node);
                    WriteQuestionTime(row[0].AddParagraph(), question, interview);
                    WriteQuestionData(row[2].AddParagraph(), question, interview);
                    if (question.FailedErrors != null && question.FailedErrors.Any())
                        WriteErrorsData(row[2].AddParagraph(), question, interview);
                    if (question.FailedWarnings != null && question.FailedWarnings.Any())
                        WriteWarningsData(row[2].AddParagraph(), question, interview);
                    if (question.AnswerComments != null && question.AnswerComments.Any())
                        WriteCommentsData(row[2].AddParagraph(), question, interview);
                    row[2].AddParagraph();

                    continue;
                }

                if (questionnaire.IsStaticText(node.Id))
                {
                    var row = table.AddRow();
                    var staticText = interview.GetStaticText(node);
                    WriteStaticTextData(row[2].AddParagraph(), staticText, interview, questionnaire);
                    if (staticText.FailedErrors != null && staticText.FailedErrors.Any())
                        WriteErrorsData(row[2].AddParagraph(), staticText, interview);
                    if (staticText.FailedWarnings != null && staticText.FailedWarnings.Any())
                        WriteWarningsData(row[2].AddParagraph(), staticText, interview);
                    continue;
                }

                if (questionnaire.IsSubSection(node.Id))
                {
                    var group = interview.GetGroup(node);
                    if (@group is InterviewTreeSection interviewTreeSection)
                    {
                        WriteSectionData(document.LastSection, interviewTreeSection.Title.Text);
                        table = GenerateQuestionsTable(document.LastSection);
                        WritePageOfContentRecord(tableOfContents, interviewTreeSection.Title.Text);
                    }
                    else
                    {
                        table = WriteGroupData(document.LastSection, @group);
                    }

                    continue;
                }

                if (questionnaire.IsRosterGroup(node.Id))
                {
                    var roster = interview.GetRoster(node);
                    table = WriteGroupData(document.LastSection, roster);
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
                section.PageSetup.LeftMargin = Unit.FromPoint(37);
                section.PageSetup.RightMargin = Unit.FromPoint(33);
                section.PageSetup.TopMargin = Unit.FromPoint(31);
            }
        }

        private void RenderBarCode(PdfPage page, IStatefulInterview? interview)
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

        private void DefineStyles(Document document)
        {
            var defaultFonts = "Noto Sans, Arial, sans-serif";

            var defaultStyle = document.Styles.AddStyle(PdfStyles.Default, StyleNames.DefaultParagraphFont);
            defaultStyle.Font.Name = defaultFonts;
            defaultStyle.Font.Bold = false;
            defaultStyle.Font.Italic = false;
            defaultStyle.Font.Color = Colors.Black;
            defaultStyle.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
            //defaultPaddingStyle.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center); 

            var tableOfContent = document.Styles.AddStyle(PdfStyles.TableOfContent, PdfStyles.Default);
            tableOfContent.ParagraphFormat.Font.Size = Unit.FromPoint(8);
            tableOfContent.ParagraphFormat.Font.Color = Colors.Black;
            //tableOfContent.ParagraphFormat.SpaceBefore = "8pt";
            //tableOfContent.ParagraphFormat.LeftIndent = "8pt";
            // tableOfContent.ParagraphFormat.LineSpacing = Unit.FromPoint(14);
            // tableOfContent.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
            tableOfContent.ParagraphFormat.LineSpacingRule = LineSpacingRule.OnePtFive;
            tableOfContent.ParagraphFormat.AddTabStop(Unit.FromPoint(37), TabAlignment.Left);

            document.Styles.AddStyle(PdfStyles.HeaderLineValue, PdfStyles.Default).Font =
                new Font() { Size = 18, Bold = true };
            
            var sectionHeader = document.Styles.AddStyle(PdfStyles.SectionHeader, PdfStyles.Default);
            sectionHeader.Font.Size = Unit.FromPoint(18); 
            //sectionHeader.ParagraphFormat.LeftIndent = "8pt";
            //sectionHeader.ParagraphFormat.Borders.Top = new Border() { Width = "1pt", Color = Colors.DarkGray };
            sectionHeader.ParagraphFormat.LineSpacing = 0;
            sectionHeader.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;
            sectionHeader.ParagraphFormat.OutlineLevel = OutlineLevel.Level1;
            //sectionHeader.ParagraphFormat.SpaceBefore = "40pt";
            sectionHeader.ParagraphFormat.SpaceAfter = "15pt";

            var groupHeader = document.Styles.AddStyle(PdfStyles.GroupHeader, PdfStyles.Default);
            groupHeader.Font.Size = "11pt";
            groupHeader.ParagraphFormat.LeftIndent = "70pt";
            groupHeader.ParagraphFormat.SpaceAfter = "15pt";
            
            document.Styles.AddStyle(PdfStyles.RosterTitle, PdfStyles.Default).Font =
                new Font() { Size = "11pt", Italic = true };

            var questionStyle = document.Styles.AddStyle(PdfStyles.QuestionTitle, PdfStyles.Default);
            questionStyle.Font.Size = "7pt";
            questionStyle.ParagraphFormat.LineSpacingRule = LineSpacingRule.OnePtFive;
            
            document.Styles.AddStyle(PdfStyles.QuestionAnswer, PdfStyles.Default).Font =
                new Font() { Size = "7pt", Bold = true };
            
            var notAnswered = document.Styles.AddStyle(PdfStyles.QuestionNotAnswered, PdfStyles.Default);
            notAnswered.Font = new Font() { Size = "7pt", Color = new Color(45, 156, 219), Italic = true };
            notAnswered.ParagraphFormat.LineSpacing = 1.5;
            
            var questionDateStyle = document.Styles.AddStyle(PdfStyles.QuestionAnswerDate, PdfStyles.Default);
            questionDateStyle.Font = new Font() { Size = "7pt" };
            questionDateStyle.ParagraphFormat.Alignment = ParagraphAlignment.Right;
            
            var questionTimeStyle = document.Styles.AddStyle(PdfStyles.QuestionAnswerTime, PdfStyles.Default);
            questionTimeStyle.Font = new Font() { Size = "7pt" };
            questionTimeStyle.ParagraphFormat.Alignment = ParagraphAlignment.Right;
            
            document.Styles.AddStyle(PdfStyles.StaticTextTitle, PdfStyles.Default).Font =
                new Font() { Size = "7pt" };
            document.Styles.AddStyle(PdfStyles.ValidateErrorTitle, PdfStyles.Default).Font =
                new Font() { Size = "7pt", Italic = true, Color = new Color(231, 73, 36)};
            document.Styles.AddStyle(PdfStyles.ValidateErrorMessage, PdfStyles.Default).Font =
                new Font() { Size = "7pt", Color = new Color(231, 73, 36) };
            document.Styles.AddStyle(PdfStyles.ValidateWarningTitle, PdfStyles.Default).Font =
                new Font() { Size = "7pt", Italic = true, Color = new Color(255, 138, 0)};
            document.Styles.AddStyle(PdfStyles.ValidateWarningMessage, PdfStyles.Default).Font =
                new Font() { Size = "7pt", Color = new Color(255, 138, 0) };

            var commentTitle = document.Styles.AddStyle(PdfStyles.CommentTitle, PdfStyles.Default);
            commentTitle.Font.Size = "6pt";
            commentTitle.ParagraphFormat.LeftIndent = "10pt";
            commentTitle.ParagraphFormat.SpaceBefore = "5pt";

            var commentAuthor = document.Styles.AddStyle(PdfStyles.CommentAuthor, PdfStyles.Default);
            commentAuthor.Font.Size = "7pt";
            commentAuthor.Font.Color = new Color(71, 27, 195);
            commentAuthor.Font.Italic = true;
            //commentAuthor.ParagraphFormat.LeftIndent = "0.5cm";

            var commentDateTime = document.Styles.AddStyle(PdfStyles.CommentDateTime, PdfStyles.Default);
            commentDateTime.Font.Size = "7pt";
            commentDateTime.Font.Color = new Color(71, 27, 195);
            commentDateTime.Font.Italic = true;
            commentDateTime.ParagraphFormat.LeftIndent = "10pt";

            var commentMessage = document.Styles.AddStyle(PdfStyles.CommentMessage, PdfStyles.Default);
            commentMessage.Font.Size = "7pt";

            var yesNoTitle = document.Styles.AddStyle(PdfStyles.YesNoTitle, PdfStyles.Default);
            yesNoTitle.Font.Size = "7pt";
            
            var headerLineTitle = document.Styles.AddStyle(PdfStyles.HeaderLineTitle, PdfStyles.Default);
            headerLineTitle.Font.Bold = true;
            headerLineTitle.Font.Size = Unit.FromPoint(7);
            headerLineTitle.ParagraphFormat.SpaceAfter = Unit.FromPoint(1);

            var headerDate = document.Styles.AddStyle(PdfStyles.HeaderLineDate, PdfStyles.Default);
            headerDate.Font.Bold = true;
            headerDate.Font.Size = Unit.FromPoint(12);

            var headerTime = document.Styles.AddStyle(PdfStyles.HeaderLineTime, PdfStyles.Default);
            headerTime.Font.Size = Unit.FromPoint(12);
        }

        private void WritePdfInterviewHeader(Section section, IQuestionnaire questionnaire, IStatefulInterview interview,
            int answeredQuestions, int unansweredQuestions, int sectionsCount, int errorsCount, int warningsCount, int commentsCount)
        {
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
                leftTopText.AddText(string.Format(PdfInterviewRes.GeneratedAt, DateTime.UtcNow.ToString(DateTimeFormat)));
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
                startedValue.AddFormattedText(interview.StartedDate.Value.ToString(DateFormat), PdfStyles.HeaderLineDate);
                startedValue.AddSpace(1);
                startedValue.AddFormattedText(interview.StartedDate.Value.ToString(TimeFormat), PdfStyles.HeaderLineTime);
            }

            if (interview.CompletedDate.HasValue)
            {
                var completedTitle = row[0].AddParagraph();
                completedTitle.Style = PdfStyles.HeaderLineTitle;
                completedTitle.Format.SpaceBefore = Unit.FromPoint(10);
                completedTitle.AddFormattedText(PdfInterviewRes.Completed, PdfStyles.HeaderLineTitle);

                var completedValue = row[0].AddParagraph();
                completedValue.AddFormattedText(interview.CompletedDate.Value.ToString(DateFormat), PdfStyles.HeaderLineDate);
                completedValue.AddSpace(1);
                completedValue.AddFormattedText(interview.CompletedDate.Value.ToString(TimeFormat), PdfStyles.HeaderLineTime);
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
            interviewStats.AddFormattedText(string.Format(PdfInterviewRes.InterviewStatisticsCommented, commentsCount))
                .Color = commentsCount > 0 ? new Color(71, 27, 195) : new Color(182, 182, 182);

            row[0].Borders.Right = new Border() { Style = BorderStyle.Single, Width = Unit.FromPoint(1) };
            row[2].Borders.Left = new Border() { Style = BorderStyle.Single, Width = Unit.FromPoint(1) };
            table.AddRow();
        }
        
        private static Stream GetEmbeddedResource(String filename)
        {
            return typeof(PdfInterviewGenerator).Assembly
                .GetManifestResourceStream($"WB.UI.Headquarters.Content.images.{filename}");
        }

        private void WriteStaticTextData(Paragraph paragraph, InterviewTreeStaticText staticText,
            IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            paragraph.Style = PdfStyles.StaticTextTitle;
            paragraph.AddWrapFormattedText(staticText.Title.Text.RemoveHtmlTags(), PdfStyles.StaticTextTitle);

            var attachmentInfo = questionnaire.GetAttachmentForEntity(staticText.Identity.Id);
            if (attachmentInfo != null)
            {
                var attachment = attachmentContentService.GetAttachmentContent(attachmentInfo.ContentId);
                if (attachment == null)
                    throw new ArgumentException("Unknown attachment");
                
                paragraph.AddLineBreak();

                if (attachment.IsImage())
                {
                    ImageSource.IImageSource imageSource = ImageSource.FromBinary(attachment.FileName, 
                        () => attachment.Content);

                    var image = paragraph.AddImage(imageSource);
                    image.LockAspectRatio = true;
                    image.Width = Unit.FromPoint(300);
                    image.Height = Unit.FromPoint(300);
                }
                else if (attachment.IsVideo())
                {
                    paragraph.AddWrapFormattedText($"{attachment.FileName}", PdfStyles.QuestionAnswer);
                }
                else if (attachment.IsAudio())
                {
                    paragraph.AddWrapFormattedText($"{attachment.FileName}", PdfStyles.QuestionAnswer);
                }
                else if (attachment.IsPdf())
                {
                    paragraph.AddWrapFormattedText($"{attachment.FileName}", PdfStyles.QuestionAnswer);
                }
            }
        }

        private Table WriteGroupData(Section section, InterviewTreeGroup @group)
        {
            var title = @group.Title.Text.RemoveHtmlTags();

            var paragraph = section.AddParagraph();
            paragraph.Style = PdfStyles.GroupHeader;
            paragraph.AddWrappedText(title);
            
            if (@group is InterviewTreeRoster roster)
            {
                paragraph.AddFormattedText(" - " + roster.RosterTitle.RemoveHtmlTags(), PdfStyles.RosterTitle);
            }
       
            return GenerateQuestionsTable(section);
        }

        private void WriteSectionData(Section section, string sectionTitle)
        {
            var title = sectionTitle.RemoveHtmlTags();
            section = section.Document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;

            var paragraph = section.AddParagraph();
            paragraph.Style = PdfStyles.SectionHeader;
            paragraph.AddBookmark(title);
            paragraph.AddWrappedText(title);
        }

        private static Table GenerateQuestionsTable(Section section)
        {
            var table = section.AddTable();
            table.Style = "Table";
            table.Borders.Color = Colors.Black;
            table.Borders.Width = 0;
            table.Borders.Left.Width = 0;
            table.Borders.Right.Width = 0;
            table.Borders.Top.Width = 0;
            table.Borders.Bottom.Width = 0;
            table.Rows.HeightRule = RowHeightRule.Auto;

            var column = table.AddColumn(Unit.FromCentimeter(2));
            column.Format.Alignment = ParagraphAlignment.Right;
            column = table.AddColumn(Unit.FromCentimeter(1));
            column = table.AddColumn(Unit.FromCentimeter(15));
            column.Format.Alignment = ParagraphAlignment.Left;
            return table;
        }

        private void WriteQuestionTime(Paragraph paragraph, InterviewTreeQuestion question,
            IStatefulInterview interview)
        {
            if (question.AnswerTime.HasValue)
            {
                paragraph.AddFormattedText(question.AnswerTime.Value.ToString(TimeFormat),
                    PdfStyles.QuestionAnswerTime);
                paragraph.AddLineBreak();
                paragraph.AddFormattedText(question.AnswerTime.Value.ToString(DateFormat),
                    PdfStyles.QuestionAnswerDate);
            }
        }

        private void WriteQuestionData(Paragraph paragraph, InterviewTreeQuestion question,
            IStatefulInterview interview)
        {
            paragraph.Style = PdfStyles.QuestionTitle;
            paragraph.AddWrapFormattedText(question.Title.Text.RemoveHtmlTags(), PdfStyles.QuestionTitle);
            paragraph.AddLineBreak();

            if (question.IsAnswered())
            {
                if (question.IsAudio)
                {
                    var audioQuestion = question.GetAsInterviewTreeAudioQuestion();
                    var audioAnswer = audioQuestion.GetAnswer();
                    paragraph.AddWrapFormattedText($"{audioAnswer.FileName} + ({audioAnswer.Length})", PdfStyles.QuestionAnswer);
                }
                else if (question.IsMultimedia)
                {
                    var multimediaQuestion = question.GetAsInterviewTreeMultimediaQuestion();
                    var fileName = multimediaQuestion.GetAnswer().FileName;
                    ImageSource.IImageSource imageSource = ImageSource.FromBinary(fileName, 
                        () => imageFileStorage.GetInterviewBinaryData(interview.Id, fileName));
                    var image = paragraph.AddImage(imageSource);
                    image.Width = Unit.FromPoint(300);
                    image.LockAspectRatio = true;
                }
                else if (question.IsArea)
                {
                    var areaQuestion = question.GetAsInterviewTreeAreaQuestion();
                    var areaAnswer = areaQuestion.GetAnswer().Value;
                    paragraph.AddWrapFormattedText(areaAnswer.ToString(), PdfStyles.QuestionAnswer);
                }
                else if (question.IsGps)
                {
                    var gpsQuestion = question.GetAsInterviewTreeGpsQuestion();
                    var geoPosition = gpsQuestion.GetAnswer().Value;
                    var mapsUrl = $"{googleMapsConfig.Value.BaseUrl}/maps/search/?api=1&query={geoPosition.Latitude},{geoPosition.Longitude}";
                    var hyperlink = paragraph.AddHyperlink(mapsUrl, HyperlinkType.Web);
                    hyperlink.AddFormattedText($"{geoPosition.Latitude}, {geoPosition.Longitude}", PdfStyles.QuestionAnswer);
                }
                else if (question.IsYesNo)
                {
                    var yesNoQuestion = question.GetAsInterviewTreeYesNoQuestion();
                    var yesNoAnswer = yesNoQuestion.GetAnswer();
                    if (yesNoAnswer.CheckedOptions.Any())
                    {
                        foreach (var answerOption in yesNoAnswer.CheckedOptions)
                        {
                            var option = interview.GetOptionForQuestionWithoutFilter(question.Identity, answerOption.Value, null);
                            var optionAnswer = answerOption.Yes ? Common.Yes : (answerOption.No ? Common.No : WebInterviewUI.Interview_Overview_NotAnswered);
                            paragraph.AddWrapFormattedText($"{optionAnswer}: ", PdfStyles.YesNoTitle);
                            paragraph.AddWrapFormattedText(option.Title, PdfStyles.QuestionAnswer);
                            paragraph.AddLineBreak();
                        }
                    }
                }
                else if (question.IsMultiFixedOption)
                {
                    var multiOptionQuestion = question.GetAsInterviewTreeMultiOptionQuestion();
                    foreach (var checkedValue in multiOptionQuestion.GetAnswer().CheckedValues)
                    {
                        var option = interview.GetOptionForQuestionWithoutFilter(question.Identity, checkedValue, null);
                        paragraph.AddWrapFormattedText(option.Title, PdfStyles.QuestionAnswer);
                        paragraph.AddLineBreak();
                    }
                }
                else if (question.IsMultiLinkedOption)
                {
                    var multiOptionQuestion = question.GetAsInterviewTreeMultiLinkedToRosterQuestion();
                    var checkedAnswers = multiOptionQuestion.GetAnswer()?.CheckedValues
                        .Select(x => new Identity(multiOptionQuestion.LinkedSourceId, x))
                        .Select(x => interview.GetQuestion(x)?.GetAnswerAsString() ?? interview.GetRoster(x)?.RosterTitle ?? string.Empty);

                    if (checkedAnswers != null)
                    {
                        foreach (var answer in checkedAnswers)
                        {
                            paragraph.AddWrapFormattedText(answer, PdfStyles.QuestionAnswer);
                            paragraph.AddLineBreak();
                        }
                    }
                }
                else if (question.IsMultiLinkedToList)
                {
                    var multiOptionQuestion = question.GetAsInterviewTreeMultiOptionLinkedToListQuestion();
                    
                    var multiToListAnswers = multiOptionQuestion.GetAnswer()?.ToDecimals()?.ToHashSet();
                    var refListQuestion = interview.FindQuestionInQuestionBranch(multiOptionQuestion.LinkedSourceId, question.Identity);
                    var refListQuestionAllOptions = ((InterviewTreeTextListQuestion)refListQuestion?.InterviewQuestion)?.GetAnswer()?.Rows;
                    var refListOptions = refListQuestionAllOptions?.Where(x => multiToListAnswers?.Contains(x.Value) ?? false).ToArray();
 
                    if (refListOptions != null)
                    {
                        foreach (var answer in refListOptions)
                        {
                            paragraph.AddWrapFormattedText(answer.Text, PdfStyles.QuestionAnswer);
                            paragraph.AddLineBreak();
                        }
                    }
                }
                else if (question.IsTextList)
                {
                    var textListQuestion = question.GetAsInterviewTreeTextListQuestion();
                    var answers = textListQuestion.GetAnswer()?.Rows;
                    if (answers != null)
                    {
                        foreach (var answer in answers)
                        {
                            paragraph.AddWrapFormattedText(answer.Text, PdfStyles.QuestionAnswer);
                            paragraph.AddLineBreak();
                        }
                    }
                }
                else
                {
                    paragraph.AddWrapFormattedText(question.GetAnswerAsString(), PdfStyles.QuestionAnswer);
                }
            }
            else
            {
                paragraph.AddWrapFormattedText(WebInterviewUI.Interview_Overview_NotAnswered, PdfStyles.QuestionNotAnswered);
            }
        }
        
        private void WriteErrorsData(Paragraph paragraph, IInterviewTreeValidateable validateable, IStatefulInterview interview)
        {
            if (validateable.FailedErrors != null && validateable.FailedErrors.Any())
            {
                paragraph.Style = PdfStyles.ValidateErrorTitle;
                
                bool isNeedAddErrorNumber = validateable.FailedErrors.Count > 1;

                foreach (var errorCondition in validateable.FailedErrors)
                {
                    paragraph.AddWrapFormattedText(PdfInterviewRes.Error, PdfStyles.ValidateErrorTitle);
                    if (isNeedAddErrorNumber)
                        paragraph.AddWrapFormattedText($" [{errorCondition.FailedConditionIndex}]", PdfStyles.ValidateErrorTitle);
                    paragraph.AddWrapFormattedText(": ", PdfStyles.ValidateErrorTitle);

                    var errorMessage = validateable.ValidationMessages[errorCondition.FailedConditionIndex];
                    paragraph.AddWrapFormattedText(errorMessage.Text.RemoveHtmlTags(), PdfStyles.ValidateErrorMessage);

                    if (validateable.FailedErrors.Last() != errorCondition)
                        paragraph.AddLineBreak();
                }
            }
        }
        
        private void WriteWarningsData(Paragraph paragraph, IInterviewTreeValidateable validateable, IStatefulInterview interview)
        {
            if (validateable.FailedWarnings != null && validateable.FailedWarnings.Any())
            {
                paragraph.Style = PdfStyles.ValidateWarningTitle;

                bool isNeedAddWarningNumber = validateable.FailedWarnings.Count > 1;

                foreach (var warningCondition in validateable.FailedWarnings)
                {
                    paragraph.AddWrapFormattedText(PdfInterviewRes.Warning, PdfStyles.ValidateWarningTitle);
                    if (isNeedAddWarningNumber)
                        paragraph.AddWrapFormattedText($" [{warningCondition.FailedConditionIndex}]", PdfStyles.ValidateWarningTitle);
                    paragraph.AddWrapFormattedText(": ", PdfStyles.ValidateWarningTitle);

                    var warningMessage = validateable.ValidationMessages[warningCondition.FailedConditionIndex];
                    paragraph.AddWrapFormattedText(warningMessage.Text.RemoveHtmlTags(), PdfStyles.ValidateWarningMessage);
                    
                    if (validateable.FailedWarnings.Last() != warningCondition)
                        paragraph.AddLineBreak();
                }
            }
        }

        private void WriteCommentsData(Paragraph commentsParagraph, InterviewTreeQuestion question, IStatefulInterview interview)
        {
            if (question.AnswerComments != null && question.AnswerComments.Any())
            {
                commentsParagraph.Style = PdfStyles.CommentTitle;
                commentsParagraph.AddWrapFormattedText(PdfInterviewRes.Comments.ToUpper(), PdfStyles.CommentTitle);

                foreach (var comment in question.AnswerComments)
                {
                    commentsParagraph.AddLineBreak();
                    commentsParagraph.AddWrapFormattedText(comment.CommentTime.ToString(DateTimeFormat), PdfStyles.CommentDateTime);
                    commentsParagraph.AddWrapFormattedText($" {comment.UserRole.ToUiString()}: ", PdfStyles.CommentAuthor);
                    commentsParagraph.AddWrapFormattedText(comment.Comment, PdfStyles.CommentMessage);
                }
            }
        }
    }
}
