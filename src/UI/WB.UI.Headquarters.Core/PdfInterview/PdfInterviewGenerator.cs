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
            ImageSource.ImageSourceImpl = new ImageSharpImageSource<SixLabors.ImageSharp.PixelFormats.Rgba32>();
            
            Document document = new Document();
            DefineStyles(document);
            var firstPageSection = document.AddSection();
            WritePdfInterviewHeader(firstPageSection, questionnaire, interview);

            var nodes = GetAllInterviewNodes(interview, user);
            foreach (Identity node in nodes)
            {
                if (questionnaire.IsQuestion(node.Id))
                {
                    var question = interview.GetQuestion(node);
                    WriteQuestionData(document.LastSection, question, interview);
                    continue;
                }

                if (questionnaire.IsStaticText(node.Id))
                {
                    var staticText = interview.GetStaticText(node);
                    WriteStaticTextData(document.LastSection, staticText, interview, questionnaire);
                    continue;
                }

                if (questionnaire.IsSubSection(node.Id))
                {
                    var group = interview.GetGroup(node);
                    if (group is InterviewTreeSection interviewTreeSection)
                    {
                        WriteSectionData(document.LastSection, interviewTreeSection);
                        WritePageOfContentRecord(firstPageSection, interviewTreeSection);
                    }
                    else
                    {
                        WriteGroupData(document.LastSection, group);
                    }

                    continue;
                }
                
                if (questionnaire.IsRosterGroup(node.Id))
                {
                    var roster = interview.GetRoster(node);
                    WriteGroupData(document.LastSection, roster);
                    continue;
                }

                if (questionnaire.IsVariable(node.Id))
                    continue;
                
                throw new ArgumentException("Unknown tree node type for entity " + node);
            }

            WritePageNumbers(document);
            
            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true);
            renderer.Document = document;
            renderer.RenderDocument();

            using var memoryStream = new MemoryStream();
            renderer.PdfDocument.Save(memoryStream);
            return memoryStream.ToArray();
        }

        private void WritePageNumbers(Document document)
        {
            foreach (Section section in document.Sections)
            {
                Paragraph footer = section.Footers.Primary.AddParagraph();
                footer.AddPageField();
                footer.AddText(PdfInterviewRes.PageOf);
                footer.AddNumPagesField();
                footer.Format.Font.Size = 8;
                footer.Format.Alignment = ParagraphAlignment.Right;            
            }
        }

        private void WritePageOfContentRecord(Section firstSection, InterviewTreeSection interviewTreeSection)
        {
            var title = interviewTreeSection.Title.Text.RemoveHtmlTags();

            var paragraph = firstSection.AddParagraph();
            paragraph.Style = PdfStyles.TableOfContent;
            Hyperlink hyperlink = paragraph.AddHyperlink(title);
            hyperlink.AddText($"{title}\t");
            hyperlink.AddPageRefField(title);
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
            //defaultPaddingStyle.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center); 

            var tableOfContent = document.Styles.AddStyle(PdfStyles.TableOfContent, PdfStyles.Default);
            tableOfContent.ParagraphFormat.Font.Color = Colors.Blue;
            tableOfContent.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right, TabLeader.Dots);

            var headerLineTitle = document.Styles.AddStyle(PdfStyles.HeaderLineTitle, PdfStyles.Default);
            headerLineTitle.Font.Bold = false;
            headerLineTitle.Font.Size = 18;
            headerLineTitle.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);

            
            document.Styles.AddStyle(PdfStyles.HeaderLineValue, PdfStyles.Default).Font =
                new Font() { Size = 18, Bold = true };
            
            var sectionHeader = document.Styles.AddStyle(PdfStyles.SectionHeader, PdfStyles.Default);
            sectionHeader.Font = new Font() { Size = 18, Bold = true, Color = new Color(63, 63,63 ) };
            //sectionHeader.ParagraphFormat.Borders.Top = new Border() { Width = "1pt", Color = Colors.DarkGray };
            sectionHeader.ParagraphFormat.LineSpacing = 0;
            sectionHeader.ParagraphFormat.OutlineLevel = OutlineLevel.Level1;
            //sectionHeader.ParagraphFormat.SpaceBefore = "40pt";
            
            document.Styles.AddStyle(PdfStyles.GroupHeader, PdfStyles.Default).Font =
                new Font() { Size = 18 };
            document.Styles.AddStyle(PdfStyles.RosterTitle, PdfStyles.Default).Font =
                new Font() { Size = 18, Italic = true };
            document.Styles.AddStyle(PdfStyles.QuestionTitle, PdfStyles.Default).Font =
                new Font() { Size = 12 };
            document.Styles.AddStyle(PdfStyles.QuestionAnswer, PdfStyles.Default).Font =
                new Font() { Size = 12, Bold = true };
            var notAnswered = document.Styles.AddStyle(PdfStyles.QuestionNotAnswered, PdfStyles.Default);
            notAnswered.Font = new Font() { Size = 10, Color = Colors.Blue };
            notAnswered.ParagraphFormat.LineSpacing = 1.5;
            
            var questionDateStyle = document.Styles.AddStyle(PdfStyles.QuestionAnswerDate, PdfStyles.Default);
            questionDateStyle.Font = new Font() { Size = 10, Italic = true, Color = new Color(63, 63,63 )};
            questionDateStyle.ParagraphFormat.Alignment = ParagraphAlignment.Right;
            
            var questionTimeStyle = document.Styles.AddStyle(PdfStyles.QuestionAnswerTime, PdfStyles.Default);
            questionTimeStyle.Font = new Font() { Size = 10, Italic = true, Color = new Color(63, 63,63 )};
            questionTimeStyle.ParagraphFormat.Alignment = ParagraphAlignment.Right;
            
            document.Styles.AddStyle(PdfStyles.StaticTextTitle, PdfStyles.Default).Font =
                new Font() { Size = 14 };
            document.Styles.AddStyle(PdfStyles.ValidateErrorTitle, PdfStyles.Default).Font =
                new Font() { Size = 9, Color = new Color(231, 73, 36)};
            document.Styles.AddStyle(PdfStyles.ValidateErrorMessage, PdfStyles.Default).Font =
                new Font() { Size = 11, Italic = true, Color = new Color(231, 73, 36) };
            document.Styles.AddStyle(PdfStyles.ValidateWarningTitle, PdfStyles.Default).Font =
                new Font() { Size = 9, Color = new Color(249, 189, 7)};
            document.Styles.AddStyle(PdfStyles.ValidateWarningMessage, PdfStyles.Default).Font =
                new Font() { Size = 10, Italic = true, Color = new Color(249, 189, 7) };
            document.Styles.AddStyle(PdfStyles.CommentAuthor, PdfStyles.Default).Font =
                new Font() { Size = 9, Color = new Color(128, 128, 128)};
            document.Styles.AddStyle(PdfStyles.CommentDateTime, PdfStyles.Default).Font =
                new Font() { Size = 9, Italic = true, Color = new Color(63, 63, 63 )};
            document.Styles.AddStyle(PdfStyles.CommentMessage, PdfStyles.Default).Font =
                new Font() { Size = 10, Italic = true};
            document.Styles.AddStyle(PdfStyles.YesNoTitle, PdfStyles.Default).Font =
                new Font() { Size = 7, Italic = true};
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
            paragraph.AddLineBreak();
            paragraph.AddLineBreak();
        }

        private void WriteStaticTextData(Section section, InterviewTreeStaticText staticText,
            IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            Paragraph paragraph = section.AddParagraph();

            paragraph.AddTab();
            paragraph.AddTab();
            paragraph.AddFormattedText(staticText.Title.Text.RemoveHtmlTags(), PdfStyles.StaticTextTitle);

            var attachmentInfo = questionnaire.GetAttachmentForEntity(staticText.Identity.Id);
            if (attachmentInfo != null)
            {
                paragraph.AddTab();
                paragraph.AddTab();

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
                    paragraph.AddFormattedText($"{attachment.FileName}", PdfStyles.QuestionAnswer);
                }
                else if (attachment.IsAudio())
                {
                    paragraph.AddFormattedText($"{attachment.FileName}", PdfStyles.QuestionAnswer);
                }
                else if (attachment.IsPdf())
                {
                    paragraph.AddFormattedText($"{attachment.FileName}", PdfStyles.QuestionAnswer);
                }
            }
            paragraph.AddLineBreak();

            WriteValidateData(paragraph, staticText, interview);
            
            paragraph.AddLineBreak();
            paragraph.AddLineBreak();
        }

        private void WriteGroupData(Section section, InterviewTreeGroup @group)
        {
            var title = @group.Title.Text.RemoveHtmlTags();

            var paragraph = section.AddParagraph();
            paragraph.Style = PdfStyles.GroupHeader;

            paragraph.AddLineBreak();
            paragraph.AddText(title);
            
            if (@group is InterviewTreeRoster roster)
            {
                paragraph.AddFormattedText(" - " + roster.RosterTitle.RemoveHtmlTags(), PdfStyles.RosterTitle);
            }
            
            paragraph.AddLineBreak();
            paragraph.AddLineBreak();
        }

        private void WriteSectionData(Section section, InterviewTreeSection @group)
        {
            var title = @group.Title.Text.RemoveHtmlTags();
            section = section.Document.AddSection();

            var paragraph = section.AddParagraph();
            paragraph.Style = PdfStyles.SectionHeader;
            paragraph.AddLineBreak();
            paragraph.AddBookmark(title);
            paragraph.AddText(title);
            paragraph.AddLineBreak();
            paragraph.AddLineBreak();
        }

        private void WriteQuestionData(Section section, InterviewTreeQuestion question,
            IStatefulInterview interview)
        {
            Paragraph paragraph = section.AddParagraph();

            if (question.AnswerTimeUtc.HasValue)
                paragraph.AddFormattedText(question.AnswerTimeUtc.Value.ToString("MMM dd, yyyy"), PdfStyles.QuestionAnswerDate);
            else
                paragraph.AddTab();
            paragraph.AddTab();
            
            paragraph.AddWrapFormattedText(question.Title.Text.RemoveHtmlTags(), PdfStyles.QuestionTitle);
            paragraph.AddLineBreak();

            if (question.AnswerTimeUtc.HasValue)
                paragraph.AddFormattedText(question.AnswerTimeUtc.Value.ToString("HH:mm"), PdfStyles.QuestionAnswerTime);

            paragraph.AddTab();
            paragraph.AddTab();

            if (question.IsAnswered())
            {
                if (question.IsAudio)
                {
                    var audioQuestion = question.GetAsInterviewTreeAudioQuestion();
                    var audioAnswer = audioQuestion.GetAnswer();
                    paragraph.AddFormattedText($"{audioAnswer.FileName} + ({audioAnswer.Length})", PdfStyles.QuestionAnswer);
                }
                else if (question.IsMultimedia)
                {
                    var multimediaQuestion = question.GetAsInterviewTreeMultimediaQuestion();
                    var fileName = multimediaQuestion.GetAnswer().FileName;
                    ImageSource.IImageSource imageSource = ImageSource.FromBinary(fileName, 
                        () => imageFileStorage.GetInterviewBinaryData(interview.Id, fileName));
                    var image = paragraph.AddImage(imageSource);
                    image.Width = Unit.FromPoint(300);
                }
                else if (question.IsArea)
                {
                    var areaQuestion = question.GetAsInterviewTreeAreaQuestion();
                    var areaAnswer = areaQuestion.GetAnswer().Value;
                    paragraph.AddFormattedText(areaAnswer.ToString(), PdfStyles.QuestionAnswer);
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
                        paragraph.AddFormattedText(PdfInterviewRes.YesNo, PdfStyles.YesNoTitle);
                        paragraph.AddLineBreak();
                        paragraph.AddTab();
                        paragraph.AddTab();
                        
                        foreach (var answerOption in yesNoAnswer.CheckedOptions)
                        {
                            var option = interview.GetOptionForQuestionWithoutFilter(question.Identity, answerOption.Value, null);
                            var optionAnswer = answerOption.Yes ? "\u2022 / \u25E6 " : (answerOption.No ? "\u25E6 / \u2022 " : "\u25E6 / \u25E6 ");
                            paragraph.AddFormattedText(optionAnswer + option.Title, PdfStyles.QuestionAnswer);
                            paragraph.AddLineBreak();
                            paragraph.AddTab();
                            paragraph.AddTab();
                        }
                    }
                }
                else if (question.IsMultiFixedOption)
                {
                    var multiOptionQuestion = question.GetAsInterviewTreeMultiOptionQuestion();
                    foreach (var checkedValue in multiOptionQuestion.GetAnswer().CheckedValues)
                    {
                        var option = interview.GetOptionForQuestionWithoutFilter(question.Identity, checkedValue, null);
                        paragraph.AddFormattedText("\u2022 " + option.Title, PdfStyles.QuestionAnswer);
                        //paragraph.AddFormattedText("\u2705 \u2611 \u2714 \u2716 \u274C " + option.Title, PdfStyles.QuestionAnswer);
                        paragraph.AddLineBreak();
                        paragraph.AddTab();
                        paragraph.AddTab();
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
                            paragraph.AddFormattedText(answer, PdfStyles.QuestionAnswer);
                            paragraph.AddLineBreak();
                            paragraph.AddTab();
                            paragraph.AddTab();
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
                            paragraph.AddFormattedText(answer.Text, PdfStyles.QuestionAnswer);
                            paragraph.AddLineBreak();
                            paragraph.AddTab();
                            paragraph.AddTab();
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
                            paragraph.AddFormattedText(answer.Text, PdfStyles.QuestionAnswer);
                            paragraph.AddLineBreak();
                            paragraph.AddTab();
                            paragraph.AddTab();
                        }
                    }
                }
                else
                {
                    paragraph.AddFormattedText(question.GetAnswerAsString(), PdfStyles.QuestionAnswer);
                }
            }
            else
            {
                paragraph.AddFormattedText(WebInterviewUI.Interview_Overview_NotAnswered.ToUpper(), PdfStyles.QuestionNotAnswered);
            }
            paragraph.AddLineBreak();

            WriteValidateData(paragraph, question, interview);
            WriteCommentsData(paragraph, question, interview);

            paragraph.AddLineBreak();
            paragraph.AddLineBreak();
        }
        
        private void WriteValidateData(Paragraph paragraph, IInterviewTreeValidateable validateable, IStatefulInterview interview)
        {
            if (validateable.FailedErrors != null && validateable.FailedErrors.Any())
            {
                paragraph.AddLineBreak();
                paragraph.AddTab();
                paragraph.AddTab();
                paragraph.AddFormattedText(WebInterviewUI.Error_plural, PdfStyles.ValidateErrorTitle);
                paragraph.AddLineBreak();

                bool isNeedAddErrorNumber = validateable.FailedErrors.Count > 1;

                foreach (var errorCondition in validateable.FailedErrors)
                {
                    var errorMessage = validateable.ValidationMessages[errorCondition.FailedConditionIndex];
                    paragraph.AddTab();
                    paragraph.AddTab();
                    paragraph.AddFormattedText(errorMessage.Text.RemoveHtmlTags(), PdfStyles.ValidateErrorMessage);
                    if (isNeedAddErrorNumber)
                        paragraph.AddFormattedText($" [{errorCondition.FailedConditionIndex}]", PdfStyles.ValidateErrorMessage);
                       
                    paragraph.AddLineBreak();
                }
            }

            if (validateable.FailedWarnings != null && validateable.FailedWarnings.Any())
            {
                paragraph.AddLineBreak();
                paragraph.AddTab();
                paragraph.AddTab();
                paragraph.AddFormattedText(WebInterviewUI.WarningsHeader, PdfStyles.ValidateWarningTitle);
                paragraph.AddLineBreak();

                bool isNeedAddWarningNumber = validateable.FailedWarnings.Count > 1;

                foreach (var warningCondition in validateable.FailedWarnings)
                {
                    var warningMessage = validateable.ValidationMessages[warningCondition.FailedConditionIndex];
                    paragraph.AddTab();
                    paragraph.AddTab();
                    paragraph.AddFormattedText(warningMessage.Text.RemoveHtmlTags(), PdfStyles.ValidateWarningMessage);
                    if (isNeedAddWarningNumber)
                        paragraph.AddFormattedText($" [{warningCondition.FailedConditionIndex}]", PdfStyles.ValidateWarningMessage);
                    paragraph.AddLineBreak();
                }
            }
        }

        private void WriteCommentsData(Paragraph paragraph, InterviewTreeQuestion question, IStatefulInterview interview)
        {
            if (question.AnswerComments != null && question.AnswerComments.Any())
            {
                paragraph.AddLineBreak();
                paragraph.AddTab();
                paragraph.AddTab();
                paragraph.AddFormattedText(PdfInterviewRes.Comments, PdfStyles.CommentAuthor);
                paragraph.AddLineBreak();

                foreach (var comment in question.AnswerComments)
                {
                    paragraph.AddTab();
                    paragraph.AddTab();
                    paragraph.AddFormattedText(comment.UserRole.ToUiString(), PdfStyles.CommentAuthor);
                    paragraph.AddFormattedText($" ({comment.CommentTime.ToString(DateTimeFormat)})", PdfStyles.CommentDateTime);
                    paragraph.AddLineBreak();
                    paragraph.AddTab();
                    paragraph.AddTab();
                    paragraph.AddFormattedText(comment.Comment, PdfStyles.CommentMessage);
                    paragraph.AddLineBreak();
                }
            }
        }
    }

    public static class ParagraphExtensions
    {
        public static Text AddWrappedText(this Paragraph paragraph, string text)
        {
            text = WrapText(text);
            return paragraph.AddText(text);
        }

        public static FormattedText AddWrapFormattedText(this Paragraph paragraph, string text, string style)
        {
            text = WrapText(text);
            return paragraph.AddFormattedText(text, style);
        }

        private static string WrapText(string text)
        {
            var strings = text.Split(' ');
            foreach (var s in strings)
            {
                if (s.Length > 15)
                {
                    var wrapString = s;
                    for (int i = 15; i < wrapString.Length; i += 15)
                    {
                        wrapString = wrapString.Insert(i, "\u200C");
                    }
                    text = text.Replace(s, wrapString);
                }
            }

            return text;
        }
    }
    
    public class PdfInterviewFontResolver : FontResolver
    {
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
