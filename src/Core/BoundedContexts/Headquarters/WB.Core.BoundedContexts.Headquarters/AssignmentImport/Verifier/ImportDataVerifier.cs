using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using messages = WB.Core.BoundedContexts.Headquarters.Resources.PreloadingVerificationMessages;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier
{
    
    internal partial class ImportDataVerifier : IPreloadedDataVerifier
    {
        public IEnumerable<PanelImportVerificationError> VerifyAnswers(AssignmentRow assignmentRow)
        {
            foreach (var assignmentValue in assignmentRow.Answers)
            foreach (var error in this.Verifiers.Select(x => x.Invoke(assignmentValue)))
                if (error != null) yield return error;
        }

        public IEnumerable<PanelImportVerificationError> VerifyColumnsAndRosters(QuestionnaireIdentity questionnaireIdentity, PreloadedFile file)
        {
            //"PL0012", messages.PL0012_QuestionWasNotFound,
            //"PL0003", messages.PL0003_ColumnWasntMappedOnQuestion
            //"PL0031", messages.PL0031_ColumnNameDuplicatesFound
            //"PL0038", messages.PL0038_UnsupportedAreaQuestion

            //messages.PL0014_ParsedValueIsNotAllowed
            //if (!this.GetAnswerOptionsAsValues(question).Contains(decimalAnswerValue))
            //    return ParseFailed(ValueParsingResult.ParsedValueIsNotAllowed, out parsedValue, out parsedSingleColumnAnswer);

            //if (latitudeColumnIndex < 0 || longitudeColumnIndex < 0)
            //{
            //    yield return new PanelImportVerificationError("PL0030", messages.PL0030_GpsFieldsRequired, this.CreateReference(0, levelData));
            //    yield break;
            //}


            //this.Verifier(this.ColumnWasntMappedOnQuestionInTemplate, "PL0003", messages.PL0003_ColumnWasntMappedOnQuestion, PreloadedDataVerificationReferenceType.Column),
            //this.Verifier(this.FileWasntMappedOnQuestionnaireLevel, "PL0004", messages.PL0004_FileWasntMappedRoster, PreloadedDataVerificationReferenceType.File),
            //this.Verifier(this.ServiceColumnsAreAbsent, "PL0007", messages.PL0007_ServiceColumnIsAbsent, PreloadedDataVerificationReferenceType.Column),
            //this.Verifier(this.OrphanRosters, "PL0008", messages.PL0008_OrphanRosterRecord),

            yield break;
        }
        
        private static Func<AssignmentValue, PanelImportVerificationError> Error<TValue>(
            Func<TValue, bool> hasError, string code, string message) where TValue : AssignmentValue => value =>
            value is TValue && hasError((TValue)value) ? ToVerificationError(code, message, value) : null;

        private static PanelImportVerificationError ToVerificationError(string code, string message, AssignmentValue assignmentValue) 
            => new PanelImportVerificationError(code, message, new PreloadedDataVerificationReference(assignmentValue.Row, 0, PreloadedDataVerificationReferenceType.Cell,
                assignmentValue.Value, assignmentValue.FileName));

        private IEnumerable<Func<AssignmentValue, PanelImportVerificationError>> Verifiers => new[]
        {
            Error<AssignmentTextAnswer>(Text_HasInvalidMask, "PL0014", messages.PL0014_ParsedValueIsNotAllowed),
            Error<AssignmentDataTimeAnswer>(DateTime_NotParsed, "PL0016", messages.PL0016_ExpectedDateTimeNotParsed),
            Error<AssignmentGpsAnswer>(Gps_NotParsed, "PL0017", messages.PL0017_ExpectedGpsNotParsed),
            Error<AssignmentIntegerAnswer>(Interger_NotParsed, "PL0018", messages.PL0018_ExpectedIntNotParsed),
            Error<AssignmentAnswer>(ExpectedDecimalNotParsed, "PL0019", messages.PL0019_ExpectedDecimalNotParsed),
            Error<AssignmentIntegerAnswer>(Integer_IsNegativeRosterSize, "PL0022", messages.PL0022_AnswerIsIncorrectBecauseIsRosterSizeAndNegative),
            Error<AssignmentResponsible>(Responsible_IsEmpty, "PL0025", messages.PL0025_ResponsibleNameIsEmpty),
            Error<AssignmentResponsible>(Responsible_NotFound, "PL0026", messages.PL0026_ResponsibleWasNotFound),
            Error<AssignmentResponsible>(Responsible_IsLocked, "PL0027", messages.PL0027_ResponsibleIsLocked),
            Error<AssignmentResponsible>(Responsible_HasInvalidRole, "PL0028", messages.PL0028_UserIsNotSupervisorOrInterviewer),
            Error<AssignmentIntegerAnswer>(Integer_ExceededRosterSize, "PL0029", string.Format(messages.PL0029_AnswerIsIncorrectBecauseIsRosterSizeAndMoreThan40, Constants.MaxRosterRowCount)),
            Error<AssignmentIntegerAnswer>(Integer_ExceededLongRosterSize, "PL0029", string.Format(messages.PL0029_AnswerIsIncorrectBecauseIsRosterSizeAndMoreThan40, Constants.MaxLongRosterRowCount)),
            Error<AssignmentGpsAnswer>(Gps_DontHaveLongitudeOrLatitude, "PL0030", messages.PL0030_GpsMandatoryFilds),
            Error<AssignmentGpsAnswer>(Gps_LatitudeMustBeGeaterThenN90AndLessThen90, "PL0032", messages.PL0032_LatitudeMustBeGeaterThenN90AndLessThen90),
            Error<AssignmentGpsAnswer>(Gps_LongitudeMustBeGeaterThenN180AndLessThen180, "PL0033", messages.PL0033_LongitudeMustBeGeaterThenN180AndLessThen180),
            Error<AssignmentGpsAnswer>(Gps_CommaSymbolIsNotAllowed, "PL0034", messages.PL0034_CommaSymbolIsNotAllowedInNumericAnswer),
            Error<AssignmentIntegerAnswer>(Integer_CommaSymbolIsNotAllowed, "PL0034", messages.PL0034_CommaSymbolIsNotAllowedInNumericAnswer),
            Error<AssignmentQuantity>(Quantity_IsNotInteger, "PL0035", messages.PL0035_QuantityNotParsed),
            Error<AssignmentQuantity>(Quantity_IsNegative, "PL0036", messages.PL0036_QuantityShouldBeGreaterThanMinus1),
            Error<AssignmentCategoricalMultiAnswer>(Categorical_AnswerExceedsMaxAnswersCount, "PL0041", messages.PL0041_AnswerExceedsMaxAnswersCount),
        };

        private bool Text_HasInvalidMask(AssignmentTextAnswer answer)
        {
            if (string.IsNullOrEmpty(answer.Value)) return false;
            if (string.IsNullOrEmpty(answer.Mask)) return false;

            return !new MaskedFormatter(answer.Mask).IsTextMaskMatched(answer.Value);
        }

        private bool DateTime_NotParsed(AssignmentDataTimeAnswer answer)
            => !string.IsNullOrWhiteSpace(answer.Value) && !answer.Answer.HasValue;

        private bool ExpectedDecimalNotParsed(AssignmentAnswer answer)
        {
            throw new NotImplementedException();
        }

        private bool Categorical_AnswerExceedsMaxAnswersCount(AssignmentCategoricalMultiAnswer answer)
            => answer.MaxAnswersCount.HasValue &&
               answer.Values.OfType<AssignmentIntegerAnswer>().Count(x => x.Answer.HasValue) > answer.MaxAnswersCount;

        private bool Gps_CommaSymbolIsNotAllowed(AssignmentGpsAnswer answer)
        {
            var latitude = answer.Values.FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Latitude).ToLower())?.Value;
            var longitude = answer.Values.FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Longitude).ToLower())?.Value;

            return !string.IsNullOrWhiteSpace(latitude) && latitude.Contains(",") ||
                   !string.IsNullOrWhiteSpace(longitude) && longitude.Contains(",");
        }

        private bool Gps_LongitudeMustBeGeaterThenN180AndLessThen180(AssignmentGpsAnswer answer)
        {
            if (this.Gps_DontHaveLongitudeOrLatitude(answer)) return false;

            var longitude = answer.Values.OfType<AssignmentDoubleAnswer>()
                .FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Longitude).ToLower()).Answer;

            return longitude < -180 || longitude > 180;
        }

        private bool Gps_LatitudeMustBeGeaterThenN90AndLessThen90(AssignmentGpsAnswer answer)
        {
            if (this.Gps_DontHaveLongitudeOrLatitude(answer)) return false;

            var latitude = answer.Values.OfType<AssignmentDoubleAnswer>()
                .FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Latitude).ToLower()).Answer;

            return latitude < -90 || latitude > 90;
        }

        private bool Gps_DontHaveLongitudeOrLatitude(AssignmentGpsAnswer answer)
        {
            if (this.Gps_NotParsed(answer)) return false;

            var latitude = answer.Values.OfType<AssignmentDoubleAnswer>()
                .FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Latitude).ToLower())?.Answer;
            var longitude = answer.Values.OfType<AssignmentDoubleAnswer>()
                .FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Longitude).ToLower())?.Answer;

            return !latitude.HasValue || !longitude.HasValue;
        }

        private bool Gps_NotParsed(AssignmentGpsAnswer answer)
        {
            var latitudeAnswer = answer.Values.OfType<AssignmentDoubleAnswer>().FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Latitude).ToLower());
            var longitudeValueAnswer = answer.Values.OfType<AssignmentDoubleAnswer>().FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Longitude).ToLower());

            return !string.IsNullOrWhiteSpace(latitudeAnswer.Value) && !latitudeAnswer.Answer.HasValue ||
                   !string.IsNullOrWhiteSpace(longitudeValueAnswer.Value) && !longitudeValueAnswer.Answer.HasValue;
        }

        private bool Interger_NotParsed(AssignmentIntegerAnswer answer)
            => !string.IsNullOrWhiteSpace(answer.Value) && !answer.Answer.HasValue;

        private bool Integer_CommaSymbolIsNotAllowed(AssignmentIntegerAnswer answer)
            => !string.IsNullOrWhiteSpace(answer.Value) && answer.Value.Contains(",");

        private bool Integer_ExceededRosterSize(AssignmentIntegerAnswer answer)
            => !this.Integer_IsNegativeRosterSize(answer) &&
               answer.IsRosterSize && !answer.IsRosterSizeForLongRoster && answer.Answer.HasValue && answer.Answer > Constants.MaxRosterRowCount;

        private bool Integer_ExceededLongRosterSize(AssignmentIntegerAnswer answer)
            => !this.Integer_IsNegativeRosterSize(answer) &&
               answer.IsRosterSizeForLongRoster && answer.Answer.HasValue && answer.Answer > Constants.MaxLongRosterRowCount;

        private bool Integer_IsNegativeRosterSize(AssignmentIntegerAnswer answer)
            => answer.IsRosterSize && answer.Answer.HasValue && answer.Answer < 0;

        private bool Responsible_HasInvalidRole(AssignmentResponsible responsible)
            => !this.Responsible_NotFound(responsible) && responsible.Responsible.IsSupervisorOrInterviewer;

        private bool Responsible_IsLocked(AssignmentResponsible responsible) 
            => !this.Responsible_NotFound(responsible) && responsible.Responsible.IsLocked;

        private bool Responsible_NotFound(AssignmentResponsible responsible) 
            => !this.Responsible_IsEmpty(responsible) && responsible.Responsible == null;

        private bool Responsible_IsEmpty(AssignmentResponsible responsible) 
            => string.IsNullOrWhiteSpace(responsible.Value);
        
        private bool Quantity_IsNegative(AssignmentQuantity quantity)
            => !Quantity_IsNotInteger(quantity) && quantity.Quantity != -1 && quantity.Quantity < 1;

        private bool Quantity_IsNotInteger(AssignmentQuantity quantity)
            => !string.IsNullOrWhiteSpace(quantity.Value) && !quantity.Quantity.HasValue;

        #region OLD OLD CODE

        private IEnumerable<Func<PreloadedDataByFile[], IPreloadedDataService, IEnumerable<PanelImportVerificationError>>> PanelVerifiers => new[]
        {
            this.Verifier(this.IdDuplication, "PL0006", messages.PL0006_IdDublication),   
            this.Verifier(this.RosterIdIsInconsistencyWithRosterSizeQuestion, "PL0009", messages.PL0009_RosterIdIsInconsistantWithRosterSizeQuestion),
            
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
        #endregion
    }
}
