#nullable enable

using System.Linq;
using MigraDocCore.DocumentObjectModel;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Infrastructure.Native.Sanitizer;
using PdfInterviewRes = WB.Core.BoundedContexts.Headquarters.Resources.PdfInterview;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview.PdfWriters
{
    public class ErrorsPdfWriter : IPdfWriter
    {
        private readonly IInterviewTreeValidateable validateable;

        public ErrorsPdfWriter(IInterviewTreeValidateable validateable)
        {
            this.validateable = validateable;
        }

        public void Write(Paragraph paragraph)
        {
            if (validateable.FailedErrors != null && validateable.FailedErrors.Any())
            {
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

        }
    }
}