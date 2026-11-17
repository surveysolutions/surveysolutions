#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using PdfInterviewRes = WB.Core.BoundedContexts.Headquarters.Resources.PdfInterview;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview.PdfWriters
{
    public class InterviewHeaderPdfWriter
    {
        private readonly List<Identity> nodes;
        private readonly IQuestionnaire questionnaire;
        private readonly IStatefulInterview interview;
        private readonly IOptions<HeadquartersConfig> headquartersConfig;
        private readonly bool isRightToLeft;

        // logo row spans the page, it is mirrored relatively to the page in right-to-left documents
        internal const double LogoRowWidth = 568;

        public InterviewHeaderPdfWriter(List<Identity> nodes, 
            IQuestionnaire questionnaire, 
            IStatefulInterview interview,
            IOptions<HeadquartersConfig> headquartersConfig,
            bool isRightToLeft = false)
        {
            this.nodes = nodes;
            this.questionnaire = questionnaire;
            this.interview = interview;
            this.headquartersConfig = headquartersConfig;
            this.isRightToLeft = isRightToLeft;
        }

        public void Write(Section section)
        {
            var questions = nodes.Where(node => questionnaire.IsQuestion(node.Id)).ToList();
            var questionsAndStaticTexts = nodes.Where(node => interview.IsEnabled(node) 
                && (questionnaire.IsQuestion(node.Id) || questionnaire.IsStaticText(node.Id))).ToList();
            int answeredQuestions = questions.Count(node => interview.WasAnswered(node)); 
            int unansweredQuestions = questions.Count(node => !interview.WasAnswered(node) && interview.IsEnabled(node)); 
            int sectionsCount = questionnaire.GetAllSections().Count(nodeId => interview.IsEnabled(new Identity(nodeId, RosterVector.Empty))); 
            int errorsCount = questionsAndStaticTexts.Count(node => !interview.IsEntityValid(node));
            int warningsCount = questionsAndStaticTexts.Count(node => !interview.IsEntityPlausible(node));
            int commentedCount = questions.Count(node => interview.GetQuestionComments(node).Any());

            var textFrame = section.AddTextFrame();
            textFrame.RelativeVertical = RelativeVertical.Page;
            textFrame.RelativeHorizontal = RelativeHorizontal.Page;
            textFrame.Top = TopPosition.Parse("31pt");
            textFrame.Width = Unit.FromPoint(590);
            textFrame.Height = Unit.FromPoint(40);
            if (isRightToLeft)
            {
                PageSetup.GetPageSize(PageFormat.A4, out var pageWidth, out _);
                textFrame.Left = Unit.FromPoint(pageWidth.Point - LogoRowWidth);
            }

            // the first column is the place of the bar code, it is drawn over the rendered page
            var headerTable = textFrame.AddTable();
            var headerColumns = Ordered(new[] { 205.0, 22, 120, 221 });
            foreach (var width in headerColumns)
                headerTable.AddColumn(Unit.FromPoint(width));
            var headerRow = headerTable.AddRow();


            var logoContent = GetEmbeddedResource("pdf_logo.png")!;
            using var logoBuffer = new MemoryStream();
            logoContent.CopyTo(logoBuffer);
            var image = headerRow[Column(1, 4)].AddImage(PdfImageSource.FromBinary(logoBuffer.ToArray()));
            image.Width = Unit.FromPoint(14);
            image.Height = Unit.FromPoint(35);
            image.LockAspectRatio = true;

            var surveySolutions = headerRow[Column(2, 4)].AddParagraph();
            surveySolutions.AddFormattedText(PdfInterviewRes.SurveySolutions, isBold: true, size: Unit.FromPoint(10));
            surveySolutions.AddLineBreak();
            surveySolutions.AddFormattedText(PdfInterviewRes.InterviewTranscript, size: Unit.FromPoint(10));
            

            if (!string.IsNullOrEmpty(headquartersConfig.Value.BaseUrl))
            {
                var leftTopText = headerRow[Column(3, 4)].AddParagraph();
                leftTopText.Format.Alignment = isRightToLeft ? ParagraphAlignment.Left : ParagraphAlignment.Right;
                leftTopText.Format.Font.Size = Unit.FromPoint(7);
                leftTopText.AddText(string.Format(PdfInterviewRes.GeneratedBy, headquartersConfig.Value.BaseUrl));
                leftTopText.AddLineBreak();
                leftTopText.AddText(string.Format(PdfInterviewRes.GeneratedAt, DateTime.UtcNow.ToString(DateTimeFormat.DateTimeWithTimezoneFormat)));
            }

            Table table = section.AddTable();
            foreach (var startPadding in Ordered(new[] { 0.0, 15, 15 }))
            {
                var column = table.AddColumn(Unit.FromPoint(175));
                if (isRightToLeft)
                    column.RightPadding = Unit.FromPoint(startPadding);
                else
                    column.LeftPadding = Unit.FromPoint(startPadding);
            }

            table.AddRow().TopPadding = Unit.FromPoint(40);
            var tableRow = table.AddRow();
            var row = new[] { tableRow[Column(0, 3)], tableRow[Column(1, 3)], tableRow[Column(2, 3)] };

            row[0].AddParagraphFormattedText(PdfInterviewRes.InterviewKey, PdfStyles.HeaderLineTitle);

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
                startedValue.AddFormattedText(interview.StartedDate.Value.ToString(DateTimeFormat.DateFormat), PdfStyles.HeaderLineDate);
                startedValue.AddSpace(1);
                startedValue.AddFormattedText(interview.StartedDate.Value.ToString(DateTimeFormat.TimeWithTimezoneFormat), PdfStyles.HeaderLineTime);
            }

            if (interview.CompletedDate.HasValue)
            {
                var completedTitle = row[0].AddParagraph();
                completedTitle.Style = PdfStyles.HeaderLineTitle;
                completedTitle.Format.SpaceBefore = Unit.FromPoint(10);
                completedTitle.AddFormattedText(PdfInterviewRes.Completed, PdfStyles.HeaderLineTitle);

                var completedValue = row[0].AddParagraph();
                completedValue.AddFormattedText(interview.CompletedDate.Value.ToString(DateTimeFormat.DateFormat), PdfStyles.HeaderLineDate);
                completedValue.AddSpace(1);
                completedValue.AddFormattedText(interview.CompletedDate.Value.ToString(DateTimeFormat.TimeWithTimezoneFormat), PdfStyles.HeaderLineTime);
            }
            
            row[1].AddParagraphFormattedText(PdfInterviewRes.Questionnaire, PdfStyles.HeaderLineTitle);
            var questionnaireTitle = row[1].AddParagraph();
            questionnaireTitle.Format.SpaceBefore = Unit.FromPoint(4);
            questionnaireTitle.AddFormattedText(questionnaire.Title, isBold: true, size: Unit.FromPoint(12));
            var questionnaireVersion = row[1].AddParagraph();
            questionnaireVersion.Format.SpaceBefore = Unit.FromPoint(4);
            questionnaireVersion.AddFormattedText(String.Format(PdfInterviewRes.Version, questionnaire.Version), 
                size: Unit.FromPoint(7));

            row[2].AddParagraphFormattedText(PdfInterviewRes.InterviewVolume, PdfStyles.HeaderLineTitle);
            var interviewStats = row[2].AddParagraph();
            interviewStats.Format.SpaceBefore = Unit.FromPoint(2);
            interviewStats.Format.Font.Size = Unit.FromPoint(9);
            //interviewStats.Format.LineSpacing = Unit.FromPoint(5);
            interviewStats.Format.LineSpacingRule = LineSpacingRule.OnePtFive;
            interviewStats.AddFormattedText(string.Format(PdfInterviewRes.InterviewStatisticsSections, sectionsCount), isBold: false);
            interviewStats.AddLineBreak();
            interviewStats.AddFormattedText(string.Format(PdfInterviewRes.InterviewStatisticsAnswered, answeredQuestions), isBold: false);
            interviewStats.AddLineBreak();
            interviewStats.AddFormattedText(string.Format(PdfInterviewRes.InterviewStatisticsUnanswered, unansweredQuestions));
            interviewStats.AddLineBreak();
            interviewStats.AddFormattedText(string.Format(PdfInterviewRes.InterviewStatisticsWarnings, warningsCount))
                .Color = warningsCount > 0 ? new Color(255, 138, 0) : new Color(182, 182, 182);
            interviewStats.AddLineBreak();
            interviewStats.AddFormattedText(string.Format(PdfInterviewRes.InterviewStatisticsErrors, errorsCount))
                .Color = errorsCount > 0 ? new Color(231, 73, 36) : new Color(33, 150, 83);
            interviewStats.AddLineBreak();
            interviewStats.AddFormattedText(string.Format(PdfInterviewRes.InterviewStatisticsCommented, commentedCount))
                .Color = commentedCount > 0 ? new Color(71, 27, 195) : new Color(182, 182, 182);

            var interviewKeyBorder = new Border() { Style = BorderStyle.Single, Width = Unit.FromPoint(1) };
            var statisticsBorder = new Border() { Style = BorderStyle.Single, Width = Unit.FromPoint(1) };
            if (isRightToLeft)
            {
                row[0].Borders.Left = interviewKeyBorder;
                row[2].Borders.Right = statisticsBorder;
            }
            else
            {
                row[0].Borders.Right = interviewKeyBorder;
                row[2].Borders.Left = statisticsBorder;
            }
            table.AddRow();
        }

        private int Column(int index, int count) => isRightToLeft ? count - 1 - index : index;

        private IEnumerable<double> Ordered(double[] values) => isRightToLeft ? values.Reverse() : values;

        private static Stream? GetEmbeddedResource(string filename)
        {
            var assembly = Assembly.GetAssembly(typeof(InterviewHeaderPdfWriter))!;
            return assembly.GetManifestResourceStream($"WB.Core.BoundedContexts.Headquarters.PdfInterview.images.{filename}");
        }
    }
}
