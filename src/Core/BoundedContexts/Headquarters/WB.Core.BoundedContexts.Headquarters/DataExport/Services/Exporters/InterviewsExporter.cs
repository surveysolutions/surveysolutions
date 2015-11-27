using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveySolutions.Implementation.ServiceVariables;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    public class InterviewsExporter
    {
        private readonly string dataFileExtension = "tab";

        private readonly ITransactionManager transactionManager;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries;
        private readonly IReadSideKeyValueStorage<InterviewData> interviewDatas;
        private readonly IExportViewFactory exportViewFactory;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly ICsvWriter csvWriter;

        public InterviewsExporter(ITransactionManager transactionManager, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries, 
            IFileSystemAccessor fileSystemAccessor, 
            IReadSideKeyValueStorage<InterviewData> interviewDatas, 
            IExportViewFactory exportViewFactory,
            InterviewDataExportSettings interviewDataExportSettings,
            ICsvWriter csvWriter)
        {
            this.transactionManager = transactionManager;
            this.interviewSummaries = interviewSummaries;
            this.fileSystemAccessor = fileSystemAccessor;
            this.interviewDatas = interviewDatas;
            this.exportViewFactory = exportViewFactory;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.csvWriter = csvWriter;
        }

        public void ExportAll(QuestionnaireExportStructure questionnaireExportStructure,
            string basePath, 
            IProgress<int> progress)
        {
            Expression<Func<InterviewSummary, bool>> expression = x => x.QuestionnaireId == questionnaireExportStructure.QuestionnaireId &&
                                         x.QuestionnaireVersion == questionnaireExportStructure.Version &&
                                         !x.IsDeleted;

            this.Export(questionnaireExportStructure, basePath, progress, expression);
        }

        public void ExportApproved(QuestionnaireExportStructure questionnaireExportStructure,
            string basePath, 
            IProgress<int> progress)
        {
            Expression<Func<InterviewSummary, bool>> expression = x => x.QuestionnaireId == questionnaireExportStructure.QuestionnaireId && 
                                         x.QuestionnaireVersion == questionnaireExportStructure.Version &&
                                         !x.IsDeleted &&
                                         x.Status == InterviewStatus.ApprovedByHeadquarters;

            this.Export(questionnaireExportStructure, basePath, progress, expression);
        }

        private void Export(QuestionnaireExportStructure questionnaireExportStructure, string basePath, IProgress<int> progress,
            Expression<Func<InterviewSummary, bool>> expression)
        {
            int totalInterviewsToExport =
                this.transactionManager.ExecuteInQueryTransaction(() => this.interviewSummaries.Query(_ => _.Count(expression)));

            int processedCount = 0;
            while (processedCount < totalInterviewsToExport)
            {
                var localProcessedCount = processedCount;
                List<Guid> interviewIdsToExport = this.transactionManager.ExecuteInQueryTransaction(() =>
                    this.interviewSummaries.Query(_ => _
                        .Where(expression)
                        .OrderBy(x => x.InterviewId)
                        .Select(x => x.InterviewId)
                        .Skip(localProcessedCount)
                        .Take(this.interviewDataExportSettings.MaxRecordsCountPerOneExportQuery)
                        .ToList()));
                if (interviewIdsToExport.Count == 0)
                    break;

                this.DoExport(questionnaireExportStructure, basePath, interviewIdsToExport);
                processedCount += interviewIdsToExport.Count;
                progress.Report(processedCount.PercentOf(totalInterviewsToExport));
            }
        }

        private void DoExport(QuestionnaireExportStructure questionnaireExportStructure, 
            string basePath, 
            List<Guid> interviewIdsToExport)
        {
            
            this.CreateDataSchemaForInterviewsInTabular(questionnaireExportStructure, basePath);
            this.ExportInterviews(interviewIdsToExport, basePath, questionnaireExportStructure);
        }

        private void CreateDataSchemaForInterviewsInTabular(QuestionnaireExportStructure questionnaireExportStructure, string basePath)
        {
            foreach (var level in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                string dataByTheLevelFilePath =
                    this.fileSystemAccessor.CombinePath(basePath, CreateFormatDataFileName(level.LevelName));

                List<string> interviewLevelHeader = new List<string> { level.LevelIdColumnName };

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

        private void ExportInterviews(List<Guid> interviewIdsToExport,
                                      string basePath,
                                      QuestionnaireExportStructure questionnaireExportStructure)
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

                foreach (var levelName in exportedData.Data.Keys)
                {
                    foreach (var dataByLevel in exportedData.Data[levelName])
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
                    var dataByTheLevelFilePath = this.fileSystemAccessor.CombinePath(basePath, CreateFormatDataFileName(level.LevelName));

                    if (result.ContainsKey(level.LevelName))
                    {
                        this.csvWriter.WriteData(dataByTheLevelFilePath, result[level.LevelName], ExportFileSettings.SeparatorOfExportedDataFile.ToString());
                    }
                }

                totalInterviewsProcessed++;
            }
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
                Data = interviewData,
            };

            return interviewExportedData;
        }

        private string CreateFormatDataFileName(string fileName)
        {
            return String.Format("{0}.{1}", fileName, dataFileExtension);
        }
    }
}