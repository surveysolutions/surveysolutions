﻿using System;
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
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
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
        private readonly ISerializer serializer;
        private readonly ICsvWriter csvWriter;

        public InterviewsExporter(ITransactionManager transactionManager, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries, 
            IFileSystemAccessor fileSystemAccessor, 
            IReadSideKeyValueStorage<InterviewData> interviewDatas, 
            IExportViewFactory exportViewFactory, 
            ISerializer serializer, 
            ICsvWriter csvWriter)
        {
            this.transactionManager = transactionManager;
            this.interviewSummaries = interviewSummaries;
            this.fileSystemAccessor = fileSystemAccessor;
            this.interviewDatas = interviewDatas;
            this.exportViewFactory = exportViewFactory;
            this.serializer = serializer;
            this.csvWriter = csvWriter;
        }


        public void ExportAll(QuestionnaireExportStructure questionnaireExportStructure,
            string basePath, IProgress<int> progress)
        {
            List<Guid> interviewIdsToExport =
             this.transactionManager.ExecuteInQueryTransaction(() =>
                 this.interviewSummaries.Query(_ =>
                         _.Where(x => x.QuestionnaireId == questionnaireExportStructure.QuestionnaireId &&
                                      x.QuestionnaireVersion == questionnaireExportStructure.Version)
                             .OrderBy(x => x.InterviewId)
                             .Select(x => x.InterviewId).ToList()));
            DoExport(questionnaireExportStructure, basePath, progress, interviewIdsToExport);
        }

        public void ExportApproved(QuestionnaireExportStructure questionnaireExportStructure,
            string basePath, 
            IProgress<int> progress)
        {
            List<Guid> interviewIdsToExport =
               this.transactionManager.ExecuteInQueryTransaction(() =>
                   this.interviewSummaries.Query(_ => _.Where(x => x.QuestionnaireId == questionnaireExportStructure.QuestionnaireId && 
                                                                   x.QuestionnaireVersion == questionnaireExportStructure.Version)
                                               .Where(x => x.Status == InterviewStatus.ApprovedByHeadquarters)
                                               .OrderBy(x => x.InterviewId)
                                               .Select(x => x.InterviewId).ToList()));

            DoExport(questionnaireExportStructure, basePath, progress, interviewIdsToExport);
        }

        private void DoExport(QuestionnaireExportStructure questionnaireExportStructure, 
            string basePath, 
            IProgress<int> progress, 
            List<Guid> interviewIdsToExport)
        {
            
            this.CreateDataSchemaForInterviewsInTabular(questionnaireExportStructure, basePath);
            this.ExportInterviews(interviewIdsToExport, basePath, questionnaireExportStructure, progress);
        }

        private void CreateDataSchemaForInterviewsInTabular(QuestionnaireExportStructure questionnaireExportStructure, string basePath)
        {
            foreach (var level in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                var dataByTheLevelFilePath =
                    this.fileSystemAccessor.CombinePath(basePath, Path.ChangeExtension(level.LevelName, this.dataFileExtension));

                var interviewLevelHeader = new List<string> { level.LevelIdColumnName };


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
                        this.csvWriter.WriteData(dataByTheLevelFilePath, result[level.LevelName], ExportFileSettings.SeparatorOfExportedDataFile.ToString());
                    }
                }

                totalInterviewsProcessed++;
                progress.Report(totalInterviewsProcessed.PercentOf(interviewIdsToExport.Count));
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
                Data = this.serializer.SerializeToByteArray(interviewData),
            };

            return interviewExportedData;
        }
    }
}