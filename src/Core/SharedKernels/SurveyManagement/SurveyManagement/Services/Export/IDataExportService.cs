using System;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Export
{
    internal interface IDataExportService 
    {
        string[] CreateAndGetStataDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string basePath);
        string[] CreateAndGetSpssDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string basePath);

        string[] CreateAndGetStataDataFilesForQuestionnaireInApprovedState(Guid questionnaireId, long questionnaireVersion, string basePath);
        string[] CreateAndGetSpssDataFilesForQuestionnaireInApprovedState(Guid questionnaireId, long questionnaireVersion, string basePath);
    }
}
