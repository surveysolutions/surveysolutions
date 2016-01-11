using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class ReadSideToTabularFormatExportService : ITabularFormatExportService
    {
        private readonly string dataFileExtension = "tab";

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICsvWriter csvWriter;
        private readonly ILogger logger;

        private readonly ITransactionManagerProvider transactionManager;
        private readonly CommentsExporter commentsExporter;
        private readonly InterviewActionsExporter interviewActionsExporter;
        private readonly InterviewsExporter interviewsExporter;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage;

        public ReadSideToTabularFormatExportService(IFileSystemAccessor fileSystemAccessor,
            ICsvWriter csvWriter, 
            ILogger logger,
            ITransactionManagerProvider transactionManager, 
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriter = csvWriter;
            this.logger = logger;
            this.transactionManager = transactionManager;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;

            this.interviewsExporter = ServiceLocator.Current.GetInstance<InterviewsExporter>();

            this.commentsExporter = ServiceLocator.Current.GetInstance<CommentsExporter>();

            this.interviewActionsExporter = ServiceLocator.Current.GetInstance<InterviewActionsExporter>();
        }

        public void ExportInterviewsInTabularFormat(QuestionnaireIdentity questionnaireIdentity, string basePath, IProgress<int> progress, CancellationToken cancellationToken)
        {
            QuestionnaireExportStructure questionnaireExportStructure = this.BuildQuestionnaireExportStructure(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);

            var exportInterviewsProgress = new Progress<int>();
            var exportCommentsProgress = new Progress<int>();
            var exportInterviewActionsProgress = new Progress<int>();

            ProggressAggregator proggressAggregator = new ProggressAggregator();
            proggressAggregator.Add(exportInterviewsProgress, 0.8);
            proggressAggregator.Add(exportCommentsProgress, 0.1);
            proggressAggregator.Add(exportInterviewActionsProgress, 0.1);

            proggressAggregator.ProgressChanged += (sender, overallProgress) => progress.Report(overallProgress);

            Stopwatch exportWatch = new Stopwatch();
            exportWatch.Start(); 
            Task.WaitAll(new[] {
                Task.Run(() => this.interviewsExporter.ExportAll(questionnaireExportStructure, basePath, exportInterviewsProgress, cancellationToken), cancellationToken),
                Task.Run(() => this.commentsExporter.ExportAll(questionnaireExportStructure, basePath, exportCommentsProgress), cancellationToken),
                Task.Run(() => this.interviewActionsExporter.ExportAll(questionnaireIdentity, basePath, exportInterviewActionsProgress), cancellationToken)
            }, cancellationToken);
            exportWatch.Stop();

            this.logger.Info($"Export with all steps (Interviews, Comments, Actions) finished for questionnaire {questionnaireIdentity}. Took {exportWatch.Elapsed:c}");
        }

        public void ExportApprovedInterviewsInTabularFormat(QuestionnaireIdentity questionnaireIdentity, string basePath, IProgress<int> progress, CancellationToken cancellationToken)
        {
            QuestionnaireExportStructure questionnaireExportStructure = this.BuildQuestionnaireExportStructure(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);

            var exportInterviewsProgress = new Progress<int>();
            var exportCommentsProgress = new Progress<int>();
            var exportInterviewActionsProgress = new Progress<int>();

            ProggressAggregator proggressAggregator = new ProggressAggregator();
            proggressAggregator.Add(exportInterviewsProgress, 0.8);
            proggressAggregator.Add(exportCommentsProgress, 0.1);
            proggressAggregator.Add(exportInterviewActionsProgress, 0.1);

            proggressAggregator.ProgressChanged += (sender, overallProgress) => progress.Report(overallProgress);

            this.interviewsExporter.ExportApproved(questionnaireExportStructure, basePath, exportInterviewsProgress, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            this.commentsExporter.ExportApproved(questionnaireExportStructure, basePath, exportCommentsProgress);
            cancellationToken.ThrowIfCancellationRequested();
            this.interviewActionsExporter.ExportApproved(questionnaireIdentity, basePath, exportInterviewActionsProgress);
        }

        public void CreateHeaderStructureForPreloadingForQuestionnaire(QuestionnaireIdentity questionnaireIdentity, string basePath)
        {
            QuestionnaireExportStructure questionnaireExportStructure =
              this.transactionManager.GetTransactionManager()
                  .ExecuteInQueryTransaction(
                      () =>
                          this.questionnaireExportStructureStorage.AsVersioned()
                              .Get(questionnaireIdentity.QuestionnaireId.FormatGuid(), questionnaireIdentity.Version));

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

                foreach (ExportedHeaderItem question in level.HeaderItems.Values)
                {
                    interviewLevelHeader.AddRange(question.ColumnNames);
                }

                if (level.LevelScopeVector.Length == 0)
                {
                    interviewLevelHeader.AddRange(ServiceColumns.SystemVariables.Select(systemVariable => systemVariable.VariableExportColumnName));
                }

                for (int i = 0; i < level.LevelScopeVector.Length; i++)
                {
                    interviewLevelHeader.Add($"{ServiceColumns.ParentId}{i + 1}");
                }

                this.csvWriter.WriteData(dataByTheLevelFilePath, new[] { interviewLevelHeader.ToArray() }, ExportFileSettings.SeparatorOfExportedDataFile.ToString());
            }
        }

        private QuestionnaireExportStructure BuildQuestionnaireExportStructure(Guid questionnaireId, long questionnaireVersion)
        {
            QuestionnaireExportStructure questionnaireExportStructure =
             this.transactionManager.GetTransactionManager()
                 .ExecuteInQueryTransaction(() =>
                         this.questionnaireExportStructureStorage.AsVersioned()
                             .Get(questionnaireId.FormatGuid(), questionnaireVersion));
            return questionnaireExportStructure;
        }
    }
}