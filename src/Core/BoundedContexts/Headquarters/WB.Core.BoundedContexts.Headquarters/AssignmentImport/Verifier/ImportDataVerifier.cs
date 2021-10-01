using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Preloading;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
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
        private readonly IQuestionOptionsRepository questionOptionsRepository;
        private readonly IQueryableReadSideRepositoryReader<Assignment, Guid> assignmentsRepository;

        public ImportDataVerifier(IFileSystemAccessor fileSystem,
            IInterviewTreeBuilder interviewTreeBuilder,
            IUserViewFactory userViewFactory,
            IQuestionOptionsRepository questionOptionsRepository,
            IQueryableReadSideRepositoryReader<Assignment, Guid> assignmentsRepository)
        {
            this.fileSystem = fileSystem;
            this.interviewTreeBuilder = interviewTreeBuilder;
            this.userViewFactory = userViewFactory;
            this.questionOptionsRepository = questionOptionsRepository;
            this.assignmentsRepository = assignmentsRepository;
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
                var questionId = questionnaire.GetQuestionIdByVariable(variableName.Value).Value;
                var questionType = questionnaire.GetQuestionType(questionId);

                if (questionType == QuestionType.Numeric)
                {
                    var questionOptions = questionnaire.GetOptionsForQuestion(questionId, null, null, null);
                    if (questionOptions.Any() || !questionnaire.IsQuestionInteger(questionId))
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
                        else
                        {
                            interviewTreeQuestion.SetAnswer(answer.Answer, DateTime.UtcNow);
                            interviewTreeQuestion.RunImportInvariantsOrThrow(new InterviewQuestionInvariants(answer.Identity, questionnaire, tree, questionOptionsRepository));
                        }
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

        public IEnumerable<PanelImportVerificationError> VerifyWebPasswords(List<PreloadingAssignmentRow> assignmentRows, IQuestionnaire questionnaire)
        {
            bool IsPrivateWebLinkWithPassword(PreloadingAssignmentRow x) => (x.WebMode?.WebMode == null || x.WebMode?.WebMode == true) && 
                                                                            (x.Quantity?.Quantity == 1 || x.Quantity?.Quantity == null) &&
                                                                            !string.IsNullOrEmpty(x.Password?.Value) &&
                                                                            string.IsNullOrEmpty(x.Email?.Value);

            var privatePasswordProtectedWebAssignments = assignmentRows.Where(IsPrivateWebLinkWithPassword).ToArray();

            var assignmentsWithDuplicatedPassword = privatePasswordProtectedWebAssignments
                .Where(x => x.Password.Value != AssignmentConstants.PasswordSpecialValue)
                .GroupBy(x => x.Password.Value)
                .Where(x => x.Count() > 1)
                .SelectMany(x => x.ToArray());

            foreach (var duplicatedPassword in assignmentsWithDuplicatedPassword)
            {
                yield return ToCellError("PL0061", messages.PL0061_DuplicatePasswordWithQuantity1,
                    duplicatedPassword, duplicatedPassword.Password);
            }

            if(assignmentsWithDuplicatedPassword.Any()) yield break;

            var questionnaireIdentity = new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version);

            foreach (var batchOfPrivatePasswordProtectedWebAssignments in privatePasswordProtectedWebAssignments.Batch(1000))
            {
                var expectedUniquePasswords = batchOfPrivatePasswordProtectedWebAssignments
                    .Select(x => x.Password.Value)
                    .Where(x => x != AssignmentConstants.PasswordSpecialValue)
                    .ToArray();

                var passwordsByWebAssignmentsInDb = this.assignmentsRepository.Query(x => x
                    .Where(y => y.Quantity == 1 && 
                                 (y.WebMode == null || y.WebMode == true) && 
                                 y.QuestionnaireId == questionnaireIdentity &&
                                 (y.Email == null || y.Email == "") &&
                                 expectedUniquePasswords.Contains(y.Password))
                    .Select(y => y.Password).ToList());

                if(!passwordsByWebAssignmentsInDb.Any()) continue;

                foreach (var assignment in batchOfPrivatePasswordProtectedWebAssignments.Where(x => passwordsByWebAssignmentsInDb.Contains(x.Password.Value)))
                {
                    yield return ToCellError("PL0061", messages.PL0061_DuplicatePasswordWithQuantity1,
                        assignment, assignment.Password);
                }
            }
        }

        public IEnumerable<PanelImportVerificationError> VerifyRowValues(PreloadingAssignmentRow assignmentRow, IQuestionnaire questionnaire)
        {
            foreach (var assignmentValue in assignmentRow.Answers)
            {
                foreach (var error in this.RowValuesVerifiers.SelectMany(x => x.Invoke(assignmentRow, assignmentValue, questionnaire)))
                    if (error != null) yield return error;
            }

            foreach (var serviceValue in (assignmentRow.RosterInstanceCodes ?? Array.Empty<AssignmentValue>()).Union(
                new[] {assignmentRow.InterviewIdValue, assignmentRow.Responsible, assignmentRow.Quantity,
                    assignmentRow.Email, assignmentRow.Password, assignmentRow.WebMode}))
            {
                if (serviceValue == null) continue;

                foreach (var error in this.RowValuesVerifiers.SelectMany(x => x.Invoke(assignmentRow, serviceValue, questionnaire)))
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

        private IEnumerable<Func<PreloadingAssignmentRow, BaseAssignmentValue, IQuestionnaire, IEnumerable<PanelImportVerificationError>>> RowValuesVerifiers => new[]
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
            Error<AssignmentResponsible>(Responsible_HasInvalidRole, "PL0028", messages.PL0028_UserIsNotHQOrSupervisorOrInterviewer),
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
            Error<AssignmentQuantity>(Quantity_ExceedsMaxInterviewsCount, "PL0054", string.Format(messages.PL0054_MaxInterviewsCountByAssignmentExeeded, Constants.MaxInterviewsCountByAssignment)),
            Error<AssignmentEmail>(Invalid_Email, "PL0055", messages.PL0055_InvalidEmail),
            Error<AssignmentPassword>(Invalid_Password, "PL0056", messages.PL0056_InvalidPassword),
            Error<AssignmentQuantity>(IncosistentQuantityAndEmail, "PL0057", messages.PL0057_IncosistentQuantityAndEmail),
            Error<AssignmentEmail>(IncosistentWebmodeAndEmail, "PL0058", messages.PL0058_IncosistentWebmodeAndEmail),
            Error<AssignmentPassword>(IncosistentWebmodeAndPassword, "PL0059", messages.PL0059_IncosistentWebmodeAndPassword),
            Error<AssignmentQuantity>(WebmodeSizeOneHasNoEmailOrPassword, "PL0060", messages.PL0060_WebmodeSizeOneHasNoEmailOrPassword),
            Error<AssignmentWebMode>(WebmodeSizeOneHasNoEmailOrPassword, "PL0060", messages.PL0060_WebmodeSizeOneHasNoEmailOrPassword),
            Error<AssignmentResponsible>(WebModeOnlyForInterviewer, "PL0062", messages.PL0062_WebModeOnlyForInterviewer),
            ErrorsByNotPermittedQuestions
        };

        private IEnumerable<PanelImportVerificationError> ErrorsByNotPermittedQuestions(PreloadingAssignmentRow row, BaseAssignmentValue value, IQuestionnaire questionnaire)
        {
            if (value == null || !(value is IAssignmentAnswer answer)) yield break;
            var questionId = questionnaire.GetQuestionIdByVariable(answer.VariableName);
            if (questionId == null) yield break;

            var questionType = questionnaire.GetQuestionType(questionId.Value);

            if (new[] {QuestionType.Area, QuestionType.Multimedia, QuestionType.Audio}.Contains(questionType)
                || ((questionType == QuestionType.MultyOption
                     || questionType == QuestionType.SingleOption)
                    && (questionnaire.IsQuestionLinked(questionId.Value) ||
                        questionnaire.IsQuestionLinkedToRoster(questionId.Value))))
            {
                var column = string.Empty;
                var content = string.Empty;

                switch (value)
                {
                    case AssignmentAnswers assignmentAnswers:
                        column = assignmentAnswers.Values[0].Column;
                        content = assignmentAnswers.Values[0].Value;
                        break;
                    case AssignmentValue assignmentValue:
                        column = assignmentValue.Column;
                        content = assignmentValue.Value;
                        break;
                }

                yield return new PanelImportVerificationError(
                    "PL0063",
                    string.Format(messages.PL0063_NoPermittedQuestion, answer.VariableName),

                    new InterviewImportReference(
                        column,
                        row.Row,
                        PreloadedDataVerificationReferenceType.Cell,
                        content,
                        row.FileName)
                );
            }
        }

        private bool WebmodeSizeOneHasNoEmailOrPassword(AssignmentWebMode webMode, PreloadingAssignmentRow assignmentRow)
            => assignmentRow.Quantity?.Quantity == null &&
               webMode.WebMode == true &&
               string.IsNullOrEmpty(assignmentRow.Password?.Value) &&
               string.IsNullOrEmpty(assignmentRow.Email?.Value);
        
        private bool WebModeOnlyForInterviewer(AssignmentResponsible webMode, PreloadingAssignmentRow assignmentRow)
            => assignmentRow.WebMode?.WebMode == true && webMode.Responsible?.InterviewerId == null;

        private bool WebmodeSizeOneHasNoEmailOrPassword(AssignmentQuantity quantity, PreloadingAssignmentRow assignmentRow)
            => quantity.Quantity == 1 &&
               assignmentRow.WebMode?.WebMode == true &&
               string.IsNullOrEmpty(assignmentRow.Password?.Value) &&
               string.IsNullOrEmpty(assignmentRow.Email?.Value);

        private bool IncosistentWebmodeAndPassword(AssignmentPassword password, PreloadingAssignmentRow assignmentRow)
            => !string.IsNullOrEmpty(password.Value) && (assignmentRow.WebMode == null || assignmentRow.WebMode.WebMode == false);

        private bool IncosistentWebmodeAndEmail(AssignmentEmail email, PreloadingAssignmentRow assignmentRow)
            => !string.IsNullOrEmpty(email.Value) && (assignmentRow.WebMode?.WebMode == false || assignmentRow.WebMode == null);

        private bool IncosistentQuantityAndEmail(AssignmentQuantity quantity, PreloadingAssignmentRow assignmentRow)
            => quantity.Quantity != 1 && !string.IsNullOrEmpty(assignmentRow.Email?.Value);

        private bool Invalid_Password(AssignmentPassword password)
        {
            if (string.IsNullOrEmpty(password.Value))
                return false;

            if (password.Value == AssignmentConstants.PasswordSpecialValue)
                return false;

            if (password.Value.Length < AssignmentConstants.PasswordLength)
                return true;

            return AssignmentConstants.PasswordStrength.Match(password.Value).Length <= 0;
        }

        private bool Invalid_Email(AssignmentEmail email)
        {
            if (string.IsNullOrEmpty(email.Value))
                return false;

            return AssignmentConstants.EmailRegex.Match(email.Value).Length <= 0;
        }

        private IEnumerable<InterviewImportReference> OrphanFirstLevelRoster(List<PreloadingAssignmentRow> allRowsByAllFiles,
            IQuestionnaire questionnaire)
        {
            // if only main file without interview id column in zip 
            // this verification should be skipped
            if (allRowsByAllFiles.Any(x => x.InterviewIdValue == null)) yield break;

            var enumerableRows = allRowsByAllFiles
                .Where(x => IsQuestionnaireFile(x.QuestionnaireOrRosterName, questionnaire))
                .Select(x => x.InterviewIdValue.Value);
            var allInterviewIdsFromMainFile = enumerableRows.ToImmutableHashSet();

            var allInterviewsIdsFromFirstLevelRoster = allRowsByAllFiles
                .Where(x => x.RosterInstanceCodes.Length == 1)
                .ToList();

            foreach (var rosterRow in allInterviewsIdsFromFirstLevelRoster)
            {
                //Non empty interview Id
                //Empty ones are handled in another check
                if (!string.IsNullOrEmpty(rosterRow.InterviewIdValue.Value) && !allInterviewIdsFromMainFile.Contains(rosterRow.InterviewIdValue.Value))
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

                    var parentRosterVector = new RosterVector(rowsByRosterInstance.Key.Shrink());

                    if (!allRosterVectorsInInterview.Contains(parentRosterVector))
                    {
                        foreach (var groupedRow in rowsByRosterInstance)
                        {
                            var rosterInstanceColumns = string.Join(", ", groupedRow.row.RosterInstanceCodes.Shrink().Select(x => x.Column));

                            yield return new InterviewImportReference(rosterInstanceColumns,
                                groupedRow.row.Row, PreloadedDataVerificationReferenceType.Cell,
                                string.Join(", ", parentRosterVector.Select(x => x.ToString())),
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

            var enumerableVariables = numericRosterSizeQuestions
                .SelectMany(questionnaire.GetRosterGroupsByRosterSizeQuestion)
                .Select(questionnaire.GetRosterVariableName);
            var rosterNamesByNumericRosters = enumerableVariables.ToImmutableHashSet();

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
                        .Select((x, i) => (row: x, expectedCode: i));

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

            if ((columnName == ServiceColumns.ResponsibleColumnName ||
                 columnName == ServiceColumns.AssignmentsCountColumnName ||
                 columnName == ServiceColumns.EmailColumnName ||
                 columnName == ServiceColumns.PasswordColumnName ||
                 columnName == ServiceColumns.WebModeColumnName ||
                 columnName == ServiceColumns.RecordAudioColumnName ||
                 columnName == ServiceColumns.CommentsColumnName) && 
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

            var questionId = questionnaire.GetQuestionIdByVariable(questionVariable);
            if (questionId == null)
                return false;

            var questionType = questionnaire.GetQuestionType(questionId.Value);
            return questionType == QuestionType.TextList && !int.TryParse(sortIndex, out _);
        }

        private bool CategoricalMultiQuestion_OptionNotFound(PreloadedFileInfo file, string columnName, IQuestionnaire questionnaire)
        {
            var compositeColumn = columnName.Split(new[] { ServiceColumns.ColumnDelimiter},
                StringSplitOptions.RemoveEmptyEntries);

            if (compositeColumn.Length < 2) return false;

            var questionId = questionnaire.GetQuestionIdByVariable(compositeColumn[0]);
            if (questionId == null)
                return false;

            var optionCode = compositeColumn[1].Replace("n", "-");

            return questionnaire.GetQuestionType(questionId.Value) == QuestionType.MultyOption 
                   && !questionnaire.IsQuestionLinked(questionId.Value) 
                   && !questionnaire.IsQuestionLinkedToRoster(questionId.Value) 
                   && questionnaire.GetOptionsForQuestion(questionId.Value, null, null, null)
                       .All(x => x.Value.ToString() != optionCode);
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
        {
            if (string.IsNullOrWhiteSpace(answer.Value))
                return false;

            if (!answer.OptionCode.HasValue)
                return true;

            var questionId = questionnaire.GetQuestionIdByVariable(answer.VariableName);
            if (questionId == null)
                return false;

            var options = questionnaire.GetOptionsForQuestion(questionId.Value, null, null, null);
            return (options?.All(x => x.Value.ToString() != answer.Value) ?? false);
        }

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

            return questionnaire.IsRosterSizeQuestion(questionId.Value) && answer.Answer.HasValue && answer.Answer < 0;
        }

        private bool Responsible_HasInvalidRole(AssignmentResponsible responsible) 
            => !string.IsNullOrWhiteSpace(responsible.Value) && responsible.Responsible != null && !responsible.Responsible.IsHQOrSupervisorOrInterviewer;

        private bool Responsible_IsLocked(AssignmentResponsible responsible)
            => !string.IsNullOrWhiteSpace(responsible.Value) && responsible.Responsible != null &&
               responsible.Responsible.IsHQOrSupervisorOrInterviewer && responsible.Responsible.IsLocked;

        private bool Responsible_NotFound(AssignmentResponsible responsible) 
            => !string.IsNullOrWhiteSpace(responsible.Value) && responsible.Responsible == null;
        
        private bool Quantity_IsNegative(AssignmentQuantity quantity)
            => quantity.Quantity.HasValue && quantity.Quantity < -1;

        private bool Quantity_ExceedsMaxInterviewsCount(AssignmentQuantity quantity)
            => quantity.Quantity.HasValue && quantity.Quantity > Constants.MaxInterviewsCountByAssignment;

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

        private static Func<PreloadingAssignmentRow, BaseAssignmentValue, IQuestionnaire, IEnumerable<PanelImportVerificationError>> Error<TValue>(
            Func<TValue, PreloadingAssignmentRow, bool> hasError, string code, string message) where TValue : AssignmentValue => (row, cell, questionnaire) =>
            cell is TValue typedCell && hasError(typedCell, row) ? new[] { ToCellError(code, message, row, typedCell) } : Array.Empty<PanelImportVerificationError>();

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
