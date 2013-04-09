using System;
using Main.Core.Documents;

namespace WB.Core.Questionnaire.ExportServices
{
    public interface IExportService
    {
        string GetQuestionnaireTemplate(Guid templateId);
    }
}