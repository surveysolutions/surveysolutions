using System.Threading;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal interface IEnvironmentContentService
    {
        void CreateEnvironmentFiles(QuestionnaireExportStructure questionnaireExportStructure, string folderPath, CancellationToken cancellationToken);
    }
}