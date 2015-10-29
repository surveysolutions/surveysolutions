using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class ExportedParaDataContentFactory : IViewFactory<ExportedDataReferenceInputModel, string>
    {
        private readonly IPlainStorageAccessor<DataExportProcessDto> dataExportProcessDtoStorage;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public ExportedParaDataContentFactory(IPlainStorageAccessor<DataExportProcessDto> dataExportProcessDtoStorage, IFileSystemAccessor fileSystemAccessor)
        {
            this.dataExportProcessDtoStorage = dataExportProcessDtoStorage;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public string Load(ExportedDataReferenceInputModel input)
        {
            var latestSuccessfulExportProcess =
                dataExportProcessDtoStorage.Query(
                    _ =>
                        _.Where(
                            d =>
                                d.DataExportType == DataExportType.ParaData &&
                                d.Status == DataExportStatus.Finished)
                            .OrderByDescending(x => x.LastUpdateDate)
                            .FirstOrDefault());
            var pathToParadataByQuestionnaire = GetPathToQuestionnaireFolder(input.QuestionnaireId,
                input.QuestionnaireVersion, latestSuccessfulExportProcess.ExportedDataPath);

            if (fileSystemAccessor.IsFileExists(pathToParadataByQuestionnaire))
                return pathToParadataByQuestionnaire;

            return null;
        }

        private string GetPathToQuestionnaireFolder(Guid questionnaireId, long version, string pathToParaDataFiles)
        {
            return this.fileSystemAccessor.CombinePath(pathToParaDataFiles,
                $"{questionnaireId}-{version}.zip");
        }
    }
}