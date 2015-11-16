using System;
using Microsoft;
namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal interface ITabularDataToExternalStatPackageExportService 
    {
        string[] CreateAndGetStataDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string[] tabularDataFiles, IProgress<int> progress);
        string[] CreateAndGetSpssDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string[] tabularDataFiles, IProgress<int> progress);
    }
}
