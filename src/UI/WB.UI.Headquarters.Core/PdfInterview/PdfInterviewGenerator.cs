using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using Main.Core.Entities.SubEntities;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;
using NHibernate.Criterion;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PdfSharpCore.Utils;
using WB.Core.BoundedContexts.Headquarters;
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
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummariesRepository;

        //private const string DateTimeFormat = "MMM dd, yyyy HH:mm";
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        
        private static class PdfStyles
        {
            public const string Default = "DefaultPdfStyle";
            public const string HeaderLineTitle = "HeaderLineTitle";
            public const string HeaderLineValue = "HeaderLineValue";
            public const string SectionHeader = "SectionHeader";
            public const string GroupHeader = "GroupHeader";
            public const string RosterTitle = "RosterTitle";
            public const string QuestionTitle = "QuestionTitle";
            public const string QuestionAnswer = "QuestionAnswer";
            public const string QuestionNotAnswered = "QuestionNotAnswered";
            public const string QuestionAnswerDate = "QuestionAnswerDate";
            public const string QuestionAnswerTime = "QuestionAnswerTime";
            public const string StaticTextTitle = "StaticTextTitle";
            public const string ValidateErrorTitle = "ValidateErrorTitle";
            public const string ValidateErrorMessage = "ValidateErrorMessage";
            public const string ValidateWarningTitle = "ValidateWarningTitle";
            public const string ValidateWarningMessage = "ValidateWarningMessage";
            public const string CommentTitle = "CommentTitle";
            public const string CommentAuthor = "CommentAuthor";
            public const string CommentDateTime = "CommentDateTime";
            public const string CommentMessage = "CommentMessage";
            public const string TableOfContent = "TableOfContent";
            public const string YesNoTitle = "YesNoTitle";
        }
        
        public PdfInterviewGenerator(IQuestionnaireStorage questionnaireStorage,
            IStatefulInterviewRepository statefulInterviewRepository,
            IImageFileStorage imageFileStorage,
            IAttachmentContentService attachmentContentService,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummariesRepository)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.imageFileStorage = imageFileStorage;
            this.attachmentContentService = attachmentContentService;
            this.interviewSummariesRepository = interviewSummariesRepository;
        }

        static readonly IFontResolver PdfInterviewFontResolver = new PdfInterviewFontResolver();

        public byte[] Generate(Guid interviewId, IPrincipal user)
        {
            var interview = statefulInterviewRepository.Get(interviewId.FormatGuid());
            if (interview == null)
                return null;

            var questionnaire = questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            if (questionnaire == null)
                return null;
            
            GlobalFontSettings.FontResolver = PdfInterviewFontResolver;
            //var fontResolver = new FontResolver();
            //GlobalFontSettings.FontResolver = fontResolver;
            //GlobalFontSettings.FontResolver = new PdfInterviewFontResolver();
            ImageSource.ImageSourceImpl = new ImageSharpImageSource<SixLabors.ImageSharp.PixelFormats.Rgba32>();
            
            Document document = new Document();
            DefineStyles(document);
            var firstPageSection = document.AddSection();
            firstPageSection.PageSetup.PageFormat = PageFormat.A4;
            WritePdfInterviewHeader(firstPageSection, questionnaire, interview);
            WriteSummaryHeader(firstPageSection);

            Table table = null;

            var nodes = GetAllInterviewNodes(interview, user);
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

            WritePageNumbers(document, questionnaire, interview);
            
            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true);
            renderer.Document = document;
            renderer.RenderDocument();

            using var memoryStream = new MemoryStream();
            renderer.PdfDocument.Save(memoryStream);
            return memoryStream.ToArray();
        }

        private void WriteSummaryHeader(Section section)
        {
            var paragraph = section.AddParagraph();
            paragraph.Style = PdfStyles.SectionHeader;
            paragraph.AddLineBreak();
            paragraph.AddText(PdfInterviewRes.Summary);

            var tableOfContent = section.AddParagraph();
            tableOfContent.Style = PdfStyles.TableOfContent;
        }

        private void WritePageNumbers(Document document, IQuestionnaire questionnaire, IStatefulInterview interview)
        {
            foreach (Section section in document.Sections)
            {
                //section.Footers.Primary.Format.SpaceAfter = "10pt";
                section.Footers.Primary.Format.LeftIndent = "0pt";
                section.Footers.Primary.Format.RightIndent = "0pt";
                section.Footers.Primary.Format.Borders.Top = new Border()
                {
                    Style = BorderStyle.Dot,
                    Width = "1pt"
                };
                
                Paragraph leftFooter = section.Footers.Primary.AddParagraph();
                leftFooter.AddPageField();
                leftFooter.AddText(PdfInterviewRes.PageOf);
                leftFooter.AddNumPagesField();
                leftFooter.Format.Font.Size = "6pt";
                leftFooter.Format.Alignment = ParagraphAlignment.Left;            
                
                Paragraph centerFooter = section.Footers.Primary.AddParagraph();
                centerFooter.AddText(questionnaire.Title);
                centerFooter.Format.Font.Size = "6pt";
                centerFooter.Format.Alignment = ParagraphAlignment.Center;            

                Paragraph rightFooter = section.Footers.Primary.AddParagraph();
                rightFooter.AddText(interview.GetInterviewKey().ToString());
                rightFooter.Format.Font.Size = "6pt";
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
            //sectionHeader.ParagraphFormat.OutlineLevel = OutlineLevel.Level1;
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

        private void WritePdfInterviewHeader(Section section, IQuestionnaire questionnaire, IStatefulInterview interview)
        {
            var interviewKey = interview.GetInterviewKey().ToString();
            var status = interview.Status.ToLocalizeString();
            var interviewSummary = interviewSummariesRepository.GetById(interview.Id.FormatGuid());

            var paragraph = section.AddParagraph();
            paragraph.Style = PdfStyles.HeaderLineTitle;
            paragraph.AddFormattedText($"{questionnaire.Title} (v. {questionnaire.Version})", PdfStyles.HeaderLineTitle);
            paragraph.AddLineBreak();
            paragraph.AddFormattedText(Common.InterviewKey + ":\t", PdfStyles.HeaderLineTitle);
            paragraph.AddFormattedText(interviewKey, PdfStyles.HeaderLineValue);
            paragraph.AddLineBreak();
            paragraph.AddFormattedText(Details.LastUpdated + ":\t", PdfStyles.HeaderLineTitle);
            paragraph.AddFormattedText(interviewSummary.UpdateDate.ToString(DateTimeFormat), PdfStyles.HeaderLineValue);
            paragraph.AddLineBreak();
            paragraph.AddFormattedText(PdfInterviewRes.GeneratedDate, PdfStyles.HeaderLineTitle);
            paragraph.AddTab();
            paragraph.AddFormattedText(DateTime.Now.ToString(DateTimeFormat), PdfStyles.HeaderLineValue);
            paragraph.AddLineBreak();
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

            var column = table.AddColumn("2cm");
            column.Format.Alignment = ParagraphAlignment.Right;
            column = table.AddColumn("1cm");
            column = table.AddColumn("15cm");
            column.Format.Alignment = ParagraphAlignment.Left;
            return table;
        }

        private void WriteQuestionData(Table table, InterviewTreeQuestion question,
            IStatefulInterview interview)
        {
            var row = table.AddRow();

            if (question.AnswerTimeUtc.HasValue)
            {
                var firstCellParagraph = row[0].AddParagraph();
                firstCellParagraph.AddFormattedText(question.AnswerTimeUtc.Value.ToString("HH:mm"), PdfStyles.QuestionAnswerTime);
                firstCellParagraph.AddLineBreak();
                firstCellParagraph.AddFormattedText(question.AnswerTimeUtc.Value.ToString("MMM dd, yyyy"), PdfStyles.QuestionAnswerDate);
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

    public static class ParagraphExtensions
    {
        public static void AddWrappedText(this Paragraph paragraph, string text)
        {
            for (int i = 0; i < text.Length; i+=15)
            {
                var str = i + 15 > text.Length
                    ? text.Substring(i)
                    : text.Substring(i, 15);
                paragraph.AddText(str);
            }
        }

        public static void AddWrapFormattedText(this Paragraph paragraph, string text, string style)
        {
            for (int i = 0; i < text.Length; i+=15)
            {
                var str = i + 15 > text.Length
                    ? text.Substring(i)
                    : text.Substring(i, 15);
                paragraph.AddFormattedText(str, style);
            }
        }
    }
    
    public class PdfInterviewFontResolver : FontResolver
    {
        static PdfInterviewFontResolver()
        {
        }

        public PdfInterviewFontResolver()
        {
            NullIfFontNotFound = true;
        }

        public override FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var fontNames = familyName.Split(',');
            foreach (var fontName in fontNames)
            {
                var fontResolverInfo = base.ResolveTypeface(fontName.Trim(), isBold, isItalic);
                if (fontResolverInfo != null)
                    return fontResolverInfo;
            }

            return null;
        }
    }
}
