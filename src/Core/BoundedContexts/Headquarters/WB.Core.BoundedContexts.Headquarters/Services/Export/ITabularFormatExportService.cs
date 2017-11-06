using System;
using System.Collections.Generic;
using System.Threading;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Services.Export
{
    public interface ITabularFormatExportService
    {
        void GenerateDescriptionFile(QuestionnaireIdentity questionnaireIdentity, string basePath, string dataFilesExtension);
        void ExportInterviewsInTabularFormat(QuestionnaireIdentity questionnaireIdentity, InterviewStatus? status, string basePath, IProgress<int> progress, CancellationToken cancellationToken);
        void CreateHeaderStructureForPreloadingForQuestionnaire(QuestionnaireIdentity questionnaireIdentity, string basePath);
        string[] GetTabularDataFilesFromFolder(string basePath);

        List<Guid> GetInterviewIdsToExport(QuestionnaireIdentity questionnaireIdentity, InterviewStatus? status,
            CancellationToken cancellationToken);
    }
}