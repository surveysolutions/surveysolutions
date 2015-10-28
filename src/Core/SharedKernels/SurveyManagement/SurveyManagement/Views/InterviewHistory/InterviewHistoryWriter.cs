using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Resources;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory
{
    public class InterviewHistoryWriter : IReadSideRepositoryWriter<InterviewHistoryView>, IReadSideRepositoryCleaner, ICacheableRepositoryWriter
    {
        private readonly ICsvWriterFactory csvWriterFactory;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly Dictionary<string, InterviewHistoryView> cache = new Dictionary<string, InterviewHistoryView>();
        private readonly int maxCountOfCachedEntities = 1024;
        private bool cacheEnabled = false;
        private readonly string pathToHistoryFiles;

        public InterviewHistoryWriter(ICsvWriterFactory csvWriterFactory, IFileSystemAccessor fileSystemAccessor,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader,
            InterviewDataExportSettings interviewDataExportSettings)
        {
            this.csvWriterFactory = csvWriterFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.interviewSummaryReader = interviewSummaryReader;

            this.pathToHistoryFiles = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath,
                interviewDataExportSettings.ExportedDataFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToHistoryFiles))
                fileSystemAccessor.CreateDirectory(this.pathToHistoryFiles);
        }


        public void Clear()
        {
            if (fileSystemAccessor.IsDirectoryExists(this.pathToHistoryFiles))
            {
                Array.ForEach(this.fileSystemAccessor.GetDirectoriesInDirectory(this.pathToHistoryFiles), (s) => this.fileSystemAccessor.DeleteDirectory(s));
                Array.ForEach(this.fileSystemAccessor.GetFilesInDirectory(this.pathToHistoryFiles), (s) => this.fileSystemAccessor.DeleteFile(s));
            }
        }

        public void Store(InterviewHistoryView view, string id)
        {
            if (this.cacheEnabled)
            {
                this.StoreUsingCache(view, id);
            }
            else
            {
                this.StoreAvoidingCache(view, id);
            }
        }


        public InterviewHistoryView GetById(string id)
        {
            if (cacheEnabled)
            {
                return this.GetByIdUsingCache(id);
            }

            var interviewSummary = this.interviewSummaryReader.GetById(id);
            if (interviewSummary == null)
                return null;

            return new InterviewHistoryView(interviewSummary.InterviewId, new List<InterviewHistoricalRecordView>(),
                interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);
        }

        public void Remove(string id)
        {
            if (this.cacheEnabled)
            {
                this.RemoveUsingCache(id);
            }
            else
            {
                RemoveAvoidingCache(id);
            }
        }

        public void EnableCache()
        {
            cacheEnabled = true;
        }

        public void DisableCache()
        {
            foreach (var viewId in cache.Keys.ToList())
            {
                var interviewHistoryView = cache[viewId];
                this.StoreAvoidingCache(interviewHistoryView, viewId);
                cache.Remove(viewId);
            }

            cacheEnabled = false;
        }

        public string GetReadableStatus()
        {
            return string.Format("Interview history -_- | cache {0} | items: {1}",
              this.cacheEnabled ? "enabled" : "disabled",
              cache.Count);
        }

        public Type ViewType { get { return typeof(InterviewHistoryView); } }

        public bool IsCacheEnabled { get { return this.cacheEnabled; } }

        public void BulkStore(List<Tuple<InterviewHistoryView, string>> bulk)
        {
            foreach (var tuple in bulk)
            {
                Store(tuple.Item1, tuple.Item2);
            }
        }

        private InterviewHistoryView GetByIdUsingCache(string id)
        {
            if (cache.ContainsKey(id))
                return cache[id];

            var interviewSummary = this.interviewSummaryReader.GetById(id);
            if (interviewSummary == null)
                return null;

            var entity= new InterviewHistoryView(interviewSummary.InterviewId, new List<InterviewHistoricalRecordView>(),
                interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);

            cache[id] = entity;

            this.ReduceCacheIfNeeded();

            return entity;
        }

        private void StoreUsingCache(InterviewHistoryView entity, string id)
        {
            this.cache[id] = entity;

            this.ReduceCacheIfNeeded();
        }

        private void RemoveUsingCache(string id)
        {
            this.cache.Remove(id);
            RemoveAvoidingCache(id);
        }

        private void ReduceCacheIfNeeded()
        {
            if (this.IsCacheLimitReached())
            {
                this.ReduceCache();
            }
        }

        private bool IsCacheLimitReached()
        {
            return this.cache.Count >= maxCountOfCachedEntities;
        }

        private void ReduceCache()
        {
            var bulk = this.cache.Keys.Take(maxCountOfCachedEntities/2).ToList();
            this.StoreBulkEntitiesToRepository(bulk);
        }

        protected virtual void StoreBulkEntitiesToRepository(IEnumerable<string> bulk)
        {
            foreach (var entityId in bulk)
            {
                var entity = cache[entityId];
                if (entity == null)
                {
                    RemoveAvoidingCache(entityId);
                }
                else
                {
                    this.StoreAvoidingCache(entity, entityId);
                }

                cache.Remove(entityId);
            }
        }

        private void RemoveAvoidingCache(string id)
        {
            var interviewSummary = this.interviewSummaryReader.GetById(id);
            if (interviewSummary == null)
                return;

            var pathToInterviewHistory = GetPathToInterviewHistoryFile(id, interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);

            if (fileSystemAccessor.IsFileExists(pathToInterviewHistory))
                fileSystemAccessor.DeleteFile(pathToInterviewHistory);
        }

        private void StoreAvoidingCache(InterviewHistoryView view, string id)
        {
            var questionnariePath = GetPathToQuestionnaireFolder(view.QuestionnaireId, view.QuestionnaireVersion);

            if (!fileSystemAccessor.IsDirectoryExists(questionnariePath))
                fileSystemAccessor.CreateDirectory(questionnariePath);

            using (var fileStream =
                    fileSystemAccessor.OpenOrCreateFile(GetPathToInterviewHistoryFile(id, view.QuestionnaireId, view.QuestionnaireVersion), true))

            using (var writer = csvWriterFactory.OpenCsvWriter(fileStream, ExportFileSettings.SeparatorOfExportedDataFile.ToString()))
            {
                foreach (var interviewHistoricalRecordView in view.Records)
                {
                    writer.WriteField(interviewHistoricalRecordView.Action);
                //    writer.WriteField(string.Join(",", interviewHistoricalRecordView.Parameters.Select(p => p.Key + ": " + p.Value)));
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

        private string GetPathToQuestionnaireFolder(Guid questionnaireId, long version)
        {
            return fileSystemAccessor.CombinePath(pathToHistoryFiles,
               string.Format("{0}-{1}", questionnaireId, version));
        }

        private string GetPathToInterviewHistoryFile(string interviewId, Guid questionnaireId, long version)
        {
            return fileSystemAccessor.CombinePath(GetPathToQuestionnaireFolder(questionnaireId, version),
                string.Format("{0}.tab", interviewId));
        }
    }
}
