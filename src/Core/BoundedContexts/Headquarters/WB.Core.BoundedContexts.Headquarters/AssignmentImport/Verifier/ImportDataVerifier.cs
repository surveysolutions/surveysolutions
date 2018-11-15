using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Preloading;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using messages = WB.Core.BoundedContexts.Headquarters.Resources.PreloadingVerificationMessages;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier
{
    
    internal class ImportDataVerifier : IPreloadedDataVerifier
    {
        readonly QuestionType[] TypesThatSupportProtection = new[]
        {
            QuestionType.MultyOption,
            QuestionType.Numeric,
            QuestionType.TextList
        };

        private readonly IFileSystemAccessor fileSystem;
        private readonly IInterviewTreeBuilder interviewTreeBuilder;
        private readonly IUserViewFactory userViewFactory;
        private IQuestionOptionsRepository questionOptionsRepository;

        public ImportDataVerifier(IFileSystemAccessor fileSystem,
            IInterviewTreeBuilder interviewTreeBuilder,
            IUserViewFactory userViewFactory,
            IQuestionOptionsRepository questionOptionsRepository)
        {
            this.fileSystem = fileSystem;
            this.interviewTreeBuilder = interviewTreeBuilder;
            this.userViewFactory = userViewFactory;
            this.questionOptionsRepository = questionOptionsRepository;
        }

        public IEnumerable<PanelImportVerificationError> VerifyProtectedVariables(string originalFileName, PreloadedFile file, IQuestionnaire questionnaire)
        {
            if (!file.FileInfo.Columns.Contains(ServiceColumns.ProtectedVariableNameColumn))
            {

                yield return ToFileError("PL0047", string.Format(messages.PL0047_ProtectedVariables_MissingColumn, ServiceColumns.ProtectedVariableNameColumn),
                    new PreloadedFileInfo
                    {
                        FileName = $"{ServiceFiles.ProtectedVariables}.tab",
                        Columns = file.FileInfo.Columns
                    }, originalFileName);
                yield break;
            }

            foreach (var preloadingRow in file.Rows)
            {
                foreach (var error in this.ProtectedVariablesVerifiers.SelectMany(x =>
                    x.Invoke(file.FileInfo.FileName, (PreloadingValue)preloadingRow.Cells[0], questionnaire)))
                    if (error != null)
                        yield return error;
            }
        }

        private IEnumerable<Func<string, PreloadingValue, IQuestionnaire, IEnumerable<PanelImportVerificationError>>> ProtectedVariablesVerifiers => new[]
        {
            Error(ProtectedVariableIsMissingInQuestionnaire, "PL0048", messages.PL0048_ProtectedVariables_VariableNotFoundInQuestionnaire),
            Error(VariableCannotBeProtected, "PL0049", messages.PL0049_ProtectedVariables_VariableNotSupportsProtection)
        };

        private bool VariableCannotBeProtected(PreloadingValue variableName, IQuestionnaire questionnaire)
        {
            if (questionnaire.HasQuestion(variableName.Value))
            {
                var question = questionnaire.GetQuestionByVariable(variableName.Value);
                var questionType = question.QuestionType;

                if (questionType == QuestionType.Numeric)
                {
                    if(question.Answers.Count > 0 || !questionnaire.IsQuestionInteger(question.PublicKey))
                        return true;
                }

                return !TypesThatSupportProtection.Contains(questionType);
            }

            return false;
        }

        private bool ProtectedVariableIsMissingInQuestionnaire(PreloadingValue protectedVariablesFile, IQuestionnaire questionnaire)
        {
            return !questionnaire.HasQuestion(protectedVariablesFile.Value);
        }

        public InterviewImportError VerifyWithInterviewTree(IList<InterviewAnswer> answers, Guid? responsibleId, IQuestionnaire questionnaire)
        {
            var answersGroupedByLevels = answers.GroupedByLevels();

            try
            {
                var tree = this.interviewTreeBuilder.BuildInterviewTree(Guid.NewGuid(), questionnaire);

                var noAnswersOnQuestionnaireLevel =
                    answersGroupedByLevels.All(x => x.FirstOrDefault()?.Identity.RosterVector.Length != 0);
                if (noAnswersOnQuestionnaireLevel)
                    tree.ActualizeTree();

                foreach (var answersInLevel in answersGroupedByLevels)
                {
                    foreach (InterviewAnswer answer in answersInLevel)
                    {
                        var interviewTreeQuestion = tree.GetQuestion(answer.Identity);
                        if (interviewTreeQuestion == null)
                        {
                            new InterviewQuestionInvariants(answer.Identity, questionnaire, tree, questionOptionsRepository).RequireQuestionExists();
                        }

                        interviewTreeQuestion.SetAnswer(answer.Answer, DateTime.UtcNow);

                        interviewTreeQuestion.RunImportInvariantsOrThrow(new InterviewQuestionInvariants(answer.Identity, questionnaire, tree, questionOptionsRepository));
                    }
                    tree.ActualizeTree();
                }

                return null;
            }
            catch (Exception ex)
            {
                var allAnswersInString = string.Join(", ",
                    answersGroupedByLevels.SelectMany(x => x.Select(_ => _.Answer)).Where(x => x != null)
                        .Select(x => x.ToString()));

                var responsible = responsibleId.HasValue
                    ? this.userViewFactory.GetUser(new UserViewInputModel(responsibleId.Value))
                    : null;

                var errorMessage = string.Format(Interviews.ImportInterviews_GenericError, allAnswersInString,
                    responsible?.UserName, ex.Message);

                var errorInfo = new List<string>();

                foreach (DictionaryEntry data in ex.Data)
                    errorInfo.Add($"{data.Key}: {data.Value}");

                return new InterviewImportError("PL0011", $"PL0011: {errorMessage}. {string.Join(", ", errorInfo)}");
            }
        }

        public IEnumerable<PanelImportVerificationError> VerifyAnswers(PreloadingAssignmentRow assignmentRow, IQuestionnaire questionnaire)
        {
            foreach (var assignmentValue in assignmentRow.Answers)
            {
                foreach (var error in this.AnswerVerifiers.SelectMany(x => x.Invoke(assignmentRow, assignmentValue, questionnaire)))
                    if (error != null) yield return error;
            }

            foreach (var serviceValue in (assignmentRow.RosterInstanceCodes ?? Array.Empty<AssignmentValue>()).Union(
                new[] {assignmentRow.InterviewIdValue, assignmentRow.Responsible, assignmentRow.Quantity}))
            {
                if (serviceValue == null) continue;

                foreach (var error in this.AnswerVerifiers.SelectMany(x => x.Invoke(assignmentRow, serviceValue, questionnaire)))
                    if (error != null) yield return error;
            }
        }

        public IEnumerable<PanelImportVerificationError> VerifyFiles(string originalFileName, PreloadedFileInfo[] files, IQuestionnaire questionnaire)
        {
            if (!files.Any(x => IsQuestionnaireFile(x.QuestionnaireOrRosterName, questionnaire)))
            {
                yield return ToFileError("PL0040", messages.PL0040_QuestionnaireDataIsNotFound,
                    new PreloadedFileInfo
                    {
                        FileName = $"{questionnaire.VariableName}.tab"
                    }, originalFileName);
                yield break;
            }

            foreach (var file in files)
            foreach (var error in this.FileVerifiers.SelectMany(x => x.Invoke(originalFileName, file, questionnaire)))
                if (error != null) yield return error;

        }

        public IEnumerable<PanelImportVerificationError> VerifyFile(string fileName, PreloadedFileInfo file, IQuestionnaire questionnaire)
        {
            foreach (var error in this.SampleFileVerifiers.SelectMany(x => x.Invoke(fileName, file, questionnaire)))
                if (error != null) yield return error;
        }

        public IEnumerable<PanelImportVerificationError> VerifyRosters(List<PreloadingAssignmentRow> allRowsByAllFiles, IQuestionnaire questionnaire)
        {
            foreach (var error in this.RosterVerifiers.SelectMany(x => x.Invoke(allRowsByAllFiles, questionnaire)))
                if (error != null) yield return error;
        }

        public IEnumerable<PanelImportVerificationError> VerifyColumns(PreloadedFileInfo[] files, IQuestionnaire questionnaire)
        {
            foreach (var file in files)
            {
                foreach (var columnName in file.Columns)
                foreach (var error in this.ColumnVerifiers.SelectMany(x => x.Invoke(file, columnName, questionnaire)))
                    if (error != null) yield return error;

                var columnNames = file.Columns.Select(x => x.ToLower());
                if (/*advanced preloading*/files.Length > 1 && !columnNames.Contains(ServiceColumns.InterviewId))
                {
                    yield return ToColumnError("PL0007", messages.PL0007_ServiceColumnIsAbsent, file.FileName, ServiceColumns.InterviewId);
                }
            }
        }

        private IEnumerable<Func<string, PreloadedFileInfo, IQuestionnaire, IEnumerable<PanelImportVerificationError>>> SampleFileVerifiers => new[]
        {
            Errors(OptionalGpsPropertyAndMissingLatitudeAndLongitude, "PL0030", messages.PL0030_GpsFieldsRequired),
            Errors(DuplicatedColumns, "PL0031", messages.PL0031_ColumnNameDuplicatesFound)
        };

        private IEnumerable<Func<string, PreloadedFileInfo, IQuestionnaire, IEnumerable<PanelImportVerificationError>>> FileVerifiers => SampleFileVerifiers.Union(new[]
        {
            Error(RosterFileNotFound, "PL0004", messages.PL0004_FileWasntMappedRoster),
            Errors(MissingRosterInstanceColumns, "PL0007", messages.PL0007_ServiceColumnIsAbsent),
            Errors(ColumnByTextListRosterSizeAnswerNotFound, "PL0052", messages.PL0052_ColumnByTextListRosterSizeAnswerNotFound),
        });

        private IEnumerable<Func<List<PreloadingAssignmentRow>, IQuestionnaire, IEnumerable<PanelImportVerificationError>>> RosterVerifiers => new[]
        {
            Error(OrphanNestedRoster, "PL0008", messages.PL0008_OrphanRosterRecord),
            Error(OrphanFirstLevelRoster, "PL0008", messages.PL0008_OrphanRosterRecord),
            Error(DuplicatedRosterInstances, "PL0006", messages.PL0006_IdDublication),
            Error(InconsistentNumericRosterInstanceCodes, "PL0053", messages.PL0053_InconsistentNumericRosterInstanceCodes)
        };

        private IEnumerable<Func<PreloadedFileInfo, string, IQuestionnaire, IEnumerable<PanelImportVerificationError>>> ColumnVerifiers => new[]
        {
            Error(UnknownColumn, "PL0003", messages.PL0003_ColumnWasntMappedOnQuestion),
            Error(TextListQuestion_InvalidSortIndex, "PL0003", messages.PL0003_ColumnWasntMappedOnQuestion),
            Error(CategoricalMultiQuestion_OptionNotFound, "PL0014", messages.PL0014_ParsedValueIsNotAllowed)
        };

        private IEnumerable<Func<PreloadingAssignmentRow, BaseAssignmentValue, IQuestionnaire, IEnumerable<PanelImportVerificationError>>> AnswerVerifiers => new[]
        {
            Error<AssignmentRosterInstanceCode>(RosterInstanceCode_NoParsed, "PL0009", messages.PL0009_RosterIdIsInconsistantWithRosterSizeQuestion),
            Error<AssignmentRosterInstanceCode>(RosterInstanceCode_InvalidCode, "PL0009", messages.PL0009_RosterIdIsInconsistantWithRosterSizeQuestion),
            Error<AssignmentTextAnswer>(Text_HasInvalidMask, "PL0014", messages.PL0014_ParsedValueIsNotAllowed),
            Error<AssignmentCategoricalSingleAnswer>(CategoricalSingle_OptionNotFound, "PL0014", messages.PL0014_ParsedValueIsNotAllowed),
            Errorq<AssignmentMultiAnswer>(CategoricalMulti_OptionCode_NotParsed, "PL0014", messages.PL0014_ParsedValueIsNotAllowed),
            Error<AssignmentDateTimeAnswer>(DateTime_NotParsed, "PL0016", messages.PL0016_ExpectedDateTimeNotParsed),
            Errors<AssignmentGpsAnswer>(Gps_NotParsed, "PL0017", messages.PL0017_ExpectedGpsNotParsed),
            Error<AssignmentIntegerAnswer>(Integer_NotParsed, "PL0018", messages.PL0018_ExpectedIntNotParsed),
            Error<AssignmentDoubleAnswer>(Double_NotParsed, "PL0019", messages.PL0019_ExpectedDecimalNotParsed),
            Error<AssignmentIntegerAnswer>(Integer_IsNegativeRosterSize, "PL0022", messages.PL0022_AnswerIsIncorrectBecauseIsRosterSizeAndNegative),
            Error<AssignmentResponsible>(Responsible_NotFound, "PL0026", messages.PL0026_ResponsibleWasNotFound),
            Error<AssignmentResponsible>(Responsible_IsLocked, "PL0027", messages.PL0027_ResponsibleIsLocked),
            Error<AssignmentResponsible>(Responsible_HasInvalidRole, "PL0028", messages.PL0028_UserIsNotSupervisorOrInterviewer),
            Error<AssignmentIntegerAnswer>(Integer_ExceededRosterSize, "PL0029", string.Format(messages.PL0029_AnswerIsIncorrectBecauseIsRosterSizeAndMoreThan40, Constants.MaxRosterRowCount)),
            Error<AssignmentIntegerAnswer>(Integer_ExceededLongRosterSize, "PL0029", string.Format(messages.PL0029_AnswerIsIncorrectBecauseIsRosterSizeAndMoreThan40, Constants.MaxLongRosterRowCount)),
            Errors<AssignmentGpsAnswer>(Gps_DontHaveLongitudeOrLatitude, "PL0030", messages.PL0030_GpsMandatoryFilds),
            Errors<AssignmentGpsAnswer>(Gps_LatitudeMustBeGreaterThenN90AndLessThen90, "PL0032", messages.PL0032_LatitudeMustBeGeaterThenN90AndLessThen90),
            Errors<AssignmentGpsAnswer>(Gps_LongitudeMustBeGreaterThenN180AndLessThen180, "PL0033", messages.PL0033_LongitudeMustBeGeaterThenN180AndLessThen180),
            Errors<AssignmentGpsAnswer>(Gps_CommaSymbolIsNotAllowed, "PL0034", messages.PL0034_CommaSymbolIsNotAllowedInNumericAnswer),
            Error<AssignmentDoubleAnswer>(Double_CommaSymbolIsNotAllowed, "PL0034", messages.PL0034_CommaSymbolIsNotAllowedInNumericAnswer),
            Error<AssignmentQuantity>(Quantity_IsNotInteger, "PL0035", messages.PL0035_QuantityNotParsed),
            Error<AssignmentQuantity>(Quantity_IsNegative, "PL0036", messages.PL0036_QuantityShouldBeGreaterThanMinus1),
            Errors<AssignmentMultiAnswer>(CategoricalMulti_AnswerExceedsMaxAnswersCount, "PL0041", messages.PL0041_AnswerExceedsMaxAnswersCount),
            Error<AssignmentInterviewId>(NoInterviewId, "PL0042", messages.PL0042_IdIsEmpty),
            Errorq<AssignmentMultiAnswer>(CategoricalMulti_AnswerMustBeGreaterOrEqualThen0, "PL0050", messages.PL0050_CategoricalMulti_AnswerMustBeGreaterOrEqualThen1),
        };

        private IEnumerable<InterviewImportReference> OrphanFirstLevelRoster(List<PreloadingAssignmentRow> allRowsByAllFiles,
            IQuestionnaire questionnaire)
        {
            // if only main file without interview id column in zip 
            // this verification should be skipped
            if (allRowsByAllFiles.Any(x => x.InterviewIdValue == null)) yield break;

            var allInterviewIdsFromMainFile = allRowsByAllFiles
                .Where(x => IsQuestionnaireFile(x.QuestionnaireOrRosterName, questionnaire))
                .Select(x => x.InterviewIdValue.Value)
                .ToHashSet();

            var allInterviewsIdsFromFirstLevelRoster = allRowsByAllFiles
                .Where(x => x.RosterInstanceCodes.Length == 1)
                .ToList();

            foreach (var rosterRow in allInterviewsIdsFromFirstLevelRoster)
            {
                if (!allInterviewIdsFromMainFile.Contains(rosterRow.InterviewIdValue.Value))
                    yield return new InterviewImportReference(rosterRow.InterviewIdValue.Column,
                        rosterRow.Row,
                        PreloadedDataVerificationReferenceType.Cell,
                        rosterRow.InterviewIdValue.Value,
                        rosterRow.FileName);
            }
        }

        private IEnumerable<InterviewImportReference> OrphanNestedRoster(List<PreloadingAssignmentRow> allRowsByAllFiles,
            IQuestionnaire questionnaire)
        {
            // if only main file without interview id column in zip 
            // this verification should be skipped
            if (allRowsByAllFiles.Any(x => x.InterviewIdValue == null)) yield break;

            foreach (var rowsByInterview in allRowsByAllFiles.GroupBy(x => x.InterviewIdValue.Value))
            {
                var rowsByRosterInstances = rowsByInterview
                    .Select(x => new
                    {
                        rosterVector = new RosterVector(x.RosterInstanceCodes.Select(y => y.Code.Value)),
                        row = x
                    }).Where(x => x.rosterVector != RosterVector.Empty).GroupBy(x => x.rosterVector).ToArray();

                var allRosterVectorsInInterview = rowsByRosterInstances.Select(x => x.Key).ToArray();

                foreach (var rowsByRosterInstance in rowsByRosterInstances)
                {
                    /*for nested rosters only, because first level roster has parent with empty roster vector*/
                    if (rowsByRosterInstance.Key.Length == 1) continue;

                    var parentRosterVactor = new RosterVector(rowsByRosterInstance.Key.Shrink());

                    if (!allRosterVectorsInInterview.Contains(parentRosterVactor))
                    {
                        foreach (var groupedRow in rowsByRosterInstance)
                        {
                            var rosterInstanceColumns = string.Join(", ", groupedRow.row.RosterInstanceCodes.Shrink().Select(x => x.Column));

                            yield return new InterviewImportReference(rosterInstanceColumns,
                                groupedRow.row.Row, PreloadedDataVerificationReferenceType.Cell,
                                string.Join(", ", parentRosterVactor.Select(x => x.ToString())),
                                groupedRow.row.FileName);
                        }
                    }
                }
            }
        }

        private IEnumerable<InterviewImportReference> InconsistentNumericRosterInstanceCodes(
            List<PreloadingAssignmentRow> allRowsByAllFiles, IQuestionnaire questionnaire)
        {
            // if only main file without interview id column in zip 
            // this verification should be skipped
            if (allRowsByAllFiles.Any(x => x.InterviewIdValue == null)) yield break;

            var numericRosterSizeQuestions = questionnaire.GetAllRosterSizeQuestions().Where(questionnaire.IsQuestionInteger).ToArray();
            if(numericRosterSizeQuestions.Length == 0)
                yield break;

            var rosterNamesByNumericRosters = numericRosterSizeQuestions
                .SelectMany(questionnaire.GetRosterGroupsByRosterSizeQuestion)
                .Select(questionnaire.GetRosterVariableName)
                .ToHashSet();

            var rowsByNumericRosters = allRowsByAllFiles.GroupBy(z => z.QuestionnaireOrRosterName)
                .Where(x => rosterNamesByNumericRosters.Contains(x.Key));

            foreach (var rowsByNumericRoster in rowsByNumericRosters)
            {
                var rosterColumnId = string.Format(ServiceColumns.IdSuffixFormat, rowsByNumericRoster.Key.ToLower());

                var rosterId = questionnaire.GetRosterIdByVariableName(rowsByNumericRoster.Key, true);
                var parentRosterIds = questionnaire.GetRostersFromTopToSpecifiedGroup(rosterId.Value)
                    .Where(x => x != rosterId).ToArray();

                IEnumerable<PreloadingAssignmentRow[]> rowsByInterviewsAndParentRoster;

                if (parentRosterIds.Length == 0)
                    rowsByInterviewsAndParentRoster = rowsByNumericRoster.GroupBy(x => x.InterviewIdValue.Value).Select(x => x.ToArray());
                else
                {
                    var parentRosterId = parentRosterIds.Last();
                    var parentRosterVariable = questionnaire.GetRosterVariableName(parentRosterId).ToLower();
                    var parentRosterColumnId = string.Format(ServiceColumns.IdSuffixFormat, parentRosterVariable);

                    rowsByInterviewsAndParentRoster = rowsByNumericRoster.GroupBy(x => new
                    {
                        x.InterviewIdValue.Value,
                        x.RosterInstanceCodes.FirstOrDefault(y => y.VariableName == parentRosterColumnId)?.Code
                    }).Select(x => x.ToArray());
                }

                foreach (var rowsByInterviewAndParentRoster in rowsByInterviewsAndParentRoster)
                {
                    var orderedRosterInstanceRows = rowsByInterviewAndParentRoster
                        .OrderBy(x => x.RosterInstanceCodes.First(y => y.VariableName == rosterColumnId).Code)
                        .Select((x, i) => (row: x, expectedCode: i + 1));

                    foreach (var rosterInstanceRow in orderedRosterInstanceRows)
                    {
                        var rosterInstanceCode =
                            rosterInstanceRow.row.RosterInstanceCodes.First(y => y.VariableName == rosterColumnId);
                        if (rosterInstanceCode.Code != rosterInstanceRow.expectedCode)
                        {
                            yield return new InterviewImportReference(rosterInstanceCode.Column,
                                rosterInstanceRow.row.Row, PreloadedDataVerificationReferenceType.Cell,
                                rosterInstanceCode.Code.ToString(), rosterInstanceRow.row.FileName);
                        }
                    }
                }
            }
        }

        private IEnumerable<InterviewImportReference> DuplicatedRosterInstances(List<PreloadingAssignmentRow> allRowsByAllFiles, IQuestionnaire questionnaire)
        {
            foreach (var rowsByFile in allRowsByAllFiles.Where(x => x.RosterInstanceCodes.Length > 0).GroupBy(x => x.FileName))
            {
                var answersByRosterInstances = rowsByFile
                    .Select(x => new {interviewid = x.InterviewIdValue.Value, rosterInstanceId = new RosterVector(x.RosterInstanceCodes.Select(y => y.Code.Value)), row = x})
                    .GroupBy(x => new {x.interviewid, x.rosterInstanceId})
                    .Where(x => x.Count() > 1);

                foreach (var answersByRosterInstance in answersByRosterInstances)
                {
                    var rosterInstanceColumns = string.Join(", ",
                        answersByRosterInstance.First().row.RosterInstanceCodes.Select(x => x.Column));

                    foreach (var duplicatedRosterInstanceRow in answersByRosterInstance)
                    {
                        yield return new InterviewImportReference(rosterInstanceColumns,
                            duplicatedRosterInstanceRow.row.Row, PreloadedDataVerificationReferenceType.Cell,
                            string.Join(", ", answersByRosterInstance.Key.rosterInstanceId.Select(x => x.ToString())),
                            rowsByFile.Key);
                    }
                }
            }
        }

        private bool NoInterviewId(AssignmentInterviewId answer)
            => string.IsNullOrWhiteSpace(answer.Value);
        
        private bool RosterFileNotFound(PreloadedFileInfo file, IQuestionnaire questionnaire)
            => !IsQuestionnaireFile(file.QuestionnaireOrRosterName, questionnaire) && 
               !questionnaire.HasRoster(file.QuestionnaireOrRosterName);

        private bool UnknownColumn(PreloadedFileInfo file, string columnName, IQuestionnaire questionnaire)
        {
            if (string.IsNullOrWhiteSpace(columnName)) return true;

            if (columnName == ServiceColumns.InterviewId) return false;

            if ((columnName == ServiceColumns.ResponsibleColumnName || columnName == ServiceColumns.AssignmentsCountColumnName) && 
                IsQuestionnaireFile(file.QuestionnaireOrRosterName, questionnaire)) return false;

            if (ServiceColumns.AllSystemVariables.Contains(columnName)) return false;

            if (GetRosterInstanceIdColumns(file, questionnaire).Any(x => x.newName == columnName || x.oldName == columnName)) return false;

            var compositeColumnValues = columnName.Split(new[] { ServiceColumns.ColumnDelimiter },
                StringSplitOptions.RemoveEmptyEntries);

            var questionOrVariableName = compositeColumnValues[0].ToLower();

            var rosterIdByVariable = questionnaire.GetRosterIdByVariableName(file.QuestionnaireOrRosterName, true);
            var rosterIds = new HashSet<Guid?>() { rosterIdByVariable };

            if (rosterIdByVariable.HasValue && !questionnaire.IsFixedRoster(rosterIdByVariable.Value))
            {
                var rosterSizeQuestionId = questionnaire.GetRosterSizeQuestion(rosterIdByVariable.Value);

                if (questionnaire.GetQuestionType(rosterSizeQuestionId) == QuestionType.TextList &&
                    questionnaire.GetQuestionVariableName(rosterSizeQuestionId).ToLower() == questionOrVariableName) return false;

                rosterIds.Clear();
                questionnaire.GetRosterGroupsByRosterSizeQuestion(rosterSizeQuestionId).ForEach(r => rosterIds.Add(r));
            }

            foreach (var rosterId in rosterIds)
            {
                foreach (var variableId in questionnaire.GetAllUnderlyingVariablesOutsideRosters(rosterId))
                    if (questionnaire.GetVariableName(variableId).ToLower() == questionOrVariableName) return false;

                foreach (var questionId in questionnaire.GetAllUnderlyingQuestionsOutsideRosters(rosterId))
                {
                    var questionVariable = questionnaire.GetQuestionVariableName(questionId).ToLower();

                    if (questionnaire.GetQuestionType(questionId) == QuestionType.GpsCoordinates)
                    {
                        if (GeoPosition.PropertyNames.Select(x => $"{questionVariable}{ServiceColumns.ColumnDelimiter}{x.ToLower()}")
                            .Contains(columnName)) return false;
                    }
                    else
                    {
                        if (questionVariable == questionOrVariableName) return false;
                    }
                }
            }

            return true;
        }

        private IEnumerable<string> MissingRosterInstanceColumns(PreloadedFileInfo file, IQuestionnaire questionnaire)
        {
            if (file.Columns == null) yield break;

            var columnNames = file.Columns.Select(x => x.ToLower());

            foreach (var rosterColumnNames in this.GetRosterInstanceIdColumns(file, questionnaire))
            {
                if (!columnNames.Any(columnName => rosterColumnNames.oldName == columnName || rosterColumnNames.newName == columnName))
                    yield return rosterColumnNames.newName;
            }
        }

        private IEnumerable<string> DuplicatedColumns(PreloadedFileInfo file, IQuestionnaire questionnaire)
        {
            if (file.Columns == null) yield break;
            foreach (var duplicatedColumns in file.Columns.GroupBy(x => x.ToLower()).Where(x => x.Count() > 1))
                // as discussed, user should see all duplicated column names in all caps
                yield return string.Join(", ", duplicatedColumns);
        }

        private IEnumerable<string> ColumnByTextListRosterSizeAnswerNotFound(PreloadedFileInfo file, IQuestionnaire questionnaire)
        {
            if (file.Columns == null) yield break;

            var rosterId = questionnaire.GetRosterIdByVariableName(file.QuestionnaireOrRosterName, true);
            if(!rosterId.HasValue) yield break;
            if(questionnaire.IsFixedRoster(rosterId.Value)) yield break;

            var rosterSizeQuestionId = questionnaire.GetRosterSizeQuestion(rosterId.Value);
            var rosterSizeQuestionType = questionnaire.GetQuestionType(rosterSizeQuestionId);
            var rosterSizeQuestionVariable = questionnaire.GetQuestionVariableName(rosterSizeQuestionId);
            var rosterSizeQuestionVariableLower = rosterSizeQuestionVariable.ToLower();

            var hasRosterSizeTextListAnswersColumn = file.Columns.Any(x =>
                string.Equals(x, rosterSizeQuestionVariableLower, StringComparison.OrdinalIgnoreCase));

            if (rosterSizeQuestionType == QuestionType.TextList && !hasRosterSizeTextListAnswersColumn)
                yield return rosterSizeQuestionVariable;
        }

        private IEnumerable<string> OptionalGpsPropertyAndMissingLatitudeAndLongitude(PreloadedFileInfo file, IQuestionnaire questionnaire)
        {
            if(file.Columns == null) yield break;

            var columnNames = file.Columns.Select(x => x.ToLower()).ToArray();
            
            var allGpsVariableNames = file.Columns
                .Select(x => x.Split(new[] { ServiceColumns.ColumnDelimiter }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Select(questionnaire.GetQuestionIdByVariable)
                .Where(x => x.HasValue)
                .Select(x=>x.Value)
                .Where(x => questionnaire.GetQuestionType(x) == QuestionType.GpsCoordinates)
                .Select(questionnaire.GetQuestionVariableName)
                .ToArray();

            foreach (var gpsVariableName in allGpsVariableNames)
            {
                var variable = gpsVariableName.ToLower();
                var hasLatitude = columnNames.Contains($"{variable}{ServiceColumns.ColumnDelimiter}{nameof(GeoPosition.Latitude).ToLower()}");
                var hasLongitude = columnNames.Contains($"{variable}{ServiceColumns.ColumnDelimiter}{nameof(GeoPosition.Longitude).ToLower()}");

                if (!hasLongitude || !hasLatitude) yield return gpsVariableName;
            }
        }

        private bool TextListQuestion_InvalidSortIndex(PreloadedFileInfo file, string columnName, IQuestionnaire questionnaire)
        {
            var compositeColumn = columnName.Split(new[] { ServiceColumns.ColumnDelimiter },
                StringSplitOptions.RemoveEmptyEntries);

            if (compositeColumn.Length < 2) return false;

            var questionVariable = compositeColumn[0];
            var sortIndex = compositeColumn[1];

            var question = questionnaire.GetQuestionByVariable(questionVariable);

            return question?.QuestionType == QuestionType.TextList && !int.TryParse(sortIndex, out _);
        }

        private bool CategoricalMultiQuestion_OptionNotFound(PreloadedFileInfo file, string columnName, IQuestionnaire questionnaire)
        {
            var compositeColumn = columnName.Split(new[] { ServiceColumns.ColumnDelimiter},
                StringSplitOptions.RemoveEmptyEntries);

            if (compositeColumn.Length < 2) return false;

            var question = questionnaire.GetQuestionByVariable(compositeColumn[0]);
            var optionCode = compositeColumn[1].Replace("n", "-");

            return question?.QuestionType == QuestionType.MultyOption && 
                   !question.LinkedToQuestionId.HasValue && !question.LinkedToRosterId.HasValue &&
                   question.Answers.All(x => x.AnswerValue != optionCode);
        }

        private bool RosterInstanceCode_InvalidCode(AssignmentRosterInstanceCode answer, IQuestionnaire questionnaire)
        {
            if (!answer.Code.HasValue) return false;

            var rosterId = questionnaire.GetRosterIdByVariableName(answer.VariableName, true);
            if (!rosterId.HasValue) return false;

            if (questionnaire.IsFixedRoster(rosterId.Value))
                return !questionnaire.GetFixedRosterCodes(rosterId.Value).Contains(answer.Code.Value);

            var rosterSizeQuestionId = questionnaire.GetRosterSizeQuestion(rosterId.Value);

            var questionType = questionnaire.GetQuestionType(rosterSizeQuestionId);
            switch (questionType)
            {
                case QuestionType.MultyOption:
                    return !questionnaire.GetMultiSelectAnswerOptionsAsValues(rosterSizeQuestionId).Contains(answer.Code.Value);
                case QuestionType.Numeric:
                case QuestionType.TextList:
                    return answer.Code < 0;
            }

            return false;
        }

        private bool RosterInstanceCode_NoParsed(AssignmentRosterInstanceCode answer) => !answer.Code.HasValue;

        private bool CategoricalSingle_OptionNotFound(AssignmentCategoricalSingleAnswer answer, IQuestionnaire questionnaire)
            => !string.IsNullOrWhiteSpace(answer.Value) &&
               (!answer.OptionCode.HasValue || (questionnaire.GetQuestionByVariable(answer.VariableName)?.Answers
                                                ?.All(x => x.AnswerValue != answer.Value) ?? false));

        private bool Text_HasInvalidMask(AssignmentTextAnswer answer, IQuestionnaire questionnaire)
        {
            if (string.IsNullOrEmpty(answer.Value)) return false;

            var questionId = questionnaire.GetQuestionIdByVariable(answer.VariableName);
            if (!questionId.HasValue) return false;

            var textMask = questionnaire.GetTextQuestionMask(questionId.Value);
            if (string.IsNullOrWhiteSpace(textMask)) return false;

            return !MaskedFormatter.IsTextMaskMatched(answer.Value, textMask);
        }

        private bool DateTime_NotParsed(AssignmentDateTimeAnswer answer)
            => !string.IsNullOrWhiteSpace(answer.Value) && !answer.Answer.HasValue;

        private bool Double_NotParsed(AssignmentDoubleAnswer answer)
            => !string.IsNullOrWhiteSpace(answer.Value) && !answer.Value.Contains(",") && !answer.Answer.HasValue;

        private bool CategoricalMulti_AnswerExceedsMaxAnswersCount(AssignmentMultiAnswer answer, IQuestionnaire questionnaire)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(answer.VariableName);
            if (!questionId.HasValue) return false;

            var maxAnswersCount = questionnaire.GetMaxSelectedAnswerOptions(questionId.Value);

            return maxAnswersCount.HasValue &&
                   answer.Values.OfType<AssignmentIntegerAnswer>().Count(x => x.Answer >= 1) > maxAnswersCount;
        }

        private IEnumerable<AssignmentAnswer> CategoricalMulti_OptionCode_NotParsed(AssignmentMultiAnswer answer, IQuestionnaire questionnaire)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(answer.VariableName);
            if (!questionId.HasValue) yield break;

            if (questionnaire.GetQuestionType(questionId.Value) != QuestionType.MultyOption) yield break;

            foreach (var assignmentAnswer in answer.Values.OfType<AssignmentIntegerAnswer>())
                if (Integer_NotParsed(assignmentAnswer)) yield return assignmentAnswer;
        }
        
        private IEnumerable<AssignmentAnswer> CategoricalMulti_AnswerMustBeGreaterOrEqualThen0(AssignmentMultiAnswer answer, IQuestionnaire questionnaire)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(answer.VariableName);
            if (!questionId.HasValue) yield break;

            if (questionnaire.GetQuestionType(questionId.Value) != QuestionType.MultyOption) yield break;

            foreach (var assignmentAnswer in answer.Values.OfType<AssignmentIntegerAnswer>())
                if (assignmentAnswer.Answer < 0) yield return assignmentAnswer;
        }
        
        private IEnumerable<AssignmentAnswer> Gps_CommaSymbolIsNotAllowed(AssignmentGpsAnswer answer)
            => answer.Values.OfType<AssignmentDoubleAnswer>().Where(answerValue =>
                !string.IsNullOrWhiteSpace(answerValue.Value) && answerValue.Value.Contains(","));

        private IEnumerable<AssignmentAnswer> Gps_LongitudeMustBeGreaterThenN180AndLessThen180(AssignmentGpsAnswer answer)
        {
            var longitude = answer.Values.OfType<AssignmentDoubleAnswer>()
                .FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Longitude).ToLower());

            if (longitude?.Answer != null && (longitude.Answer < -180 || longitude.Answer > 180))
                yield return longitude;
        }

        private IEnumerable<AssignmentAnswer> Gps_LatitudeMustBeGreaterThenN90AndLessThen90(AssignmentGpsAnswer answer)
        {
            var latitude = answer.Values.OfType<AssignmentDoubleAnswer>()
                .FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Latitude).ToLower());

            if (latitude?.Answer != null && (latitude.Answer < -90 || latitude.Answer > 90))
                yield return latitude;
        }

        private bool Gps_DontHaveLongitudeOrLatitude(AssignmentGpsAnswer answer, IQuestionnaire questionnaire)
        {
            var latitude = answer.Values.OfType<AssignmentDoubleAnswer>()
                .FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Latitude).ToLower())?.Answer;
            var longitude = answer.Values.OfType<AssignmentDoubleAnswer>()
                .FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Longitude).ToLower())?.Answer;

            return !latitude.HasValue && longitude.HasValue || latitude.HasValue && !longitude.HasValue;
        }

        private IEnumerable<AssignmentAnswer> Gps_NotParsed(AssignmentGpsAnswer answer)
        {
            foreach (var answerValue in answer.Values)
            {
                if(string.IsNullOrWhiteSpace(answerValue.Value)) continue;

                switch (answerValue)
                {
                    case AssignmentDoubleAnswer asDouble:
                        if (!asDouble.Value.Contains(",") && !asDouble.Answer.HasValue) yield return asDouble;
                        break;
                    case AssignmentDateTimeAnswer asDateTime:
                        if (!asDateTime.Answer.HasValue) yield return asDateTime;
                        break;
                }
            }
        }

        private bool Integer_NotParsed(AssignmentIntegerAnswer answer)
            => !string.IsNullOrWhiteSpace(answer.Value) && (answer.Value.Contains(",") || !answer.Answer.HasValue);

        private bool Double_CommaSymbolIsNotAllowed(AssignmentDoubleAnswer answer)
            => !string.IsNullOrWhiteSpace(answer.Value) && answer.Value.Contains(",");

        private bool Integer_ExceededRosterSize(AssignmentIntegerAnswer answer, IQuestionnaire questionnaire)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(answer.VariableName);
            if (!questionId.HasValue) return false;
            if (!questionnaire.IsRosterSizeQuestion(questionId.Value)) return false;

            return !questionnaire.IsQuestionIsRosterSizeForLongRoster(questionId.Value) &&
                answer.Answer.HasValue && answer.Answer > questionnaire.GetMaxRosterRowCount();
        }

        private bool Integer_ExceededLongRosterSize(AssignmentIntegerAnswer answer, IQuestionnaire questionnaire)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(answer.VariableName);
            if (!questionId.HasValue) return false;

            return questionnaire.IsQuestionIsRosterSizeForLongRoster(questionId.Value) &&
                   answer.Answer.HasValue && answer.Answer > questionnaire.GetMaxLongRosterRowCount();
        }

        private bool Integer_IsNegativeRosterSize(AssignmentIntegerAnswer answer, IQuestionnaire questionnaire)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(answer.VariableName);
            if (!questionId.HasValue) return false;

            return questionnaire.IsRosterSizeQuestion(questionId.Value) && answer.Answer.HasValue && answer.Answer < Constants.MinLongRosterRowCount;
        }

        private bool Responsible_HasInvalidRole(AssignmentResponsible responsible) 
            => !string.IsNullOrWhiteSpace(responsible.Value) && responsible.Responsible != null && !responsible.Responsible.IsSupervisorOrInterviewer;

        private bool Responsible_IsLocked(AssignmentResponsible responsible)
            => !string.IsNullOrWhiteSpace(responsible.Value) && responsible.Responsible != null &&
               responsible.Responsible.IsSupervisorOrInterviewer && responsible.Responsible.IsLocked;

        private bool Responsible_NotFound(AssignmentResponsible responsible) 
            => !string.IsNullOrWhiteSpace(responsible.Value) && responsible.Responsible == null;
        
        private bool Quantity_IsNegative(AssignmentQuantity quantity)
            => quantity.Quantity.HasValue && quantity.Quantity < -1;

        private bool Quantity_IsNotInteger(AssignmentQuantity quantity)
            => !string.IsNullOrWhiteSpace(quantity.Value) && !quantity.Quantity.HasValue;

        private bool IsQuestionnaireFile(string questionnaireOrRosterName, IQuestionnaire questionnaire)
        {
            var inputFileName = this.fileSystem.MakeStataCompatibleFileName(questionnaireOrRosterName);
            var questionnaireFileName = this.fileSystem.MakeStataCompatibleFileName(questionnaire.Title);

            return string.Equals(inputFileName, questionnaireFileName, StringComparison.InvariantCultureIgnoreCase) ||
                   string.Equals(inputFileName, questionnaire.VariableName, StringComparison.InvariantCultureIgnoreCase);
        }

        private IEnumerable<(string oldName, string newName)> GetRosterInstanceIdColumns(PreloadedFileInfo file, IQuestionnaire questionnaire)
        {
            var rosterId = questionnaire.GetRosterIdByVariableName(file.QuestionnaireOrRosterName, true);
            if (!rosterId.HasValue) yield break;

            int indexOfRosterSizeSource = 1;
            foreach (var parentRosterId in questionnaire.GetRostersFromTopToSpecifiedGroup(rosterId.Value))
            {
                var newName = string.Format(ServiceColumns.IdSuffixFormat, questionnaire.GetRosterVariableName(parentRosterId).ToLower());
                var oldName = $"{ServiceColumns.ParentId}{indexOfRosterSizeSource++}".ToLower();

                yield return (oldName, newName);
            }
        }

        private static Func<string, PreloadedFileInfo, IQuestionnaire, IEnumerable<PanelImportVerificationError>> Error(
            Func<PreloadedFileInfo, IQuestionnaire, bool> hasError, string code, string message)
        {
            return (originalFileName, file, questionnaire) =>
            {
                return hasError(file, questionnaire)
                    ? new[] {ToFileError(code, message, file, originalFileName)}
                    : Array.Empty<PanelImportVerificationError>();
            };
        }

        private static Func<string, PreloadingValue, IQuestionnaire, IEnumerable<PanelImportVerificationError>> Error(
            Func<PreloadingValue, IQuestionnaire, bool> hasError, string code, string message)
        {
            return (originalFileName, row, questionnaire) => hasError(row, questionnaire)
                ? new[] {ToCellError(originalFileName, code, message, row.Row, row.Column, row.Value)}
                : Array.Empty<PanelImportVerificationError>();
        }

        private static PanelImportVerificationError ToCellError(string originalFileName, string code, string message, int row,
            string columnName, string columnValue)
        {
            return new PanelImportVerificationError(code, message,
                new InterviewImportReference(columnName, row,
                    PreloadedDataVerificationReferenceType.Cell, columnValue, originalFileName));
        }

        private static Func<string, PreloadedFileInfo, IQuestionnaire, IEnumerable<PanelImportVerificationError>> Errors(
            Func<PreloadedFileInfo, IQuestionnaire, IEnumerable<string>> getColumnsWithErrors, string code, string message) => (originalFileName, file, questionnaire) =>
            getColumnsWithErrors(file, questionnaire).Select(x=> ToColumnError(code, message, file.FileName, ToNewColumnFormat(x)));

        private static Func<List<PreloadingAssignmentRow>, IQuestionnaire, IEnumerable<PanelImportVerificationError>> Error(
            Func<List<PreloadingAssignmentRow>, IQuestionnaire, IEnumerable<InterviewImportReference>> getRowsWithErrors,
            string code, string message)
            => (allRowsByAllFiles, questionnaire) =>
            {
                var rowsWithErrors = getRowsWithErrors(allRowsByAllFiles, questionnaire).ToArray();
                return rowsWithErrors.Any() ? new []{ new PanelImportVerificationError(code, message, rowsWithErrors)} : Array.Empty<PanelImportVerificationError>();
            };

        private static Func<PreloadedFileInfo, string, IQuestionnaire, IEnumerable<PanelImportVerificationError>> Error(
            Func<PreloadedFileInfo, string, IQuestionnaire, bool> hasError, string code, string message) => (file, columnName, questionnaire) =>
            hasError(file, columnName?.ToLower(), questionnaire) ? new []{ToColumnError(code, message, file.FileName, ToNewColumnFormat(columnName))} : Array.Empty<PanelImportVerificationError>();
        
        private static Func<PreloadingAssignmentRow, BaseAssignmentValue, IQuestionnaire, IEnumerable<PanelImportVerificationError>> Error<TValue>(
            Func<TValue, bool> hasError, string code, string message) where TValue : AssignmentValue => (row, cell, questionnaire) =>
            cell is TValue typedCell && hasError(typedCell) ? new []{ToCellError(code, message, row, typedCell) } : Array.Empty<PanelImportVerificationError>();

        private static Func<PreloadingAssignmentRow, BaseAssignmentValue, IQuestionnaire, IEnumerable<PanelImportVerificationError>> Error<TValue>(
            Func<TValue, IQuestionnaire, bool> hasError, string code, string message) where TValue : AssignmentValue => (row, cell, questionnaire) =>
            cell is TValue typedCell && hasError(typedCell, questionnaire) ? new []{ToCellError(code, message, row, typedCell) } : Array.Empty<PanelImportVerificationError>();

        private static Func<PreloadingAssignmentRow, BaseAssignmentValue, IQuestionnaire, IEnumerable<PanelImportVerificationError>>
            Errors<TValue>(Func<TValue, IQuestionnaire, bool> hasError, string code, string message) where TValue : AssignmentAnswers
        {
            IEnumerable<PanelImportVerificationError> verify(PreloadingAssignmentRow row, BaseAssignmentValue cell, IQuestionnaire questionnaire)
            {
                if (!(cell is TValue compositeAnswer)) yield break;
                if (hasError(compositeAnswer, questionnaire))
                {
                    // as discussed, user should see questionnaire's variable name for question if generic error by multiply columns
                    var originalQuestionVariableName = compositeAnswer.VariableName;
                    
                    var questionId = questionnaire.GetQuestionIdByVariable(originalQuestionVariableName);
                    if (questionId.HasValue)
                        originalQuestionVariableName = questionnaire.GetQuestionVariableName(questionId.Value);

                    yield return ToCellError(code, message, row, originalQuestionVariableName, null);
                }
            }

            return verify;
        }

        private static Func<PreloadingAssignmentRow, BaseAssignmentValue, IQuestionnaire, IEnumerable<PanelImportVerificationError>>
            Errors<TValue>(Func<TValue, IEnumerable<AssignmentAnswer>> hasError, string code, string message) where TValue: AssignmentAnswers
        {
            IEnumerable<PanelImportVerificationError> verify(PreloadingAssignmentRow row, BaseAssignmentValue cell, IQuestionnaire questionnaire)
            {
                if (!(cell is TValue compositeAnswer)) yield break;

                foreach (var assignmentAnswerWithError in hasError(compositeAnswer))
                    yield return ToCellError(code, message, row, assignmentAnswerWithError.Column, assignmentAnswerWithError.Value);
            }

            return verify;
        }

        private static Func<PreloadingAssignmentRow, BaseAssignmentValue, IQuestionnaire, IEnumerable<PanelImportVerificationError>>
            Errorq<TValue>(Func<TValue, IQuestionnaire, IEnumerable<AssignmentAnswer>> hasError, string code, string message) where TValue : AssignmentAnswers
        {
            IEnumerable<PanelImportVerificationError> verify(PreloadingAssignmentRow row, BaseAssignmentValue cell, IQuestionnaire questionnaire)
            {
                if (!(cell is TValue compositeAnswer)) yield break;

                foreach (var assignmentAnswerWithError in hasError(compositeAnswer, questionnaire))
                    yield return ToCellError(code, message, row, assignmentAnswerWithError.Column, assignmentAnswerWithError.Value);
            }

            return verify;
        }

        private static PanelImportVerificationError ToFileError(string code, string message, PreloadedFileInfo fileInfo, string originalFileName)
            => new PanelImportVerificationError(code, message, new InterviewImportReference(PreloadedDataVerificationReferenceType.File, fileInfo.FileName, originalFileName));

        private static PanelImportVerificationError ToColumnError(string code, string message, string fileName, string columnName)
            => new PanelImportVerificationError(code, message,
                new InterviewImportReference(PreloadedDataVerificationReferenceType.Column,
                    ToNewColumnFormat(columnName), fileName));

        private static PanelImportVerificationError ToCellError(string code, string message, PreloadingAssignmentRow row, AssignmentValue assignmentValue)
            => new PanelImportVerificationError(code, message, new InterviewImportReference(assignmentValue.Column, row.Row, PreloadedDataVerificationReferenceType.Cell,
                assignmentValue.Value, row.FileName));

        private static PanelImportVerificationError ToCellError(string code, string message,
            PreloadingAssignmentRow row, string column, string value)
            => new PanelImportVerificationError(code, message,
                new InterviewImportReference(ToNewColumnFormat(column), row.Row,
                    PreloadedDataVerificationReferenceType.Cell, value, row.FileName));

        private static string ToNewColumnFormat(string columnName)
        {
            var lowerColumnName = columnName.ToLower();
            if (lowerColumnName == ServiceColumns.InterviewId) return columnName;
            if (ServiceColumns.AllSystemVariables.Contains(lowerColumnName)) return columnName;

            var compositeColumnValues = columnName.Split(new[] { ServiceColumns.ColumnDelimiter },
                StringSplitOptions.RemoveEmptyEntries);

            if (compositeColumnValues.Length != 2) return columnName;

            return string.Format(ServiceColumns.IdSuffixFormat, compositeColumnValues[0]) == columnName
                ? columnName
                : $"{compositeColumnValues[0]}[{compositeColumnValues[1]}]";
        }
    }
}
