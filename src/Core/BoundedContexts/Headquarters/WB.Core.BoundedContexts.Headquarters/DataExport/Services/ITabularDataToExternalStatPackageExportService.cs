using System;
using System.Threading;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal interface ITabularDataToExternalStatPackageExportService 
    {
        string[] CreateAndGetStataDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string[] tabularDataFiles, IProgress<int> progress, CancellationToken cancellationToken);
        string[] CreateAndGetSpssDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string[] tabularDataFiles, IProgress<int> progress, CancellationToken cancellationToken);
    }
}
