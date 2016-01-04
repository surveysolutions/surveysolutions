using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using StatData.Core;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class TabularDataToExternalStatPackageExportService : ITabularDataToExternalStatPackageExportService
    {
        private readonly ITransactionManagerProvider transactionManager;
        private readonly IFileSystemAccessor fileSystemAccessor;

        private readonly ILogger logger;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureWriter;
        private readonly ITabFileReader tabReader;
        private readonly IDatasetWriterFactory datasetWriterFactory;
        private readonly IDataQueryFactory dataQueryFactory;

        private readonly IQuestionnaireLabelFactory questionnaireLabelFactory;

        public TabularDataToExternalStatPackageExportService(
            IFileSystemAccessor fileSystemAccessor,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureWriter,
            ITransactionManagerProvider transactionManager,
            ILogger logger,
            ITabFileReader tabReader,
            IDataQueryFactory dataQueryFactory,
            IDatasetWriterFactory datasetWriterFactory, IQuestionnaireLabelFactory questionnaireLabelFactory)
        {
            this.questionnaireExportStructureWriter = questionnaireExportStructureWriter;
            this.transactionManager = transactionManager;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;

            this.tabReader = tabReader;
            this.datasetWriterFactory = datasetWriterFactory;
            this.questionnaireLabelFactory = questionnaireLabelFactory;
            this.dataQueryFactory = dataQueryFactory;
        }

        private string StataFileNameExtension { get { return ".dta"; } }
        private string SpssFileNameExtension { get { return ".sav"; } }

        public string[] CreateAndGetStataDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string[] tabularDataFiles, IProgress<int> progress, CancellationToken cancellationToken)
        {
            return this.CreateAndGetExportDataFiles(questionnaireId, questionnaireVersion, DataExportFormat.STATA, tabularDataFiles, progress, cancellationToken);
        }

        public string[] CreateAndGetSpssDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string[] tabularDataFiles, IProgress<int> progress, CancellationToken cancellationToken)
        {
            return this.CreateAndGetExportDataFiles(questionnaireId, questionnaireVersion, DataExportFormat.SPSS, tabularDataFiles, progress, cancellationToken);
        }


        private string[] CreateAndGetExportDataFiles(Guid questionnaireId, long questionnaireVersion, DataExportFormat format, string[] dataFiles, IProgress<int> progress, CancellationToken cancellationToken)
        {
            string currentDataInfo = string.Empty;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var questionnaireExportStructure =
                    this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(() =>
                        this.questionnaireExportStructureWriter.AsVersioned()
                            .Get(questionnaireId.FormatGuid(), questionnaireVersion));

                if (questionnaireExportStructure == null)
                    return new string[0];

                var result = new List<string>();
                string fileExtention = format == DataExportFormat.STATA
                    ? this.StataFileNameExtension
                    : this.SpssFileNameExtension;
                var writer = this.datasetWriterFactory.CreateDatasetWriter(format);
                int processdFiles = 0;

                foreach (var tabFile in dataFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    currentDataInfo = $"filename: {tabFile}";
                    string dataFilePath = this.fileSystemAccessor.ChangeExtension(tabFile, fileExtention);

                    var meta = this.tabReader.GetMetaFromTabFile(tabFile);

                    var levelName = this.fileSystemAccessor.GetFileNameWithoutExtension(tabFile);

                    var questionnaireLevel =
                        questionnaireExportStructure.HeaderToLevelMap.Values.FirstOrDefault(
                            x => string.Equals(x.LevelName, levelName, StringComparison.InvariantCultureIgnoreCase));

                    if (questionnaireLevel != null)
                        UpdateMetaWithLabels(meta,
                            this.questionnaireLabelFactory.CreateLabelsForQuestionnaireLevel(
                                questionnaireExportStructure, questionnaireLevel.LevelScopeVector));

                    using (IDataQuery tabStreamDataQuery = dataQueryFactory.CreateDataQuery(tabFile))
                    {
                        writer.WriteToFile(dataFilePath, meta, tabStreamDataQuery);
                    }
                    result.Add(dataFilePath);

                    processdFiles++;
                    progress.Report(processdFiles.PercentOf(dataFiles.Length));

                }
                return result.ToArray();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exc)
            {
                this.logger.Error($"Error on data export (questionnaireId:{questionnaireId}, questionnaireVersion:{questionnaireVersion}): ", exc);
                this.logger.Error(currentDataInfo);
            }

            return new string[0];
        }

        private static void UpdateMetaWithLabels(IDatasetMeta meta, QuestionnaireLabels questionnaireLabels)
        {
            foreach (var datasetVariable in meta.Variables)
            {
                if (!questionnaireLabels.ContainsVariable(datasetVariable.VarName))
                    continue;

                var variableLabels = questionnaireLabels[datasetVariable.VarName];

                datasetVariable.VarLabel = variableLabels.Label;

                var valueSet = new ValueSet();

                foreach (var variableValueLabel in variableLabels.VariableValueLabels)
                {
                    double value;
                    if (double.TryParse(variableValueLabel.Value, out value))
                        valueSet.Add(value, variableValueLabel.Label);
                }

                meta.AssociateValueSet(datasetVariable.VarName, valueSet);
            }
        }
    }
}
