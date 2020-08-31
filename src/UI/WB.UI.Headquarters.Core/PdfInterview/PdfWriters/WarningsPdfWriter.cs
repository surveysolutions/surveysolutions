using System.Linq;
using MigraDocCore.DocumentObjectModel;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Infrastructure.Native.Sanitizer;
using PdfInterviewRes = WB.Core.BoundedContexts.Headquarters.Resources.PdfInterview;

namespace WB.UI.Headquarters.PdfInterview.PdfWriters
{
    public class WarningsPdfWriter : IPdfWriter
    {
        private readonly IInterviewTreeValidateable validateable;

        public WarningsPdfWriter(IInterviewTreeValidateable validateable)
        {
            this.validateable = validateable;
        }

        public void Write(Paragraph paragraph)
        {
            if (validateable.FailedWarnings != null && validateable.FailedWarnings.Any())
            {
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
    }
}