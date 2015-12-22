using System;
using System.Collections.Generic;
using ddidotnet;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveySolutions.Implementation.ServiceVariables;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class MetadataExportService : IMetadataExportService
    {
        private readonly ITransactionManagerProvider transactionManager;
        private readonly IFileSystemAccessor fileSystemAccessor;

        private readonly ILogger logger;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureWriter;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage;

        private readonly IMetaDescriptionFactory metaDescriptionFactory;

        public MetadataExportService(
            IFileSystemAccessor fileSystemAccessor,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureWriter,
            ITransactionManagerProvider transactionManager,
            ILogger logger,
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage,
            IMetaDescriptionFactory metaDescriptionFactory)
        {
            this.questionnaireExportStructureWriter = questionnaireExportStructureWriter;
            this.transactionManager = transactionManager;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            this.questionnaireDocumentVersionedStorage = questionnaireDocumentVersionedStorage;

            this.metaDescriptionFactory = metaDescriptionFactory;
        }
        
        public string CreateAndGetDDIMetadataFileForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string basePath)
        {
            try
            {
                QuestionnaireDocumentVersioned bigTemplateObject;
                QuestionnaireExportStructure questionnaireExportStructure;
                var shouldTransactionBeStarted = !this.transactionManager.GetTransactionManager().IsQueryTransactionStarted;
                if(shouldTransactionBeStarted)
                    this.transactionManager.GetTransactionManager().BeginQueryTransaction();
                try
                {
                    bigTemplateObject = this.questionnaireDocumentVersionedStorage.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion);
                    questionnaireExportStructure = this.questionnaireExportStructureWriter.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion);
                }
                finally
                {
                    if (shouldTransactionBeStarted)
                        this.transactionManager.GetTransactionManager().RollbackQueryTransaction();
                }

                if (questionnaireExportStructure == null)
                    return string.Empty;

                Dictionary<string, string> varLabels;
                Dictionary<string, Dictionary<double, string>> varValueLabels;
                Dictionary<string, Dictionary<string, string>> labelsForServiceColumns;
                questionnaireExportStructure.CollectLabels(out labelsForServiceColumns, out varLabels, out varValueLabels);

                IMetaDescription metaDescription = this.metaDescriptionFactory.CreateMetaDescription();

                metaDescription.Document.Title = bigTemplateObject.Questionnaire.Title;
                metaDescription.Study.Title = bigTemplateObject.Questionnaire.Title;
                metaDescription.Study.Idno = "QUEST";
                
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
                        v1.Name = string.Format("{0}{1}", ServiceColumns.ParentId, i + 1);
                    }
                }

                var pathToWrite = this.fileSystemAccessor.CombinePath(basePath, ExportFileSettings.GetDDIFileName(string.Format("{0}_{1}_ddi", questionnaireId, questionnaireVersion)));
                metaDescription.WriteXml(pathToWrite);
                return pathToWrite;
            }

            catch (Exception exc)
            {
                this.logger.Error(string.Format("Error on DDI metadata creation (questionnaireId:{0}, questionnaireVersion:{1}): ", questionnaireId, questionnaireVersion), exc);
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
                    variable.VariableScale = DdiVariableScale.Scale;
                    break;
                case QuestionType.SingleOption:
                case QuestionType.MultyOption:
                    variable = hhDataFile.AddVariable(DdiDataType.Numeric);
                    variable.VariableScale = DdiVariableScale.Nominal;
                    break;
                case QuestionType.GpsCoordinates:
                    variable = hhDataFile.AddVariable(DdiDataType.Numeric);
                    variable.VariableScale = DdiVariableScale.Scale;
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