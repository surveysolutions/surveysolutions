using System;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal interface ITabularDataToExternalStatPackageExportService 
    {
        string[] CreateAndGetStataDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string[] tabularDataFiles);
        string[] CreateAndGetSpssDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string[] tabularDataFiles);
    }
}
