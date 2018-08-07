using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using StatData.Core;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class TabularDataToExternalStatPackageExportService : ITabularDataToExternalStatPackageExportService
    {
        private readonly ITransactionManagerProvider transactionManager;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ILogger logger;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly ITabFileReader tabReader;
        private readonly IDatasetWriterFactory datasetWriterFactory;
        private readonly IDataQueryFactory dataQueryFactory;

        private readonly IQuestionnaireLabelFactory questionnaireLabelFactory;

        private readonly IExportSeviceDataProvider exportSeviceDataProvider;

        public TabularDataToExternalStatPackageExportService(
            IFileSystemAccessor fileSystemAccessor,
            ITransactionManagerProvider transactionManager,
            ILogger logger,
            ITabFileReader tabReader,
            IDataQueryFactory dataQueryFactory,
            IDatasetWriterFactory datasetWriterFactory, 
            IQuestionnaireLabelFactory questionnaireLabelFactory,
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage,
            IExportSeviceDataProvider exportSeviceDataProvider)
        {
            this.transactionManager = transactionManager;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;

            this.tabReader = tabReader;
            this.datasetWriterFactory = datasetWriterFactory;
            this.questionnaireLabelFactory = questionnaireLabelFactory;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.dataQueryFactory = dataQueryFactory;
            this.exportSeviceDataProvider = exportSeviceDataProvider;

        }

        private const string StataFileNameExtension = ExportFileSettings.StataDataFileExtension;
        private const string SpssFileNameExtension = ExportFileSettings.SpssDataFileExtension;

        public string[] CreateAndGetStataDataFilesForQuestionnaire(Guid questionnaireId, 
            long questionnaireVersion,
            string[] tabularDataFiles, 
            IProgress<int> progress, 
            CancellationToken cancellationToken)
        {
            return this.CreateAndGetExportDataFiles(questionnaireId, questionnaireVersion, DataExportFormat.STATA, tabularDataFiles, progress, cancellationToken);
        }

        public string[] CreateAndGetSpssDataFilesForQuestionnaire(Guid questionnaireId, 
            long questionnaireVersion, 
            string[] tabularDataFiles, 
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            return this.CreateAndGetExportDataFiles(questionnaireId, questionnaireVersion, DataExportFormat.SPSS, tabularDataFiles, progress, cancellationToken);
        }

        private string[] CreateAndGetExportDataFiles(Guid questionnaireId, long questionnaireVersion, DataExportFormat format, 
            string[] dataFiles, IProgress<int> progress, CancellationToken cancellationToken)
        {
            string currentDataInfo = string.Empty;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var questionnaire = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);

                var questionnaireExportStructure =
                    this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(() =>
                        this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(questionnaire));

                if (questionnaireExportStructure == null)
                    return new string[0];

                var labelsForQuestionnaire = this.questionnaireLabelFactory.CreateLabelsForQuestionnaire(questionnaireExportStructure);

                var serviceDataLabels = this.exportSeviceDataProvider.GetServiceDataLabels();

                var result = new List<string>();
                string fileExtention = format == DataExportFormat.STATA
                    ? StataFileNameExtension
                    : SpssFileNameExtension;
                var writer = this.datasetWriterFactory.CreateDatasetWriter(format);
                long processdFiles = 0;

                foreach (var tabFile in dataFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    currentDataInfo = $"filename: {tabFile}";
                    string dataFilePath = this.fileSystemAccessor.ChangeExtension(tabFile, fileExtention);

                    var meta = this.tabReader.GetMetaFromTabFile(tabFile);

                    var fileName = this.fileSystemAccessor.GetFileNameWithoutExtension(tabFile);

                    var questionnaireLevelLabels =
                        labelsForQuestionnaire.FirstOrDefault(
                            x => string.Equals(x.LevelName, fileName, StringComparison.InvariantCultureIgnoreCase));

                    if (questionnaireLevelLabels != null)
                        UpdateMetaWithLabels(meta, questionnaireLevelLabels);
                    else if (serviceDataLabels.ContainsKey(fileName))
                        UpdateMetaWithLabels(meta, serviceDataLabels[fileName]);

                    meta.ExtendedMissings.Add(ExportFormatSettings.MissingNumericQuestionValue, "missing");
                    meta.ExtendedStrMissings.Add(ExportFormatSettings.MissingStringQuestionValue, "missing");

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

        private static void UpdateMetaWithLabels(IDatasetMeta meta, QuestionnaireLevelLabels questionnaireLevelLabels)
        {
            for (int index = 0; index < meta.Variables.Length; index++)
            {
                if (!questionnaireLevelLabels.ContainsVariable(meta.Variables[index].VarName))
                    continue;

                var variableLabels = questionnaireLevelLabels[meta.Variables[index].VarName];

                meta.Variables[index] = new DatasetVariable(meta.Variables[index].VarName)
                {
                    Storage = GetGtorageType(variableLabels.ValueType)
                };

                meta.Variables[index].VarLabel = variableLabels.Label;

                var valueSet = new ValueSet();

                foreach (var variableValueLabel in variableLabels.VariableValueLabels)
                {
                    double value;
                    if (double.TryParse(variableValueLabel.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                        valueSet.Add(value, variableValueLabel.Label);
                }

                meta.AssociateValueSet(meta.Variables[index].VarName, valueSet);
            }
        }

        private static void UpdateMetaWithLabels(IDatasetMeta meta, Dictionary<string, string> serviceLabels)
        {
            foreach (var variable in meta.Variables)
            {
                if (!serviceLabels.ContainsKey(variable.VarName))
                    continue;

                variable.VarLabel = serviceLabels[variable.VarName];
            }
        }

        private static VariableStorage GetGtorageType(ExportValueType variableLabelsValueType)
        {
            switch (variableLabelsValueType)
            {
                case ExportValueType.String:
                    return VariableStorage.StringStorage;
                case ExportValueType.NumericInt:
                    return VariableStorage.NumericIntegerStorage;
                case ExportValueType.Numeric:
                    return VariableStorage.NumericStorage;
                case ExportValueType.Date:
                    return VariableStorage.DateStorage;
                case ExportValueType.DateTime:
                    return VariableStorage.DateTimeStorage;
                case ExportValueType.Boolean:
                    return VariableStorage.NumericIntegerStorage;

                case ExportValueType.Unknown:
                default:
                    return VariableStorage.UnknownStorage;
            }
        }
    }
}
