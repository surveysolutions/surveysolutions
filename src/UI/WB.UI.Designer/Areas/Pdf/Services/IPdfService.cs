using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.UI.Designer.Areas.Pdf.Services;

public interface IPdfService
{
    Task<PdfGenerationProgress> Enqueue(QuestionnaireRevision id, Guid? translation, DocumentType documentType, int? timezoneOffsetMinutes);
    PdfGenerationProgress? Status(QuestionnaireRevision id, Guid? translation, DocumentType documentType);
    Task<PdfGenerationProgress> Retry(QuestionnaireRevision id, Guid? translation, DocumentType documentType);
    byte[]? Download(QuestionnaireRevision id, Guid? translation, DocumentType documentType);
}
