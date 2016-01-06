using System;
using System.Globalization;
using System.Linq;
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
            IMetaDescriptionFactory metaDescriptionFactory, 
            IQuestionnaireLabelFactory questionnaireLabelFactory)
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
            QuestionnaireDocumentVersioned bigTemplateObject = GetQuestionnaireDocument(questionnaireId, questionnaireVersion);
            QuestionnaireExportStructure questionnaireExportStructure = GetQuestionnaireExportStructure(questionnaireId, questionnaireVersion);

            if (questionnaireExportStructure == null || bigTemplateObject == null)
            {
                return string.Empty;
            }

            try
            {
                IMetadataWriter metadataWriter = this.metaDescriptionFactory.CreateMetaDescription();

                var questionnaireLabelsForAllLevels =
                    questionnaireLabelFactory.CreateLabelsForQuestionnaire(questionnaireExportStructure);

                metadataWriter.SetMetadataTitle(bigTemplateObject.Questionnaire.Title);

                foreach (var questionnaireLevelLabels in questionnaireLabelsForAllLevels)
                {
                    var hhDataFile = metadataWriter.CreateDdiDataFile(questionnaireLevelLabels.LevelName);

                    foreach (LabeledVariable variableLabel in questionnaireLevelLabels.LabeledVariable)
                    {
                        if (variableLabel.QuestionId.HasValue)
                        {
                            var questionItem =
                                bigTemplateObject.Questionnaire.FirstOrDefault<IQuestion>(q => q.PublicKey == variableLabel.QuestionId.Value);

                            if (questionItem == null)
                                continue;

                            var variable = metadataWriter.AddDdiVariableToFile(hhDataFile, variableLabel.VariableName,
                                GetDdiDataType(questionItem.QuestionType), variableLabel.Label, questionItem.Instructions,
                                questionItem.QuestionText, GetDdiVariableScale(questionItem.QuestionType));

                            foreach (VariableValueLabel variableValueLabel in variableLabel.VariableValueLabels)
                            {
                                decimal value;
                                if (decimal.TryParse(variableValueLabel.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                                    metadataWriter.AddValueLabelToVariable(variable, value, variableValueLabel.Label);
                            }
                        }
                        else
                        {
                            metadataWriter.AddDdiVariableToFile(hhDataFile, variableLabel.VariableName,
                                DdiDataType.DynString, variableLabel.Label, null, null, null);
                        }
                    }
                }

                var pathToWrite = this.fileSystemAccessor.CombinePath(basePath, ExportFileSettings.GetDDIFileName(
                    $"{questionnaireId}_{questionnaireVersion}_ddi"));

                metadataWriter.SaveMetadataInFile(pathToWrite);

                return pathToWrite;
            }
            catch (Exception exc)
            {
                this.logger.Error(
                    $"Error on DDI metadata creation (questionnaireId:{questionnaireId}, questionnaireVersion:{questionnaireVersion}): ", exc);
            }

            return string.Empty;
        }

        private QuestionnaireExportStructure GetQuestionnaireExportStructure(Guid questionnaireId, long questionnaireVersion)
        {
            return
                this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(
                    () =>
                        this.questionnaireExportStructureWriter.AsVersioned()
                            .Get(questionnaireId.FormatGuid(), questionnaireVersion));
        }

        private QuestionnaireDocumentVersioned GetQuestionnaireDocument(Guid questionnaireId, long questionnaireVersion)
        {
            return
                this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(
                    () => this.questionnaireDocumentVersionedStorage.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion));
        }

        private DdiDataType GetDdiDataType(QuestionType questionType)
        {
            return
                new[]
                {QuestionType.Numeric, QuestionType.SingleOption, QuestionType.MultyOption, QuestionType.GpsCoordinates}
                    .Contains(questionType)
                    ? DdiDataType.Numeric
                    : DdiDataType.DynString;
        }

        private DdiVariableScale? GetDdiVariableScale(QuestionType questionType)
        {
            switch (questionType)
            {
                case QuestionType.Numeric:
                case QuestionType.GpsCoordinates:
                    return DdiVariableScale.Scale;
                case QuestionType.SingleOption:
                case QuestionType.MultyOption:
                    return DdiVariableScale.Nominal;
            }
            return null;
        }
    }
}