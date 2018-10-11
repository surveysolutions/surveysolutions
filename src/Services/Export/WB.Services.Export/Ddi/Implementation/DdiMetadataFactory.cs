using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ddidotnet;
using Microsoft.Extensions.Logging;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.CsvExport.Implementation.DoFiles;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Ddi.Implementation
{
    public class DdiMetadataFactory : IDdiMetadataFactory
    {
        private readonly IFileSystemAccessor fileSystemAccessor;

        private readonly ILogger<DdiMetadataFactory> logger;

        private readonly IQuestionnaireExportStructureFactory questionnaireExportStructureStorage;
        private readonly IQuestionnaireStorage questionnaireStorage;

        private readonly IMetaDescriptionFactory metaDescriptionFactory;

        private readonly IQuestionnaireLabelFactory questionnaireLabelFactory;
        public DdiMetadataFactory(
            IFileSystemAccessor fileSystemAccessor,
            ILogger<DdiMetadataFactory> logger,
            IMetaDescriptionFactory metaDescriptionFactory, 
            IQuestionnaireLabelFactory questionnaireLabelFactory,
            IQuestionnaireStorage questionnaireStorage, 
            IQuestionnaireExportStructureFactory questionnaireExportStructureStorage)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            this.metaDescriptionFactory = metaDescriptionFactory;
            this.questionnaireLabelFactory = questionnaireLabelFactory;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
        }

        public async Task<string> CreateDDIMetadataFileForQuestionnaireInFolder(TenantInfo tenant,
            QuestionnaireId questionnaireId, string basePath)
        {
            var bigTemplateObject = await questionnaireStorage.GetQuestionnaireAsync(tenant, questionnaireId);
            QuestionnaireExportStructure questionnaireExportStructure = 
                this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(tenant, bigTemplateObject);

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

                    foreach (DataExportVariable variableLabel in questionnaireLevelLabels.LabeledVariable)
                    {
                        if (variableLabel.EntityId.HasValue)
                        {
                            var questionItem = bigTemplateObject.Find<Question>(variableLabel.EntityId.Value);

                            if (questionItem != null)
                            {
                                var variable = metadataWriter.AddDdiVariableToFile(hhDataFile, 
                                    variableLabel.VariableName,
                                    this.GetDdiDataType(questionItem.QuestionType), 
                                    variableLabel.Label,
                                    questionItem.Instructions,
                                    questionItem.QuestionText, 
                                    this.GetDdiVariableScale(questionItem.QuestionType));

                                foreach (VariableValueLabel variableValueLabel in variableLabel.VariableValueLabels)
                                {
                                    if (decimal.TryParse(variableValueLabel.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                                        metadataWriter.AddValueLabelToVariable(variable, value, variableValueLabel.Label);
                                }   

                                continue;
                            }

                            var variableItem = bigTemplateObject.Find<Variable>(variableLabel.EntityId.Value);
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
                    $"{questionnaireId}_ddi"));

                metadataWriter.SaveMetadataInFile(pathToWrite);

                return pathToWrite;
            }
            catch (Exception exc)
            {
                this.logger.LogError(
                    exc, $"Error on DDI metadata creation (questionnaireId:{questionnaireId}): ");
            }

            return string.Empty;
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
