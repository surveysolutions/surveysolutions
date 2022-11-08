#nullable enable

using System;
using System.Linq;
using Humanizer;
using Microsoft.Extensions.Options;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using WB.Core.BoundedContexts.Headquarters.Configs;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Sanitizer;
using PdfInterviewRes = WB.Core.BoundedContexts.Headquarters.Resources.PdfInterview;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview.PdfWriters
{
    public class QuestionPdfWriter : IPdfWriter
    {
        private readonly InterviewTreeQuestion question;
        private readonly IStatefulInterview interview;
        private readonly IQuestionnaire questionnaire;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IOptions<GoogleMapsConfig> googleMapsConfig;
        private readonly IAttachmentContentService attachmentContentService;

        public QuestionPdfWriter(InterviewTreeQuestion question,
            IStatefulInterview interview,
            IQuestionnaire questionnaire,
            IImageFileStorage imageFileStorage,
            IOptions<GoogleMapsConfig> googleMapsConfig,
            IAttachmentContentService attachmentContentService)
        {
            this.question = question;
            this.interview = interview;
            this.questionnaire = questionnaire;
            this.imageFileStorage = imageFileStorage;
            this.googleMapsConfig = googleMapsConfig;
            this.attachmentContentService = attachmentContentService;
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
                    var audioText = paragraph.AddFormattedText(audioAnswer.FileName, answerStyle);
                    audioText.Color = textColor;
                    audioText.Italic = true;
                    audioText = paragraph.AddFormattedText($" - {audioAnswer.Length}", answerStyle);
                    audioText.Color = textColor;
                    audioText.Italic = true;
                    audioText.Bold = false;
                }
                else if (question.IsMultimedia)
                {
                    paragraph.Format.LineSpacingRule = LineSpacingRule.Single;
                    var multimediaQuestion = question.GetAsInterviewTreeMultimediaQuestion();
                    var fileName = multimediaQuestion.GetAnswer().FileName;
                    var binaryData = imageFileStorage.GetInterviewBinaryData(interview.Id, fileName);
                    if (binaryData != null)
                    {
                        ImageSource.IImageSource imageSource = ImageSource.FromBinary(fileName, () => binaryData);
                        var image = paragraph.AddImage(imageSource);
                        image.Width = Unit.FromPoint(300);
                        image.Height = Unit.FromPoint(300);
                    }

                    paragraph.AddLineBreak();
                    var imageName = paragraph.AddFormattedText($"{fileName}", answerStyle);
                    imageName.Color = textColor;
                    imageName.Italic = true;
                    var imageSize = paragraph.AddFormattedText($" - ({SizeInKb(binaryData?.Length ?? 0)})", answerStyle);
                    imageSize.Color = textColor;
                    imageSize.Italic = true;
                    imageSize.Bold = false;
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
                        var order = questionnaire.ShouldQuestionRecordAnswersOrder(question.Identity.Id);
                        int index = 1;
                        foreach (var answerOption in yesNoAnswer.CheckedOptions)
                        {
                            var option = interview.GetOptionForQuestionWithoutFilter(question.Identity, answerOption.Value, null);
                            var optionAnswer = answerOption.Yes ? PdfInterviewRes.Yes : (answerOption.No ? PdfInterviewRes.No : PdfInterviewRes.NotAnswered);
                            if (order)
                                paragraph.AddWrapFormattedText($"#{index++} ", PdfStyles.YesNoTitle);
                            paragraph.AddWrapFormattedText($"{optionAnswer}: ", PdfStyles.YesNoTitle);
                            paragraph.AddWrapFormattedText(option.Title, answerStyle, textColor);
                            WriteOptionAttachmentIfNeed(paragraph, option);
                            paragraph.AddLineBreak();
                        }
                    }
                }
                else if (question.IsMultiFixedOption)
                {
                    var multiOptionQuestion = question.GetAsInterviewTreeMultiOptionQuestion();
                    var order = questionnaire.ShouldQuestionRecordAnswersOrder(question.Identity.Id);
                    int index = 1;
                    foreach (var checkedValue in multiOptionQuestion.GetAnswer().CheckedValues)
                    {
                        var option = interview.GetOptionForQuestionWithoutFilter(question.Identity, checkedValue, null);
                        if (order)
                            paragraph.AddWrapFormattedText($"#{index++}: ", PdfStyles.YesNoTitle);
                        paragraph.AddWrapFormattedText(option.Title, answerStyle, textColor);
                        WriteOptionAttachmentIfNeed(paragraph, option);
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
                    var refListQuestionAllOptions = (refListQuestion?.InterviewQuestion as InterviewTreeTextListQuestion)?.GetAnswer()?.Rows;
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
                else if (question.IsSingleFixedOption)
                {
                    var singleOptionQuestion = question.GetAsInterviewTreeSingleOptionQuestion();
                    var selectedValue = singleOptionQuestion.GetAnswer().SelectedValue;
                    var option = questionnaire.GetOptionForQuestionByOptionValue(question.Identity.Id, selectedValue, null);
                    paragraph.AddWrapFormattedText(option.Title, PdfStyles.QuestionAnswer, textColor);
                    WriteOptionAttachmentIfNeed(paragraph, option);
                    paragraph.AddLineBreak();
                }
                else if (question.IsCascading)
                {
                    var cascadingQuestion = question.GetAsInterviewTreeCascadingQuestion();
                    var selectedValue = cascadingQuestion.GetAnswer().SelectedValue;
                    var parentValue = cascadingQuestion.GetCascadingParentQuestion().GetAnswer().SelectedValue;
                    var option = questionnaire.GetOptionForQuestionByOptionValue(question.Identity.Id, selectedValue, parentValue);
                    paragraph.AddWrapFormattedText(option.Title, PdfStyles.QuestionAnswer, textColor);
                    WriteOptionAttachmentIfNeed(paragraph, option);
                    paragraph.AddLineBreak();
                }
                else if (question.IsInteger)
                {
                    paragraph.AddWrapFormattedText(question.GetAnswerAsString(), PdfStyles.QuestionAnswer, textColor);
                    
                    var answerValue = question.GetAsInterviewTreeIntegerQuestion().GetAnswer()?.Value;
                    if (!answerValue.HasValue) return;
                    
                    var specialValue = questionnaire.GetOptionForQuestionByOptionValue(question.Identity.Id, answerValue.Value, null);
                    if (specialValue == null) return;
                        
                    WriteOptionAttachmentIfNeed(paragraph, specialValue);
                    paragraph.AddLineBreak();
                }
                else if (question.IsDouble)
                {
                    paragraph.AddWrapFormattedText(question.GetAnswerAsString(), PdfStyles.QuestionAnswer, textColor);
                    
                    var answerValue = question.GetAsInterviewTreeDoubleQuestion().GetAnswer()?.Value;
                    if (!answerValue.HasValue || Math.Truncate(answerValue.Value) != answerValue.Value) return;
                    
                    try
                    {
                        var specialValue = questionnaire.GetOptionForQuestionByOptionValue(question.Identity.Id, Convert.ToInt32(answerValue.Value), null);
                        if (specialValue == null) return;
                            
                        WriteOptionAttachmentIfNeed(paragraph, specialValue);
                        paragraph.AddLineBreak();
                    }
                    catch (OverflowException)
                    {
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
                paragraph.AddWrapFormattedText(PdfInterviewRes.NotAnswered, nonAnswerStyle);
            }
        }

        private void WriteOptionAttachmentIfNeed(Paragraph paragraph, CategoricalOption option)
        {
            if (!string.IsNullOrEmpty(option.AttachmentName))
            {
                var attachmentId = questionnaire.GetAttachmentIdByName(option.AttachmentName);
                new AttachmentPdfWriter(attachmentId, interview, questionnaire, attachmentContentService)
                    .Write(paragraph);
            }
        }

        private string SizeInKb(int bytes) => bytes.Bytes().ToString("KB");
    }
}
