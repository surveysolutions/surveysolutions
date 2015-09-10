using System;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Export
{
    internal interface ITabularDataToExternalStatPackageExportService 
    {
        string[] CreateAndGetStataDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string[] tabularDataFiles);
        string[] CreateAndGetSpssDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string[] tabularDataFiles);
    }
}
