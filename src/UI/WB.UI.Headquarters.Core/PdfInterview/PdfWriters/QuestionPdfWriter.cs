using System.Linq;
using Microsoft.Extensions.Options;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Sanitizer;
using WB.UI.Headquarters.Configs;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.PdfInterview.PdfWriters
{
    public class QuestionPdfWriter : IPdfWriter
    {
        private readonly InterviewTreeQuestion question;
        private readonly IStatefulInterview interview;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IOptions<GoogleMapsConfig> googleMapsConfig;

        public QuestionPdfWriter(InterviewTreeQuestion question,
            IStatefulInterview interview,
            IImageFileStorage imageFileStorage,
            IOptions<GoogleMapsConfig> googleMapsConfig)
        {
            this.question = question;
            this.interview = interview;
            this.imageFileStorage = imageFileStorage;
            this.googleMapsConfig = googleMapsConfig;
        }

        public void Write(Paragraph paragraph)
        {
            var isPrefilled = interview.IsQuestionPrefilled(question.Identity);
            paragraph.Style = PdfStyles.QuestionTitle;
            paragraph.AddWrapFormattedText(question.Title.Text.RemoveHtmlTags(), PdfStyles.QuestionTitle);
            paragraph.AddLineBreak();

            var answerStyle = isPrefilled ? PdfStyles.IdentifyerQuestionAnswer : PdfStyles.QuestionAnswer;

            if (question.IsAnswered())
            {
                var textColor = !question.IsValid
                    ? PdfColors.Error
                    : (!question.IsPlausible ? PdfColors.Warning : PdfColors.Default);
                
                if (question.IsAudio)
                {
                    var audioQuestion = question.GetAsInterviewTreeAudioQuestion();
                    var audioAnswer = audioQuestion.GetAnswer();
                    paragraph.AddWrapFormattedText($"{audioAnswer.FileName} + ({audioAnswer.Length})", answerStyle, textColor);
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
                    paragraph.AddWrapFormattedText(areaAnswer.ToString(), answerStyle, textColor);
                }
                else if (question.IsGps)
                {
                    var gpsQuestion = question.GetAsInterviewTreeGpsQuestion();
                    var geoPosition = gpsQuestion.GetAnswer().Value;
                    var mapsUrl = $"{googleMapsConfig.Value.BaseUrl}/maps/search/?api=1&query={geoPosition.Latitude},{geoPosition.Longitude}";
                    var hyperlink = paragraph.AddHyperlink(mapsUrl, HyperlinkType.Web);
                    var geoText = hyperlink.AddFormattedText($"{geoPosition.Latitude}, {geoPosition.Longitude}", answerStyle);
                    geoText.Color = textColor;
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
                            paragraph.AddWrapFormattedText(option.Title, answerStyle, textColor);
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
                        paragraph.AddWrapFormattedText(option.Title, answerStyle, textColor);
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
                            paragraph.AddWrapFormattedText(answer, answerStyle, textColor);
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
                            paragraph.AddWrapFormattedText(answer.Text, answerStyle, textColor);
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
                            paragraph.AddWrapFormattedText(answer.Text, answerStyle, textColor);
                            paragraph.AddLineBreak();
                        }
                    }
                }
                else
                {
                    paragraph.AddWrapFormattedText(question.GetAnswerAsString(), PdfStyles.QuestionAnswer, textColor);
                }
            }
            else
            {
                var nonAnswerStyle = isPrefilled ? PdfStyles.IdentifyerQuestionNotAnswered : PdfStyles.QuestionNotAnswered;
                paragraph.AddWrapFormattedText(WebInterviewUI.Interview_Overview_NotAnswered, nonAnswerStyle);
            }
        }
    }
}