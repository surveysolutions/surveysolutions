﻿using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
{
    internal class FilebasedExportedDataAccessor : IFilebasedExportedDataAccessor
    {
        private const string ExportedDataFolderName = "ExportedData";
        private readonly string pathToExportedData;
        private readonly IExportFileNameService exportFileNameService;

        public FilebasedExportedDataAccessor(
            IFileSystemAccessor fileSystemAccessor,
            InterviewDataExportSettings interviewDataExportSettings, 
            IExportFileNameService exportFileNameService)
        {
            this.exportFileNameService = exportFileNameService;
            this.pathToExportedData = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath, ExportedDataFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedData))
                fileSystemAccessor.CreateDirectory(this.pathToExportedData);
        }

        public string GetArchiveFilePathForExportedData(QuestionnaireIdentity questionnaireId, DataExportFormat format,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            fromDate = format == DataExportFormat.Binary ? null : fromDate;
            toDate = format == DataExportFormat.Binary ? null : toDate;

            var statusSuffix = status != null && format != DataExportFormat.Binary ? status.ToString() : "All";

            return this.exportFileNameService.GetFileNameForTabByQuestionnaire(questionnaireId, this.pathToExportedData,
                format, statusSuffix, fromDate, toDate);
        }

        public string GetExportDirectory()
        {
            return this.pathToExportedData;
        }
    }
}
