using System;
using ddidotnet;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

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

        private readonly IQuestionnaireLabelFactory questionnaireLabelFactory;
        public MetadataExportService(
            IFileSystemAccessor fileSystemAccessor,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureWriter,
            ITransactionManagerProvider transactionManager,
            ILogger logger,
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage,
            IMetaDescriptionFactory metaDescriptionFactory, IQuestionnaireLabelFactory questionnaireLabelFactory)
        {
            this.questionnaireExportStructureWriter = questionnaireExportStructureWriter;
            this.transactionManager = transactionManager;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            this.questionnaireDocumentVersionedStorage = questionnaireDocumentVersionedStorage;

            this.metaDescriptionFactory = metaDescriptionFactory;
            this.questionnaireLabelFactory = questionnaireLabelFactory;
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

                IMetaDescription metaDescription = this.metaDescriptionFactory.CreateMetaDescription();

                metaDescription.Document.Title = bigTemplateObject.Questionnaire.Title;
                metaDescription.Study.Title = bigTemplateObject.Questionnaire.Title;
                metaDescription.Study.Idno = "QUEST";
                
                foreach (var headerStructureForLevel in questionnaireExportStructure.HeaderToLevelMap.Values)
                {
                    QuestionnaireLabels questionnaireLabels =
                        this.questionnaireLabelFactory.CreateLabelsForQuestionnaireLevel(questionnaireExportStructure, headerStructureForLevel.LevelScopeVector);

                    var hhDataFile = metaDescription.AddDataFile(questionnaireLabels.LevelName);

                    foreach (LabeledVariable variableLabel in questionnaireLabels.LabeledVariable)
                    {
                        if (variableLabel.QuestionId.HasValue)
                        {
                            var questionItem =
                                bigTemplateObject.Questionnaire.FirstOrDefault<IQuestion>(
                                    q => q.PublicKey == variableLabel.QuestionId.Value);

                            if (questionItem == null)
                                continue;


                            var variable = this.AddVariableToDdiFileAndReturnIt(hhDataFile, questionItem.QuestionType,
                                variableLabel.VariableName);

                            if (!string.IsNullOrWhiteSpace(questionItem.Instructions))
                                variable.IvuInstr = questionItem.Instructions;

                            variable.QstnLit = questionItem.QuestionText;

                            variable.Label = variableLabel.Label;

                            foreach (VariableValueLabel variableValueLabel in variableLabel.VariableValueLabels)
                            {
                                decimal value;
                                if(decimal.TryParse(variableValueLabel.Value, out value))
                                    variable.AddValueLabel(value, variableValueLabel.Label);
                            }
                        }
                        else
                        {
                            var column = hhDataFile.AddVariable(DdiDataType.DynString);
                            column.Name = variableLabel.VariableName;
                            column.Label = variableLabel.Label;
                        }
                    }
                }

                var pathToWrite = this.fileSystemAccessor.CombinePath(basePath, ExportFileSettings.GetDDIFileName(
                    $"{questionnaireId}_{questionnaireVersion}_ddi"));
                metaDescription.WriteXml(pathToWrite);
                return pathToWrite;
            }

            catch (Exception exc)
            {
                this.logger.Error(
                    $"Error on DDI metadata creation (questionnaireId:{questionnaireId}, questionnaireVersion:{questionnaireVersion}): ", exc);
            }

            return string.Empty;
        }

        private DdiVariable AddVariableToDdiFileAndReturnIt(DdiDataFile hhDataFile, QuestionType questionType, string columnName)
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