using System;
using System.Collections.Generic;
using System.Linq;
using StatData.Core;
using ddidotnet;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;


namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class SqlToDataExportService : IDataExportService
    {
        private readonly ITransactionManagerProvider transactionManager;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ITabularFormatExportService tabularFormatExportService;
        private readonly string parentId = "ParentId";
        private readonly ILogger logger;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureWriter;
        private readonly ITabFileReader tabReader;
        private readonly IDatasetWriterFactory datasetWriterFactory;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage;

        public SqlToDataExportService(
            IFileSystemAccessor fileSystemAccessor,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureWriter,
            ITransactionManagerProvider transactionManager,
            ILogger logger,
            ITabFileReader tabReader,
            IDatasetWriterFactory datasetWriterFactory,
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage, 
            ITabularFormatExportService tabularFormatExportService)
        {
            this.questionnaireExportStructureWriter = questionnaireExportStructureWriter;
            this.transactionManager = transactionManager;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;

            this.tabReader = tabReader;
            this.datasetWriterFactory = datasetWriterFactory;

            this.questionnaireDocumentVersionedStorage = questionnaireDocumentVersionedStorage;
            this.tabularFormatExportService = tabularFormatExportService;
        }

        private string StataFileNameExtension { get { return ".dta"; } }
        private string SpssFileNameExtension { get { return ".sav"; } }

        public string[] CreateAndGetStataDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string basePath)
        {
            tabularFormatExportService.ExportInterviewsInTabularFormatAsync(questionnaireId, questionnaireVersion, basePath).WaitAndUnwrapException();
            return CreateAndGetExportDataFiles(questionnaireId, questionnaireVersion, basePath, ExportDataType.Stata, fileSystemAccessor.GetFilesInDirectory(basePath));
        }
        
        public string[] CreateAndGetSpssDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string basePath)
        {
            tabularFormatExportService.ExportInterviewsInTabularFormatAsync(questionnaireId, questionnaireVersion, basePath).WaitAndUnwrapException();
            return CreateAndGetExportDataFiles(questionnaireId, questionnaireVersion, basePath, ExportDataType.Spss, fileSystemAccessor.GetFilesInDirectory(basePath));
        }

        private void CollectLabels(QuestionnaireExportStructure structure, out Dictionary<string, string> labels, out Dictionary<string, Dictionary<double, string>> varValueLabels)
        {
            labels = new Dictionary<string, string>();
            varValueLabels = new Dictionary<string, Dictionary<double,string>>();

            foreach (var headerStructureForLevel in structure.HeaderToLevelMap.Values)
            {
                foreach (ExportedHeaderItem headerItem in headerStructureForLevel.HeaderItems.Values)
                {
                    bool hasLabels = headerItem.Labels != null && headerItem.Labels.Count > 0;
                    
                    if (hasLabels)
                    {
                        string labelName = headerItem.VariableName;
                        if (!varValueLabels.ContainsKey(labelName))
                        {
                            var items = headerItem.Labels.Values.ToDictionary(item => Double.Parse(item.Caption),item => item.Title ?? string.Empty);
                            varValueLabels.Add(labelName, items);
                        }
                    }

                    for (int i = 0; i < headerItem.ColumnNames.Length; i++)
                    {
                        if (!labels.ContainsKey(headerItem.ColumnNames[i]))
                            labels.Add(headerItem.ColumnNames[i], headerItem.Titles[i] ?? string.Empty);
                    }
                }

                if (headerStructureForLevel.LevelLabels == null) continue;
                
                var levelLabelName = headerStructureForLevel.LevelIdColumnName;
                if (varValueLabels.ContainsKey(levelLabelName)) continue;
                    
                var labelItems = headerStructureForLevel.LevelLabels.ToDictionary(item => Double.Parse(item.Caption), item => item.Title ?? String.Empty);
                varValueLabels.Add(levelLabelName, labelItems);
            }
        }

        private string[] CreateAndGetExportDataFiles(Guid questionnaireId, long questionnaireVersion, string basePath, ExportDataType exportType, string[] dataFiles)
        {
            string currentDataInfo = string.Empty;
            try
            {
                var structure = this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(() =>
                    questionnaireExportStructureWriter.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion));

                if (structure == null)
                    return new string[0];

                Dictionary<string, string> varLabels;
                Dictionary<string, Dictionary<double, string>> varValueLabels;

                CollectLabels(structure, out varLabels, out varValueLabels);

                var result = new List<string>();
                string fileExtention = exportType == ExportDataType.Stata
                    ? StataFileNameExtension
                    : SpssFileNameExtension;
                var writer = datasetWriterFactory.CreateDatasetWriter(exportType);
                foreach (var tabFile in dataFiles)
                {
                    string dataFile = fileSystemAccessor.ChangeExtension(tabFile, fileExtention);
                    var meta = tabReader.GetMetaFromTabFile(tabFile);
                    
                    UpdateMetaWithLabels(meta, varLabels, varValueLabels);
                    currentDataInfo = string.Format("filename: {0}", tabFile);
                    var data = tabReader.GetDataFromTabFile(tabFile);
                    writer.WriteToFile(dataFile, meta, data);
                    result.Add(dataFile);
                }

                return result.ToArray();

            }
            catch (Exception exc)
            {
                logger.Error(string.Format("Error on data export (questionnaireId:{0}, questionnaireVersion:{1}): ", questionnaireId, questionnaireVersion), exc);
                logger.Error(currentDataInfo);
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

        public string[] CreateAndGetStataDataFilesForQuestionnaireInApprovedState(Guid questionnaireId, long questionnaireVersion,
            string basePath)
        {
            tabularFormatExportService.ExportApprovedInterviewsInTabularFormatAsync(questionnaireId, questionnaireVersion, basePath).WaitAndUnwrapException();
            return CreateAndGetExportDataFiles(questionnaireId, questionnaireVersion, basePath, ExportDataType.Stata, fileSystemAccessor.GetFilesInDirectory(basePath));
        }

        public string[] CreateAndGetSpssDataFilesForQuestionnaireInApprovedState(Guid questionnaireId, long questionnaireVersion,
            string basePath)
        {
            tabularFormatExportService.ExportApprovedInterviewsInTabularFormatAsync(questionnaireId, questionnaireVersion, basePath).WaitAndUnwrapException();
            return CreateAndGetExportDataFiles(questionnaireId, questionnaireVersion, basePath, ExportDataType.Spss, fileSystemAccessor.GetFilesInDirectory(basePath));
        }

        public string CreateAndGetDDIMetadataFileForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string basePath)
        {
            try
            {
                QuestionnaireDocumentVersioned bigTemplateObject;
                QuestionnaireExportStructure questionnaireExportStructure;

                this.transactionManager.GetTransactionManager().BeginQueryTransaction();
                try
                {
                    bigTemplateObject = this.questionnaireDocumentVersionedStorage.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion);
                    questionnaireExportStructure = questionnaireExportStructureWriter.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion);
                }
                finally
                {
                    this.transactionManager.GetTransactionManager().RollbackQueryTransaction();
                }

                

                if (questionnaireExportStructure == null)
                    return string.Empty;

                Dictionary<string, string> varLabels;
                Dictionary<string, Dictionary<double, string>> varValueLabels;
                CollectLabels(questionnaireExportStructure, out varLabels, out varValueLabels);

                MetaDescription metaDescription = new MetaDescription
                {
                    Document = {Title = bigTemplateObject.Questionnaire.Title},
                    Study =
                    {
                        Title = bigTemplateObject.Questionnaire.Title,
                        Idno = "QUEST"
                    }
                };


                foreach (var headerStructureForLevel in questionnaireExportStructure.HeaderToLevelMap.Values)
                {
                    var hhDataFile = metaDescription.AddDataFile(headerStructureForLevel.LevelName);

                    var idColumn = hhDataFile.AddVariable(DdiDataType.DynString);
                    idColumn.Name = headerStructureForLevel.LevelIdColumnName;

                    if (headerStructureForLevel.IsTextListScope)
                    {
                        foreach (var name in headerStructureForLevel.ReferencedNames)
                        {
                            var v1 = hhDataFile.AddVariable(DdiDataType.DynString);
                            v1.Name = name;
                        }
                    }

                    foreach (ExportedHeaderItem question in headerStructureForLevel.HeaderItems.Values)
                    {
                        var questionItem = bigTemplateObject.Questionnaire.FirstOrDefault<IQuestion>(q => q.PublicKey == question.PublicKey);
                        foreach (var columnName in question.ColumnNames)
                        {
                            var variable = this.AddVadiableToDdiFileAndGet(hhDataFile, question.QuestionType, columnName);
                            
                            if (questionItem != null && !string.IsNullOrWhiteSpace(questionItem.Instructions))
                                variable.IvuInstr = questionItem.Instructions;

                            if (questionItem != null)
                                variable.QstnLit = questionItem.QuestionText;

                            if (varLabels.ContainsKey(columnName))
                                variable.Label = varLabels[columnName];

                            if (varValueLabels.ContainsKey(question.VariableName))
                            {
                                foreach (var label in varValueLabels[question.VariableName])
                                {
                                    variable.AddValueLabel(Convert.ToDecimal(label.Key), label.Value);
                                }
                            }
                        }
                    }

                    for (int i = 0; i < headerStructureForLevel.LevelScopeVector.Length; i++)
                    {
                        var v1 = hhDataFile.AddVariable(DdiDataType.DynString);
                        v1.Name = string.Format("{0}{1}", parentId, i + 1);
                    }
                }

                var pathToWrite = fileSystemAccessor.CombinePath(basePath, ExportFileSettings.GetDDIFileName(string.Format("{0}_{1}_ddi", questionnaireId, questionnaireVersion)));
                metaDescription.WriteXml(pathToWrite);
                return pathToWrite;
            }

            catch (Exception exc)
            {
                logger.Error(string.Format("Error on DDI metadata creation (questionnaireId:{0}, questionnaireVersion:{1}): ", questionnaireId, questionnaireVersion), exc);
            }

            return string.Empty;
        }

        private DdiVariable AddVadiableToDdiFileAndGet(DdiDataFile hhDataFile, QuestionType questionType, string columnName)
        {
            DdiVariable variable = null;
            switch (questionType)
            {
                case QuestionType.Numeric:
                    variable = hhDataFile.AddVariable(DdiDataType.Numeric);
                    variable.VariableScale = DdiVariableScale.Ordinal;
                    break;
                case QuestionType.SingleOption: 
                case QuestionType.MultyOption:
                    variable = hhDataFile.AddVariable(DdiDataType.Numeric);
                    variable.VariableScale = DdiVariableScale.Nominal;
                    break;
                case QuestionType.GpsCoordinates: 
                    variable = hhDataFile.AddVariable(DdiDataType.Numeric);
                    variable.VariableScale = DdiVariableScale.Ordinal;
                    break;

                default:
                    variable = hhDataFile.AddVariable(DdiDataType.DynString);
                    break; 
            }
            variable.Name = columnName;
            return variable;
        }
    }
}
