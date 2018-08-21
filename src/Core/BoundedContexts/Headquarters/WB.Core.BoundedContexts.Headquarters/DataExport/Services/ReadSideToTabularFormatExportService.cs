using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class ReadSideToTabularFormatExportService : ITabularFormatExportService
    {
        private readonly string dataFileExtension = "tab";

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICsvWriter csvWriter;
        private readonly ILogger logger;

        private readonly CommentsExporter commentsExporter;
        private readonly InterviewActionsExporter interviewActionsExporter;
        private readonly IInterviewsExporter interviewsExporter;
        private readonly DiagnosticsExporter diagnosticsExporter;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries;
        private readonly InterviewDataExportSettings exportSettings;
        private readonly IProductVersion productVersion;

        public ReadSideToTabularFormatExportService(IFileSystemAccessor fileSystemAccessor,
            ICsvWriter csvWriter, 
            ILogger logger,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries,
            InterviewDataExportSettings exportSettings, 
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage,
            IProductVersion productVersion,
            IInterviewsExporter interviewsExporter,
            CommentsExporter commentsExporter,
            InterviewActionsExporter interviewActionsExporter,
            DiagnosticsExporter diagnosticsExporter)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriter = csvWriter;
            this.logger = logger;
            this.interviewSummaries = interviewSummaries;
            this.exportSettings = exportSettings;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.productVersion = productVersion;
            this.interviewsExporter = interviewsExporter;
            this.commentsExporter = commentsExporter;
            this.interviewActionsExporter = interviewActionsExporter;
            this.diagnosticsExporter = diagnosticsExporter;
        }

        public void GenerateDescriptionFile(QuestionnaireIdentity questionnaireIdentity, string basePath, string dataFilesExtension)
        {
            QuestionnaireExportStructure questionnaireExportStructure = this.GetQuestionnaireExportStructure(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);

            var descriptionBuilder = new StringBuilder();
            descriptionBuilder.AppendLine(
                $"Exported from Survey Solutions Headquarters {this.productVersion} on {DateTime.Today:D}");

            foreach (var level in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                string fileName = $"{level.LevelName}{dataFilesExtension}";
                var variables = level.HeaderItems.Values.Select(question => question.VariableName);

                descriptionBuilder.AppendLine();
                descriptionBuilder.AppendLine(fileName);
                descriptionBuilder.AppendLine(string.Join(", ", variables));
            }

            this.fileSystemAccessor.WriteAllText(
                Path.Combine(basePath, "export__readme.txt"),
                descriptionBuilder.ToString());
        }

        public void ExportInterviewsInTabularFormat(QuestionnaireIdentity questionnaireIdentity, InterviewStatus? status, string basePath, IProgress<int> progress, CancellationToken cancellationToken, DateTime? fromDate, DateTime? toDate)
        {
            QuestionnaireExportStructure questionnaireExportStructure = this.GetQuestionnaireExportStructure(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);

            var exportInterviewsProgress = new Progress<int>();
            var exportCommentsProgress = new Progress<int>();
            var exportInterviewActionsProgress = new Progress<int>();

            ProggressAggregator proggressAggregator = new ProggressAggregator();
            proggressAggregator.Add(exportInterviewsProgress, 0.8);
            proggressAggregator.Add(exportCommentsProgress, 0.1);
            proggressAggregator.Add(exportInterviewActionsProgress, 0.1);

            proggressAggregator.ProgressChanged += (sender, overallProgress) => progress.Report(overallProgress);

            var interviewsToExport = GetInterviewsToExport(questionnaireIdentity, status, cancellationToken, fromDate, toDate).ToList();
            var interviewIdsToExport = interviewsToExport.Select(x => x.Id).ToList();

            Stopwatch exportWatch = Stopwatch.StartNew();

            Task.WaitAll(new[] {
                Task.Run(() => this.commentsExporter.Export(questionnaireExportStructure, interviewIdsToExport, basePath, exportCommentsProgress), cancellationToken),
                Task.Run(() => this.interviewActionsExporter.Export(questionnaireIdentity, interviewIdsToExport, basePath, exportInterviewActionsProgress), cancellationToken),
                Task.Run(() => this.interviewsExporter.Export(questionnaireExportStructure, interviewsToExport, basePath, exportInterviewsProgress, cancellationToken), cancellationToken),
                Task.Run(() => this.diagnosticsExporter.Export(interviewIdsToExport, basePath, exportInterviewsProgress, cancellationToken), cancellationToken),
            }, cancellationToken);

            exportWatch.Stop();

            this.logger.Info($"Export with all steps finished for questionnaire {questionnaireIdentity}. " +
                             $"Took {exportWatch.Elapsed:c} to export {interviewIdsToExport.Count} interviews");
        }

        public IEnumerable<InterviewToExport> GetInterviewsToExport(QuestionnaireIdentity questionnaireIdentity,
            InterviewStatus? status, CancellationToken cancellationToken, DateTime? fromDate, DateTime? toDate)
        {
            string lastRecivedId = null;
            var skipInterviewsCount = 0;
            List<InterviewToExport> batchInterviews;

            var stopwatch = Stopwatch.StartNew();

            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                batchInterviews = this.interviewSummaries.Query(_ => this.Filter(_, questionnaireIdentity, status, fromDate, toDate)
                        .OrderBy(x => x.InterviewId)
                        .Where(x => lastRecivedId == null || x.SummaryId.CompareTo(lastRecivedId) > 0)
                        .Select(x => new InterviewToExport(x.InterviewId, x.Key, x.ErrorsCount, x.Status))
                        .Take(this.exportSettings.InterviewIdsQueryBatchSize)
                        .ToList());

                if (batchInterviews.Count > 0)
                    lastRecivedId = batchInterviews.Last().Id.FormatGuid();

                skipInterviewsCount += batchInterviews.Count;
                this.logger.Debug($"Received {skipInterviewsCount:n0} interview ids.");

                foreach (var interview in batchInterviews)
                    yield return interview;

            } while (batchInterviews.Count > 0);

            stopwatch.Stop();

            this.logger.Info($"Received {skipInterviewsCount:N0} interviewIds to start export. " +
                             $"Took {stopwatch.Elapsed:g} to complete.");
        }

        private IQueryable<InterviewSummary> Filter(IQueryable<InterviewSummary> queryable,
            QuestionnaireIdentity questionnaireIdentity, InterviewStatus? status, DateTime? fromDate, DateTime? toDate)
        {
            queryable = queryable.Where(x => x.QuestionnaireId == questionnaireIdentity.QuestionnaireId &&
                                             x.QuestionnaireVersion == questionnaireIdentity.Version);

            if (status.HasValue)
            {
                var filteredByStatus = status.Value;
                queryable = queryable.Where(x => x.Status == filteredByStatus);
            }

            if (fromDate.HasValue)
            {
                var filteredFromDate = fromDate.Value;
                queryable = queryable.Where(x => x.UpdateDate >= filteredFromDate);
            }

            if(toDate.HasValue)
            {
                var filteredToDate = toDate.Value;
                queryable = queryable.Where(x => x.UpdateDate < filteredToDate);
            }

            return queryable;
        }

        public void CreateHeaderStructureForPreloadingForQuestionnaire(QuestionnaireIdentity questionnaireIdentity, string basePath)
        {
            QuestionnaireExportStructure questionnaireExportStructure =
                this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(questionnaireIdentity);

            if (questionnaireExportStructure == null)
                return;

            this.CreateDataSchemaForInterviewsInTabular(questionnaireExportStructure, basePath);
        }

        public string[] GetTabularDataFilesFromFolder(string basePath)
        {
            var filesInDirectory = this.fileSystemAccessor.GetFilesInDirectory(basePath).Where(fileName => fileName.EndsWith("." + this.dataFileExtension)).ToArray();
            return filesInDirectory;
        }

        private void CreateDataSchemaForInterviewsInTabular(QuestionnaireExportStructure questionnaireExportStructure, string basePath)
        {
            foreach (var level in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                var dataByTheLevelFilePath =
                    this.fileSystemAccessor.CombinePath(basePath, Path.ChangeExtension(level.LevelName, this.dataFileExtension));

                var interviewLevelHeader = new List<string> {level.LevelIdColumnName};


                if (level.IsTextListScope)
                {
                    interviewLevelHeader.AddRange(level.ReferencedNames);
                }

                foreach (IExportedHeaderItem headerItem in level.HeaderItems.Values)
                {
                    interviewLevelHeader.AddRange(headerItem.ColumnHeaders.Select(x=> x.Name));
                }

                if (level.LevelScopeVector.Length == 0)
                {
                    interviewLevelHeader.AddRange(ServiceColumns.SystemVariables.Values.Select(systemVariable => systemVariable.VariableExportColumnName));
                }

                interviewLevelHeader.AddRange(questionnaireExportStructure.GetAllParentColumnNamesForLevel(level.LevelScopeVector));

                this.csvWriter.WriteData(dataByTheLevelFilePath, new[] { interviewLevelHeader.ToArray() }, ExportFileSettings.DataFileSeparator.ToString());
            }
        }

        private QuestionnaireExportStructure GetQuestionnaireExportStructure(Guid questionnaireId, long questionnaireVersion)
            => this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(
                new QuestionnaireIdentity(questionnaireId, questionnaireVersion));
    }
}
