#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
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

        private const string DateTimeFormat = "yyyy-MM-dd HH:mm zzz";
        private const string TimeFormat = "HH:mm zzz";
        private const string DateFormat = "yyyy-MM-dd";
        
        public PdfInterviewGenerator(IQuestionnaireStorage questionnaireStorage,
            IStatefulInterviewRepository statefulInterviewRepository,
            IImageFileStorage imageFileStorage,
            IAttachmentContentService attachmentContentService)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.imageFileStorage = imageFileStorage;
            this.attachmentContentService = attachmentContentService;
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
            var questions = nodes.Where(node => questionnaire.IsQuestion(node.Id)).ToList();
            int answeredQuestions = questions.Count(node => interview.WasAnswered(node)); 
            int unansweredQuestions = questions.Count(node => !interview.WasAnswered(node) && interview.IsEnabled(node)); 
            int sectionsCount = questionnaire.GetAllSections().Count(nodeId => interview.IsEnabled(new Identity(nodeId, RosterVector.Empty))); 
            int errorsCount = nodes.Count(node => !interview.IsEntityValid(node));
            int warningsCount = nodes.Count(node => !interview.IsEntityPlausible(node));
            int commentedCount = questions.Count(node => interview.GetQuestionComments(node).Any());

            //GlobalFontSettings.FontResolver = PdfInterviewFontResolver;
            //var fontResolver = new FontResolver();
            //GlobalFontSettings.FontResolver = fontResolver;
            //GlobalFontSettings.FontResolver = new PdfInterviewFontResolver();
            //ImageSource.ImageSourceImpl = new ImageSharpImageSource<SixLabors.ImageSharp.PixelFormats.Rgba32>();
            
            PdfDocument pdfDocument = new PdfDocument();
            Document document = new Document();
            DefineStyles(document);
            var firstPageSection = document.AddSection();
            firstPageSection.PageSetup.PageFormat = PageFormat.A4;
            WritePdfInterviewHeader(firstPageSection, questionnaire, interview, 
                answeredQuestions, unansweredQuestions, sectionsCount, errorsCount, warningsCount, commentedCount);
            WriteSummaryHeader(firstPageSection);

            Table table = null;

            foreach (Identity node in nodes)
            {
                if (questionnaire.IsQuestion(node.Id))
                {
                    var question = interview.GetQuestion(node);
                    WriteQuestionData(table, question, interview);
                    continue;
                }

                if (questionnaire.IsStaticText(node.Id))
                {
                    var staticText = interview.GetStaticText(node);
                    WriteStaticTextData(table, staticText, interview, questionnaire);
                    continue;
                }

                if (questionnaire.IsSubSection(node.Id))
                {
                    var group = interview.GetGroup(node);
                    if (group is InterviewTreeSection interviewTreeSection)
                    {
                        table = WriteSectionData(document.LastSection, interviewTreeSection);
                        WritePageOfContentRecord(firstPageSection, interviewTreeSection);
                    }
                    else
                    {
                        table = WriteGroupData(document.LastSection, group);
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
            var position = new XPoint(Convert.ToDouble(60), Convert.ToDouble(0));
            XSize size = new XSize(Convert.ToDouble(130), Convert.ToDouble(40));
            barcode.Size = size;
            gfx.DrawBarCode(barcode, XBrushes.Black, fontBarcode, position);
        }

        private void WriteSummaryHeader(Section section)
        {
            var paragraph = section.AddParagraph();
            paragraph.Style = PdfStyles.SectionHeader;
            paragraph.AddLineBreak();
            paragraph.AddText(PdfInterviewRes.TableOfContent);

            var tableOfContent = section.AddParagraph();
            tableOfContent.Style = PdfStyles.TableOfContent;
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

        private void WritePageOfContentRecord(Section firstSection, InterviewTreeSection interviewTreeSection)
        {
            var title = interviewTreeSection.Title.Text.RemoveHtmlTags();

            var paragraph = firstSection.LastParagraph;
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

            var defaultPaddingStyle = document.Styles.AddStyle(PdfStyles.Default, StyleNames.DefaultParagraphFont);
            defaultPaddingStyle.Font.Name = defaultFonts;
            defaultPaddingStyle.Font.Bold = false;
            defaultPaddingStyle.Font.Italic = false;
            defaultPaddingStyle.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
            //defaultPaddingStyle.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center); 

            var tableOfContent = document.Styles.AddStyle(PdfStyles.TableOfContent, PdfStyles.Default);
            tableOfContent.ParagraphFormat.Font.Size = "8pt";
            tableOfContent.ParagraphFormat.Font.Color = Colors.Black;
            tableOfContent.ParagraphFormat.SpaceBefore = "8pt";
            tableOfContent.ParagraphFormat.LeftIndent = "8pt";
            tableOfContent.ParagraphFormat.LineSpacing = "16pt";
            tableOfContent.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
            tableOfContent.ParagraphFormat.AddTabStop("1.5cm", TabAlignment.Left);

            var headerLineTitle = document.Styles.AddStyle(PdfStyles.HeaderLineTitle, PdfStyles.Default);
            headerLineTitle.Font.Bold = false;
            headerLineTitle.Font.Size = 18;
            headerLineTitle.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);

            
            document.Styles.AddStyle(PdfStyles.HeaderLineValue, PdfStyles.Default).Font =
                new Font() { Size = 18, Bold = true };
            
            var sectionHeader = document.Styles.AddStyle(PdfStyles.SectionHeader, PdfStyles.Default);
            sectionHeader.Font = new Font() { Size = "16pt", Color = Colors.Black };
            sectionHeader.ParagraphFormat.LeftIndent = "8pt";
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
        }

        private void WritePdfInterviewHeader(Section section, IQuestionnaire questionnaire, IStatefulInterview interview,
            int answeredQuestions, int unansweredQuestions, int sectionsCount, int errorsCount, int warningsCount, int commentsCount)
        {
            var interviewKey = interview.GetInterviewKey().ToString();
            //QuestionnaireDocument d; d.Metadata.

            /*Code3of9Standard BarCode39 = new PdfSharp.Drawing.BarCodes.Code3of9Standard();
            BarCode39.TextLocation = new PdfSharp.Drawing.BarCodes.TextLocation();
            BarCode39.Text = txtDOCU;//value of code to draw on page
            BarCode39.StartChar = Convert.ToChar("*");
            BarCode39.EndChar = Convert.ToChar("*");
            BarCode39.Direction = PdfSharp.Drawing.BarCodes.CodeDirection.LeftToRight;
            PdfPage Page_BARCODE = PDFdoc.Pages[Itxt];
            XGraphics gfxBARCODE = XGraphics.FromPdfPage(Page_BARCODE);
            XFont fontBARCODE = new XFont("Arial", 14, XFontStyle.Regular);
            BarCode39.Size.ToXPoint();
            gfxBARCODE.DrawBarCode(BarCode39, XBrushes.Black,fontBARCODE,new XPoint(Convert.ToDouble(str_dbl_X), Convert.ToDouble(str_dbl_Y)));
            */
            /*{
                //var barcodeTf = section.Elements.AddTextFrame();
                var barcode = section.Elements.AddBarcode();
                barcode.RelativeHorizontal = RelativeHorizontal.Page;
                barcode.RelativeVertical = RelativeVertical.Page;
                barcode.FillFormat = new FillFormat() {Visible = true};
                barcode.Orientation = TextOrientation.Horizontal;
                barcode.Code = interviewKey;
                barcode.Text = true;
                barcode.Type = BarcodeType.Barcode39;
                barcode.Width = "6cm";
                barcode.Height = "2cm";
                //barcode.Left = LeftPosition.Parse("2cm");
                barcode.Left = LeftPosition.Parse("2cm");
                barcode.Top = TopPosition.Parse("0cm");
                barcode.LineFormat.Color = Colors.Black;
                barcode.FillFormat.Color = Colors.Black;
                            //barcode.
            // barcode.BearerBars = true;
            // barcode.LineHeight = 20;
            // barcode.LineRatio = 2;

            }*/

            /*{
                var barcode = section.Elements.AddTextFrame();
                //barcode.Left = LeftPosition.Parse("2cm");
                //barcode.Top = TopPosition.Parse("0cm");
                barcode.WrapFormat.DistanceTop = Unit.FromCentimeter(0);
                barcode.WrapFormat.DistanceLeft = Unit.FromCentimeter(2);
                barcode.RelativeHorizontal = RelativeHorizontal.Page;
                barcode.RelativeVertical = RelativeVertical.Page;
                barcode.Width = Unit.FromCentimeter(7);
                barcode.Height = Unit.FromCentimeter(2);
                var barcodeText = barcode.AddParagraph(interviewKey);
                barcodeText.Format.Font.Name = "Libre Barcode 128";
                barcodeText.Format.Font.Size = Unit.FromPoint(30);
            }*/

            var logoContent = GetEmbeddedResource("headquarter_logo.png");
            ImageSource.IImageSource logoImageSource = ImageSource.FromStream("logo.png", () => logoContent);
            var image = section.AddImage(logoImageSource);
            image.Width = Unit.FromPoint(100);
            image.LockAspectRatio = true;
            image.Left = LeftPosition.Parse("132pt"); 
            //image.Left = LeftPosition.Parse("0pt");
            image.Top = TopPosition.Parse("0pt");
            
            /*var questionnaireDocument = questionnaireStorage.GetQuestionnaireDocument(interview.QuestionnaireIdentity);
            if (questionnaireDocument != null && !string.IsNullOrEmpty(questionnaireDocument.Title))
            {
                var textFrame = section.AddTextFrame();
                textFrame.RelativeVertical = RelativeVertical.Page;
                textFrame.RelativeHorizontal = RelativeHorizontal.Page;
                textFrame.Top = TopPosition.Parse("0pt");
                textFrame.Left = LeftPosition.Parse("750pt"); 
                textFrame.Width = Unit.FromPoint(240);
                textFrame.Height = Unit.FromPoint(40);
                var leftTopText = textFrame.AddParagraph();
                //var leftTopText = section.AddParagraph();
                leftTopText.Format.Alignment = ParagraphAlignment.Right;
                leftTopText.Format.Font.Size = Unit.FromPoint(11);
                leftTopText.Format.Font.Bold = true;
                leftTopText.AddText(questionnaireDocument.Title);
            }*/



            Table table = section.AddTable();
            table.TopPadding = Unit.FromPoint(10);
            table.AddColumn(Unit.FromCentimeter(5));
            table.AddColumn(Unit.FromCentimeter(7));
            table.AddColumn(Unit.FromCentimeter(5));

            table.AddRow();
            var row = table.AddRow();

            var interviewKeyTitle = row[0].AddParagraph();
            interviewKeyTitle.Format.Font.Size = "8pt"; 
            interviewKeyTitle.Format.Font.Bold = true;
            interviewKeyTitle.AddText(Common.InterviewKey);

            var interviewKeyValue = row[0].AddParagraph();
            interviewKeyValue.Format.Font.Size = "18pt"; 
            interviewKeyValue.AddText(interviewKey);
            interviewKeyValue.Format.SpaceAfter = Unit.FromPoint(16);

            // date generate
            {
                var generatedDateTime = DateTime.Now;
                var title = row[0].AddParagraph();
                title.Format.SpaceBefore = Unit.FromPoint(16);
                title.Format.Font.Size = Unit.FromPoint(8); 
                title.Format.Font.Bold = true;
                title.AddText(PdfInterviewRes.Generated);

                var dateValue = row[0].AddParagraph();
                dateValue.Format.Font.Size = Unit.FromPoint(12); 
                dateValue.Format.Font.Bold = true; 
                dateValue.AddText(generatedDateTime.ToString(DateFormat));

                var timeValue = row[0].AddParagraph();
                timeValue.Format.Font.Size = Unit.FromPoint(12); 
                timeValue.AddText(generatedDateTime.ToString(TimeFormat));
            }

            if (interview.StartedDate.HasValue)
            {
                var startedTitle = row[0].AddParagraph();
                startedTitle.Format.SpaceBefore = Unit.FromPoint(16);
                startedTitle.Format.Font.Size = Unit.FromPoint(8); 
                startedTitle.Format.Font.Bold = true;
                startedTitle.AddText(PdfInterviewRes.Started);

                var startedValue = row[0].AddParagraph();
                startedValue.Format.Font.Size = Unit.FromPoint(12); 
                startedValue.Format.Font.Bold = true; 
                startedValue.AddText(interview.StartedDate.Value.ToString(DateFormat));

                var startedTimeValue = row[0].AddParagraph();
                startedTimeValue.Format.Font.Size = Unit.FromPoint(12); 
                startedTimeValue.AddText(interview.StartedDate.Value.ToString(TimeFormat));
            }

            if (interview.CompletedDate.HasValue)
            {
                var completedTitle = row[0].AddParagraph();
                completedTitle.Format.SpaceBefore = Unit.FromPoint(16);
                completedTitle.Format.Font.Size = Unit.FromPoint(8); 
                completedTitle.Format.Font.Bold = true;
                completedTitle.AddText(PdfInterviewRes.Completed);

                var completedValue = row[0].AddParagraph();
                completedValue.Format.Font.Size = Unit.FromPoint(12); 
                completedValue.Format.Font.Bold = true; 
                completedValue.AddText(interview.CompletedDate.Value.ToString(DateFormat));

                var completedTimeValue = row[0].AddParagraph();
                completedTimeValue.Format.Font.Size = Unit.FromPoint(12); 
                completedTimeValue.AddText(interview.CompletedDate.Value.ToString(TimeFormat));
            }
            

            var questionnaireTitle = row[1].AddParagraph();
            questionnaireTitle.Format.Font.Size = "8pt"; 
            questionnaireTitle.Format.Font.Bold = true;
            questionnaireTitle.AddText(Common.Questionnaire);
            
            var questionnaireValue = row[1].AddParagraph();
            questionnaireValue.Format.Font.Size = "12pt"; 
            questionnaireValue.Format.Font.Bold = true; 
            questionnaireValue.AddText(questionnaire.Title);

            var questionnaireVersion = row[1].AddParagraph();
            questionnaireVersion.Format.Font.Size = "6pt"; 
            questionnaireVersion.AddText(String.Format(PdfInterviewRes.Version, questionnaire.Version));

            var interviewStats = row[2].AddParagraph();
            interviewStats.Format.Font.Size = Unit.FromPoint(8);
            var formattedText = interviewStats.AddFormattedText(string.Format(PdfInterviewRes.InterviewStatistics, answeredQuestions, sectionsCount));

            if (unansweredQuestions > 0)
            {
                interviewStats.AddLineBreak();
                formattedText = interviewStats.AddFormattedText(string.Format(PdfInterviewRes.InterviewStatisticsUnanswered, unansweredQuestions));
            }

            if (errorsCount > 0)
            {
                interviewStats.AddLineBreak();
                formattedText = interviewStats.AddFormattedText(string.Format(PdfInterviewRes.InterviewStatisticsErrors, errorsCount));
                formattedText.Color = new Color(231, 73, 36);
            }

            if (warningsCount > 0)
            {
                interviewStats.AddLineBreak();
                formattedText = interviewStats.AddFormattedText(string.Format(PdfInterviewRes.InterviewStatisticsWarnings, warningsCount));
                formattedText.Color = new Color(255, 138, 0);
            }

            if (commentsCount > 0)
            {
                interviewStats.AddLineBreak();
                formattedText = interviewStats.AddFormattedText(string.Format(PdfInterviewRes.InterviewStatisticsCommented, commentsCount));
                formattedText.Color = new Color(71, 27, 195);
            }


            row[0].Borders.Right = new Border() { Style = BorderStyle.Single, Width = "1pt" };
            row[2].Borders.Left = new Border() { Style = BorderStyle.Single, Width = "1pt" };

            row = table.AddRow();
            row.Borders.Bottom = new Border() { Style = BorderStyle.Single, Width = "1pt"};
            row[0].AddParagraph().Format.Font.Size = "1pt";
            row = table.AddRow();
            row.Borders.Top = new Border() { Style = BorderStyle.Single, Width = "1pt"};
        }
        
        private static Stream GetEmbeddedResource(String filename)
        {
            System.Reflection.Assembly a = typeof(PdfInterviewGenerator).Assembly;
            Stream resFileStream = a.GetManifestResourceStream($"WB.UI.Headquarters.Content.images.{filename}");
            return resFileStream;
        }

        private void WriteStaticTextData(Table table, InterviewTreeStaticText staticText,
            IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            var row = table.AddRow();
            Paragraph paragraph = row[2].AddParagraph();
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
                    //image.WrapFormat = new WrapFormat() { Style = WrapStyle.Through }; 
                    image.Width = Unit.FromPoint(300);
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

            WriteValidateData(row[2], staticText, interview);
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

        private Table WriteSectionData(Section section, InterviewTreeSection @group)
        {
            var title = @group.Title.Text.RemoveHtmlTags();
            section = section.Document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;

            var paragraph = section.AddParagraph();
            paragraph.Style = PdfStyles.SectionHeader;
            paragraph.AddBookmark(title);
            paragraph.AddWrappedText(title);

            return GenerateQuestionsTable(section);
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

        private void WriteQuestionData(Table table, InterviewTreeQuestion question,
            IStatefulInterview interview)
        {
            var row = table.AddRow();

            if (question.AnswerTime.HasValue)
            {
                var firstCellParagraph = row[0].AddParagraph();
                firstCellParagraph.AddFormattedText(question.AnswerTime.Value.ToString(TimeFormat), PdfStyles.QuestionAnswerTime);
                firstCellParagraph.AddLineBreak();
                firstCellParagraph.AddFormattedText(question.AnswerTime.Value.ToString(DateFormat), PdfStyles.QuestionAnswerDate);
            }
            
            Paragraph paragraph = row[2].AddParagraph();
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
                    var mapsUrl = $"https://www.google.com/maps/search/?api=1&query={geoPosition.Latitude},{geoPosition.Longitude}";
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

            WriteValidateData(row[2], question, interview);
            WriteCommentsData(row[2], question, interview);

            row[2].AddParagraph();
        }
        
        private void WriteValidateData(Cell cell, IInterviewTreeValidateable validateable, IStatefulInterview interview)
        {
            if (validateable.FailedErrors != null && validateable.FailedErrors.Any())
            {
                var paragraph = cell.AddParagraph();
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

            if (validateable.FailedWarnings != null && validateable.FailedWarnings.Any())
            {
                var paragraph = cell.AddParagraph();
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

        private void WriteCommentsData(Cell tableCell, InterviewTreeQuestion question, IStatefulInterview interview)
        {
            if (question.AnswerComments != null && question.AnswerComments.Any())
            {
                var commentsParagraph = tableCell.AddParagraph();
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
