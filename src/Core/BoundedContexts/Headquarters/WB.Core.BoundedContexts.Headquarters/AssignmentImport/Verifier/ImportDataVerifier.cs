using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using messages = WB.Core.BoundedContexts.Headquarters.Resources.PreloadingVerificationMessages;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier
{
    
    internal partial class ImportDataVerifier : IPreloadedDataVerifier
    {
        private readonly IUserViewFactory userViewFactory;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly Dictionary<string, UserToVerify> users = new Dictionary<string, UserToVerify>();
        
        public ImportDataVerifier(IUserViewFactory userViewFactory, IQuestionnaireStorage questionnaireStorage)
        {
            this.userViewFactory = userViewFactory;
            this.questionnaireStorage = questionnaireStorage;
        }

        
        public IEnumerable<PanelImportVerificationError> VerifyAnswers(QuestionnaireIdentity questionnaireIdentity, PreloadedFile file)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);
            var questionnairetoVerify = this.ToQuestionnaire(questionnaire);

            foreach (var assignmentRow in this.ToAssignmentRows(questionnairetoVerify, file))
            foreach (var assignmentValue in assignmentRow.Answers)
            foreach (var error in this.Verifiers.Select(x => x.Invoke(assignmentValue, questionnairetoVerify)))
                if (error != null) yield return error;
        }

        public IEnumerable<PanelImportVerificationError> VerifyColumnsAndRosters(QuestionnaireIdentity questionnaireIdentity, PreloadedFile file)
        {
            yield break;
        }

        private static Func<AssignmentValue, Questionnaire, PanelImportVerificationError> Error<TValue>(
            Func<TValue, Questionnaire, bool> hasError, string code, string message) where TValue: AssignmentValue => (value, questionnaire) =>
            value is TValue && hasError((TValue)value, questionnaire) ? ToVerificationError(code, message, value) : null;

        private static Func<AssignmentValue, Questionnaire, PanelImportVerificationError> Error<TValue>(
            Func<TValue, bool> hasError, string code, string message) where TValue : AssignmentValue => (value, questionnaire) =>
            value is TValue && hasError((TValue)value) ? ToVerificationError(code, message, value) : null;

        private static PanelImportVerificationError ToVerificationError(string code, string message, AssignmentValue assignmentValue) 
            => new PanelImportVerificationError(code, message, new PreloadedDataVerificationReference(assignmentValue.Row, 0, PreloadedDataVerificationReferenceType.Cell,
                assignmentValue.Value, assignmentValue.FileName));

        private IEnumerable<Func<AssignmentValue, Questionnaire, PanelImportVerificationError>> Verifiers => new[]
        {
            Error<AssignmentResponsible>(ResponsibleIsEmpty, "PL0025", messages.PL0025_ResponsibleNameIsEmpty),
            Error<AssignmentResponsible>(ResponsibleNotFound, "PL0026", messages.PL0026_ResponsibleWasNotFound),
            Error<AssignmentResponsible>(ResponsibleIsLocked, "PL0027", messages.PL0027_ResponsibleIsLocked),
            Error<AssignmentResponsible>(ResponsibleHasInvalidRole, "PL0028", messages.PL0028_UserIsNotSupervisorOrInterviewer),
            Error<AssignmentQuantity>(QuantityIsNotInteger, "PL0035", messages.PL0035_QuantityNotParsed),
            Error<AssignmentQuantity>(QuantityIsNegative, "PL0036", messages.PL0036_QuantityShouldBeGreaterThanMinus1),
            Error<AssignmentAnswer>(QuestionNotFound, "PL0003", messages.PL0003_ColumnWasntMappedOnQuestion),
            Error<AssignmentAnswer>(QuestionIsNegativeRosterSize, "PL0022", messages.PL0022_AnswerIsIncorrectBecauseIsRosterSizeAndNegative),
            Error<AssignmentAnswer>(QuestionExceededRosterSize, "PL0029", string.Format(messages.PL0029_AnswerIsIncorrectBecauseIsRosterSizeAndMoreThan40, Constants.MaxRosterRowCount)),
            Error<AssignmentAnswer>(QuestionExceededLongRosterSize, "PL0029", string.Format(messages.PL0029_AnswerIsIncorrectBecauseIsRosterSizeAndMoreThan40, Constants.MaxLongRosterRowCount)),
            Error<AssignmentAnswers>(Gps_DontHaveLongitudeOrLatitude, "PL0030", messages.PL0030_GpsMandatoryFilds),
            Error<AssignmentAnswers>(Gps_LatitudeMustBeGeaterThenN90AndLessThen90, "PL0032", messages.PL0032_LatitudeMustBeGeaterThenN90AndLessThen90),
            Error<AssignmentAnswers>(Gps_LongitudeMustBeGeaterThenN180AndLessThen180, "PL0033", messages.PL0033_LongitudeMustBeGeaterThenN180AndLessThen180),
        };

        private bool Gps_LongitudeMustBeGeaterThenN180AndLessThen180(AssignmentAnswers answer, Questionnaire questionnaire)
        {
            if(questionnaire.Questions[answer.VariableName].Type != InterviewQuestionType.Gps) return false;
            if (this.Gps_DontHaveLongitudeOrLatitude(answer, questionnaire)) return false;

            var longitude = answer.Values.FirstOrDefault(x => x.VariableName == "longitude").Answer.AsInt;

            return longitude < -180 || longitude > 180;
        }

        private bool Gps_LatitudeMustBeGeaterThenN90AndLessThen90(AssignmentAnswers answer, Questionnaire questionnaire)
        {
            if (questionnaire.Questions[answer.VariableName].Type != InterviewQuestionType.Gps) return false;
            if (this.Gps_DontHaveLongitudeOrLatitude(answer, questionnaire)) return false;

            var latitude = answer.Values.FirstOrDefault(x => x.VariableName == "latitude").Answer.AsInt;

            return latitude < -90 || latitude > 90;
        }

        private bool Gps_DontHaveLongitudeOrLatitude(AssignmentAnswers answer, Questionnaire questionnaire)
        {
            if(questionnaire.Questions[answer.VariableName].Type == InterviewQuestionType.Gps) return false;

            var latitude = answer.Values.FirstOrDefault(x => x.VariableName == "latitude")?.Answer?.AsInt;
            var longitude =  answer.Values.FirstOrDefault(x => x.VariableName == "longitude")?.Answer?.AsInt;

            return !latitude.HasValue || !longitude.HasValue;
        }

        private bool QuestionExceededRosterSize(AssignmentAnswer answer, Questionnaire questionnaire)
        {
            if (this.QuestionIsNegativeRosterSize(answer, questionnaire)) return false;

            return !questionnaire.Questions[answer.VariableName].IsRosterSizeForLongRoster &&
                   answer.Answer?.AsInt != null && answer.Answer.AsInt > Constants.MaxLongRosterRowCount;
        }

        private bool QuestionExceededLongRosterSize(AssignmentAnswer answer, Questionnaire questionnaire)
        {
            if (this.QuestionIsNegativeRosterSize(answer, questionnaire)) return false;

            return questionnaire.Questions[answer.VariableName].IsRosterSizeForLongRoster &&
                   answer.Answer?.AsInt != null && answer.Answer.AsInt > Constants.MaxLongRosterRowCount;
        }

        private bool QuestionIsNegativeRosterSize(AssignmentAnswer answer, Questionnaire questionnaire)
        {
            if (this.QuestionNotFound(answer, questionnaire)) return false;

            var question = questionnaire.Questions[answer.VariableName];

            if (!question.IsRosterSize || question.Type != InterviewQuestionType.Integer) return false;

            return answer.Answer.AsInt < 0;
        }

        private bool QuestionNotFound(AssignmentAnswer answer, Questionnaire questionnaire) 
            => !questionnaire.Questions.ContainsKey(answer.VariableName);

        private bool ResponsibleHasInvalidRole(AssignmentResponsible responsible)
            => !this.ResponsibleNotFound(responsible) && responsible.Responsible.IsSupervisorOrInterviewer;

        private bool ResponsibleIsLocked(AssignmentResponsible responsible) 
            => !this.ResponsibleNotFound(responsible) && responsible.Responsible.IsLocked;

        private bool ResponsibleNotFound(AssignmentResponsible responsible) 
            => !this.ResponsibleIsEmpty(responsible) && responsible.Responsible == null;

        private bool ResponsibleIsEmpty(AssignmentResponsible responsible) 
            => string.IsNullOrWhiteSpace(responsible.Value);

        private bool QuantityIsNotInteger(AssignmentQuantity quantity)
        {
            if (string.IsNullOrWhiteSpace(quantity.Value) ||
                quantity.Value == "-1" ||
                quantity.Value == "INF") return false;

            return quantity.Quantity == null;
        }
        private bool QuantityIsNegative(AssignmentQuantity quantity)
            => !QuantityIsNotInteger(quantity) && quantity.Quantity < 1;

        #region OLD OLD CODE

        private int GetColumnIndexByHeaderName(PreloadedDataByFile dataFile, string columnName)
            => dataFile?.Header.ToList().FindIndex(header => string.Equals(header, columnName, StringComparison.InvariantCultureIgnoreCase)) ?? -1;

        private int GetResponsibleColumnIndex(PreloadedDataByFile data) =>
            this.GetColumnIndexByHeaderName(data, ServiceColumns.ResponsibleColumnName);

        public bool HasResponsibleNames(PreloadedDataByFile data) => this.GetResponsibleColumnIndex(data) >= 0;

        public IEnumerable<PanelImportVerificationError> VerifyPanelFiles(PreloadedDataByFile[] allLevels, IPreloadedDataService dataService,
            QuestionnaireIdentity questionnaireIdentity)
        {
            throw new NotImplementedException();
        }

        public ImportDataInfo GetDetails(PreloadedDataByFile data)
        {
            var importDataInfo = new ImportDataInfo
            {
                EntitiesCount = data?.Content?.Length ?? 0,
            };

            if (!this.HasResponsibleNames(data)) return importDataInfo;

            var userNames = GetUserNames(data);

            var dbUsers = this.userViewFactory.GetUsersByUserNames(userNames);

            importDataInfo.SupervisorsCount = dbUsers.Count(x => x.IsSupervisor);
            importDataInfo.EnumeratorsCount = dbUsers.Count(x => x.IsInterviewer);

            return importDataInfo;
        }

        public IEnumerable<PanelImportVerificationError> VerifyAssignmentsSample(PreloadedDataByFile data, IPreloadedDataService preloadedDataService)
            => this.PrefilledVerifiers.SelectMany(verifier => verifier.Invoke(new[]
            {
                new PreloadedDataByFile(preloadedDataService.GetValidFileNameForTopLevelQuestionnaire(),
                    data.Header, data.Content)
            }, preloadedDataService));

        private IEnumerable<Func<PreloadedDataByFile[], IPreloadedDataService, IEnumerable<PanelImportVerificationError>>> PanelVerifiers => new[]
        {
            this.Verifier(this.FileWasntMappedOnQuestionnaireLevel, "PL0004", messages.PL0004_FileWasntMappedRoster, PreloadedDataVerificationReferenceType.File),
            this.Verifier(this.ServiceColumnsAreAbsent, "PL0007", messages.PL0007_ServiceColumnIsAbsent, PreloadedDataVerificationReferenceType.Column),
            this.Verifier(this.IdDuplication, "PL0006", messages.PL0006_IdDublication),
            this.Verifier(this.OrphanRosters, "PL0008", messages.PL0008_OrphanRosterRecord),
            this.Verifier(this.RosterIdIsInconsistencyWithRosterSizeQuestion, "PL0009", messages.PL0009_RosterIdIsInconsistantWithRosterSizeQuestion),
            
        }.Union(this.PrefilledVerifiers);

        private IEnumerable<Func<PreloadedDataByFile[], IPreloadedDataService, IEnumerable<PanelImportVerificationError>>> PrefilledVerifiers => new[]
        {
            this.Verifier(this.ColumnWasntMappedOnQuestionInTemplate, "PL0003", messages.PL0003_ColumnWasntMappedOnQuestion, PreloadedDataVerificationReferenceType.Column),
            this.Verifier(this.ErrorsByQuestionsWasntParsed),
            this.Verifier(this.ColumnDuplications),
            this.Verifier(this.ErrorsByMultipleChoiseQuestions, QuestionType.MultyOption),
        };

        private IEnumerable<string> ColumnWasntMappedOnQuestionInTemplate(PreloadedDataByFile levelData, IPreloadedDataService preloadedDataService)
        {
            var levelExportStructure = preloadedDataService.FindLevelInPreloadedData(levelData.FileName);
            if (levelExportStructure == null)
                yield break;

            var parentColumnNames = preloadedDataService.GetAllParentColumnNamesForLevel(levelExportStructure.LevelScopeVector).ToList();
            var referenceNames = levelExportStructure.ReferencedNames ?? new string[0];
            var listOfParentIdColumns = this.GetListOfParentIdColumns(levelData, levelExportStructure).ToArray();
            var listOfPermittedExtraColumns = this.GetListOfPermittedExtraColumnsForLevel(levelExportStructure).ToArray(); 
            var listOfServiceVariableNames = ServiceColumns.SystemVariables.Values.Select(x => x.VariableExportColumnName).ToList();

            foreach (var columnName in levelData.Header)
            {
                if (parentColumnNames.Any(x => string.Equals(columnName, x, StringComparison.OrdinalIgnoreCase)))
                    continue;

                if (string.Equals(columnName, levelExportStructure.LevelIdColumnName, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                if (listOfServiceVariableNames.Any(x => string.Equals(columnName, x, StringComparison.OrdinalIgnoreCase)))
                    continue;

                if (listOfParentIdColumns.Contains(columnName))
                    continue;

                if (referenceNames.Contains(columnName))
                    continue;

                if (listOfPermittedExtraColumns.Contains(columnName))
                    continue;

                if (!levelExportStructure.HeaderItems.Values.Any(headerItem => headerItem.ColumnHeaders.Any(x => x.Name.Equals(columnName,StringComparison.OrdinalIgnoreCase))))
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

            if (levelData.Header.Count(h => string.Equals(h, levelExportStructure.LevelIdColumnName, StringComparison.InvariantCultureIgnoreCase)) != 1)
                yield return levelExportStructure.LevelIdColumnName;

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
                            messages.PL0031_ColumnNameDuplicatesFound,
                            columnsWithDuplicate.Value.ToArray()));
        }
                
        private IEnumerable<PanelImportVerificationError> ErrorsByMultipleChoiseQuestions(HeaderStructureForLevel level,
            ExportedQuestionHeaderItem question,
            PreloadedDataByFile levelData,
            IPreloadedDataService preloadedDataService)
        {
            var maxAnswersCount = preloadedDataService.GetMaxAnswersCount(question.VariableName);
            if (!maxAnswersCount.HasValue)
            {
                yield break;
            }

            for (int rowIndex = 0; rowIndex < levelData.Content.Length; rowIndex++)
            {
                var exportedHeaderItem = level.HeaderItems[question.PublicKey];
                List<decimal> answers = new List<decimal>();
                foreach (var header in exportedHeaderItem.ColumnHeaders)
                {
                    var columnIndex = preloadedDataService.GetColumnIndexByHeaderName(levelData, header.Name);

                    if (columnIndex > -1)
                    {
                        var parsedValue = this.GetValue<decimal>(levelData.Content[rowIndex], levelData.Header, columnIndex, level, preloadedDataService);
                        if (parsedValue.HasValue)
                        {
                            answers.Add(parsedValue.Value);

                            if (answers.Count > maxAnswersCount)
                            {
                                yield return new PanelImportVerificationError("PL0041",
                                    string.Format(messages.PL0041_AnswerExceedsMaxAnswersCount,
                                        maxAnswersCount),
                                    this.CreateReference(columnIndex, rowIndex, levelData));
                                yield break;
                            }
                        }
                    }
                }

            }
        }

        private IEnumerable<PanelImportVerificationError> ErrorsByGpsQuestions(
            HeaderStructureForLevel level,
            ExportedQuestionHeaderItem gpsExportedQuestion,
            PreloadedDataByFile levelData,
            IPreloadedDataService preloadedDataService)
        {
            var latitudeColumnIndex = preloadedDataService.GetColumnIndexByHeaderName(levelData,
                $"{gpsExportedQuestion.VariableName}__latitude");

            var longitudeColumnIndex = preloadedDataService.GetColumnIndexByHeaderName(levelData,
                $"{gpsExportedQuestion.VariableName}__longitude");

            if (latitudeColumnIndex < 0 || longitudeColumnIndex < 0)
            {
                yield return new PanelImportVerificationError("PL0030", messages.PL0030_GpsFieldsRequired, this.CreateReference(0, levelData));
                yield break;
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

        private IEnumerable<PanelImportVerificationError> ErrorsByQuestionsWasntParsed(PreloadedDataByFile levelData,
            IPreloadedDataService preloadedDataService)
        {
            var exportedLevel = preloadedDataService.FindLevelInPreloadedData(levelData.FileName);
            if (exportedLevel == null)
                yield break;

            var parentColumnNames = preloadedDataService.GetAllParentColumnNamesForLevel(exportedLevel.LevelScopeVector)
                .ToList();
            for (int columnIndex = 0; columnIndex < levelData.Header.Length; columnIndex++)
            {
                var columnName = levelData.Header[columnIndex];
                if (parentColumnNames.Contains(columnName))
                    continue;

                if (preloadedDataService.IsVariableColumn(columnName))
                    continue;

                for (int rowIndex = 0; rowIndex < levelData.Content.Length; rowIndex++)
                {
                    var row = levelData.Content[rowIndex];
                    var answer = row[columnIndex];
                    answer = answer?
                        .Replace(ExportFormatSettings.MissingStringQuestionValue, string.Empty)
                        .Replace(ExportFormatSettings.MissingNumericQuestionValue, string.Empty);

                    if (string.IsNullOrEmpty(answer))
                        continue;

                    var parsedResult = preloadedDataService.ParseQuestionInLevel(answer, columnName, exportedLevel, out _);

                    switch (parsedResult)
                    {
                        case ValueParsingResult.OK:
                            continue;

                        case ValueParsingResult.AnswerAsDecimalWasNotParsed:
                            yield return
                                new PanelImportVerificationError("PL0019",
                                    messages.PL0019_ExpectedDecimalNotParsed,
                                    new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                        PreloadedDataVerificationReferenceType.Cell,
                                        string.Format("{0}:{1}", levelData.Header[columnIndex],
                                            row[columnIndex]),
                                        levelData.FileName));
                            break;
                        case ValueParsingResult.CommaIsUnsupportedInAnswer:
                            yield return
                                new PanelImportVerificationError("PL0034",
                                    messages.PL0034_CommaSymbolIsNotAllowedInNumericAnswer,
                                    new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                        PreloadedDataVerificationReferenceType.Cell,
                                        string.Format("{0}:{1}", levelData.Header[columnIndex],
                                            row[columnIndex]),
                                        levelData.FileName));
                            break;
                        case ValueParsingResult.AnswerAsIntWasNotParsed:
                            yield return
                                new PanelImportVerificationError("PL0018",
                                    messages.PL0018_ExpectedIntNotParsed,
                                    new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                        PreloadedDataVerificationReferenceType.Cell,
                                        string.Format("{0}:{1}", levelData.Header[columnIndex],
                                            row[columnIndex]),
                                        levelData.FileName));
                            break;
                        case ValueParsingResult.AnswerAsGpsWasNotParsed:
                            yield return
                                new PanelImportVerificationError("PL0017",
                                    messages.PL0017_ExpectedGpsNotParsed,
                                    new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                        PreloadedDataVerificationReferenceType.Cell,
                                        string.Format("{0}:{1}", levelData.Header[columnIndex],
                                            row[columnIndex]),
                                        levelData.FileName));
                            break;
                        case ValueParsingResult.AnswerAsDateTimeWasNotParsed:
                            yield return
                                new PanelImportVerificationError("PL0016",
                                    messages.PL0016_ExpectedDateTimeNotParsed,
                                    new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                        PreloadedDataVerificationReferenceType.Cell,
                                        string.Format("{0}:{1}", levelData.Header[columnIndex],
                                            row[columnIndex]),
                                        levelData.FileName));
                            break;
                        case ValueParsingResult.QuestionTypeIsIncorrect:
                            yield return
                                new PanelImportVerificationError("PL0015",
                                    messages.PL0015_QuestionTypeIsIncorrect,
                                    new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                        PreloadedDataVerificationReferenceType.Cell,
                                        string.Format("{0}:{1}", levelData.Header[columnIndex],
                                            row[columnIndex]),
                                        levelData.FileName));
                            break;
                        case ValueParsingResult.ParsedValueIsNotAllowed:
                            yield return
                                new PanelImportVerificationError("PL0014",
                                    messages.PL0014_ParsedValueIsNotAllowed,
                                    new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                        PreloadedDataVerificationReferenceType.Cell,
                                        string.Format("{0}:{1}", levelData.Header[columnIndex],
                                            row[columnIndex]),
                                        levelData.FileName));
                            break;
                        case ValueParsingResult.ValueIsNullOrEmpty:
                            yield return
                                new PanelImportVerificationError("PL0013",
                                    messages.PL0013_ValueIsNullOrEmpty,
                                    new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                        PreloadedDataVerificationReferenceType.Cell,
                                        string.Format("{0}:{1}", levelData.Header[columnIndex],
                                            row[columnIndex]),
                                        levelData.FileName));
                            break;
                        case ValueParsingResult.QuestionWasNotFound:
                            yield return
                                new PanelImportVerificationError("PL0012",
                                    messages.PL0012_QuestionWasNotFound,
                                    new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                        PreloadedDataVerificationReferenceType.Cell,
                                        string.Format("{0}:{1}", levelData.Header[columnIndex],
                                            row[columnIndex]),
                                        levelData.FileName));
                            break;
                        case ValueParsingResult.UnsupportedAreaQuestion:
                            yield return
                                new PanelImportVerificationError("PL0038",
                                    messages.PL0038_UnsupportedAreaQuestion,
                                    new PreloadedDataVerificationReference(columnIndex, rowIndex,
                                        PreloadedDataVerificationReferenceType.Cell,
                                        string.Format("{0}:{1}", levelData.Header[columnIndex],
                                            row[columnIndex]),
                                        levelData.FileName));
                            break;
                        default:
                            yield return
                                new PanelImportVerificationError("PL0011",
                                    messages.PL0011_GeneralError,
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
            Func<HeaderStructureForLevel,ExportedQuestionHeaderItem, PreloadedDataByFile, IPreloadedDataService, IEnumerable<PanelImportVerificationError>> exportedQuestionVerifier,
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
                        .OfType<ExportedQuestionHeaderItem>()
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
            => new PreloadedDataVerificationReference(null, y, PreloadedDataVerificationReferenceType.Column, levelData.Header[y], levelData.FileName);

        private PreloadedDataVerificationReference CreateReference(int x, int y, PreloadedDataByFile levelData) 
            => new PreloadedDataVerificationReference(x, y, PreloadedDataVerificationReferenceType.Cell, $"{levelData.Header[x]}:{levelData.Content[y][x]}", levelData.FileName);
        

        private string[] GetUserNames(PreloadedDataByFile data)
        {
            var responsibleColumnIndex = this.GetResponsibleColumnIndex(data);

            if (responsibleColumnIndex < 0)
                return Array.Empty<string>();

            return data.Content.Select(x => x[responsibleColumnIndex]?.ToLower()).Distinct().ToArray();
        }
        #endregion
    }
}
