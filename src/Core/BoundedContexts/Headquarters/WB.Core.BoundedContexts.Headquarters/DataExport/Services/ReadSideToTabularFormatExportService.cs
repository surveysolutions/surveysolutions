using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
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
        private const double InterviewsExportProgressModifier = 0.8;
        private readonly string dataFileExtension = "tab";

        private readonly string separator;

        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICsvWriter csvWriter;
        private readonly ISerializer serializer;

        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage;

        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries;
        private readonly IReadSideKeyValueStorage<InterviewData> interviewDatas;
        private readonly IExportViewFactory exportViewFactory;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage;
        private readonly ITransactionManager transactionManager;
        private readonly CommentsExporter commentsExporter;
        private readonly InterviewActionsExporter interviewActionsExporter;

        public ReadSideToTabularFormatExportService(
            ITransactionManagerProvider transactionManagerProvider,
            IFileSystemAccessor fileSystemAccessor,
            ICsvWriter csvWriter,
            ISerializer serializer,
            InterviewDataExportSettings interviewDataExportSettings,
            IQueryableReadSideRepositoryReader<InterviewStatuses> interviewActionsDataStorage,
            IQueryableReadSideRepositoryReader<InterviewCommentaries> interviewCommentariesStorage,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries, 
            IReadSideKeyValueStorage<InterviewData> interviewDatas, 
            IExportViewFactory exportViewFactory, 
            ITransactionManager transactionManager, 
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage)
        {
            this.transactionManagerProvider = transactionManagerProvider;
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriter = csvWriter;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.interviewSummaries = interviewSummaries;
            this.interviewDatas = interviewDatas;
            this.exportViewFactory = exportViewFactory;
            this.transactionManager = transactionManager;
            this.questionnaireDocumentVersionedStorage = questionnaireDocumentVersionedStorage;
            this.serializer = serializer;

            this.separator = ExportFileSettings.SeparatorOfExportedDataFile.ToString();

            this.commentsExporter = new CommentsExporter(interviewDataExportSettings, 
                fileSystemAccessor, 
                csvWriter, 
                interviewCommentariesStorage,
                transactionManager);

            this.interviewActionsExporter = new InterviewActionsExporter(interviewDataExportSettings, fileSystemAccessor, csvWriter, transactionManager, interviewActionsDataStorage);
        }

        public void ExportInterviewsInTabularFormatAsync(QuestionnaireIdentity questionnaireIdentity, string basePath, IProgress<int> progress)
        {
            QuestionnaireExportStructure questionnaireExportStructure = this.BuildQuestionnaireExportStructure(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);

            List<Guid> interviewIdsToExport =
                this.transactionManager.ExecuteInQueryTransaction(() =>
                    this.interviewSummaries.Query(_ =>
                            _.Where(x => x.QuestionnaireId == questionnaireIdentity.QuestionnaireId && x.QuestionnaireVersion == questionnaireIdentity.Version)
                                .OrderBy(x => x.InterviewId)
                                .Select(x => x.InterviewId).ToList()));

            this.CreateDataSchemaForInterviewsInTabular(questionnaireExportStructure, basePath);
            this.ExportInterviews(interviewIdsToExport, basePath, questionnaireExportStructure, progress);

            this.commentsExporter.ExportAll(questionnaireExportStructure, basePath, progress);
            this.interviewActionsExporter.ExportAll(questionnaireIdentity, basePath, progress);
        }

        private void ExportInterviews(List<Guid> interviewIdsToExport, 
            string basePath, 
            QuestionnaireExportStructure questionnaireExportStructure, 
            IProgress<int> progress)
        {
            int totalInterviewsProcessed = 0;
            foreach (var interviewId in interviewIdsToExport)
            {
                var interviewData = this.transactionManager.ExecuteInQueryTransaction(() => this.interviewDatas.GetById(interviewId));

                InterviewDataExportView interviewExportStructure =
                    this.exportViewFactory.CreateInterviewDataExportView(questionnaireExportStructure,
                        interviewData); 

                InterviewExportedDataRecord exportedData = this.CreateInterviewExportedData(
                    interviewExportStructure, questionnaireExportStructure.QuestionnaireId, questionnaireExportStructure.Version);

                var result = new Dictionary<string, List<string[]>>();
                Dictionary<string, string[]> deserializedExportedData =
                    this.serializer.Deserialize<Dictionary<string, string[]>>(exportedData.Data);
                foreach (var levelName in deserializedExportedData.Keys)
                {
                    foreach (var dataByLevel in deserializedExportedData[levelName])
                    {
                        if (!result.ContainsKey(levelName))
                        {
                            result.Add(levelName, new List<string[]>());
                        }

                        result[levelName].Add(dataByLevel.Split(ExportFileSettings.SeparatorOfExportedDataFile));
                    }
                }

                foreach (var level in questionnaireExportStructure.HeaderToLevelMap.Values)
                {
                    var dataByTheLevelFilePath = this.fileSystemAccessor.CombinePath(basePath, Path.ChangeExtension(level.LevelName, this.dataFileExtension));

                    if (result.ContainsKey(level.LevelName))
                    {
                        this.csvWriter.WriteData(dataByTheLevelFilePath, result[level.LevelName], this.separator);
                    }
                }

                totalInterviewsProcessed++;
                progress.Report((int) (totalInterviewsProcessed.PercentOf(interviewIdsToExport.Count) * InterviewsExportProgressModifier));
            }
        }

        public void ExportApprovedInterviewsInTabularFormatAsync(QuestionnaireIdentity questionnaireIdentity, string basePath, IProgress<int> progress)
        {
            QuestionnaireExportStructure questionnaireExportStructure = this.BuildQuestionnaireExportStructure(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
            List<Guid> interviewIdsToExport =
                this.transactionManager.ExecuteInQueryTransaction(() =>
                    this.interviewSummaries.Query(_ => _.Where(x => x.QuestionnaireId == questionnaireIdentity.QuestionnaireId && x.QuestionnaireVersion == questionnaireIdentity.Version)
                                                .Where(x => x.Status == InterviewStatus.ApprovedByHeadquarters)
                                                .OrderBy(x => x.InterviewId)
                                                .Select(x => x.InterviewId).ToList()));

            this.CreateDataSchemaForInterviewsInTabular(questionnaireExportStructure, basePath);
            this.ExportInterviews(interviewIdsToExport, basePath, questionnaireExportStructure, progress);

            this.commentsExporter.ExportApproved(questionnaireExportStructure, basePath, progress);
            this.interviewActionsExporter.ExportApproved(questionnaireIdentity, basePath, progress);
        }

        public void CreateHeaderStructureForPreloadingForQuestionnaire(QuestionnaireIdentity questionnaireIdentity, string basePath)
        {
            QuestionnaireExportStructure questionnaireExportStructure =
              this.transactionManagerProvider.GetTransactionManager()
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

                this.csvWriter.WriteData(dataByTheLevelFilePath, new[] { interviewLevelHeader.ToArray() }, this.separator);
            }
        }

        private QuestionnaireExportStructure BuildQuestionnaireExportStructure(Guid questionnaireId, long questionnaireVersion)
        {
            QuestionnaireDocumentVersioned questionnaire =
                this.transactionManager.ExecuteInQueryTransaction(() => 
                    this.questionnaireDocumentVersionedStorage.AsVersioned()
                        .Get(questionnaireId.FormatGuid(), questionnaireVersion));
            var questionnaireExportStructure =
                this.exportViewFactory.CreateQuestionnaireExportStructure(questionnaire.Questionnaire, questionnaireVersion);
            return questionnaireExportStructure;
        }

        private InterviewExportedDataRecord CreateInterviewExportedData(InterviewDataExportView interviewDataExportView, Guid questionnaireId, long questionnaireVersion)
        {
            var interviewData = new Dictionary<string, string[]>();

            var stringSeparator = ExportFileSettings.SeparatorOfExportedDataFile.ToString();
            foreach (var interviewDataExportLevelView in interviewDataExportView.Levels)
            {
                var recordsByLevel = new List<string>();
                foreach (var interviewDataExportRecord in interviewDataExportLevelView.Records)
                {
                    var parametersToConcatenate = new List<string> { interviewDataExportRecord.RecordId };

                    parametersToConcatenate.AddRange(interviewDataExportRecord.ReferenceValues);

                    foreach (var exportedQuestion in interviewDataExportRecord.Questions)
                    {
                        parametersToConcatenate.AddRange(exportedQuestion.Answers.Select(itemValue => string.IsNullOrEmpty(itemValue) ? "" : itemValue));
                    }

                    parametersToConcatenate.AddRange(interviewDataExportRecord.SystemVariableValues);
                    parametersToConcatenate.AddRange(interviewDataExportRecord.ParentRecordIds);

                    recordsByLevel.Add(string.Join(stringSeparator,
                            parametersToConcatenate.Select(v => v.Replace(stringSeparator, ""))));
                }
                interviewData.Add(interviewDataExportLevelView.LevelName, recordsByLevel.ToArray());
            }
            var interviewExportedData = new InterviewExportedDataRecord
            {
                InterviewId = interviewDataExportView.InterviewId.FormatGuid(),
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                Data = this.serializer.SerializeToByteArray(interviewData),
            };

            return interviewExportedData;
        }
    }
}