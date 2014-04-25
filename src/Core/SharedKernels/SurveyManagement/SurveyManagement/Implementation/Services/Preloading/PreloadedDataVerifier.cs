using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Raven.Abstractions.Extensions;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Factories;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
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
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireRosterStructure> questionnaireRosterStructureStorage;

        private readonly IQuestionDataParser questionDataParser;

        private readonly IQuestionnaireFactory questionnaireFactory;
        private readonly string[] serviceColumns = { "Id", "ParentId" };
        private readonly IPreloadedDataServiceFactory preloadedDataServiceFactory;

        public PreloadedDataVerifier(
            IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage,
            IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireExportStructureStorage,
            IQuestionDataParser questionDataParser, IQuestionnaireFactory questionnaireFactory,
            IVersionedReadSideRepositoryReader<QuestionnaireRosterStructure> questionnaireRosterStructureStorage,
            IPreloadedDataServiceFactory preloadedDataServiceFactory)
        {
            this.questionnaireDocumentVersionedStorage = questionnaireDocumentVersionedStorage;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.questionDataParser = questionDataParser;
            this.questionnaireFactory = questionnaireFactory;
            this.questionnaireRosterStructureStorage = questionnaireRosterStructureStorage;
            this.preloadedDataServiceFactory = preloadedDataServiceFactory;
        }

        public IEnumerable<PreloadedDataVerificationError> Verify(Guid questionnaireId, long version, PreloadedDataByFile[] data)
        {
            var questionnaire = questionnaireDocumentVersionedStorage.GetById(questionnaireId, version);
            var questionnaireExportStructure = questionnaireExportStructureStorage.GetById(questionnaireId, version);
            var questionnaireRosterStructure = questionnaireRosterStructureStorage.GetById(questionnaireId, version);
            if (questionnaire == null || questionnaireExportStructure == null || questionnaireRosterStructure == null)
            {
                yield return new PreloadedDataVerificationError("PL0001", PreloadingVerificationMessages.PL0001_NoQuestionnaire);
                yield break;
            }

            questionnaire.Questionnaire.ConnectChildrenWithParent();

            var errorsMessagess =
                from verifier in this.AtomicVerifiers
                let errors =
                    verifier.Invoke(data, questionnaire.Questionnaire,
                        this.preloadedDataServiceFactory.CreatePreloadedDataService(questionnaireExportStructure,
                            questionnaireRosterStructure, questionnaire.Questionnaire))
                from error in errors
                select error;

            foreach (var preloadedDataVerificationError in errorsMessagess)
            {
                yield return preloadedDataVerificationError;
            }
        }

        private
            IEnumerable
                <Func<PreloadedDataByFile[], QuestionnaireDocument, IPreloadedDataService, IEnumerable<PreloadedDataVerificationError>>>
            AtomicVerifiers
        {
            get
            {
                return new[]
                {
                    Verifier(CoulmnWasntMappedOnQuestionInTemplate, "PL0003",
                        PreloadingVerificationMessages.PL0003_ColumnWasntMappedOnQuestion, PreloadedDataVerificationReferenceType.Column),
                    Verifier(FileWasntMappedOnQuestionnaireLevel, "PL0004", PreloadingVerificationMessages.PL0004_FileWasntMappedRoster,
                        PreloadedDataVerificationReferenceType.File),
                    Verifier(QuestionWasntParsed, "PL0005", PreloadingVerificationMessages.PL0005_QuestionDataTypeMismatch),
                    Verifier(IdDublication, "PL0006", PreloadingVerificationMessages.PL0006_IdDublication),
                    Verifier(ServiceColumnsAreAbsent, "PL0007", PreloadingVerificationMessages.PL0007_ServiceColumnIsAbsent,
                        PreloadedDataVerificationReferenceType.Column),
                    Verifier(OrphanRosters, "PL0008", PreloadingVerificationMessages.PL0008_OrphanRosterRecord),
                    Verifier(RosterIdIsInconsistantWithRosterSizeQuestion, "PL0009", PreloadingVerificationMessages.PL0009_RosterIdIsInconsistantWithRosterSizeQuestion)
                };
            }
        }

        private IEnumerable<string> CoulmnWasntMappedOnQuestionInTemplate(PreloadedDataByFile levelData, QuestionnaireDocument questionnaire,
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

        private IEnumerable<string> ServiceColumnsAreAbsent(PreloadedDataByFile levelData, QuestionnaireDocument questionnaire,
            IPreloadedDataService preloadedDataService)
        {
            foreach (var serviceColumn in serviceColumns)
            {
                if (!levelData.Header.Contains(serviceColumn))
                {
                    yield return serviceColumn;
                }
            }
        }

        private IEnumerable<PreloadedDataVerificationReference> OrphanRosters(PreloadedDataByFile levelData, PreloadedDataByFile[] allLevels,
            QuestionnaireDocument questionnaire, IPreloadedDataService preloadedDataService)
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
            PreloadedDataByFile[] allLevels, QuestionnaireDocument questionnaire, IPreloadedDataService preloadedDataService)
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
                decimal[] ids = preloadedDataService.GetAvalibleIdListForParent(parentDataFile,levelExportStructure.LevelId, parentIdValue);
                
                if(ids==null)
                    continue;
                
                decimal decimalId;
                if (!decimal.TryParse(idValue, out decimalId))
                    yield return
                        new PreloadedDataVerificationReference(idCoulmnIndexFile, y, PreloadedDataVerificationReferenceType.Cell, idValue,
                            levelData.FileName);
                if(!ids.Contains(decimalId))
                    yield return
                       new PreloadedDataVerificationReference(idCoulmnIndexFile, y, PreloadedDataVerificationReferenceType.Cell, idValue,
                           levelData.FileName);
            }
        }

        private IEnumerable<PreloadedDataVerificationReference> IdDublication(PreloadedDataByFile levelData,
            IPreloadedDataService preloadedDataService,
            Func<string, IQuestion> getQuestionByStataCaption, Func<Guid, IEnumerable<decimal>> getAnswerOptionsAsValues)
        {
            var idColumnIndex = preloadedDataService.GetIdColumnIndex(levelData);
            var parentIdColumnIndex = preloadedDataService.GetParentIdColumnIndex(levelData);
            
            if(idColumnIndex<0 || parentIdColumnIndex<0)
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

        private IEnumerable<PreloadedDataVerificationReference> QuestionWasntParsed(PreloadedDataByFile levelData,
            IPreloadedDataService preloadedDataService, Func<string, IQuestion> getQuestionByStataCaption, Func<Guid, IEnumerable<decimal>> getAnswerOptionsAsValues)
        {
            var presentQuestions = preloadedDataService.GetColumnIndexesGoupedByQuestionVariableName(levelData);
            
            if(presentQuestions==null)
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
                        var parsedAnswer = questionDataParser.Parse(answer, presentQuestion.Key, getQuestionByStataCaption, getAnswerOptionsAsValues);
                        if (!parsedAnswer.HasValue)
                            yield return
                                new PreloadedDataVerificationReference(answerIndex, y, PreloadedDataVerificationReferenceType.Cell, string.Format("{0}:{1}", levelData.Header[answerIndex], row[answerIndex]),
                                    levelData.FileName);
                    }
                }
            }
        }

        private bool FileWasntMappedOnQuestionnaireLevel(PreloadedDataByFile levelData, IPreloadedDataService preloadedDataService)
        {
            var levelExportStructure = preloadedDataService.FindLevelInPreloadedData(levelData.FileName);
            if (levelExportStructure == null)
                return true;
            return false;
        }

        private Func<PreloadedDataByFile[], QuestionnaireDocument, IPreloadedDataService, IEnumerable<PreloadedDataVerificationError>> Verifier(
            Func<PreloadedDataByFile, QuestionnaireDocument, IPreloadedDataService, IEnumerable<string>> getErrors, string code, string message, PreloadedDataVerificationReferenceType type)
        {
            return (data, questionnaire, rosterDataService) =>
                data.SelectMany(level => getErrors(level, questionnaire, rosterDataService).Select(entity => new PreloadedDataVerificationError(code, message, new PreloadedDataVerificationReference(type, entity, level.FileName))));
        }

        private Func<PreloadedDataByFile[], QuestionnaireDocument, IPreloadedDataService, IEnumerable<PreloadedDataVerificationError>> Verifier(
           Func<PreloadedDataByFile, IPreloadedDataService, bool> hasErrors, string code, string message, PreloadedDataVerificationReferenceType type)
        {
            return (data, questionnaire, rosterDataService) =>
                data.Where(level => hasErrors(level, rosterDataService)).Select(
                    level =>
                        new PreloadedDataVerificationError(code, message,
                            new PreloadedDataVerificationReference(type, null, level.FileName)));
        }

        private
            Func<PreloadedDataByFile[], QuestionnaireDocument, IPreloadedDataService, IEnumerable<PreloadedDataVerificationError>>
            Verifier(
            Func
                <PreloadedDataByFile, PreloadedDataByFile[], QuestionnaireDocument, IPreloadedDataService, IEnumerable<PreloadedDataVerificationReference>> getErrors, string code, string message)
        {
            return (data, questionnaire, rosterDataService) => data.SelectMany(
                level => getErrors(level, data, questionnaire, rosterDataService)
                    .Select(
                        entity =>
                            new PreloadedDataVerificationError(code, message, entity)));
        }

        private Func<PreloadedDataByFile[], QuestionnaireDocument, IPreloadedDataService, IEnumerable<PreloadedDataVerificationError>> Verifier(
           Func<PreloadedDataByFile, IPreloadedDataService, Func<string, IQuestion>, Func<Guid, IEnumerable<decimal>>, IEnumerable<PreloadedDataVerificationReference>> getErrors, string code, string message)
        {
            return (data, questionnaire, rosterDataService) =>
            {
                IQuestionnaire questionnarie = questionnaireFactory.CreateTemporaryInstance(questionnaire);
                return
                    data.SelectMany(
                        level => getErrors(level, rosterDataService, questionnarie.GetQuestionByStataCaption, questionnarie.GetAnswerOptionsAsValues)
                            .Select(
                                entity =>
                                    new PreloadedDataVerificationError(code, message, entity )));
            };
        }
    }
}
