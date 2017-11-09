using System;
using System.Globalization;
using System.Linq;
using ddidotnet;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Ddi.Impl
{
    internal class DdiMetadataFactory : IDdiMetadataFactory
    {
        private readonly IFileSystemAccessor fileSystemAccessor;

        private readonly ILogger logger;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly IQuestionnaireStorage questionnaireStorage;

        private readonly IMetaDescriptionFactory metaDescriptionFactory;

        private readonly IQuestionnaireLabelFactory questionnaireLabelFactory;
        public DdiMetadataFactory(
            IFileSystemAccessor fileSystemAccessor,
            ILogger logger,
            IMetaDescriptionFactory metaDescriptionFactory, 
            IQuestionnaireLabelFactory questionnaireLabelFactory,
            IQuestionnaireStorage questionnaireStorage, 
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            this.metaDescriptionFactory = metaDescriptionFactory;
            this.questionnaireLabelFactory = questionnaireLabelFactory;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
        }

        public string CreateDDIMetadataFileForQuestionnaireInFolder(QuestionnaireIdentity questionnaireId, string basePath)
        {
            QuestionnaireDocument bigTemplateObject = this.GetQuestionnaireDocument(questionnaireId);
            QuestionnaireExportStructure questionnaireExportStructure = this.GetQuestionnaireExportStructure(questionnaireId);

            if (questionnaireExportStructure == null || bigTemplateObject == null)
            {
                return string.Empty;
            }

            try
            {
                IMetadataWriter metadataWriter = this.metaDescriptionFactory.CreateMetaDescription();

                var questionnaireLabelsForAllLevels =
                    this.questionnaireLabelFactory.CreateLabelsForQuestionnaire(questionnaireExportStructure);

                metadataWriter.SetMetadataTitle(bigTemplateObject.Title);

                foreach (var questionnaireLevelLabels in questionnaireLabelsForAllLevels)
                {
                    var hhDataFile = metadataWriter.CreateDdiDataFile(questionnaireLevelLabels.LevelName);

                    foreach (LabeledVariable variableLabel in questionnaireLevelLabels.LabeledVariable)
                    {
                        if (variableLabel.EntityId.HasValue)
                        {
                            var questionItem = bigTemplateObject.Find<IQuestion>(variableLabel.EntityId.Value);

                            if (questionItem != null)
                            {
                                var variable = metadataWriter.AddDdiVariableToFile(hhDataFile, variableLabel.VariableName,
                                    this.GetDdiDataType(questionItem.QuestionType), variableLabel.Label, questionItem.Instructions,
                                    questionItem.QuestionText, this.GetDdiVariableScale(questionItem.QuestionType));

                                foreach (VariableValueLabel variableValueLabel in variableLabel.VariableValueLabels)
                                {
                                    if (decimal.TryParse(variableValueLabel.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                                        metadataWriter.AddValueLabelToVariable(variable, value, variableValueLabel.Label);
                                }   

                                continue;
                            }

                            var variableItem = bigTemplateObject.Find<IVariable>(variableLabel.EntityId.Value);
                            if (variableItem != null)
                            {
                                var variable = metadataWriter.AddDdiVariableToFile(hhDataFile, variableLabel.VariableName,
                                    this.GetDdiDataType(variableItem.Type), variableLabel.Label, null,
                                    variableItem.Expression, this.GetDdiVariableScale(variableItem.Type));

                                foreach (VariableValueLabel variableValueLabel in variableLabel.VariableValueLabels)
                                {
                                    if (decimal.TryParse(variableValueLabel.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                                        metadataWriter.AddValueLabelToVariable(variable, value, variableValueLabel.Label);
                                }

                                continue;
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
                    $"{questionnaireId.QuestionnaireId}_{questionnaireId.Version}_ddi"));

                metadataWriter.SaveMetadataInFile(pathToWrite);

                return pathToWrite;
            }
            catch (Exception exc)
            {
                this.logger.Error(
                    $"Error on DDI metadata creation (questionnaireId:{questionnaireId.QuestionnaireId}, questionnaireVersion:{questionnaireId.Version}): ", exc);
            }

            return string.Empty;
        }

        private QuestionnaireExportStructure GetQuestionnaireExportStructure(QuestionnaireIdentity questionnaireId)
        {
            return
                this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(questionnaireId);
        }

        private QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireIdentity questionnaireId)
        {
            return this.questionnaireStorage.GetQuestionnaireDocument(questionnaireId);
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

        private DdiDataType GetDdiDataType(VariableType variableType)
        {
            return new[] {VariableType.Double, VariableType.LongInteger, VariableType.Boolean}.Contains(variableType)
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

        private DdiVariableScale? GetDdiVariableScale(VariableType variableType)
        {
            switch (variableType)
            {
                case VariableType.Double:
                case VariableType.LongInteger:
                case VariableType.Boolean:
                    return DdiVariableScale.Scale;
            }
            return null;
        }
    }
}