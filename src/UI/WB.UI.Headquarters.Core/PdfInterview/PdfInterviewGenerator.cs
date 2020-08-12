using System;
using System.Drawing;
using System.IO;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Shapes;
using MigraDocCore.Rendering;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PdfSharpCore.Utils;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Services.Impl;
using Color = MigraDocCore.DocumentObjectModel.Color;
using Font = MigraDocCore.DocumentObjectModel.Font;

namespace WB.UI.Headquarters.PdfInterview
{
    public class PdfInterviewGenerator : IPdfInterviewGenerator
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IAttachmentContentService attachmentContentService;

        private static class PdfFonts
        {
            public static Font HeaderLineTitle = new Font() { Size = 22, Bold = true,  };
            public static Font SectionHeader = new Font() { Size = 20, Bold = true, Name = "RobotoBold", Color = new Color(63, 63,63 ) };
            public static Font GroupHeader = new Font() { Size = 20, Name = "RobotoRegular" };
            public static Font RosterTitle = new Font() { Size = 20, Name = "RobotoRegular", Italic = true };
            public static Font QuestionTitle = new Font() { Size = 16, Name = "RobotoLight" };
            public static Font QuestionAnswer = new Font() { Size = 14, Name = "Arial, sans-serif"};
            public static Font QuestionNotAnswered = new Font() { Size = 11, Name = "TrebuchetMSBold"};
            public static Font QuestionAnswerDate = new Font() { Size = 12, Italic = true, Name = "Arial, sans-serif", Color = new Color(219, 223, 226)};
            public static Font QuestionAnswerTime = new Font() { Size = 12, Italic = true, Name = "Arial, sans-serif", Color = new Color(63, 63,63 )};
            public static Font StaticTextTitle = new Font() { Size = 16, Name = "RobotoLight" };
            public static Font ValidateErrorTitle = new Font() { Size = 10, Name = "TrebuchetMSBold", Color = new Color(231, 73, 36)};
            public static Font ValidateErrorMessage = new Font() { Size = 12, Name = "Arial, Helvetica, sans-serif", Italic = true, Color = new Color(231, 73, 36) };
            public static Font CommentAuthor = new Font() { Size = 10, Name = "TrebuchetMSBold", Color = new Color(128, 128, 128)};
            public static Font CommentDateTime = new Font() { Size = 10, Name = "TrebuchetMSBold", Color = new Color(219, 223, 226)};
            public static Font CommentMessage = new Font() { Size = 12, Name = "Arial, Helvetica, sans-serif", Italic = true};
        }
        
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

        public byte[] Generate(Guid interviewId)
        {
            var interview = statefulInterviewRepository.Get(interviewId.FormatGuid());
            if (interview == null)
                return null;

            var questionnaire = questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            //GlobalFontSettings.FontResolver = new PdfInterviewFontResolver();
            ImageSource.ImageSourceImpl = new ImageSharpImageSource<SixLabors.ImageSharp.PixelFormats.Bgr24>();
            
            Document document = new Document();
            DefineStyles(document);
            var section = document.AddSection();
            WritePdfInterviewHeader(section, questionnaire, interview);

            var nodes = interview.GetAllInterviewNodes();
            foreach (IInterviewTreeNode node in nodes)
            {
                switch (node)
                {
                    case InterviewTreeStaticText staticText:
                        WriteStaticTextData(section, staticText, interview, questionnaire);
                        break;
                    case InterviewTreeQuestion question:
                        WriteQuestionData(section, question, interview);
                        break;
                    case InterviewTreeVariable variable:
                        break;
                    case InterviewTreeGroup @group:
                        WriteGroupData(section, group);
                        break;
                    default: 
                        throw new ArgumentException("Unknown tree node type" + node.GetType().Name);
                }
            }
            
            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true);
            renderer.Document = document;
            renderer.RenderDocument();

