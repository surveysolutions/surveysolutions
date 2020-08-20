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

        private const string DateTimeFormat = "MMM dd, yyyy HH:mm";
        //private const string DateTimeFormat = "YYYY-MM-DD HH:mm:ss";
        
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
                    WriteGroupData(document.LastSection, group);
                    if (group is InterviewTreeSection interviewTreeSection)
                        WritePageOfContentRecord(firstPageSection, interviewTreeSection);

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
            var paragraph = firstSection.AddParagraph();
            paragraph.Style = "TOC";
            var title = interviewTreeSection.Title.Text.RemoveHtmlTags();
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
            var defaultPaddingStyle = document.Styles.AddStyle(PdfStyles.Default, StyleNames.DefaultParagraphFont);
            defaultPaddingStyle.Font.Name = "Noto Sans, Arial, sans-serif";
            defaultPaddingStyle.Font.Bold = false;
            defaultPaddingStyle.Font.Italic = false;
            defaultPaddingStyle.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center); 

            document.Styles.AddStyle(PdfStyles.HeaderLineTitle, PdfStyles.Default).Font =
                new Font() { Size = 18, Bold = false };
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
                new Font() { Size = 9, Color = new Color(219, 223, 226)};
            document.Styles.AddStyle(PdfStyles.CommentMessage, PdfStyles.Default).Font =
                new Font() { Size = 10, Italic = true};
        }

        private void WritePdfInterviewHeader(Section section, IQuestionnaire questionnaire, IStatefulInterview interview)
        {
            var interviewKey = interview.GetInterviewKey().ToString();
            var status = interview.Status.ToLocalizeString();
            var interviewSummary = interviewSummariesRepository.GetById(interview.Id.FormatGuid());

            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText($"{questionnaire.Title} (v. {questionnaire.Version})", PdfStyles.HeaderLineTitle);
            paragraph.AddLineBreak();
            paragraph.AddFormattedText(Common.InterviewKey + ": ", PdfStyles.HeaderLineTitle);
            paragraph.AddFormattedText(interviewKey, PdfStyles.HeaderLineValue);
            paragraph.AddLineBreak();
            paragraph.AddFormattedText(Details.Status.Replace(@"{{ name }}", ""), PdfStyles.HeaderLineTitle);
            paragraph.AddFormattedText(status, PdfStyles.HeaderLineValue);
            paragraph.AddLineBreak();
            if (interview.CompletedDate.HasValue)
            {
                paragraph.AddFormattedText(PdfInterviewRes.CompleteDate, PdfStyles.HeaderLineTitle);
                paragraph.AddFormattedText(interview.CompletedDate.Value.ToString(DateTimeFormat), PdfStyles.HeaderLineValue);
                paragraph.AddLineBreak();
            }
            paragraph.AddFormattedText(Details.LastUpdated + ": ", PdfStyles.HeaderLineTitle);
            paragraph.AddFormattedText(interviewSummary.UpdateDate.ToString(DateTimeFormat), PdfStyles.HeaderLineValue);
            paragraph.AddLineBreak();
            paragraph.AddFormattedText(PdfInterviewRes.GeneratedDate, PdfStyles.HeaderLineTitle);
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
            var isSection = @group is InterviewTreeSection;
            var title = @group.Title.Text.RemoveHtmlTags();

            if (isSection)
            {
                section = section.Document.AddSection();
            }

            var paragraph = section.AddParagraph();
            paragraph.Style = isSection 
                ? PdfStyles.SectionHeader
                : PdfStyles.GroupHeader;

            paragraph.AddLineBreak();
            paragraph.AddText(title);
            
            if (@group is InterviewTreeRoster roster)
            {
                paragraph.AddFormattedText(" - " + roster.RosterTitle.RemoveHtmlTags(), PdfStyles.RosterTitle);
            }
            
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
            
            paragraph.AddFormattedText(question.Title.Text.RemoveHtmlTags(), PdfStyles.QuestionTitle);
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

                foreach (var errorCondition in validateable.FailedErrors)
                {
                    var errorMessage = validateable.ValidationMessages[errorCondition.FailedConditionIndex];
                    paragraph.AddTab();
                    paragraph.AddTab();
                    paragraph.AddFormattedText(errorMessage.Text.RemoveHtmlTags(), PdfStyles.ValidateErrorMessage);
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

                foreach (var warningCondition in validateable.FailedWarnings)
                {
                    var warningMessage = validateable.ValidationMessages[warningCondition.FailedConditionIndex];
                    paragraph.AddTab();
                    paragraph.AddTab();
                    paragraph.AddFormattedText(warningMessage.Text.RemoveHtmlTags(), PdfStyles.ValidateWarningMessage);
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
                    paragraph.AddFormattedText($" ({comment.CommentTime.ToString("MMM dd HH:mm")})", PdfStyles.CommentDateTime);
                    paragraph.AddLineBreak();
                    paragraph.AddTab();
                    paragraph.AddTab();
                    paragraph.AddFormattedText(comment.Comment, PdfStyles.CommentMessage);
                    paragraph.AddLineBreak();
                }
            }
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
