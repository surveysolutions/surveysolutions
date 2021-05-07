#nullable enable

using MigraDocCore.DocumentObjectModel;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Infrastructure.Native.Sanitizer;
using PdfInterviewRes = WB.Core.BoundedContexts.Headquarters.Resources.PdfInterview;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview.PdfWriters
{
    public class VariablePdfWriter: IPdfWriter
    {
        private readonly InterviewTreeVariable variable;
        private readonly IStatefulInterview interview;
        private readonly IQuestionnaire questionnaire;

        public VariablePdfWriter(InterviewTreeVariable variable,
            IStatefulInterview interview, 
            IQuestionnaire questionnaire)
        {
            this.variable = variable;
            this.interview = interview;
            this.questionnaire = questionnaire;
        }

        public void Write(Paragraph paragraph)
        {
            var isPrefilled = questionnaire.IsPrefilled(variable.Identity.Id);
            paragraph.Style = PdfStyles.VariableTitle;
            var title = questionnaire.GetVariableLabel(variable.Identity.Id);
            paragraph.AddWrapFormattedText(title.RemoveHtmlTags(), PdfStyles.VariableTitle);
            paragraph.AddLineBreak();

            var answerStyle = isPrefilled ? PdfStyles.IdentifyerVariableValue : PdfStyles.VariableValue;

            if (variable.HasValue)
            {
                paragraph.AddWrapFormattedText(variable.GetValueAsString(), answerStyle, PdfColors.Default);
            }
            else
            {
                var nonAnswerStyle = isPrefilled ? PdfStyles.IdentifyerVariableNotCalculated : PdfStyles.VariableNotCalculated;
                paragraph.AddWrapFormattedText(PdfInterviewRes.NotCalculated, nonAnswerStyle);
            }
        }
    }
}