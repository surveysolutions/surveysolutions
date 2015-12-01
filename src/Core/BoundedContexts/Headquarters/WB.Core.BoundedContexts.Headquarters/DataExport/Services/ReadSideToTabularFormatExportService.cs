using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveySolutions.Implementation.ServiceVariables;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class ReadSideToTabularFormatExportService : ITabularFormatExportService
    {
        private readonly string dataFileExtension = "tab";

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICsvWriter csvWriter;

        private readonly ITransactionManager transactionManager;
        private readonly CommentsExporter commentsExporter;
        private readonly InterviewActionsExporter interviewActionsExporter;
        private readonly InterviewsExporter interviewsExporter;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage;

        public ReadSideToTabularFormatExportService(IFileSystemAccessor fileSystemAccessor,
            ICsvWriter csvWriter,
            InterviewDataExportSettings interviewDataExportSettings,
            IQueryableReadSideRepositoryReader<InterviewStatuses> interviewActionsDataStorage,
            IQueryableReadSideRepositoryReader<InterviewCommentaries> interviewCommentariesStorage, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries, 
            IReadSideKeyValueStorage<InterviewData> interviewDatas, 
            IExportViewFactory exportViewFactory, 
            ITransactionManager transactionManager, 
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriter = csvWriter;
            this.transactionManager = transactionManager;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;

            this.interviewsExporter = new InterviewsExporter(transactionManager, 
                interviewSummaries, 
                fileSystemAccessor, 
                interviewDatas, 
                exportViewFactory,
                interviewDataExportSettings, 
                csvWriter);

            this.commentsExporter = new CommentsExporter(interviewDataExportSettings, 
                fileSystemAccessor, 
                csvWriter, 
                interviewCommentariesStorage,
                transactionManager);

            this.interviewActionsExporter = new InterviewActionsExporter(interviewDataExportSettings, fileSystemAccessor, csvWriter, transactionManager, interviewActionsDataStorage);
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

            this.interviewsExporter.ExportAll(questionnaireExportStructure, basePath, exportInterviewsProgress, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            this.commentsExporter.ExportAll(questionnaireExportStructure, basePath, exportCommentsProgress);
            cancellationToken.ThrowIfCancellationRequested();
            this.interviewActionsExporter.ExportAll(questionnaireIdentity, basePath, exportInterviewActionsProgress);
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
              this.transactionManager
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
             this.transactionManager
                 .ExecuteInQueryTransaction(() =>
                         this.questionnaireExportStructureStorage.AsVersioned()
                             .Get(questionnaireId.FormatGuid(), questionnaireVersion));
            return questionnaireExportStructure;
        }
    }
}