using System;
using System.Globalization;
using System.Linq;
using ddidotnet;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Ddi.Impl
{
    internal class DdiMetadataFactory : IDdiMetadataFactory
    {
        private readonly IFileSystemAccessor fileSystemAccessor;

        private readonly ILogger logger;
        private readonly IPlainKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        private readonly IMetaDescriptionFactory metaDescriptionFactory;

        private readonly IQuestionnaireLabelFactory questionnaireLabelFactory;
        public DdiMetadataFactory(
            IFileSystemAccessor fileSystemAccessor,
            ILogger logger,
            IMetaDescriptionFactory metaDescriptionFactory, 
            IQuestionnaireLabelFactory questionnaireLabelFactory,
            IPlainKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage, 
            IPlainQuestionnaireRepository plainQuestionnaireRepository)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            this.metaDescriptionFactory = metaDescriptionFactory;
            this.questionnaireLabelFactory = questionnaireLabelFactory;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
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
                        if (variableLabel.QuestionId.HasValue)
                        {
                            var questionItem =
                                bigTemplateObject.Find<IQuestion>(variableLabel.QuestionId.Value);

                            if (questionItem == null)
                                continue;

                            var variable = metadataWriter.AddDdiVariableToFile(hhDataFile, variableLabel.VariableName,
                                this.GetDdiDataType(questionItem.QuestionType), variableLabel.Label, questionItem.Instructions,
                                questionItem.QuestionText, this.GetDdiVariableScale(questionItem.QuestionType));

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
                this.questionnaireExportStructureStorage.GetById(questionnaireId.ToString());
        }

        private QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireIdentity questionnaireId)
        {
            return this.plainQuestionnaireRepository.GetQuestionnaireDocument(questionnaireId);
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