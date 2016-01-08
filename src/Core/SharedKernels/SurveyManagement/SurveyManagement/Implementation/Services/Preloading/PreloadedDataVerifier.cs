using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Properties;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Core.SharedKernels.SurveySolutions.Implementation.ServiceVariables;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading
{
    internal class PreloadedDataVerifier : IPreloadedDataVerifier
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage;

        private readonly IUserViewFactory userViewFactory;
        
        private readonly IPreloadedDataServiceFactory preloadedDataServiceFactory;

        public PreloadedDataVerifier(
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage,
            IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage,
            IPreloadedDataServiceFactory preloadedDataServiceFactory,
            IUserViewFactory userViewFactory)
        {
            this.questionnaireDocumentVersionedStorage = questionnaireDocumentVersionedStorage;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.questionnaireRosterStructureStorage = questionnaireRosterStructureStorage;
            this.preloadedDataServiceFactory = preloadedDataServiceFactory;
            this.userViewFactory = userViewFactory;

        }

        public VerificationStatus VerifySample(Guid questionnaireId, long version, PreloadedDataByFile data)
        {
            VerificationStatus status = new VerificationStatus(); 

            if (data == null)
            {
                status.Errors = new[] { new PreloadedDataVerificationError("PL0024", PreloadingVerificationMessages.PL0024_DataWasNotFound) };
                return status;
            }

            var preloadedDataService = CreatePreloadedDataService(questionnaireId, version);
            if (preloadedDataService == null)
            {
                status.Errors = new[] { new PreloadedDataVerificationError("PL0001", PreloadingVerificationMessages.PL0001_NoQuestionnaire) };
                return status;
            }
            var errors = new List<PreloadedDataVerificationError>();

            var datas = new[] { new PreloadedDataByFile(data.Id, preloadedDataService.GetValidFileNameForTopLevelQuestionnaire(), data.Header, data.Content) };

            errors.AddRange(
                this.Verifier(this.CoulmnWasntMappedOnQuestionInTemplate, "PL0003",
                    PreloadingVerificationMessages.PL0003_ColumnWasntMappedOnQuestion, PreloadedDataVerificationReferenceType.Column)(datas,
                        preloadedDataService));

            if(ShouldVerificationBeContinued(errors))
                errors.AddRange(this.ErrorsByQuestionsWasntParsed(datas, preloadedDataService));

            if (ShouldVerificationBeContinued(errors))
                errors.AddRange(this.ErrorsByResposibleName(datas, preloadedDataService, acceptInterviewers: true));

            if (ShouldVerificationBeContinued(errors))
                errors.AddRange(this.Verifier(this.ErrorsByGpsQuestions, QuestionType.GpsCoordinates)(datas, preloadedDataService));

            if (ShouldVerificationBeContinued(errors))
                errors.AddRange(this.Verifier(this.ErrorsByNumericQuestions, QuestionType.Numeric)(datas, preloadedDataService));

            if (ShouldVerificationBeContinued(errors))
                errors.AddRange(this.Verifier(this.ColumnDuplications)(datas, preloadedDataService));

            status.Errors = errors.Count > 100 ? errors.Take(100).ToList() : errors;

            var supervisorNameIndex = preloadedDataService.GetColumnIndexByHeaderName(data, ServiceColumns.ResponsibleColumnName);
            status.WasSupervisorProvided = supervisorNameIndex >= 0;

            return status;
        }

        private bool ShouldVerificationBeContinued(List<PreloadedDataVerificationError> errors)
        {
            return errors.Count < 100;
        }

        public VerificationStatus VerifyPanel(Guid questionnaireId, long version, PreloadedDataByFile[] data)
        {
            VerificationStatus status = new VerificationStatus();

            if (data == null || !data.Any())
            {
                status.Errors = new[]{ new PreloadedDataVerificationError("PL0024", PreloadingVerificationMessages.PL0024_DataWasNotFound) };
                return status;
            }

            var preloadedDataService = CreatePreloadedDataService(questionnaireId, version);
            if (preloadedDataService == null)
            {
                status.Errors = new[]{ new PreloadedDataVerificationError("PL0001", PreloadingVerificationMessages.PL0001_NoQuestionnaire)};
                return status;
            }

            var errorsMessagess =
                from verifier in this.AtomicVerifiers
                let errors =
                    verifier.Invoke(data, preloadedDataService)
                from error in errors
                select error;

            status.Errors = errorsMessagess.ToArray();

            var topLevel = preloadedDataService.GetTopLevelData(data);

            var supervisorNameIndex = preloadedDataService.GetColumnIndexByHeaderName(topLevel, ServiceColumns.ResponsibleColumnName);
            status.WasSupervisorProvided = supervisorNameIndex >= 0;

            return status;
        }
        
        private IPreloadedDataService CreatePreloadedDataService(Guid questionnaireId, long version)
        {
            var questionnaire = this.questionnaireDocumentVersionedStorage.AsVersioned().Get(questionnaireId.FormatGuid(), version);
            var questionnaireExportStructure = this.questionnaireExportStructureStorage.AsVersioned().Get(questionnaireId.FormatGuid(), version);
            var questionnaireRosterStructure = this.questionnaireRosterStructureStorage.AsVersioned().Get(questionnaireId.FormatGuid(), version);

            if (questionnaireExportStructure == null || questionnaireRosterStructure == null || questionnaire == null)
            {
                return null;
            }

            questionnaire.Questionnaire.ConnectChildrenWithParent();
            return this.preloadedDataServiceFactory. CreatePreloadedDataService(questionnaireExportStructure,
                questionnaireRosterStructure, questionnaire.Questionnaire);
        }

        private IEnumerable<Func<PreloadedDataByFile[], IPreloadedDataService, IEnumerable<PreloadedDataVerificationError>>> AtomicVerifiers
        {
            get
            {
                return new[]
                {
                    this.Verifier(this.CoulmnWasntMappedOnQuestionInTemplate, "PL0003",
                        PreloadingVerificationMessages.PL0003_ColumnWasntMappedOnQuestion,
                        PreloadedDataVerificationReferenceType.Column),
                    this.Verifier(this.FileWasntMappedOnQuestionnaireLevel, "PL0004",
                        PreloadingVerificationMessages.PL0004_FileWasntMappedRoster,
                        PreloadedDataVerificationReferenceType.File),
                    this.Verifier(this.ServiceColumnsAreAbsent, "PL0007",
                        PreloadingVerificationMessages.PL0007_ServiceColumnIsAbsent,
                        PreloadedDataVerificationReferenceType.Column),
                    this.Verifier(this.IdDuplication, "PL0006", PreloadingVerificationMessages.PL0006_IdDublication),
                    this.Verifier(this.OrphanRosters, "PL0008", PreloadingVerificationMessages.PL0008_OrphanRosterRecord),
                    this.Verifier(this.RosterIdIsInconsistencyWithRosterSizeQuestion, "PL0009",
                        PreloadingVerificationMessages.PL0009_RosterIdIsInconsistantWithRosterSizeQuestion),
                    this.ErrorsByQuestionsWasntParsed,
                    this.Verifier(this.ColumnDuplications),
                    this.Verifier(this.ErrorsByGpsQuestions, QuestionType.GpsCoordinates),
                    this.Verifier(this.ErrorsByNumericQuestions, QuestionType.Numeric),
                    
                    this.ErrorsBySupervisorName
                };
            }
        }

        private IEnumerable<string> CoulmnWasntMappedOnQuestionInTemplate(PreloadedDataByFile levelData,
            IPreloadedDataService preloadedDataService)
        {
            var levelExportStructure = preloadedDataService.FindLevelInPreloadedData(levelData.FileName);
            if (levelExportStructure == null)
                yield break;
            var referenceNames = levelExportStructure.ReferencedNames ?? new string[0];
            var listOfParentIdColumns = this.GetListOfParentIdColumns(levelData, levelExportStructure).ToArray();
            var listOfPermittedExtraColumns = this.GetListOfPermittedExtraColumnsForLevel(levelExportStructure).ToArray(); 

            foreach (var columnName in levelData.Header)
            {
                if (string.Equals(columnName, ServiceColumns.Id, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                if (listOfParentIdColumns.Contains(columnName))
                    continue;

                if (referenceNames.Contains(columnName))
                    continue;

                if (listOfPermittedExtraColumns.Contains(columnName))
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
            var levelExportStructure = preloadedDataService.FindLevelInPreloadedData(levelData.FileName);
            if (levelExportStructure == null)
                yield break;

            if (levelData.Header.Count(h => string.Equals(h, ServiceColumns.Id, StringComparison.InvariantCultureIgnoreCase)) != 1)
                yield return ServiceColumns.Id;

            var listOfParentIdColumns = this.GetListOfParentIdColumns(levelData, levelExportStructure);
            foreach (var parentIdColumn in listOfParentIdColumns)
            {
                if (levelData.Header.Count(h => string.Equals(h, parentIdColumn, StringComparison.InvariantCultureIgnoreCase)) != 1)
                    yield return parentIdColumn;
            }
        }

        private IEnumerable<string> GetListOfParentIdColumns(PreloadedDataByFile levelData, HeaderStructureForLevel levelExportStructure)
        {
            if (levelExportStructure.LevelScopeVector == null || levelExportStructure.LevelScopeVector.Length == 0)
                yield break;

            var columnsStartWithParentId =
                levelData.Header.Where(h => h.StartsWith(ServiceColumns.ParentId, StringComparison.InvariantCultureIgnoreCase)).ToList();

            var listOfAvailableParentIdIndexes = levelExportStructure.LevelScopeVector.Select((l, i) => i + 1).ToArray();

            foreach (var columnStartWithParentId in columnsStartWithParentId)
            {
                var parentNumberString = columnStartWithParentId.Substring(ServiceColumns.ParentId.Length);
                int parentNumber;
                if (int.TryParse(parentNumberString, out parentNumber))
                {
                    if (listOfAvailableParentIdIndexes.Contains(parentNumber))
                        yield return columnStartWithParentId;
                }
            }
        }

        private IEnumerable<string> GetListOfPermittedExtraColumnsForLevel(HeaderStructureForLevel levelExportStructure)
        {
            if (levelExportStructure.LevelScopeVector == null || levelExportStructure.LevelScopeVector.Length == 0)
            {
                yield return ServiceColumns.ResponsibleColumnName;
            }
            
        }

        private IEnumerable<PreloadedDataVerificationReference> OrphanRosters(PreloadedDataByFile levelData, PreloadedDataByFile[] allLevels,
            IPreloadedDataService preloadedDataService)
        {
            var parentDataFile = preloadedDataService.GetParentDataFile(levelData.FileName, allLevels);

            if (parentDataFile == null)
                yield break;

            var parentIdColumnIndexes = preloadedDataService.GetParentIdColumnIndexes(levelData);
            var parentIdColumnIndexesForParentDataFile = preloadedDataService.GetParentIdColumnIndexes(parentDataFile);
            var idColumnIndexInParentFile = preloadedDataService.GetIdColumnIndex(parentDataFile);

            if (idColumnIndexInParentFile < 0 || parentIdColumnIndexes == null || parentIdColumnIndexes.Length==0 ||
                parentIdColumnIndexesForParentDataFile == null)
                yield break;

            var parentIds =
                parentDataFile.Content.Select(
                    row =>
                        JoinRowIdWithParentIdsInParentIdsVector(row[idColumnIndexInParentFile],
                            CreateParentIdsVector(row, parentIdColumnIndexesForParentDataFile))).ToList();
            
            for (int y = 0; y < levelData.Content.Length; y++)
            {

                var parentIdValues = CreateParentIdsVector(levelData.Content[y], parentIdColumnIndexes);
                if (!parentIds.Any(p => p.SequenceEqual(parentIdValues)))
                    yield return
                        new PreloadedDataVerificationReference(parentIdColumnIndexes.First(), y, PreloadedDataVerificationReferenceType.Cell,
                            string.Join(",", parentIdValues), levelData.FileName);
            }
        }

        private string[] JoinRowIdWithParentIdsInParentIdsVector(string id, string[] parentIds)
        {
            var result = new string[parentIds.Count() + 1];
            result[0] = id;
            for (int i = 1; i < result.Length; i++)
            {
                result[i] = parentIds[i - 1];
            }
            return result;
        }

        private IEnumerable<PreloadedDataVerificationReference> RosterIdIsInconsistencyWithRosterSizeQuestion(PreloadedDataByFile levelData,
            PreloadedDataByFile[] allLevels, IPreloadedDataService preloadedDataService)
        {
            var levelExportStructure = preloadedDataService.FindLevelInPreloadedData(levelData.FileName);
            if (levelExportStructure == null)
                yield break;

            var parentDataFile = preloadedDataService.GetParentDataFile(levelData.FileName, allLevels);

            if (parentDataFile == null)
                yield break;

            var idCoulmnIndexFile = preloadedDataService.GetIdColumnIndex(levelData);
            var parentIdColumnIndexes = preloadedDataService.GetParentIdColumnIndexes(levelData);

            if (idCoulmnIndexFile < 0 || parentIdColumnIndexes == null)
                yield break;

            for (int y = 0; y < levelData.Content.Length; y++)
            {
                var idValue = levelData.Content[y][idCoulmnIndexFile];
                
                decimal[] ids = preloadedDataService.GetAvailableIdListForParent(parentDataFile, levelExportStructure.LevelScopeVector,
                    CreateParentIdsVector(levelData.Content[y], parentIdColumnIndexes));

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

        private string[] CreateParentIdsVector(string[] content, int[] parentIdColumnIndexes)
        {
            return parentIdColumnIndexes.Select(parentIdColumnIndex => content[parentIdColumnIndex]).ToArray();
        }

        private IEnumerable<PreloadedDataVerificationReference> IdDuplication(PreloadedDataByFile levelData, PreloadedDataByFile[] allLevels,
            IPreloadedDataService preloadedDataService)
        {
            var idColumnIndex = preloadedDataService.GetIdColumnIndex(levelData);
            var parentIdColumnIndexes = preloadedDataService.GetParentIdColumnIndexes(levelData);

            if (idColumnIndex < 0 || parentIdColumnIndexes == null)
                yield break;

            var idAndParentContainer = new HashSet<string>();
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
                var parentIdValue = string.Join(",", parentIdColumnIndexes.Select(parentidIndex => levelData.Content[y][parentidIndex]));
                string itemKey = String.Format("{0}\t{1}", idValue, parentIdValue);
                if (idAndParentContainer.Contains(itemKey))
                {
                    yield return
                        new PreloadedDataVerificationReference(idColumnIndex, y, PreloadedDataVerificationReferenceType.Cell,
                            string.Format("id:{0}, parentId: {1}", idValue, parentIdValue), levelData.FileName);
                    continue;
                }
                idAndParentContainer.Add(itemKey);
            }
        }

        private IEnumerable<PreloadedDataVerificationError> ColumnDuplications(
            PreloadedDataByFile preloadedDataByFile,
            IPreloadedDataService preloadedDataService)
        {
            var columnNameOnCountOfOccurrenceMap = new Dictionary<string, List<PreloadedDataVerificationReference>>();

            for (int i = 0; i < preloadedDataByFile.Header.Length; i++)
            {
                var header = preloadedDataByFile.Header[i];

                var headerLowerCase = header.Trim().ToLower();
                if (!columnNameOnCountOfOccurrenceMap.ContainsKey(headerLowerCase))
                    columnNameOnCountOfOccurrenceMap[headerLowerCase] = new List<PreloadedDataVerificationReference>();

                columnNameOnCountOfOccurrenceMap[headerLowerCase].Add(CreateReference(i, preloadedDataByFile));
            }

            return
                columnNameOnCountOfOccurrenceMap.Where(c => c.Value.Count > 1).Select(
                    columnsWithDuplicate =>
                        new PreloadedDataVerificationError("PL0031",
                            PreloadingVerificationMessages.PL0031_ColumnNameDuplicatesFound,
                            columnsWithDuplicate.Value.ToArray()));
        }

        private IEnumerable<PreloadedDataVerificationError> ErrorsByGpsQuestions(
            HeaderStructureForLevel level,
            ExportedHeaderItem gpsExportedQuestion,
            PreloadedDataByFile levelData,
            IPreloadedDataService preloadedDataService)
        {
            var gpsAnswersColumnIndexes = GetColumnIndexesWhichContainsAnswersOnQuestion(gpsExportedQuestion, levelData, preloadedDataService);

            for (int rowIndex = 0; rowIndex < levelData.Content.Length; rowIndex++)
            {
                var latitudeColumnIndex = preloadedDataService.GetColumnIndexByHeaderName(levelData, string.Format("{0}__latitude", gpsExportedQuestion.VariableName));
                var longitudeColumnIndex = preloadedDataService.GetColumnIndexByHeaderName(levelData, string.Format("{0}__longitude", gpsExportedQuestion.VariableName));

                var latitude = latitudeColumnIndex < 0
                    ? null
                    : GetValue<double>(levelData.Content[rowIndex], levelData.Header,
                        latitudeColumnIndex, level, preloadedDataService);
                var longitude = longitudeColumnIndex < 0
                    ? null
                    : GetValue<double>(levelData.Content[rowIndex], levelData.Header,
                        longitudeColumnIndex, level, preloadedDataService);

                if (!latitude.HasValue || !longitude.HasValue)
                {
                    var columnsIdsWithNotEmptyValuesInRow =
                        gpsAnswersColumnIndexes.Where(i => !string.IsNullOrEmpty(levelData.Content[rowIndex][i]))
                            .ToArray();

                    foreach (var notEmptyColumnIndex in columnsIdsWithNotEmptyValuesInRow)
                    {
                        yield return new PreloadedDataVerificationError("PL0030",
                            PreloadingVerificationMessages.PL0030_GpsMandatoryFilds,
                            CreateReference(notEmptyColumnIndex, rowIndex, levelData));
                    }
                }

                if (latitude.HasValue && (latitude.Value < -91 || latitude.Value > 90))
                    yield return new PreloadedDataVerificationError("PL0032",
                        PreloadingVerificationMessages
                            .PL0032_LatitudeMustBeGeaterThenN90AndLessThen90,
                        CreateReference(latitudeColumnIndex, rowIndex, levelData));

                if (longitude.HasValue && (longitude.Value < -181 || longitude.Value > 180))
                    yield return new PreloadedDataVerificationError("PL0033",
                        PreloadingVerificationMessages
                            .PL0033_LongitudeMustBeGeaterThenN180AndLessThen180,
                        CreateReference(longitudeColumnIndex, rowIndex, levelData));
            }
        }

        private IEnumerable<PreloadedDataVerificationError> ErrorsByNumericQuestions(
            HeaderStructureForLevel level,
            ExportedHeaderItem numericExportedQuestion,
            PreloadedDataByFile levelData,
            IPreloadedDataService preloadedDataService)
        {
            if (!preloadedDataService.IsQuestionRosterSize(numericExportedQuestion.VariableName))
                yield break;

            var columnIndex = preloadedDataService.GetColumnIndexByHeaderName(levelData,
                numericExportedQuestion.VariableName);

            if (columnIndex < 0)
                yield break;

            for (int rowIndex = 0; rowIndex < levelData.Content.Length; rowIndex++)
            {
                var parsedValue = GetValue<int>(levelData.Content[rowIndex], levelData.Header,
                    columnIndex, level, preloadedDataService);

                if (!parsedValue.HasValue)
                    continue;

                if (parsedValue < 0)
                {
                    yield return new PreloadedDataVerificationError("PL0022",
                        PreloadingVerificationMessages
                            .PL0022_AnswerIsIncorrectBecauseIsRosterSizeAndNegative,
                        CreateReference(columnIndex, rowIndex, levelData));
                }

                if (parsedValue > Constants.MaxRosterRowCount)
                {
                    {
                        yield return new PreloadedDataVerificationError("PL0029",
                            PreloadingVerificationMessages
                                .PL0029_AnswerIsIncorrectBecauseIsRosterSizeAndMoreThan40,
                            CreateReference(columnIndex, rowIndex, levelData));
                    }
                }
            }
        }

        private int[] GetColumnIndexesWhichContainsAnswersOnQuestion(
            ExportedHeaderItem exportedQuestion,
            PreloadedDataByFile levelData,
            IPreloadedDataService preloadedDataService)
        {
            var result = new List<int>();

            foreach (var columnName in exportedQuestion.ColumnNames)
            {
                var columnIndex = preloadedDataService.GetColumnIndexByHeaderName(levelData, columnName);
                if (columnIndex >= 0)
                    result.Add(columnIndex);
            }

            return result.ToArray();
        }

        private T? GetValue<T>(string[] row, string[] header, int columnIndex, HeaderStructureForLevel level, IPreloadedDataService preloadedDataService) where T : struct
        {
            object valueParseResult;

            ValueParsingResult parseResult = preloadedDataService.ParseQuestionInLevel(row[columnIndex],
                header[columnIndex], level, out valueParseResult);

            if (parseResult != ValueParsingResult.OK)
                return null;
            try
            {
                return (T?) valueParseResult;
            }
            catch (InvalidCastException)
            {
                return null;
            }
        }
        private IEnumerable<PreloadedDataVerificationError> ErrorsByQuestionsWasntParsed(PreloadedDataByFile[] allLevels,
            IPreloadedDataService preloadedDataService)
        {
            foreach (var levelData in allLevels)
            {
                var exportedLevel = preloadedDataService.FindLevelInPreloadedData(levelData.FileName);
                if(exportedLevel==null)
                    continue;

                for (int columnIndex = 0; columnIndex < levelData.Header.Length; columnIndex++)
                {
                    var columnName = levelData.Header[columnIndex];

                    for (int rowIndex = 0; rowIndex < levelData.Content.Length; rowIndex++)
                    {
                        var row = levelData.Content[rowIndex];
                        var answer = row[columnIndex];
                        if (string.IsNullOrEmpty(answer))
                            continue;

                        object parsedValue;
                        var parsedResult = preloadedDataService.ParseQuestionInLevel(answer, columnName, exportedLevel, out parsedValue);

                        switch (parsedResult)
                        {
                            case ValueParsingResult.OK:
                                continue;

                            case ValueParsingResult.AnswerAsDecimalWasNotParsed:
                                yield return
                                    new PreloadedDataVerificationError("PL0019",
                                        PreloadingVerificationMessages.PL0019_ExpectedDecimalNotParsed,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.CommaIsUnsupportedInAnswer:
                                yield return
                                    new PreloadedDataVerificationError("PL0034",
                                        PreloadingVerificationMessages.PL0034_CommaSymbolIsNotAllowedInNumericAnswer,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.AnswerAsIntWasNotParsed:
                                yield return
                                    new PreloadedDataVerificationError("PL0018",
                                        PreloadingVerificationMessages.PL0018_ExpectedIntNotParsed,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.AnswerAsGpsWasNotParsed:
                                yield return
                                    new PreloadedDataVerificationError("PL0017",
                                        PreloadingVerificationMessages.PL0017_ExpectedGpsNotParsed,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.AnswerAsDateTimeWasNotParsed:
                                yield return
                                    new PreloadedDataVerificationError("PL0016",
                                        PreloadingVerificationMessages.PL0016_ExpectedDateTimeNotParsed,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.QuestionTypeIsIncorrect:
                                yield return
                                    new PreloadedDataVerificationError("PL0015",
                                        PreloadingVerificationMessages.PL0015_QuestionTypeIsIncorrect,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.ParsedValueIsNotAllowed:
                                yield return
                                    new PreloadedDataVerificationError("PL0014",
                                        PreloadingVerificationMessages.PL0014_ParsedValueIsNotAllowed,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.ValueIsNullOrEmpty:
                                yield return
                                    new PreloadedDataVerificationError("PL0013",
                                        PreloadingVerificationMessages.PL0013_ValueIsNullOrEmpty,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.QuestionWasNotFound:
                                yield return
                                    new PreloadedDataVerificationError("PL0012",
                                        PreloadingVerificationMessages.PL0012_QuestionWasNotFound,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.UnsupportedLinkedQuestion:
                                yield return
                                    new PreloadedDataVerificationError("PL0010",
                                        PreloadingVerificationMessages.PL0010_UnsupportedLinkedQuestion,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.UnsupportedMultimediaQuestion:
                                yield return
                                    new PreloadedDataVerificationError("PL0023",
                                        PreloadingVerificationMessages.PL0023_UnsupportedMultimediaQuestion,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.GeneralErrorOccured:
                            default:
                                yield return
                                    new PreloadedDataVerificationError("PL0011",
                                        PreloadingVerificationMessages.PL0011_GeneralError,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                        }
                    }
                }
            }
        }

        private Func<PreloadedDataByFile[], IPreloadedDataService, IEnumerable<PreloadedDataVerificationError>> Verifier(
            Func<PreloadedDataByFile, IPreloadedDataService, IEnumerable<PreloadedDataVerificationError>> fileValidator)
        {
            return (data, preloadedDataService) =>
            {
                return data.SelectMany(level => fileValidator(level, preloadedDataService));
            };
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
            Func<HeaderStructureForLevel,ExportedHeaderItem, PreloadedDataByFile, IPreloadedDataService, IEnumerable<PreloadedDataVerificationError>> exportedQuestionVerifier,
            QuestionType questionType)
        {
            return (datas, preloadedDataService) =>
            {
                var result = new List<PreloadedDataVerificationError>();

                foreach (var levelData in datas)
                {
                    var levelExportStructure = preloadedDataService.FindLevelInPreloadedData(levelData.FileName);
                    if (levelExportStructure == null)
                        continue;

                    var exportedQuestions =
                        levelExportStructure.HeaderItems.Values.Where(h => h.QuestionType == questionType);

                    foreach (var exportedQuestion in exportedQuestions)
                    {
                        result.AddRange(exportedQuestionVerifier(levelExportStructure, exportedQuestion, levelData, preloadedDataService));
                    }
                }
                return result;
            };
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

        private PreloadedDataVerificationReference CreateReference(int y, PreloadedDataByFile levelData)
        {
            return new PreloadedDataVerificationReference(null, y,
                PreloadedDataVerificationReferenceType.Column,
                levelData.Header[y],
                levelData.FileName);
        }

        private PreloadedDataVerificationReference CreateReference(int x, int y, PreloadedDataByFile levelData)
        {
            return new PreloadedDataVerificationReference(x, y,
                PreloadedDataVerificationReferenceType.Cell,
                string.Format("{0}:{1}", levelData.Header[x],
                    levelData.Content[y][x]),
                levelData.FileName);
        }

        private IEnumerable<PreloadedDataVerificationError> ErrorsBySupervisorName(PreloadedDataByFile[] allLevels,
            IPreloadedDataService preloadedDataService)
        {
            return ErrorsByResposibleName(allLevels, preloadedDataService, false);
        }
        private IEnumerable<PreloadedDataVerificationError> ErrorsByResposibleName(PreloadedDataByFile[] allLevels, IPreloadedDataService preloadedDataService, bool acceptInterviewers)
        {
            foreach (var levelData in allLevels)
            {
                var supervisorNameIndex = preloadedDataService.GetColumnIndexByHeaderName(levelData, ServiceColumns.ResponsibleColumnName);

                if (supervisorNameIndex < 0)
                    continue;

                var supervisorCache = new Dictionary<string, Tuple<bool, bool>>();

                for (int y = 0; y < levelData.Content.Length; y++)
                {
                    var row = levelData.Content[y];
                    var name = row[supervisorNameIndex];

                    if (String.IsNullOrWhiteSpace(name))
                    {
                        yield return
                            new PreloadedDataVerificationError("PL0025",
                                PreloadingVerificationMessages.PL0025_SupervisorNameIsEmpty,
                                new PreloadedDataVerificationReference(supervisorNameIndex, y,
                                    PreloadedDataVerificationReferenceType.Cell,
                                    "",
                                    levelData.FileName));
                        continue;
                    }

                    var userState = GetUserStateAndUpdateCache(supervisorCache, name, acceptInterviewers);

                    if (userState == null)
                    {

                        yield return
                            new PreloadedDataVerificationError("PL0026",
                                PreloadingVerificationMessages.PL0026_SupervisorWasNotFound,
                                new PreloadedDataVerificationReference(supervisorNameIndex, y,
                                    PreloadedDataVerificationReferenceType.Cell,
                                    "",
                                    levelData.FileName));
                        continue;
                    }
                    if (userState.Item1)
                    {
                        yield return
                            new PreloadedDataVerificationError("PL0027",
                                PreloadingVerificationMessages.PL0027_SupervisorIsLocked,
                                new PreloadedDataVerificationReference(supervisorNameIndex, y,
                                    PreloadedDataVerificationReferenceType.Cell,
                                    "",
                                    levelData.FileName));
                        continue;
                    }
                    if (!userState.Item2)
                    {
                        yield return
                            new PreloadedDataVerificationError("PL0028",
                                PreloadingVerificationMessages.PL0028_UserIsNotSupervisor,
                                new PreloadedDataVerificationReference(supervisorNameIndex, y,
                                    PreloadedDataVerificationReferenceType.Cell,
                                    "",
                                    levelData.FileName));
                    }
                }
            }
        }

        private Tuple<bool, bool> GetUserStateAndUpdateCache(Dictionary<string, Tuple<bool, bool>> supervisorCache, string name, bool acceptInterviewers)
        {
            if (supervisorCache.ContainsKey(name))
                return supervisorCache[name];
            
            var user = userViewFactory.Load(new UserViewInputModel(UserName: name, UserEmail: null));
            if (user == null || user.IsArchived)
            {
                supervisorCache.Add(name, null);
                return null;
            }
            
            var item = new Tuple<bool, bool>(user.IsLockedByHQ, user.IsSupervisor() || (acceptInterviewers && user.Roles.Any(role => role == UserRoles.Operator)));
            supervisorCache.Add(name, item);
            return item;
        }
    }
}
