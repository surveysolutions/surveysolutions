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
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class TabularDataToExternalStatPackageExportService : ITabularDataToExternalStatPackageExportService
    {
        private readonly ITransactionManagerProvider transactionManager;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ILogger logger;
        private readonly IQuestionnaireProjectionsRepository questionnaireProjectionsRepository;
        private readonly ITabFileReader tabReader;
        private readonly IDatasetWriterFactory datasetWriterFactory;
        private readonly IDataQueryFactory dataQueryFactory;

        private readonly IQuestionnaireLabelFactory questionnaireLabelFactory;

        public TabularDataToExternalStatPackageExportService(
            IFileSystemAccessor fileSystemAccessor,
            ITransactionManagerProvider transactionManager,
            ILogger logger,
            ITabFileReader tabReader,
            IDataQueryFactory dataQueryFactory,
            IDatasetWriterFactory datasetWriterFactory, 
            IQuestionnaireLabelFactory questionnaireLabelFactory, 
            IQuestionnaireProjectionsRepository questionnaireProjectionsRepository)
        {
            this.transactionManager = transactionManager;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;

            this.tabReader = tabReader;
            this.datasetWriterFactory = datasetWriterFactory;
            this.questionnaireLabelFactory = questionnaireLabelFactory;
            this.questionnaireProjectionsRepository = questionnaireProjectionsRepository;
            this.dataQueryFactory = dataQueryFactory;
        }

        private string StataFileNameExtension { get { return ".dta"; } }
        private string SpssFileNameExtension { get { return ".sav"; } }

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

        private string[] CreateAndGetExportDataFiles(Guid questionnaireId, long questionnaireVersion, DataExportFormat format, string[] dataFiles, IProgress<int> progress, CancellationToken cancellationToken)
        {
            string currentDataInfo = string.Empty;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var questionnaireExportStructure =
                    this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(() =>
                        this.questionnaireProjectionsRepository.GetQuestionnaireExportStructure(
                            new QuestionnaireIdentity(questionnaireId, questionnaireVersion)));

                if (questionnaireExportStructure == null)
                    return new string[0];

                var labelsForQuestionnaire = this.questionnaireLabelFactory.CreateLabelsForQuestionnaire(questionnaireExportStructure);

                var result = new List<string>();
                string fileExtention = format == DataExportFormat.STATA
                    ? this.StataFileNameExtension
                    : this.SpssFileNameExtension;
                var writer = this.datasetWriterFactory.CreateDatasetWriter(format);
                long processdFiles = 0;

                foreach (var tabFile in dataFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    currentDataInfo = $"filename: {tabFile}";
                    string dataFilePath = this.fileSystemAccessor.ChangeExtension(tabFile, fileExtention);

                    var meta = this.tabReader.GetMetaFromTabFile(tabFile);

                    var levelName = this.fileSystemAccessor.GetFileNameWithoutExtension(tabFile);

                    var questionnaireLevelLabels =
                        labelsForQuestionnaire.FirstOrDefault(
                            x => string.Equals(x.LevelName, levelName, StringComparison.InvariantCultureIgnoreCase));

                    if (questionnaireLevelLabels != null)
                        UpdateMetaWithLabels(meta, questionnaireLevelLabels);

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
            foreach (var datasetVariable in meta.Variables)
            {
                if (!questionnaireLevelLabels.ContainsVariable(datasetVariable.VarName))
                    continue;

                var variableLabels = questionnaireLevelLabels[datasetVariable.VarName];

                datasetVariable.VarLabel = variableLabels.Label;

                var valueSet = new ValueSet();

                foreach (var variableValueLabel in variableLabels.VariableValueLabels)
                {
                    double value;
                    if (double.TryParse(variableValueLabel.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                        valueSet.Add(value, variableValueLabel.Label);
                }

                meta.AssociateValueSet(datasetVariable.VarName, valueSet);
            }
        }
    }
}
