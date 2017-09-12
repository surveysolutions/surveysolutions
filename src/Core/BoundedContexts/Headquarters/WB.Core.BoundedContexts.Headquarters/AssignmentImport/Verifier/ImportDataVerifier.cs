using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier
{
    internal class ImportDataVerifier : IPreloadedDataVerifier
    {
        private class UserToVerify
        {
            public bool IsLocked { get; }
            public bool IsSupervisorOrInterviewer => IsInterviewer || IsSupervisor; 
            public bool IsSupervisor { get; }
            public bool IsInterviewer { get; }

            public UserToVerify(bool isLocked, bool isSupervisor, bool isInterviewer)
            {
                this.IsLocked = isLocked;
                this.IsInterviewer = isInterviewer;
                this.IsSupervisor = isSupervisor;
            }
        }

      	private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly IUserViewFactory userViewFactory;
        private readonly IPreloadedDataServiceFactory preloadedDataServiceFactory;
        private readonly IRosterStructureService rosterStructureService;

        public ImportDataVerifier(
            IPreloadedDataServiceFactory preloadedDataServiceFactory,
            IUserViewFactory userViewFactory, 
            IQuestionnaireStorage questionnaireStorage,
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage,
            IRosterStructureService rosterStructureService)
        {
            this.preloadedDataServiceFactory = preloadedDataServiceFactory;
            this.userViewFactory = userViewFactory;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.rosterStructureService = rosterStructureService;
        }

        public ImportDataVerificationState VerifyAssignmentsSample(Guid questionnaireId, long version, PreloadedDataByFile data)
        {
            var status = new ImportDataVerificationState();

            if (data?.Content == null || data.Content.Length == 0)
            {
                status.Errors = new List<PanelImportVerificationError> { new PanelImportVerificationError("PL0024", PreloadingVerificationMessages.PL0024_DataWasNotFound) };
                return status;
            }

            IPreloadedDataService preloadedDataService = this.CreatePreloadedDataService(questionnaireId, version);

            if (preloadedDataService == null)
            {
                status.Errors = new List<PanelImportVerificationError> { new PanelImportVerificationError("PL0001", PreloadingVerificationMessages.PL0001_NoQuestionnaire) };
                return status;
            }

            var errors = new List<PanelImportVerificationError>();

            var datas = new[] { new PreloadedDataByFile(data.Id, preloadedDataService.GetValidFileNameForTopLevelQuestionnaire(), data.Header, data.Content) };

            errors.AddRange(
                this.Verifier(this.ColumnWasntMappedOnQuestionInTemplate, "PL0003",
                    PreloadingVerificationMessages.PL0003_ColumnWasntMappedOnQuestion, PreloadedDataVerificationReferenceType.Column)(datas,
                        preloadedDataService));

            if(this.ShouldVerificationBeContinued(errors))
                errors.AddRange(this.ErrorsByQuestionsWasntParsed(datas, preloadedDataService));

            if (this.ShouldVerificationBeContinued(errors))
                errors.AddRange(this.ErrorsByResposibleName(datas, preloadedDataService));

            if (this.ShouldVerificationBeContinued(errors))
                errors.AddRange(this.ErrorsByQuantityColumn(datas, preloadedDataService));

            if (this.ShouldVerificationBeContinued(errors))
                errors.AddRange(this.Verifier(this.ErrorsByGpsQuestions, QuestionType.GpsCoordinates)(datas, preloadedDataService));

            if (this.ShouldVerificationBeContinued(errors))
                errors.AddRange(this.Verifier(this.ErrorsByNumericQuestions, QuestionType.Numeric)(datas, preloadedDataService));

            if (this.ShouldVerificationBeContinued(errors))
                errors.AddRange(this.Verifier(this.ColumnDuplications)(datas, preloadedDataService));

            status.Errors = errors.Count > 100 ? errors.Take(100).ToList() : errors;
            status.EntitiesCount = datas.FirstOrDefault()?.Content.Length ?? 0;
            if (!status.Errors.Any())
            {
                CountResposiblesInDataFile(datas, preloadedDataService, status);
            }
            var responsibleNameIndex = preloadedDataService.GetColumnIndexByHeaderName(data, ServiceColumns.ResponsibleColumnName);
            status.WasResponsibleProvided = responsibleNameIndex >= 0;

            return status;
        }

        private void CountResposiblesInDataFile(PreloadedDataByFile[] allLevels, IPreloadedDataService preloadedDataService, ImportDataVerificationState status)
        {
            var responsibleCache = new Dictionary<string, UserToVerify>();

            foreach (var levelData in allLevels)
            {
                var responsibleNameIndex = preloadedDataService.GetColumnIndexByHeaderName(levelData, ServiceColumns.ResponsibleColumnName);

                if (responsibleNameIndex < 0)
                    continue;

                foreach (var row in levelData.Content)
                {
                    var name = row[responsibleNameIndex];

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    this.GetResponsibleAndUpdateCache(responsibleCache, name);
                }
            }

            foreach (var userState in responsibleCache.Values)
            {
                if (userState.IsInterviewer)
                    status.EnumeratorsCount++;
                if (userState.IsSupervisor)
                    status.SupervisorsCount++;
            }
        }

        private bool ShouldVerificationBeContinued(List<PanelImportVerificationError> errors)
        {
            return errors.Count < 100;
        }

        public void VerifyPanelFiles(Guid questionnaireId, long version, PreloadedDataByFile[] data, AssignmentImportStatus status)
        {
            if (data == null || !data.Any())
            {
                status.VerificationState.Errors = new List<PanelImportVerificationError> { new PanelImportVerificationError("PL0024", PreloadingVerificationMessages.PL0024_DataWasNotFound) };
                return;
            }

            var preloadedDataService = this.CreatePreloadedDataService(questionnaireId, version);
            if (preloadedDataService == null)
            {
                status.VerificationState.Errors = new List<PanelImportVerificationError>{ new PanelImportVerificationError("PL0001", PreloadingVerificationMessages.PL0001_NoQuestionnaire)};
                return;
            }

            status.TotalCount = this.AtomicVerifiers.Count() + 1;

            foreach (var verifier in AtomicVerifiers)
            {
                var errors = verifier.Invoke(data, preloadedDataService).ToList();
                status.ProcessedCount++;
                status.VerificationState.Errors.AddRange(errors);
            }
            
            var topLevel = preloadedDataService.GetTopLevelData(data);

            status.VerificationState.EntitiesCount = topLevel?.Content?.Length ?? 0;
            if (!status.VerificationState.Errors.Any() && topLevel!=null)
            {
                CountResposiblesInDataFile(topLevel.ToEnumerable().ToArray(), preloadedDataService, status.VerificationState);
            }

            var responsibleNameIndex = preloadedDataService.GetColumnIndexByHeaderName(topLevel, ServiceColumns.ResponsibleColumnName);
            status.VerificationState.WasResponsibleProvided = responsibleNameIndex >= 0;
        }
        
        private IPreloadedDataService CreatePreloadedDataService(Guid questionnaireId, long version)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireId, version);
            var questionnaireExportStructure =
                this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(
                    new QuestionnaireIdentity(questionnaireId, version));

            var questionnaireRosterStructure = this.rosterStructureService.GetRosterScopes(questionnaire);

            if (questionnaireExportStructure == null || questionnaireRosterStructure == null || questionnaire == null)
            {
                return null;
            }
            
            return this.preloadedDataServiceFactory. CreatePreloadedDataService(questionnaireExportStructure,
                questionnaireRosterStructure, questionnaire);
        }

        private IEnumerable<Func<PreloadedDataByFile[], IPreloadedDataService, IEnumerable<PanelImportVerificationError>>> AtomicVerifiers => new[]
        {
            this.Verifier(this.ColumnWasntMappedOnQuestionInTemplate, "PL0003",
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
                    
            this.ErrorsByResposibleName
        };

        private IEnumerable<string> ColumnWasntMappedOnQuestionInTemplate(PreloadedDataByFile levelData, IPreloadedDataService preloadedDataService)
        {
            var levelExportStructure = preloadedDataService.FindLevelInPreloadedData(levelData.FileName);
            if (levelExportStructure == null)
                yield break;
            var referenceNames = levelExportStructure.ReferencedNames ?? new string[0];
            var listOfParentIdColumns = this.GetListOfParentIdColumns(levelData, levelExportStructure).ToArray();
            var listOfPermittedExtraColumns = this.GetListOfPermittedExtraColumnsForLevel(levelExportStructure).ToArray(); 
            var listOfServiceVariableNames = ServiceColumns.SystemVariables.Select(x => x.VariableExportColumnName).ToList();

            foreach (var columnName in levelData.Header)
            {
                if (string.Equals(columnName, ServiceColumns.Id, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                if (listOfServiceVariableNames.Any(x => string.Equals(columnName, x, StringComparison.OrdinalIgnoreCase)))
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
                yield return ServiceColumns.AssignmentsCountColumnName;
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
                        this.JoinRowIdWithParentIdsInParentIdsVector(row[idColumnIndexInParentFile],
                            this.CreateParentIdsVector(row, parentIdColumnIndexesForParentDataFile))).ToList();
            
            for (int y = 0; y < levelData.Content.Length; y++)
            {

                var parentIdValues = this.CreateParentIdsVector(levelData.Content[y], parentIdColumnIndexes);
                if (!parentIds.Any(p => p.SequenceEqual(parentIdValues)))
                    yield return
                        new PreloadedDataVerificationReference(parentIdColumnIndexes.First(), y, PreloadedDataVerificationReferenceType.Cell,
                            string.Join(",", parentIdValues), levelData.FileName);
            }
        }

        private string[] JoinRowIdWithParentIdsInParentIdsVector(string id, string[] parentIds)
        {
            var result = new string[parentIds.Length + 1];
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
                
                var ids = preloadedDataService.GetAvailableIdListForParent(parentDataFile, levelExportStructure.LevelScopeVector,
                    this.CreateParentIdsVector(levelData.Content[y], parentIdColumnIndexes), allLevels);

                if (ids == null)
                    continue;

                decimal decimalId;
                if (!decimal.TryParse(idValue, out decimalId))
                    yield return
                        new PreloadedDataVerificationReference(idCoulmnIndexFile, y, PreloadedDataVerificationReferenceType.Cell, idValue,
                            levelData.FileName);
                if (!ids.Contains(Convert.ToInt32(decimalId)))
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

        private IEnumerable<PanelImportVerificationError> ColumnDuplications(
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

                columnNameOnCountOfOccurrenceMap[headerLowerCase].Add(this.CreateReference(i, preloadedDataByFile));
            }

            return
                columnNameOnCountOfOccurrenceMap.Where(c => c.Value.Count > 1).Select(
                    columnsWithDuplicate =>
                        new PanelImportVerificationError("PL0031",
                            PreloadingVerificationMessages.PL0031_ColumnNameDuplicatesFound,
                            columnsWithDuplicate.Value.ToArray()));
        }

        private IEnumerable<PanelImportVerificationError> ErrorsByGpsQuestions(
            HeaderStructureForLevel level,
            ExportedHeaderItem gpsExportedQuestion,
            PreloadedDataByFile levelData,
            IPreloadedDataService preloadedDataService)
        {
            var latitudeColumnIndex = preloadedDataService.GetColumnIndexByHeaderName(levelData,
                $"{gpsExportedQuestion.VariableName}__latitude");

            var longitudeColumnIndex = preloadedDataService.GetColumnIndexByHeaderName(levelData,
                $"{gpsExportedQuestion.VariableName}__longitude");

            var altitudeColumnIndex = preloadedDataService.GetColumnIndexByHeaderName(levelData,
                $"{gpsExportedQuestion.VariableName}__altitude");

            if (latitudeColumnIndex < 0 && longitudeColumnIndex < 0 && altitudeColumnIndex < 0)
                yield break;

            if (latitudeColumnIndex < 0 || longitudeColumnIndex < 0)
            {
                yield return new PanelImportVerificationError("PL0030", PreloadingVerificationMessages.PL0030_GpsFieldsRequired, this.CreateReference(0, levelData));
                yield break;
            }

            for (int rowIndex = 0; rowIndex < levelData.Content.Length; rowIndex++)
            {
                var latitude = this.GetValue<double>(levelData.Content[rowIndex], levelData.Header,
                        latitudeColumnIndex, level, preloadedDataService);

                var longitude = this.GetValue<double>(levelData.Content[rowIndex], levelData.Header,
                        longitudeColumnIndex, level, preloadedDataService);

                if (!latitude.HasValue && !longitude.HasValue)
                    continue;

                if (!latitude.HasValue)
                {
                    yield return new PanelImportVerificationError("PL0030",
                        PreloadingVerificationMessages.PL0030_GpsMandatoryFilds,
                        this.CreateReference(latitudeColumnIndex, rowIndex, levelData));
                }

                if (!longitude.HasValue)
                {
                    yield return new PanelImportVerificationError("PL0030",
                        PreloadingVerificationMessages.PL0030_GpsMandatoryFilds,
                        this.CreateReference(longitudeColumnIndex, rowIndex, levelData));
                }

                if (latitude.HasValue && (latitude.Value < -90 || latitude.Value > 90))
                    yield return new PanelImportVerificationError("PL0032",
                        PreloadingVerificationMessages
                            .PL0032_LatitudeMustBeGeaterThenN90AndLessThen90,
                        this.CreateReference(latitudeColumnIndex, rowIndex, levelData));

                if (longitude.HasValue && (longitude.Value < -180 || longitude.Value > 180))
                    yield return new PanelImportVerificationError("PL0033",
                        PreloadingVerificationMessages
                            .PL0033_LongitudeMustBeGeaterThenN180AndLessThen180,
                        this.CreateReference(longitudeColumnIndex, rowIndex, levelData));
            }
        }

        private IEnumerable<PanelImportVerificationError> ErrorsByNumericQuestions(
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

            var isRosterSizeForLongRoster = preloadedDataService.IsRosterSizeQuestionForLongRoster(numericExportedQuestion.PublicKey);

            for (int rowIndex = 0; rowIndex < levelData.Content.Length; rowIndex++)
            {
                var parsedValue = this.GetValue<int>(levelData.Content[rowIndex], levelData.Header,
                    columnIndex, level, preloadedDataService);

                if (!parsedValue.HasValue)
                    continue;

                if (parsedValue < 0)
                {
                    
                    yield return new PanelImportVerificationError("PL0022",
                        PreloadingVerificationMessages.PL0022_AnswerIsIncorrectBecauseIsRosterSizeAndNegative,
                        this.CreateReference(columnIndex, rowIndex, levelData));
                }

                var maxNumericValue = isRosterSizeForLongRoster
                    ? Constants.MaxLongRosterRowCount
                    : Constants.MaxRosterRowCount;

                if (parsedValue > maxNumericValue)
                {
                    yield return new PanelImportVerificationError("PL0029",
                        string.Format(PreloadingVerificationMessages.PL0029_AnswerIsIncorrectBecauseIsRosterSizeAndMoreThan40, maxNumericValue),
                        this.CreateReference(columnIndex, rowIndex, levelData));
                }
            }
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
        private IEnumerable<PanelImportVerificationError> ErrorsByQuestionsWasntParsed(PreloadedDataByFile[] allLevels,
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
                        answer = answer?
                            .Replace(ExportFormatSettings.MissingStringQuestionValue, string.Empty)
                            .Replace(ExportFormatSettings.MissingNumericQuestionValue, string.Empty);
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
                                    new PanelImportVerificationError("PL0019",
                                        PreloadingVerificationMessages.PL0019_ExpectedDecimalNotParsed,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.CommaIsUnsupportedInAnswer:
                                yield return
                                    new PanelImportVerificationError("PL0034",
                                        PreloadingVerificationMessages.PL0034_CommaSymbolIsNotAllowedInNumericAnswer,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.AnswerAsIntWasNotParsed:
                                yield return
                                    new PanelImportVerificationError("PL0018",
                                        PreloadingVerificationMessages.PL0018_ExpectedIntNotParsed,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.AnswerAsGpsWasNotParsed:
                                yield return
                                    new PanelImportVerificationError("PL0017",
                                        PreloadingVerificationMessages.PL0017_ExpectedGpsNotParsed,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.AnswerAsDateTimeWasNotParsed:
                                yield return
                                    new PanelImportVerificationError("PL0016",
                                        PreloadingVerificationMessages.PL0016_ExpectedDateTimeNotParsed,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.QuestionTypeIsIncorrect:
                                yield return
                                    new PanelImportVerificationError("PL0015",
                                        PreloadingVerificationMessages.PL0015_QuestionTypeIsIncorrect,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.ParsedValueIsNotAllowed:
                                yield return
                                    new PanelImportVerificationError("PL0014",
                                        PreloadingVerificationMessages.PL0014_ParsedValueIsNotAllowed,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.ValueIsNullOrEmpty:
                                yield return
                                    new PanelImportVerificationError("PL0013",
                                        PreloadingVerificationMessages.PL0013_ValueIsNullOrEmpty,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.QuestionWasNotFound:
                                yield return
                                    new PanelImportVerificationError("PL0012",
                                        PreloadingVerificationMessages.PL0012_QuestionWasNotFound,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.UnsupportedLinkedQuestion:
                                yield return
                                    new PanelImportVerificationError("PL0010",
                                        PreloadingVerificationMessages.PL0010_UnsupportedLinkedQuestion,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.UnsupportedMultimediaQuestion:
                                yield return
                                    new PanelImportVerificationError("PL0023",
                                        PreloadingVerificationMessages.PL0023_UnsupportedMultimediaQuestion,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.UnsupportedAreaQuestion:
                                yield return
                                    new PanelImportVerificationError("PL0038",
                                        PreloadingVerificationMessages.PL0038_UnsupportedAreaQuestion,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.UnsupportedAudioQuestion:
                                yield return
                                    new PanelImportVerificationError("PL0039",
                                        PreloadingVerificationMessages.PL0039_UnsupportedAudioQuestion,
                                        new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                            PreloadedDataVerificationReferenceType.Cell,
                                            string.Format("{0}:{1}", levelData.Header[columnIndex],
                                                row[columnIndex]),
                                            levelData.FileName));
                                break;
                            case ValueParsingResult.GeneralErrorOccured:
                            default:
                                yield return
                                    new PanelImportVerificationError("PL0011",
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

        private Func<PreloadedDataByFile[], IPreloadedDataService, IEnumerable<PanelImportVerificationError>> Verifier(
            Func<PreloadedDataByFile, IPreloadedDataService, IEnumerable<PanelImportVerificationError>> fileValidator)
        {
            return (data, preloadedDataService) => data.SelectMany(level => fileValidator(level, preloadedDataService));
        }

        private Func<PreloadedDataByFile[], IPreloadedDataService, IEnumerable<PanelImportVerificationError>> Verifier(
            Func<PreloadedDataByFile, IPreloadedDataService, IEnumerable<string>> getErrors, 
            string code, 
            string message,
            PreloadedDataVerificationReferenceType type)
        {
            return (data, rosterDataService) => data
                .SelectMany(level => getErrors(level, rosterDataService)
                .Select(entity => new PanelImportVerificationError(code, message, new PreloadedDataVerificationReference(type, entity, level.FileName))));
        }

        private Func<PreloadedDataByFile[], IPreloadedDataService, IEnumerable<PanelImportVerificationError>> Verifier(
            Func<HeaderStructureForLevel,ExportedHeaderItem, PreloadedDataByFile, IPreloadedDataService, IEnumerable<PanelImportVerificationError>> exportedQuestionVerifier,
            QuestionType questionType)
        {
            return (datas, preloadedDataService) =>
            {
                var result = new List<PanelImportVerificationError>();

                foreach (var levelData in datas)
                {
                    var levelExportStructure = preloadedDataService.FindLevelInPreloadedData(levelData.FileName);
                    if (levelExportStructure == null)
                        continue;

                    var exportedQuestions = levelExportStructure.HeaderItems.Values
                        .OfType<ExportedHeaderItem>()
                        .Where(h => h.QuestionType == questionType);

                    foreach (var exportedQuestion in exportedQuestions)
                    {
                        result.AddRange(exportedQuestionVerifier(levelExportStructure, exportedQuestion, levelData, preloadedDataService));
                    }
                }
                return result;
            };
        }

        private Func<PreloadedDataByFile[], IPreloadedDataService, IEnumerable<PanelImportVerificationError>> Verifier(
            Func<PreloadedDataByFile, PreloadedDataByFile[], IPreloadedDataService, IEnumerable<PreloadedDataVerificationReference>>
                getErrors, string code, string message)
        {
            return (data, rosterDataService) =>
            {
                return data
                    .SelectMany(level => getErrors(level, data, rosterDataService)
                    .Select(entity => new PanelImportVerificationError(code, message, entity)));
            };
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

        private IEnumerable<PanelImportVerificationError> ErrorsByQuantityColumn(PreloadedDataByFile[] allLevels, IPreloadedDataService preloadedDataService)
        {
            foreach (var levelData in allLevels)
            {
                var quantityColumnIndex = preloadedDataService.GetColumnIndexByHeaderName(levelData, ServiceColumns.AssignmentsCountColumnName);

                if (quantityColumnIndex < 0)
                    continue;

                for (int y = 0; y < levelData.Content.Length; y++)
                {
                    var row = levelData.Content[y];
                    var quantityString = row[quantityColumnIndex]?.Trim();

                    if (String.IsNullOrWhiteSpace(quantityString))
                        continue;

                    if (quantityString == "-1" || quantityString == "INF")
                        continue;

                    if (!int.TryParse(quantityString, out int quantity))
                    {
                        yield return
                            new PanelImportVerificationError("PL0035",
                                PreloadingVerificationMessages.PL0035_QuantityNotParsed,
                                new PreloadedDataVerificationReference(quantityColumnIndex, y,
                                    PreloadedDataVerificationReferenceType.Cell,
                                    "",
                                    levelData.FileName));
                        continue;
                    }

                    if (quantity < -1)
                    {
                        yield return
                            new PanelImportVerificationError("PL0036",
                                PreloadingVerificationMessages.PL0036_QuantityShouldBeGreaterThanMinus1,
                                new PreloadedDataVerificationReference(quantityColumnIndex, y,
                                    PreloadedDataVerificationReferenceType.Cell,
                                    "",
                                    levelData.FileName));
                        continue;
                    }
                }
            }
        }

        private IEnumerable<PanelImportVerificationError> ErrorsByResposibleName( PreloadedDataByFile[] allLevels, IPreloadedDataService preloadedDataService)
        {
            foreach (var levelData in allLevels)
            {
                var responsibleNameIndex = preloadedDataService.GetColumnIndexByHeaderName(levelData, ServiceColumns.ResponsibleColumnName);

                if (responsibleNameIndex < 0)
                    continue;

                var responsibleCache = new Dictionary<string, UserToVerify>();

                for (int y = 0; y < levelData.Content.Length; y++)
                {
                    var row = levelData.Content[y];
                    var name = row[responsibleNameIndex];

                    if (String.IsNullOrWhiteSpace(name))
                    {
                        yield return
                            new PanelImportVerificationError("PL0025",
                                PreloadingVerificationMessages.PL0025_ResponsibleNameIsEmpty,
                                new PreloadedDataVerificationReference(responsibleNameIndex, y,
                                    PreloadedDataVerificationReferenceType.Cell,
                                    "",
                                    levelData.FileName));
                        continue;
                    }

                    var userState = this.GetResponsibleAndUpdateCache(responsibleCache, name);

                    if (userState == null)
                    {

                        yield return
                            new PanelImportVerificationError("PL0026",
                                PreloadingVerificationMessages.PL0026_ResponsibleWasNotFound,
                                new PreloadedDataVerificationReference(responsibleNameIndex, y,
                                    PreloadedDataVerificationReferenceType.Cell,
                                    "",
                                    levelData.FileName));
                        continue;
                    }
                    if (userState.IsLocked)
                    {
                        yield return
                            new PanelImportVerificationError("PL0027",
                                PreloadingVerificationMessages.PL0027_ResponsibleIsLocked,
                                new PreloadedDataVerificationReference(responsibleNameIndex, y,
                                    PreloadedDataVerificationReferenceType.Cell,
                                    "",
                                    levelData.FileName));
                        continue;
                    }
                    if (!userState.IsSupervisorOrInterviewer)
                    {
                        yield return
                            new PanelImportVerificationError("PL0028",
                                PreloadingVerificationMessages.PL0028_UserIsNotSupervisorOrInterviewer,
                                new PreloadedDataVerificationReference(responsibleNameIndex, y,
                                    PreloadedDataVerificationReferenceType.Cell,
                                    "",
                                    levelData.FileName));
                    }
                }
            }
        }

        private UserToVerify GetResponsibleAndUpdateCache(Dictionary<string, UserToVerify> responsiblesCache, string userName)
        {
            var userNameLowerCase = userName.ToLower();
            if (!responsiblesCache.ContainsKey(userNameLowerCase))
            {
                var user = this.userViewFactory.GetUser(new UserViewInputModel(UserName: userNameLowerCase, UserEmail: null));

                var userNotExistOrArchived = user == null || user.IsArchived;

                responsiblesCache[userNameLowerCase] = userNotExistOrArchived ? null : new UserToVerify
                (
                    user.IsLockedByHQ || user.IsLockedBySupervisor,
                    user.IsSupervisor(),
                    user.Roles.Any(role => role == UserRoles.Interviewer)
                );
            }

            return responsiblesCache[userNameLowerCase];
        }
    }
}
