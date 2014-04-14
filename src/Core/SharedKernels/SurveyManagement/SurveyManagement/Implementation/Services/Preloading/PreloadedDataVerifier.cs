using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Raven.Abstractions.Extensions;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Factories;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Properties;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading
{
    internal class PreloadedDataVerifier : IPreloadedDataVerifier
    {
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage;
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireExportStructureStorage;
        private readonly IQuestionDataParser questionDataParser;

        private readonly IQuestionnaireFactory questionnaireFactory;
        private readonly string[] columnsToExcluteFromQuestionMapping = { "Id", "ParentId" };
        private readonly IRosterDataService rosterDataService;
        public PreloadedDataVerifier(
            IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage,
            IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireExportStructureStorage, IRosterDataService rosterDataService, IQuestionDataParser questionDataParser, IQuestionnaireFactory questionnaireFactory)
        {
            this.questionnaireDocumentVersionedStorage = questionnaireDocumentVersionedStorage;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.rosterDataService = rosterDataService;
            this.questionDataParser = questionDataParser;
            this.questionnaireFactory = questionnaireFactory;
        }

        public IEnumerable<PreloadedDataVerificationError> Verify(Guid questionnaireId, long version, PreloadedDataByFile[] data)
        {
            var questionnaire = questionnaireDocumentVersionedStorage.GetById(questionnaireId, version);
            var questionnaireExportStructure = questionnaireExportStructureStorage.GetById(questionnaireId, version);
            if (questionnaire == null || questionnaireExportStructure == null)
            {
                yield return new PreloadedDataVerificationError("PL0001", PreloadingVerificationMessages.PL0001_NoQuestionnaire);
                yield break;
            }

            if (data.Length > 1)
            {
                yield return new PreloadedDataVerificationError("PL0002", PreloadingVerificationMessages.PL0002_MoreThenOneLevel);
            }

            questionnaire.Questionnaire.ConnectChildrenWithParent();

            var errorsMessagess=
                from verifier in this.AtomicVerifiers
                let errors = verifier.Invoke(data, questionnaire.Questionnaire, questionnaireExportStructure)
                from error in errors
                select error;

            foreach (var preloadedDataVerificationError in errorsMessagess)
            {
                yield return preloadedDataVerificationError;
            }
        }

        private IEnumerable<Func<PreloadedDataByFile[],QuestionnaireDocument,QuestionnaireExportStructure, IEnumerable<PreloadedDataVerificationError>>> AtomicVerifiers
        {
            get
            {
                return new[]
                {
                    Verifier(CoulmnWasntMappedOnQuestionInTemplate, "PL0003",PreloadingVerificationMessages.PL0003_ColumnWasntMappedOnQuestion, PreloadedDataVerificationReferenceType.Column),
                    Verifier(FileWasntMappedOnQuestionnaireLevel, "PL0004", PreloadingVerificationMessages.PL0004_FileWasntMappedRoster, PreloadedDataVerificationReferenceType.File),
                    Verifier(QuestionWasntParsed, "PL0005", PreloadingVerificationMessages.PL0005_QuestionDataTypeMismatch, PreloadedDataVerificationReferenceType.Cell)
                };
            }
        }

        private IEnumerable<string> CoulmnWasntMappedOnQuestionInTemplate(PreloadedDataByFile levelData, QuestionnaireDocument questionnaire, QuestionnaireExportStructure exportStructure)
        {
            var levelExportStructure = rosterDataService.FindLevelInPreloadedData(levelData, exportStructure);
            if(levelExportStructure==null)
                yield break;

            foreach (var columnName in levelData.Header)
            {
                if(columnsToExcluteFromQuestionMapping.Contains(columnName))
                    continue;
                if (!levelExportStructure.HeaderItems.Values.Any(headerItem => headerItem.ColumnNames.Contains(columnName)))
                    yield return columnName;
            }
        }

        private IEnumerable<PreloadedDataVerificationReference> QuestionWasntParsed(PreloadedDataByFile levelData, Func<string, IQuestion> getQuestionByStataCaption, Func<Guid, IEnumerable<decimal>> getAnswerOptionsAsValues)
        {
            for (int y = 0; y < levelData.Content.Length; y++)
            {
                var row = levelData.Content[y];
                for (int x = 0; x < Math.Min(row.Length, levelData.Header.Length); x++)
                {
                    if(string.IsNullOrEmpty(row[x]))
                        continue;
                    if (columnsToExcluteFromQuestionMapping.Contains(levelData.Header[x]))
                        continue;
                    var parsedAnswer = questionDataParser.Parse(row[x], levelData.Header[x], getQuestionByStataCaption, getAnswerOptionsAsValues);
                    if (!parsedAnswer.HasValue)
                        yield return
                            new PreloadedDataVerificationReference(x, y, PreloadedDataVerificationReferenceType.Cell, row[x],
                                levelData.FileName);
                }
            }
        }

        private bool FileWasntMappedOnQuestionnaireLevel(PreloadedDataByFile levelData, QuestionnaireExportStructure exportStructure)
        {
            var levelExportStructure = rosterDataService.FindLevelInPreloadedData(levelData, exportStructure);
            if (levelExportStructure == null)
                return true;
            return false;
        }

        private Func<PreloadedDataByFile[], QuestionnaireDocument, QuestionnaireExportStructure, IEnumerable<PreloadedDataVerificationError>> Verifier(
            Func<PreloadedDataByFile, QuestionnaireDocument, QuestionnaireExportStructure, IEnumerable<string>> getErrors, string code, string message, PreloadedDataVerificationReferenceType type)
        {
            return (data, questionnaire, exportStructure) =>
                data.SelectMany(level => getErrors(level, questionnaire, exportStructure).Select(entity => new PreloadedDataVerificationError(code, message, new PreloadedDataVerificationReference(type, entity,level.FileName))));
        }

        private Func<PreloadedDataByFile[], QuestionnaireDocument, QuestionnaireExportStructure, IEnumerable<PreloadedDataVerificationError>> Verifier(
            Func<PreloadedDataByFile, QuestionnaireExportStructure, bool> hasErrors, string code, string message, PreloadedDataVerificationReferenceType type)
        {
            return (data, questionnaire, exportStructure) =>
                data.Where(level => hasErrors(level, exportStructure)).Select(
                    level =>
                        new PreloadedDataVerificationError(code, message,
                            new PreloadedDataVerificationReference(type, null, level.FileName)));
        }

        private Func<PreloadedDataByFile[], QuestionnaireDocument, QuestionnaireExportStructure, IEnumerable<PreloadedDataVerificationError>> Verifier(
           Func<PreloadedDataByFile, Func<string, IQuestion>, Func<Guid, IEnumerable<decimal>>, IEnumerable<PreloadedDataVerificationReference>> getErrors, string code, string message, PreloadedDataVerificationReferenceType type)
        {
            return (data, questionnaire, exportStructure) =>
            {
                IQuestionnaire questionnarie = questionnaireFactory.CreateTemporaryInstance(questionnaire);
                return
                    data.SelectMany(
                        level => getErrors(level, questionnarie.GetQuestionByStataCaption, questionnarie.GetAnswerOptionsAsValues)
                            .Select(
                                entity =>
                                    new PreloadedDataVerificationError(code, message, entity )));
            };
        }
    }
}
