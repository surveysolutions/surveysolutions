using System;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Export
{
    internal interface IDataExportService
    {
        void CreateHeaderStructureForPreloadingForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string targetFolder);
        string[] GetDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string basePath);
        string[] GetDataFilesForQuestionnaireByInterviewsInApprovedState(Guid questionnaireId, long questionnaireVersion, string basePath);

        string[] CreateAndGetStataDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string basePath);
        string[] CreateAndGetSpssDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string basePath);

        string[] CreateAndGetStataDataFilesForQuestionnaireInApprovedState(Guid questionnaireId, long questionnaireVersion, string basePath);
        string[] CreateAndGetSpssDataFilesForQuestionnaireInApprovedState(Guid questionnaireId, long questionnaireVersion, string basePath);

    }
}
