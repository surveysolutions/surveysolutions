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
        private readonly string[] serviceColumns = { "Id", "ParentId" };
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
                    Verifier(QuestionWasntParsed, "PL0005", PreloadingVerificationMessages.PL0005_QuestionDataTypeMismatch),
                    Verifier(IdDublication, "PL0006", PreloadingVerificationMessages.PL0006_IdDublication),
                    Verifier(ServiceColumnsAreAbsent, "PL0007",PreloadingVerificationMessages.PL0007_ServiceColumnIsAbsent, PreloadedDataVerificationReferenceType.Column)
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
                if(this.serviceColumns.Contains(columnName))
                    continue;
                if (!levelExportStructure.HeaderItems.Values.Any(headerItem => headerItem.ColumnNames.Contains(columnName)))
                    yield return columnName;
            }
        }

        private IEnumerable<string> ServiceColumnsAreAbsent(PreloadedDataByFile levelData, QuestionnaireDocument questionnaire,
            QuestionnaireExportStructure exportStructure)
        {
            foreach (var serviceColumn in serviceColumns )
            {
                if (!levelData.Header.Contains(serviceColumn))
                {
                    yield return serviceColumn;
                }
            }
        }

        private IEnumerable<PreloadedDataVerificationReference> OrphanRosters(PreloadedDataByFile levelData,PreloadedDataByFile[] allLevels, QuestionnaireDocument questionnaire,
            QuestionnaireExportStructure exportStructure)
        {
            yield break;
         /*   var levelExportStructure = rosterDataService.FindLevelInPreloadedData(levelData, exportStructure);
            if (levelExportStructure == null)
                yield break;

            Guid? parentLevelId = GetParentLevelId(levelExportStructure, questionnaire, exportStructure);
            if(!parentLevelId.HasValue)
                yield break;

            var parentLevel = exportStructure.HeaderToLevelMap.Values.FirstOrDefault(level => level.LevelId == parentLevelId);
            if (parentLevel == null)
                yield break;

            var parentDataFile = allLevels.FirstOrDefault(l => l.FileName.Contains(parentLevel.LevelName));
            if(parentDataFile==null)
                yield break;

            var parentIdColumnIndex = GetParentColumnIndex(levelData);
            var idCoulmnIndexInParentFile = GetIdColumnIndex(levelData);
            var parentIds =parentDataFile.Content.Select(row=>row[idCoulmnIndexInParentFile]).ToList();

            for (int y = 0; y < levelData.Content.Length; y++)
            {
                var parentIdValue = levelData.Content[y][parentIdColumnIndex];
                if (!parentIds.Contains(parentIdValue))
                    yield return
                        new PreloadedDataVerificationReference(parentIdColumnIndex, y, PreloadedDataVerificationReferenceType.Cell,
                            parentIdValue, levelData.FileName);
            }*/
        }

        private Guid? GetParentLevelId(HeaderStructureForLevel level, QuestionnaireDocument questionnaire,
            QuestionnaireExportStructure exportStructure)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<PreloadedDataVerificationReference> IdDublication(PreloadedDataByFile levelData,
            Func<string, IQuestion> getQuestionByStataCaption, Func<Guid, IEnumerable<decimal>> getAnswerOptionsAsValues)
        {
            var idColumnIndex = GetIdColumnIndex(levelData);
            var parentIdColumnIndex = GetParentColumnIndex(levelData);
            var idAndParentContainer = new HashSet<KeyValuePair<string,string>>();
            for (int y = 0; y < levelData.Content.Length; y++)
            {
                var idValue = levelData.Content[y][idColumnIndex];
                if (string.IsNullOrEmpty(idValue))
                {
                    yield return new PreloadedDataVerificationReference(idColumnIndex,y,PreloadedDataVerificationReferenceType.Cell,"",levelData.FileName);
                    continue;
                }
                var idAndParentPair = new KeyValuePair<string, string>(idValue, levelData.Content[y][parentIdColumnIndex]);
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

        private static int GetIdColumnIndex(PreloadedDataByFile levelData)
        {
            return levelData.Header.ToList().FindIndex(header => header == "Id");
        }

        private static int GetParentColumnIndex(PreloadedDataByFile levelData)
        {
            return levelData.Header.ToList().FindIndex(header => header == "ParentId");
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
                    if (this.serviceColumns.Contains(levelData.Header[x]))
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
           Func<PreloadedDataByFile, Func<string, IQuestion>, Func<Guid, IEnumerable<decimal>>, IEnumerable<PreloadedDataVerificationReference>> getErrors, string code, string message)
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
