using System;
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

namespace WB.UI.Headquarters.PdfInterview
{
    public class PdfInterviewGenerator : IPdfInterviewGenerator
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IAttachmentContentService attachmentContentService;

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
            var interviewKey = interview.GetInterviewKey().ToString();
            var status = interview.Status.ToLocalizeString();

            ImageSource.ImageSourceImpl = new ImageSharpImageSource<SixLabors.ImageSharp.PixelFormats.Bgr24>();
            
            Document document = new Document();
            var section = document.AddSection();
            var paragraph = section.AddParagraph();
            paragraph.Format.Font.Size = 18;
            paragraph.Format.Font.Color = Color.FromCmyk(100, 30, 20, 50);
            paragraph.AddFormattedText($"{questionnaire.Title} (v. {questionnaire.Version})", TextFormat.Bold);
            paragraph.AddLineBreak();
            paragraph.AddFormattedText(Common.InterviewKey + ": ", TextFormat.Bold);
            paragraph.AddFormattedText(interviewKey, TextFormat.Bold);
            paragraph.AddLineBreak();
            paragraph.AddFormattedText(Details.Status.Replace(@"{{ name }}", status), TextFormat.Bold);
            paragraph.AddLineBreak();
            paragraph.AddLineBreak();
            paragraph.AddLineBreak();
            
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

        private void WriteStaticTextData(Section section, InterviewTreeStaticText staticText,
            IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            Paragraph paragraph = section.AddParagraph();

            paragraph.AddTab();
            paragraph.AddTab();
            paragraph.AddFormattedText(staticText.Title.Text, TextFormat.Italic);

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
            
            paragraph.AddLineBreak();
            paragraph.AddLineBreak();
        }

        private void WriteGroupData(Section section, InterviewTreeGroup @group)
        {
            var paragraph = section.AddParagraph();
            paragraph.Format.Font.Size = 20;
            paragraph.AddFormattedText(group.Title.Text);
            paragraph.AddLineBreak();
            paragraph.AddLineBreak();
        }

        private void WriteQuestionData(Section section, InterviewTreeQuestion question,
            IStatefulInterview interview)
        {
            Paragraph paragraph = section.AddParagraph();

            if (question.AnswerTimeUtc.HasValue)
                paragraph.AddText(question.AnswerTimeUtc.Value.ToShortDateString());
            else
                paragraph.AddTab();

            paragraph.AddTab();
            
            paragraph.AddFormattedText(question.Title.Text, TextFormat.Italic);
            paragraph.AddLineBreak();

            if (question.AnswerTimeUtc.HasValue)
                paragraph.AddText(question.AnswerTimeUtc.Value.ToShortTimeString());
            else
                paragraph.AddTab();

            paragraph.AddTab();

            if (question.IsAnswered())
            {
                if (question.IsAudio)
                {
                    var audioQuestion = question.GetAsInterviewTreeAudioQuestion();
                    paragraph.AddText(audioQuestion.GetAnswer().FileName);
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
                    paragraph.AddText(areaQuestion.GetAnswer().Value.ToString());
                }
                else if (question.IsGps)
                {
                    var gpsQuestion = question.GetAsInterviewTreeGpsQuestion();
                    var geoPosition = gpsQuestion.GetAnswer().Value;
                    paragraph.AddText($"{geoPosition.Latitude}, {geoPosition.Longitude}");
                }
                else
                {
                    paragraph.AddFormattedText(question.GetAnswerAsString(), TextFormat.Bold);
                }
            }
            else
            {
                paragraph.AddText(WebInterviewUI.Interview_Overview_NotAnswered);
            }
            
            paragraph.AddLineBreak();
            paragraph.AddLineBreak();
        }
        
        /*internal void SaveImageAsPdf(string imageFileName, string pdfFileName, int width = 600)
        {
            using (var document = new PdfDocument())
            {
                PdfPage page = document.AddPage();
                using (XImage image = XImage.FromImageSource(imageFileName))
                {
                    var height = (int)(((double)width / (double)image.PixelWidth) * image.PixelHeight);

                    page.Width = width;
                    page.Height = height;

                    XGraphics graphics = XGraphics.FromPdfPage(page);
                    graphics.DrawImage(image, 0, 0, width, height);                
                }
                document.Save(pdfFileName);
            }
        }*/
    }
}