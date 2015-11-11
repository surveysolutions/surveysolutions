using System;
using System.Collections.Generic;
using StatData.Core;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
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

        public TabularDataToExternalStatPackageExportService(
            IFileSystemAccessor fileSystemAccessor,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureWriter,
            ITransactionManagerProvider transactionManager,
            ILogger logger,
            ITabFileReader tabReader,
            IDatasetWriterFactory datasetWriterFactory)
        {
            this.questionnaireExportStructureWriter = questionnaireExportStructureWriter;
            this.transactionManager = transactionManager;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;

            this.tabReader = tabReader;
            this.datasetWriterFactory = datasetWriterFactory;
        }

        private string StataFileNameExtension { get { return ".dta"; } }
        private string SpssFileNameExtension { get { return ".sav"; } }

        public string[] CreateAndGetStataDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string[] tabularDataFiles)
        {
            return this.CreateAndGetExportDataFiles(questionnaireId, questionnaireVersion, DataExportFormat.STATA, tabularDataFiles);
        }

        public string[] CreateAndGetSpssDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string[] tabularDataFiles)
        {
            return this.CreateAndGetExportDataFiles(questionnaireId, questionnaireVersion, DataExportFormat.SPSS, tabularDataFiles);
        }
       

        private string[] CreateAndGetExportDataFiles(Guid questionnaireId, long questionnaireVersion, DataExportFormat format, string[] dataFiles)
        {
            string currentDataInfo = string.Empty;
            try
            {
                var questionnaireExportStructure = this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(() =>
                    this.questionnaireExportStructureWriter.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion));

                if (questionnaireExportStructure == null)
                    return new string[0];

                Dictionary<string, string> varLabels;
                Dictionary<string, Dictionary<double, string>> varValueLabels;

                questionnaireExportStructure.CollectLabels(out varLabels, out varValueLabels);

                var result = new List<string>();
                string fileExtention = format == DataExportFormat.STATA
                    ? this.StataFileNameExtension
                    : this.SpssFileNameExtension;
                var writer = this.datasetWriterFactory.CreateDatasetWriter(format);
                foreach (var tabFile in dataFiles)
                {
                    currentDataInfo = string.Format("filename: {0}", tabFile);
                    string dataFile = this.fileSystemAccessor.ChangeExtension(tabFile, fileExtention);
                    var meta = this.tabReader.GetMetaFromTabFile(tabFile);
                    UpdateMetaWithLabels(meta, varLabels, varValueLabels);
                    var data = this.tabReader.GetDataFromTabFile(tabFile);
                    writer.WriteToFile(dataFile, meta, data);
                    result.Add(dataFile);
                }

                return result.ToArray();

            }
            catch (Exception exc)
            {
                this.logger.Error(string.Format("Error on data export (questionnaireId:{0}, questionnaireVersion:{1}): ", questionnaireId, questionnaireVersion), exc);
                this.logger.Error(currentDataInfo);
            }

            return new string[0];
        }

        private static void UpdateMetaWithLabels(IDatasetMeta meta, Dictionary<string, string> varLabels, Dictionary<string, Dictionary<double, string>> varValueLabels)
        {
            foreach (var datasetVariable in meta.Variables)
            {
                if (varLabels.ContainsKey(datasetVariable.VarName))
                    datasetVariable.VarLabel = varLabels[datasetVariable.VarName];

                if (varValueLabels.ContainsKey(datasetVariable.VarName))
                {
                    var valueSet = new ValueSet();
                    foreach (var variable in varValueLabels[datasetVariable.VarName])
                    {
                        valueSet.Add(variable.Key, variable.Value);
                    }

                    meta.AssociateValueSet(datasetVariable.VarName, valueSet);
                }
            }
        }
    }
}