            using var memoryStream = new MemoryStream();
            renderer.PdfDocument.Save(memoryStream);
            return memoryStream.ToArray();
        }

        private void DefineStyles(Document document)
        {
            var style = document.Styles["ddd"];
        }

        private static void WritePdfInterviewHeader(Section section, IQuestionnaire questionnaire, IStatefulInterview interview)
        {
            var interviewKey = interview.GetInterviewKey().ToString();
            var status = interview.Status.ToLocalizeString();

            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText($"{questionnaire.Title} (v. {questionnaire.Version})", PdfFonts.HeaderLineTitle);
            paragraph.AddLineBreak();
            paragraph.AddFormattedText(Common.InterviewKey + ": ", PdfFonts.HeaderLineTitle);
            paragraph.AddFormattedText(interviewKey, PdfFonts.HeaderLineTitle);
            paragraph.AddLineBreak();
            paragraph.AddFormattedText(Details.Status.Replace(@"{{ name }}", status), PdfFonts.HeaderLineTitle);
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
            paragraph.AddFormattedText(staticText.Title.Text, PdfFonts.StaticTextTitle);

            var attachment = questionnaire.GetAttachmentForEntity(staticText.Identity.Id);
            if (attachment != null)
            {
                paragraph.AddTab();
                paragraph.AddTab();
                
                ImageSource.IImageSource imageSource = ImageSource.FromBinary(attachment.Name, 
                    () => attachmentContentService.GetAttachmentContent(attachment.ContentId).Content);

                var image = paragraph.AddImage(imageSource);
                image.Width = Unit.FromPoint(400);
            }

            WriteValidateData(paragraph, staticText, interview);
            
            paragraph.AddLineBreak();
            paragraph.AddLineBreak();
        }

        private void WriteGroupData(Section section, InterviewTreeGroup @group)
        {
            var paragraph = section.AddParagraph();
            var font = group is InterviewTreeSection 
                ? PdfFonts.SectionHeader
                : PdfFonts.GroupHeader;

            if (@group is InterviewTreeRoster roster)
            {
                paragraph.AddFormattedText(roster.RosterTitle + " - ", PdfFonts.RosterTitle);
            }
            
            paragraph.AddFormattedText(group.Title.Text, font);
            paragraph.AddLineBreak();
            paragraph.AddLineBreak();
        }

        private void WriteQuestionData(Section section, InterviewTreeQuestion question,
            IStatefulInterview interview)
        {
            Paragraph paragraph = section.AddParagraph();

            if (question.AnswerTimeUtc.HasValue)
                paragraph.AddFormattedText(question.AnswerTimeUtc.Value.ToString("MMM DD"), PdfFonts.QuestionAnswerDate);
            else
                paragraph.AddTab();

            paragraph.AddTab();
            
            paragraph.AddFormattedText(question.Title.Text, PdfFonts.QuestionTitle);
            paragraph.AddLineBreak();

            if (question.AnswerTimeUtc.HasValue)
                paragraph.AddFormattedText(question.AnswerTimeUtc.Value.ToString("HH:mm"), PdfFonts.QuestionAnswerTime);
            else
                paragraph.AddTab();

            paragraph.AddTab();

            if (question.IsAnswered())
            {
                if (question.IsAudio)
                {
                    var audioQuestion = question.GetAsInterviewTreeAudioQuestion();
                    paragraph.AddFormattedText(audioQuestion.GetAnswer().FileName, PdfFonts.QuestionAnswer);
                }
                else if (question.IsMultimedia)
                {
                    var multimediaQuestion = question.GetAsInterviewTreeMultimediaQuestion();
                    var fileName = multimediaQuestion.GetAnswer().FileName;
                    ImageSource.IImageSource imageSource = ImageSource.FromBinary(fileName, 
                        () => imageFileStorage.GetInterviewBinaryData(interview.Id, fileName));
                    var image = paragraph.AddImage(imageSource);
                    image.Width = Unit.FromPoint(400);
                }
                else if (question.IsArea)
                {
                    var areaQuestion = question.GetAsInterviewTreeAreaQuestion();
                    paragraph.AddFormattedText(areaQuestion.GetAnswer().Value.ToString(), PdfFonts.QuestionAnswer);
                }
                else if (question.IsGps)
                {
                    var gpsQuestion = question.GetAsInterviewTreeGpsQuestion();
                    var geoPosition = gpsQuestion.GetAnswer().Value;
                    paragraph.AddFormattedText($"{geoPosition.Latitude}, {geoPosition.Longitude}", PdfFonts.QuestionAnswer);
                }
                else
                {
                    paragraph.AddFormattedText(question.GetAnswerAsString(), PdfFonts.QuestionAnswer);
                }
            }
            else
            {
                paragraph.AddFormattedText(WebInterviewUI.Interview_Overview_NotAnswered, PdfFonts.QuestionNotAnswered);
            }
            
            WriteValidateData(paragraph, question, interview);
            WriteCommentsData(paragraph, question, interview);

            paragraph.AddLineBreak();
            paragraph.AddLineBreak();
        }
        
        private void WriteValidateData(Paragraph paragraph, IInterviewTreeValidateable validateable, IStatefulInterview interview)
        {
            foreach (var error in validateable.ValidationMessages)
            {
                paragraph.AddTab();
                paragraph.AddTab();
                paragraph.AddFormattedText(error.Text, PdfFonts.ValidateErrorTitle);
                paragraph.AddLineBreak();
                paragraph.AddTab();
                paragraph.AddTab();
                paragraph.AddFormattedText(error.Text, PdfFonts.ValidateErrorMessage);
                paragraph.AddLineBreak();
            }
        }

        private void WriteCommentsData(Paragraph paragraph, InterviewTreeQuestion question, IStatefulInterview interview)
        {
            foreach (var comment in question.AnswerComments)
            {
                paragraph.AddTab();
                paragraph.AddTab();
                paragraph.AddFormattedText(comment.UserRole.ToUiString(), PdfFonts.CommentAuthor);
                paragraph.AddFormattedText($" ({comment.CommentTime.ToString()})", PdfFonts.CommentDateTime);
                paragraph.AddLineBreak();
                paragraph.AddTab();
                paragraph.AddTab();
                paragraph.AddFormattedText(comment.Comment, PdfFonts.CommentMessage);
                paragraph.AddLineBreak();
            }
        }
    }
    
    public class PdfInterviewFontResolver : IFontResolver
    {
        public byte[] GetFont(string faceName)
        {
            using(var ms = new MemoryStream())
            {
                //FontFamily fontFamily = new FontFamily(faceName);
                //fontFamily.
                using(var fs = File.Open(faceName, FileMode.Open))
                {
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    return ms.ToArray();
                }
            }
        }

        public string DefaultFontName => "OpenSans";

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("OpenSans", StringComparison.CurrentCultureIgnoreCase))
            {
                if (isBold && isItalic)
                {
                    return new FontResolverInfo("OpenSans-BoldItalic.ttf");
                }
                else if (isBold)
                {
                    return new FontResolverInfo("OpenSans-Bold.ttf");
                }
                else if (isItalic)
                {
                    return new FontResolverInfo("OpenSans-Italic.ttf");
                }
                else
                {
                    return new FontResolverInfo("OpenSans-Regular.ttf");
                }
            }
            return null;
        }
    }
}
