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
    public class InterviewHistoryWriter : IReadSideRepositoryWriter<InterviewHistoryView>, IReadSideRepositoryCleaner, IChacheableRepositoryWriter
    {
        private readonly ICsvWriterFactory csvWriterFactory;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly Dictionary<string, InterviewHistoryView> cache = new Dictionary<string, InterviewHistoryView>();
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private bool cacheEnabled = false;

        public InterviewHistoryWriter(ICsvWriterFactory csvWriterFactory, IFileSystemAccessor fileSystemAccessor,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader, IFilebasedExportedDataAccessor filebasedExportedDataAccessor)
        {
            this.csvWriterFactory = csvWriterFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.interviewSummaryReader = interviewSummaryReader;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
        }


        public void Clear()
        {
            filebasedExportedDataAccessor.CleanExportHistoryFolder();
        }

        public InterviewHistoryView GetById(string id)
        {
            if (cacheEnabled)
            {
                if (cache.ContainsKey(id))
                    return cache[id];
                return null;
            }

            var interviewSummary = this.interviewSummaryReader.GetById(id);
            if (interviewSummary == null)
                return null;

            return new InterviewHistoryView(interviewSummary.InterviewId, new List<InterviewHistoricalRecordView>(),
                interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);
        }

        public void Remove(string id)
        {
            if (cacheEnabled)
            {
                cache.Remove(id);
            }

            var interviewSummary = this.interviewSummaryReader.GetById(id);
            if (interviewSummary == null)
                return;

            var pathToInterviewHistory = GetPathToInterviewHistoryFile(id, interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);

            if(fileSystemAccessor.IsFileExists(pathToInterviewHistory))
                fileSystemAccessor.DeleteFile(pathToInterviewHistory);
        }

        public void Store(InterviewHistoryView view, string id)
        {
            if (cacheEnabled)
            {
                cache[id] = view;
                return;
            }
            this.StoreImpl(view,id);
        }

        private void StoreImpl(InterviewHistoryView view, string id)
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
                    writer.WriteField(string.Join(",", interviewHistoricalRecordView.Parameters.Select(p => p.Key + ": " + p.Value)));
                    writer.WriteField(interviewHistoricalRecordView.OriginatorName);
                    writer.WriteField(interviewHistoricalRecordView.OriginatorRole);
                    writer.WriteField(interviewHistoricalRecordView.Timestamp.ToString("d", CultureInfo.InvariantCulture));
                    writer.WriteField(interviewHistoricalRecordView.Timestamp.ToString("T", CultureInfo.InvariantCulture));
                    writer.NextRecord();
                }
            }
        }

        private string GetPathToQuestionnaireFolder(Guid questionnaireId, long version)
        {
            return filebasedExportedDataAccessor.GetFolderPathOfHistoryByQuestionnaire(questionnaireId, version);
        }

        private string GetPathToInterviewHistoryFile(string interviewId, Guid questionnaireId, long version)
        {
            return fileSystemAccessor.CombinePath(GetPathToQuestionnaireFolder(questionnaireId, version),
                string.Format("{0}.tab", interviewId));
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
                this.StoreImpl(interviewHistoryView, viewId);
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

        public Type ViewType { get { return typeof (InterviewHistoryView); } }

        public bool IsCacheEnabled { get { return this.cacheEnabled; } }
        
        public void BulkStore(List<Tuple<InterviewHistoryView, string>> bulk)
        {
            foreach (var tuple in bulk)
            {
                Store(tuple.Item1, tuple.Item2);
            }
        }
    }
}
