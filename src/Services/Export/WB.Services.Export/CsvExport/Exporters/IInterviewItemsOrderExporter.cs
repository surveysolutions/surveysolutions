using System;
using System.Collections.Generic;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.CsvExport.Exporters
{
    public interface IInterviewItemsOrderExporter
    {
        Dictionary<Guid, string> GetFileNames(QuestionnaireDocument questionnaire);
        void WriteHeaders(QuestionnaireDocument questionnaire, string basePath, string dataFileExtension);
        Dictionary<string, List<string[]>> Export(QuestionnaireDocument questionnaire, List<InterviewEntity> interviewEntities, Guid interviewId, string interviewKey);
    }
}
