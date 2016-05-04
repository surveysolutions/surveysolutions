using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
{
    internal class TabularParaDataAccessor : IParaDataAccessor
    {
        private readonly ICsvWriter csvWriter;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IArchiveUtils archiveUtils;
        private readonly Dictionary<string, InterviewHistoryView> cache = new Dictionary<string, InterviewHistoryView>();

        private readonly string pathToHistoryFiles;

        public TabularParaDataAccessor(
            ICsvWriter csvWriter, 
            IFileSystemAccessor fileSystemAccessor,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader,
            InterviewDataExportSettings interviewDataExportSettings, 
            IArchiveUtils archiveUtils)
        {
            this.csvWriter = csvWriter;
            this.fileSystemAccessor = fileSystemAccessor;
            this.interviewSummaryReader = interviewSummaryReader;
            this.archiveUtils = archiveUtils;

            this.pathToHistoryFiles = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath,
                interviewDataExportSettings.ExportedDataFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToHistoryFiles))
                fileSystemAccessor.CreateDirectory(this.pathToHistoryFiles);
        }


        public void ClearParaData()
        {
            if (this.fileSystemAccessor.IsDirectoryExists(this.pathToHistoryFiles))
            {
                Array.ForEach(this.fileSystemAccessor.GetDirectoriesInDirectory(this.pathToHistoryFiles), (s) => this.fileSystemAccessor.DeleteDirectory(s));
            }
        }

        public void ArchiveParaDataExport()
        {
            var createdFolders = this.fileSystemAccessor.GetDirectoriesInDirectory(this.pathToHistoryFiles);
            foreach (var createdFolder in createdFolders)
            {
                var archiveFilePath = createdFolder + ".zip";

                if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
                    this.fileSystemAccessor.DeleteFile(archiveFilePath);

                var filesToArchive = new List<string>();

                filesToArchive.AddRange(this.fileSystemAccessor.GetFilesInDirectory(createdFolder));

                this.archiveUtils.ZipFiles(filesToArchive, archiveFilePath);
            }
        }
        public void PersistParaDataExport()
        {
            foreach (var viewId in this.cache.Keys.ToList())
            {
                var interviewHistoryView = this.cache[viewId];
                this.StoreAvoidingCache(interviewHistoryView, viewId);
                this.cache.Remove(viewId);
            }
        }

        public void Store(InterviewHistoryView view, string id)
        {
            this.StoreUsingCache(view, id);
        }

        public InterviewHistoryView GetById(string id)
        {
            return this.GetByIdUsingCache(id);
        }

        public void Remove(string id)
        {
            this.RemoveUsingCache(id);
        }

        public void BulkStore(List<Tuple<InterviewHistoryView, string>> bulk)
        {
            foreach (var tuple in bulk)
            {
                this.Store(tuple.Item1, tuple.Item2);
            }
        }

        private InterviewHistoryView GetByIdUsingCache(string id)
        {
            if (this.cache.ContainsKey(id))
                return this.cache[id];

            var interviewSummary = this.interviewSummaryReader.GetById(id);
            if (interviewSummary == null)
                return null;

            var entity= new InterviewHistoryView(interviewSummary.InterviewId, new List<InterviewHistoricalRecordView>(),
                interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);

            this.cache[id] = entity;

            return entity;
        }

        private void StoreUsingCache(InterviewHistoryView entity, string id)
        {
            this.cache[id] = entity;
        }

        private void RemoveUsingCache(string id)
        {
            this.cache.Remove(id);
            this.RemoveAvoidingCache(id);
        }

        protected virtual void StoreBulkEntitiesToRepository(IEnumerable<string> bulk)
        {
            foreach (var entityId in bulk)
            {
                var entity = this.cache[entityId];
                if (entity == null)
                {
                    this.RemoveAvoidingCache(entityId);
                }
                else
                {
                    this.StoreAvoidingCache(entity, entityId);
                }

                this.cache.Remove(entityId);
            }
        }

        private void RemoveAvoidingCache(string id)
        {
            var interviewSummary = this.interviewSummaryReader.GetById(id);
            if (interviewSummary == null)
                return;

            var pathToInterviewHistory = this.GetPathToInterviewHistoryFile(id, interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);

            if (this.fileSystemAccessor.IsFileExists(pathToInterviewHistory))
                this.fileSystemAccessor.DeleteFile(pathToInterviewHistory);
        }

        private void StoreAvoidingCache(InterviewHistoryView view, string id)
        {
            var questionnairePath = this.GetPathToFolderWithParaDataByQuestionnaire(view.QuestionnaireId, view.QuestionnaireVersion);

            if (!this.fileSystemAccessor.IsDirectoryExists(questionnairePath))
                this.fileSystemAccessor.CreateDirectory(questionnairePath);

            using (var fileStream =
                    this.fileSystemAccessor.OpenOrCreateFile(this.GetPathToInterviewHistoryFile(id, view.QuestionnaireId, view.QuestionnaireVersion), true))

            using (var writer = this.csvWriter.OpenCsvWriter(fileStream, ExportFileSettings.SeparatorOfExportedDataFile.ToString()))
            {
                foreach (var interviewHistoricalRecordView in view.Records)
                {
                    writer.WriteField(interviewHistoricalRecordView.Action);
                    writer.WriteField(interviewHistoricalRecordView.OriginatorName);
                    writer.WriteField(interviewHistoricalRecordView.OriginatorRole);
                    if (interviewHistoricalRecordView.Timestamp.HasValue)
                    {
                        writer.WriteField(interviewHistoricalRecordView.Timestamp.Value.ToString("d",
                            CultureInfo.InvariantCulture));
                        writer.WriteField(interviewHistoricalRecordView.Timestamp.Value.ToString("T",
                            CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        writer.WriteField(string.Empty);
                        writer.WriteField(string.Empty);
                    }
                    foreach (var value in interviewHistoricalRecordView.Parameters.Values)
                    {
                        writer.WriteField(value);
                    }
                    writer.NextRecord();
                }
            }
        }

        public  string GetPathToParaDataByQuestionnaire(Guid questionnaireId, long version)
        {
            return GetPathToFolderWithParaDataByQuestionnaire(questionnaireId, version) + ".zip";
        }

        private string GetPathToFolderWithParaDataByQuestionnaire(Guid questionnaireId, long version)
        {
            return this.fileSystemAccessor.CombinePath(this.pathToHistoryFiles,
                $"{questionnaireId}-{version}");
        }

        private string GetPathToInterviewHistoryFile(string interviewId, Guid questionnaireId, long version)
        {
            return this.fileSystemAccessor.CombinePath(this.GetPathToFolderWithParaDataByQuestionnaire(questionnaireId, version),
                $"{interviewId}.tab");
        }

        public Type ViewType { get { return typeof (InterviewHistoryView); } }
        public string GetReadableStatus()
        {
            return $"Interview history -_- | items: {this.cache.Count}";
        }
    }
}
