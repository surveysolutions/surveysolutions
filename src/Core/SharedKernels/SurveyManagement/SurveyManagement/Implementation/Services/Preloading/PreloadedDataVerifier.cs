using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Properties;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading
{
    internal class PreloadedDataVerifier : IPreloadedDataVerifier
    {
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage;
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireExportStructureStorage;
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireRosterStructure> questionnaireRosterStructureStorage;

        private readonly string[] serviceColumns = { "Id", "ParentId" };
        private readonly IPreloadedDataServiceFactory preloadedDataServiceFactory;

        public PreloadedDataVerifier(
            IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage,
            IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireExportStructureStorage,
            IVersionedReadSideRepositoryReader<QuestionnaireRosterStructure> questionnaireRosterStructureStorage,
            IPreloadedDataServiceFactory preloadedDataServiceFactory)
        {
            this.questionnaireDocumentVersionedStorage = questionnaireDocumentVersionedStorage;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.questionnaireRosterStructureStorage = questionnaireRosterStructureStorage;
            this.preloadedDataServiceFactory = preloadedDataServiceFactory;
        }

        public IEnumerable<PreloadedDataVerificationError> VerifySample(Guid questionnaireId, long version, PreloadedDataByFile data)
        {
            var preloadedDataService = CreatePreloadedDataService(questionnaireId, version);
            if (preloadedDataService == null)
            {
                return new []{new PreloadedDataVerificationError("PL0001", PreloadingVerificationMessages.PL0001_NoQuestionnaire)};
            }
            var result = new List<PreloadedDataVerificationError>();
            var datas = new [] { data };
            result.AddRange(
                this.Verifier(this.CoulmnWasntMappedOnQuestionInTemplate, "PL0003",
                    PreloadingVerificationMessages.PL0003_ColumnWasntMappedOnQuestion, PreloadedDataVerificationReferenceType.Column)(datas,
                        preloadedDataService));
            result.AddRange(this.ErrorsByQuestionsWasntParsed(datas, preloadedDataService));
            return result;
        }

        public IEnumerable<PreloadedDataVerificationError> VerifyPanel(Guid questionnaireId, long version, PreloadedDataByFile[] data)
        {
            var preloadedDataService = CreatePreloadedDataService(questionnaireId, version);
            if (preloadedDataService == null)
            {
                yield return new PreloadedDataVerificationError("PL0001", PreloadingVerificationMessages.PL0001_NoQuestionnaire);
                yield break;
            }

            var errorsMessagess =
                from verifier in this.AtomicVerifiers
                let errors =
                    verifier.Invoke(data, preloadedDataService)
                from error in errors
                select error;

            foreach (var preloadedDataVerificationError in errorsMessagess)
            {
                yield return preloadedDataVerificationError;
            }
        }

        private IPreloadedDataService CreatePreloadedDataService(Guid questionnaireId, long version)
        {
            var questionnaire = this.questionnaireDocumentVersionedStorage.GetById(questionnaireId, version);
            var questionnaireExportStructure = this.questionnaireExportStructureStorage.GetById(questionnaireId, version);
            var questionnaireRosterStructure = this.questionnaireRosterStructureStorage.GetById(questionnaireId, version);
            if (questionnaire == null || questionnaireExportStructure == null || questionnaireRosterStructure == null)
            {
                return null;
            }

            questionnaire.Questionnaire.ConnectChildrenWithParent();

            return this.preloadedDataServiceFactory.CreatePreloadedDataService(questionnaireExportStructure,
                questionnaireRosterStructure, questionnaire.Questionnaire);
        }

        private IEnumerable<Func<PreloadedDataByFile[], IPreloadedDataService, IEnumerable<PreloadedDataVerificationError>>> AtomicVerifiers
        {
            get
            {
                return new[]
                {
                    this.Verifier(this.CoulmnWasntMappedOnQuestionInTemplate, "PL0003",
                        PreloadingVerificationMessages.PL0003_ColumnWasntMappedOnQuestion, PreloadedDataVerificationReferenceType.Column),
                    this.Verifier(this.FileWasntMappedOnQuestionnaireLevel, "PL0004",
                        PreloadingVerificationMessages.PL0004_FileWasntMappedRoster, PreloadedDataVerificationReferenceType.File),
                    this.Verifier(this.ServiceColumnsAreAbsent, "PL0007", PreloadingVerificationMessages.PL0007_ServiceColumnIsAbsent,
                        PreloadedDataVerificationReferenceType.Column),
                    this.Verifier(this.IdDuplication, "PL0006", PreloadingVerificationMessages.PL0006_IdDublication),
                    this.Verifier(this.OrphanRosters, "PL0008", PreloadingVerificationMessages.PL0008_OrphanRosterRecord),
                    this.Verifier(this.RosterIdIsInconsistantWithRosterSizeQuestion, "PL0009",
                        PreloadingVerificationMessages.PL0009_RosterIdIsInconsistantWithRosterSizeQuestion),
                    this.ErrorsByQuestionsWasntParsed
                };
            }
        }

        private IEnumerable<string> CoulmnWasntMappedOnQuestionInTemplate(PreloadedDataByFile levelData,
            IPreloadedDataService preloadedDataService)
        {
            var levelExportStructure = preloadedDataService.FindLevelInPreloadedData(levelData.FileName);
            if (levelExportStructure == null)
                yield break;
            foreach (var columnName in levelData.Header)
            {
                if (this.serviceColumns.Contains(columnName))
                    continue;
                if (!levelExportStructure.HeaderItems.Values.Any(headerItem => headerItem.ColumnNames.Contains(columnName)))
                    yield return columnName;
            }
        }

        private IEnumerable<string> FileWasntMappedOnQuestionnaireLevel(PreloadedDataByFile levelData,
            IPreloadedDataService preloadedDataService)
        {
            var levelExportStructure = preloadedDataService.FindLevelInPreloadedData(levelData.FileName);
            if (levelExportStructure == null)
                return new[] { "" };
            return Enumerable.Empty<string>();
        }

        private IEnumerable<string> ServiceColumnsAreAbsent(PreloadedDataByFile levelData, IPreloadedDataService preloadedDataService)
        {
            foreach (var serviceColumn in this.serviceColumns)
            {
                if (!levelData.Header.Contains(serviceColumn))
                {
                    yield return serviceColumn;
                }
            }
        }

        private IEnumerable<PreloadedDataVerificationReference> OrphanRosters(PreloadedDataByFile levelData, PreloadedDataByFile[] allLevels,
            IPreloadedDataService preloadedDataService)
        {
            var parentDataFile = preloadedDataService.GetParentDataFile(levelData.FileName, allLevels);

            if (parentDataFile == null)
                yield break;

            var parentIdColumnIndex = preloadedDataService.GetParentIdColumnIndex(levelData);
            var idCoulmnIndexInParentFile = preloadedDataService.GetIdColumnIndex(parentDataFile);
            var parentIds = parentDataFile.Content.Select(row => row[idCoulmnIndexInParentFile]).ToList();

            for (int y = 0; y < levelData.Content.Length; y++)
            {
                var parentIdValue = levelData.Content[y][parentIdColumnIndex];
                if (!parentIds.Contains(parentIdValue))
                    yield return
                        new PreloadedDataVerificationReference(parentIdColumnIndex, y, PreloadedDataVerificationReferenceType.Cell,
                            parentIdValue, levelData.FileName);
            }
        }

        private IEnumerable<PreloadedDataVerificationReference> RosterIdIsInconsistantWithRosterSizeQuestion(PreloadedDataByFile levelData,
            PreloadedDataByFile[] allLevels, IPreloadedDataService preloadedDataService)
        {
            var levelExportStructure = preloadedDataService.FindLevelInPreloadedData(levelData.FileName);
            if (levelExportStructure == null)
                yield break;

            var parentDataFile = preloadedDataService.GetParentDataFile(levelData.FileName, allLevels);

            if (parentDataFile == null)
                yield break;

            var idCoulmnIndexFile = preloadedDataService.GetIdColumnIndex(levelData);
            var parentIdColumnIndex = preloadedDataService.GetParentIdColumnIndex(levelData);
            for (int y = 0; y < levelData.Content.Length; y++)
            {
                var parentIdValue = levelData.Content[y][parentIdColumnIndex];
                var idValue = levelData.Content[y][idCoulmnIndexFile];
                decimal[] ids = preloadedDataService.GetAvalibleIdListForParent(parentDataFile, levelExportStructure.LevelScopeVector, parentIdValue);

                if (ids == null)
                    continue;

                decimal decimalId;
                if (!decimal.TryParse(idValue, out decimalId))
                    yield return
                        new PreloadedDataVerificationReference(idCoulmnIndexFile, y, PreloadedDataVerificationReferenceType.Cell, idValue,
                            levelData.FileName);
                if (!ids.Contains(decimalId))
                    yield return
                        new PreloadedDataVerificationReference(idCoulmnIndexFile, y, PreloadedDataVerificationReferenceType.Cell, idValue,
                            levelData.FileName);
            }
        }

        private IEnumerable<PreloadedDataVerificationReference> IdDuplication(PreloadedDataByFile levelData, PreloadedDataByFile[] allLevels,
            IPreloadedDataService preloadedDataService)
        {
            var idColumnIndex = preloadedDataService.GetIdColumnIndex(levelData);
            var parentIdColumnIndex = preloadedDataService.GetParentIdColumnIndex(levelData);

            if (idColumnIndex < 0 || parentIdColumnIndex < 0)
                yield break;

            var idAndParentContainer = new HashSet<KeyValuePair<string, string>>();
            for (int y = 0; y < levelData.Content.Length; y++)
            {
                var idValue = levelData.Content[y][idColumnIndex];
                if (string.IsNullOrEmpty(idValue))
                {
                    yield return
                        new PreloadedDataVerificationReference(idColumnIndex, y, PreloadedDataVerificationReferenceType.Cell, "",
                            levelData.FileName);
                    continue;
                }
                var parentIdValue = levelData.Content[y][parentIdColumnIndex];
                var idAndParentPair = new KeyValuePair<string, string>(idValue, parentIdValue);
                if (idAndParentContainer.Contains(idAndParentPair))
                {
                    yield return
                        new PreloadedDataVerificationReference(idColumnIndex, y, PreloadedDataVerificationReferenceType.Cell,
                            string.Format("id:{0}, parentId: {1}", idAndParentPair.Key, idAndParentPair.Value), levelData.FileName);
                    continue;
                }
                idAndParentContainer.Add(idAndParentPair);
            }
        }


        private IEnumerable<PreloadedDataVerificationError> ErrorsByQuestionsWasntParsed(PreloadedDataByFile[] allLevels,
            IPreloadedDataService preloadedDataService)
        {
            foreach (var levelData in allLevels)
            {
                var presentQuestions = preloadedDataService.GetColumnIndexesGoupedByQuestionVariableName(levelData);

                if (presentQuestions == null)
                    yield break;

                for (int y = 0; y < levelData.Content.Length; y++)
                {
                    var row = levelData.Content[y];
                    foreach (var presentQuestion in presentQuestions)
                    {
                        foreach (var answerIndex in presentQuestion.Value)
                        {
                            var answer = row[answerIndex];
                            if (string.IsNullOrEmpty(answer))
                                continue;

                            KeyValuePair<Guid, object> value;
                            var parsedResult = preloadedDataService.ParseQuestion(answer, presentQuestion.Key, out value);

                            switch (parsedResult)
                            {
                                case ValueParsingResult.OK:
                                    continue;
                                case ValueParsingResult.AnswerAsDecimalWasNotParsed:
                                    yield return
                                        new PreloadedDataVerificationError("PL0019",
                                            PreloadingVerificationMessages.PL0019_ExpectedDecimalNotParsed,
                                            new PreloadedDataVerificationReference(answerIndex, y,
                                                PreloadedDataVerificationReferenceType.Cell,
                                                string.Format("{0}:{1}", levelData.Header[answerIndex], row[answerIndex]),
                                                levelData.FileName));
                                    break;
                                case ValueParsingResult.AnswerAsIntWasNotParsed:
                                    yield return
                                        new PreloadedDataVerificationError("PL0018",
                                            PreloadingVerificationMessages.PL0018_ExpectedIntNotParsed,
                                            new PreloadedDataVerificationReference(answerIndex, y,
                                                PreloadedDataVerificationReferenceType.Cell,
                                                string.Format("{0}:{1}", levelData.Header[answerIndex], row[answerIndex]),
                                                levelData.FileName));
                                    break;
                                case ValueParsingResult.AnswerAsGpsWasNotParsed:
                                    yield return
                                        new PreloadedDataVerificationError("PL0017",
                                            PreloadingVerificationMessages.PL0017_ExpectedGpsNotParsed,
                                            new PreloadedDataVerificationReference(answerIndex, y,
                                                PreloadedDataVerificationReferenceType.Cell,
                                                string.Format("{0}:{1}", levelData.Header[answerIndex], row[answerIndex]),
                                                levelData.FileName));
                                    break;
                                case ValueParsingResult.AnswerAsDateTimeWasNotParsed:
                                    yield return
                                        new PreloadedDataVerificationError("PL0016",
                                            PreloadingVerificationMessages.PL0016_ExpectedDateTimeNotParsed,
                                            new PreloadedDataVerificationReference(answerIndex, y,
                                                PreloadedDataVerificationReferenceType.Cell,
                                                string.Format("{0}:{1}", levelData.Header[answerIndex], row[answerIndex]),
                                                levelData.FileName));
                                    break;
                                case ValueParsingResult.QuestionTypeIsIncorrect:
                                    yield return
                                        new PreloadedDataVerificationError("PL0015",
                                            PreloadingVerificationMessages.PL0015_QuestionTypeIsIncorrect,
                                            new PreloadedDataVerificationReference(answerIndex, y,
                                                PreloadedDataVerificationReferenceType.Cell,
                                                string.Format("{0}:{1}", levelData.Header[answerIndex], row[answerIndex]),
                                                levelData.FileName));
                                    break;
                                case ValueParsingResult.ParsedValueIsNotAllowed:
                                    yield return
                                        new PreloadedDataVerificationError("PL0014",
                                            PreloadingVerificationMessages.PL0014_ParsedValueIsNotAllowed,
                                            new PreloadedDataVerificationReference(answerIndex, y,
                                                PreloadedDataVerificationReferenceType.Cell,
                                                string.Format("{0}:{1}", levelData.Header[answerIndex], row[answerIndex]),
                                                levelData.FileName));
                                    break;
                                case ValueParsingResult.ValueIsNullOrEmpty:
                                    yield return
                                        new PreloadedDataVerificationError("PL0013",
                                            PreloadingVerificationMessages.PL0013_ValueIsNullOrEmpty,
                                            new PreloadedDataVerificationReference(answerIndex, y,
                                                PreloadedDataVerificationReferenceType.Cell,
                                                string.Format("{0}:{1}", levelData.Header[answerIndex], row[answerIndex]),
                                                levelData.FileName));
                                    break;
                                case ValueParsingResult.QuestionWasNotFound:
                                    yield return
                                        new PreloadedDataVerificationError("PL0012",
                                            PreloadingVerificationMessages.PL0012_QuestionWasNotFound,
                                            new PreloadedDataVerificationReference(answerIndex, y,
                                                PreloadedDataVerificationReferenceType.Cell,
                                                string.Format("{0}:{1}", levelData.Header[answerIndex], row[answerIndex]),
                                                levelData.FileName));
                                    break;
                                case ValueParsingResult.UnsupportedLinkedQuestion:
                                    yield return
                                        new PreloadedDataVerificationError("PL0010",
                                            PreloadingVerificationMessages.PL0010_UnsupportedLinkedQuestion,
                                            new PreloadedDataVerificationReference(answerIndex, y,
                                                PreloadedDataVerificationReferenceType.Cell,
                                                string.Format("{0}:{1}", levelData.Header[answerIndex], row[answerIndex]),
                                                levelData.FileName));
                                    break;
                                case ValueParsingResult.GeneralErrorOccured:
                                default:
                                    yield return
                                        new PreloadedDataVerificationError("PL0011", PreloadingVerificationMessages.PL0011_GeneralError,
                                            new PreloadedDataVerificationReference(answerIndex, y,
                                                PreloadedDataVerificationReferenceType.Cell,
                                                string.Format("{0}:{1}", levelData.Header[answerIndex], row[answerIndex]),
                                                levelData.FileName));
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private Func<PreloadedDataByFile[], IPreloadedDataService, IEnumerable<PreloadedDataVerificationError>> Verifier(
            Func<PreloadedDataByFile, IPreloadedDataService, IEnumerable<string>> getErrors, string code, string message,
            PreloadedDataVerificationReferenceType type)
        {
            return (data, rosterDataService) =>
                data.SelectMany(
                    level =>
                        getErrors(level, rosterDataService)
                            .Select(
                                entity =>
                                    new PreloadedDataVerificationError(code, message,
                                        new PreloadedDataVerificationReference(type, entity, level.FileName))));
        }

        private Func<PreloadedDataByFile[], IPreloadedDataService, IEnumerable<PreloadedDataVerificationError>> Verifier(
            Func<PreloadedDataByFile, PreloadedDataByFile[], IPreloadedDataService, IEnumerable<PreloadedDataVerificationReference>>
                getErrors, string code, string message)
        {
            return (data, rosterDataService) => data.SelectMany(
                level => getErrors(level, data, rosterDataService)
                    .Select(
                        entity =>
                            new PreloadedDataVerificationError(code, message, entity)));
        }
    }
}
