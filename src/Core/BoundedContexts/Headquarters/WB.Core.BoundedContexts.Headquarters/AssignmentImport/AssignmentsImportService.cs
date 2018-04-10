using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Main.Core.Entities.SubEntities;
using Npgsql;
using NpgsqlTypes;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public partial class AssignmentsImportService : IAssignmentsImportService
    {
        private const string AssignmentsToImportTableName = "\"plainstore\".\"assignmenttoimport\"";
        private const string AssignmentsImportProcessTableName = "\"plainstore\".\"assignmentsimportprocess\"";

        private readonly ICsvReader csvReader;
        private readonly IArchiveUtils archiveUtils;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IUserViewFactory userViewFactory;
        private readonly IFileSystemAccessor fileSystem;
        private readonly IPreloadedDataVerifier verifier;
        private readonly IAuthorizedUser authorizedUser;
        private readonly ISessionProvider sessionProvider;
        private readonly AssignmentsImportTask assignmentsImportTask;
        private readonly IPlainStorageAccessor<AssignmentsImportProcess> importAssignmentsProcessRepository;
        private readonly IPlainStorageAccessor<AssignmentImportData> importAssignmentsRepository;
        private readonly ISerializer serializer;

        public AssignmentsImportService(ICsvReader csvReader, IArchiveUtils archiveUtils, 
            IQuestionnaireStorage questionnaireStorage, IUserViewFactory userViewFactory, 
            IFileSystemAccessor fileSystem,
            IPreloadedDataVerifier verifier,
            IAuthorizedUser authorizedUser,
            ISessionProvider sessionProvider,
            AssignmentsImportTask assignmentsImportTask,
            IPlainStorageAccessor<AssignmentsImportProcess> importAssignmentsProcessRepository,
            IPlainStorageAccessor<AssignmentImportData> importAssignmentsRepository,
            ISerializer serializer)
        {
            this.csvReader = csvReader;
            this.archiveUtils = archiveUtils;
            this.questionnaireStorage = questionnaireStorage;
            this.userViewFactory = userViewFactory;
            this.fileSystem = fileSystem;
            this.verifier = verifier;
            this.authorizedUser = authorizedUser;
            this.sessionProvider = sessionProvider;
            this.assignmentsImportTask = assignmentsImportTask;
            this.importAssignmentsProcessRepository = importAssignmentsProcessRepository;
            this.importAssignmentsRepository = importAssignmentsRepository;
            this.serializer = serializer;
        }

        public IEnumerable<PanelImportVerificationError> VerifySimple(PreloadedFile file, QuestionnaireIdentity questionnaireIdentity)
        {
            if (this.assignmentsImportTask.IsJobRunning())
                throw new PreloadingException(PreloadingVerificationMessages.HasAssignmentsToImport);

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            bool hasErrors = false;

            foreach (var columnError in this.verifier.VerifyColumns(new[] { file.FileInfo }, questionnaire))
            {
                hasErrors = true;
                yield return columnError;
            }

            if (hasErrors) yield break;

            var assignmentRows = new List<AssignmentRow>();

            foreach (var assignmentRow in this.GetAssignmentRows(file, questionnaire))
            {
                foreach (var answerError in this.verifier.VerifyAnswers(assignmentRow, questionnaire))
                    yield return answerError;

                assignmentRows.Add(assignmentRow);
            }

            if (hasErrors) yield break;

            this.Save(file.FileInfo.FileName, assignmentRows.Select(row =>
            {
                var responsible = row.Answers.OfType<AssignmentResponsible>().FirstOrDefault();
                var quantity = row.Answers.OfType<AssignmentQuantity>().FirstOrDefault();
                var answers = row.Answers.OfType<AssignmentAnswer>().ToArray();

                return ToAssignmentToImport(answers, responsible, quantity, RosterVector.Empty, questionnaire);
            }).ToArray());
            assignmentsImportTask.Run();
        }

        public IEnumerable<PanelImportVerificationError> VerifyPanel(string originalFileName,
            PreloadedFile[] allImportedFiles,
            QuestionnaireIdentity questionnaireIdentity)
        {
            if (this.assignmentsImportTask.IsJobRunning())
                throw new PreloadingException(PreloadingVerificationMessages.HasAssignmentsToImport);

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            bool hasErrors = false;

            var preloadedFileInfos = allImportedFiles.Select(x => x.FileInfo).ToArray();

            foreach (var fileInfo in preloadedFileInfos)
                foreach (var fileError in this.verifier.VerifyFile(fileInfo, questionnaire))
                {
                    hasErrors = true;
                    yield return fileError;
                }

            if (hasErrors) yield break;

            foreach (var columnError in this.verifier.VerifyColumns(preloadedFileInfos, questionnaire))
            {
                hasErrors = true;
                yield return columnError;
            }

            if (hasErrors) yield break;

            var assignmentRows = new List<AssignmentRow>();

            foreach (var importedFile in allImportedFiles)
                foreach (var assignmentRow in this.GetAssignmentRows(importedFile, questionnaire))
                {
                    foreach (var answerError in this.verifier.VerifyAnswers(assignmentRow, questionnaire))
                    {
                        hasErrors = true;
                        yield return answerError;
                    }

                    assignmentRows.Add(assignmentRow);
                }

            if (hasErrors) yield break;

            foreach (var rosterError in this.verifier.VerifyRosters(assignmentRows, questionnaire))
            {
                hasErrors = true;
                yield return rosterError;
            }

            if (hasErrors) yield break;

            this.Save(originalFileName, ConcatRosters(assignmentRows, questionnaire));
            assignmentsImportTask.Run();
        }

        private IList<AssignmentImportData> ConcatRosters(List<AssignmentRow> assignmentRows,
            IQuestionnaire questionnaire)
        {
            var answersGroupedByRosters = assignmentRows.GroupBy(assignmentRow => assignmentRow.InterviewIdValue.Value)
                .Select(g => new
                {
                    quantity = g.Select(x => x.Answers.OfType<AssignmentQuantity>()).FirstOrDefault(),
                    responsible = g.Select(x => x.Answers.OfType<AssignmentResponsible>()).FirstOrDefault(),
                    rows = g.Select(x => new
                    {
                        rosterVector = new RosterVector(x.RosterInstanceCodes.Select(y => y.Code.Value).ToArray()),
                        answers = x.Answers.OfType<AssignmentAnswer>().ToArray()
                    })
                });

            var data = answersGroupedByRosters.SelectMany(x => x.rows.GroupBy(y => y.rosterVector)
                .Select(z => ToAssignmentToImport(
                    z.SelectMany(i => i.answers).ToArray(),
                    x.responsible.FirstOrDefault(),
                    x.quantity.FirstOrDefault(), z.Key, questionnaire))).ToList();

            return AddRosterSizeAnswersByRosterInstances(data, questionnaire);
        }

        private List<AssignmentImportData> AddRosterSizeAnswersByRosterInstances(
            List<AssignmentImportData> assignmentImportData, IQuestionnaire questionnaire)
        {
            var allAnsweredIdenities = assignmentImportData
                .SelectMany(x => x.PreloadedData.Answers.Select(y => y.Identity)).ToArray();

            var rosterSizeQuestions = questionnaire.GetAllRosterSizeQuestions();

            foreach (var rosterSizeQuestion in rosterSizeQuestions)
            {
                var questionType = questionnaire.GetQuestionType(rosterSizeQuestion);

                switch (questionType)
                {
                    case QuestionType.MultyOption:
                        break;
                    case QuestionType.Numeric:
                        break;
                    case QuestionType.TextList:
                        var allTextListIdentities = allAnsweredIdenities.Where(x => x.Id == rosterSizeQuestion);

                        break;
                }
            }
            
            return assignmentImportData;
        }

        public AssignmentImportData GetAssignmentToImport()
            => this.importAssignmentsRepository.Query(x => x.FirstOrDefault());

        public void RemoveImportedAssignment(AssignmentImportData assignment)
            => this.importAssignmentsRepository.Remove(new[] { assignment });

        public AssignmentsImportStatus GetImportStatus()
        {
            var process = this.importAssignmentsProcessRepository.Query(x => x.FirstOrDefault());
            var assignmentsInQueue = this.importAssignmentsRepository.Query(x => x.Count());

            return new AssignmentsImportStatus
            {
                IsOwnerOfRunningProcess = process?.Responsible == this.authorizedUser.UserName,
                IsInProgress = assignmentsInQueue > 0,
                TotalAssignmentsWithResponsible = process?.AssignedToInterviewersCount + process?.AssignedToSupervisorsCount ?? 0,
                AssignmentsInQueue = assignmentsInQueue,
                FileName = process?.FileName,
                StartedDate = process?.StartedDate,
                ResponsibleName = process?.Responsible,
                QuestionnaireIdentity = process?.QuestionnaireId == null ? null : QuestionnaireIdentity.Parse(process.QuestionnaireId)
            };
        }

        public void RemoveAllAssignmentsToImport()
        => this.sessionProvider.GetSession().Connection.Execute(
                $"DELETE FROM {AssignmentsToImportTableName};" +
                $"DELETE FROM {AssignmentsImportProcessTableName};");

        private void Save(string fileName, IList<AssignmentImportData> assignments)
        {
            this.SaveProcess(fileName, assignments);
            this.SaveAssignments(assignments);
        }

        private void SaveProcess(string fileName, IList<AssignmentImportData> assignments)
        {
            var process = this.importAssignmentsProcessRepository.Query(x => x.FirstOrDefault()) ??
                          new AssignmentsImportProcess();

            process.FileName = fileName;
            process.AssignedToInterviewersCount = assignments.Count(x => x.InterviewerId.HasValue);
            process.AssignedToSupervisorsCount = assignments.Count(x => x.SupervisorId.HasValue);
            process.Responsible = this.authorizedUser.UserName;
            process.StartedDate = DateTime.UtcNow;

            this.importAssignmentsProcessRepository.Store(process, process?.Id);
        }

        private void SaveAssignments(IList<AssignmentImportData> assignments)
        {
            var npgsqlConnection = this.sessionProvider.GetSession().Connection as NpgsqlConnection;

            using (var writer = npgsqlConnection.BeginBinaryImport($"COPY  {AssignmentsToImportTableName} (interviewer, supervisor, quantity, answers) " +
                                                                   "FROM STDIN BINARY;"))
            {
                foreach (var assignmentToImport in assignments)
                {
                    writer.StartRow();
                    writer.Write(assignmentToImport.InterviewerId, NpgsqlDbType.Uuid);
                    writer.Write(assignmentToImport.SupervisorId, NpgsqlDbType.Uuid);
                    writer.Write(assignmentToImport.Quantity, NpgsqlDbType.Integer);
                    writer.Write(this.serializer.Serialize(assignmentToImport.PreloadedData.Answers), NpgsqlDbType.Jsonb);
                }
            }
        }

        private AssignmentImportData ToAssignmentToImport(AssignmentAnswer[] answers, AssignmentResponsible responsible,
            AssignmentQuantity quantity, RosterVector rosterVector, IQuestionnaire questionnaire) =>
            new AssignmentImportData
            {
                InterviewerId = responsible?.Responsible?.InterviewerId,
                SupervisorId = responsible?.Responsible?.SupervisorId,
                Quantity = quantity?.Quantity,
                PreloadedData = new PreloadedDataDto(
                    answers.Select(x => ToInterviewAnswer(x, rosterVector, questionnaire)).Where(x => x != null).ToList())
            };

        private InterviewAnswer ToInterviewAnswer(AssignmentAnswer value, RosterVector rosterVector, IQuestionnaire questionnaire)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(value.VariableName);
            if (!questionId.HasValue) return null;

            var questionType = questionnaire.GetQuestionType(questionId.Value);

            var isRosterSizeQuestion = questionnaire.IsRosterSizeQuestion(questionId.Value);

            if (isRosterSizeQuestion && questionType == QuestionType.Numeric ||
                questionType == QuestionType.MultyOption) return null;

            var answer = new InterviewAnswer { Identity = Identity.Create(questionId.Value, rosterVector) };

            var isLinkedToQuestion = questionnaire.IsQuestionLinked(questionId.Value);
            var isLinkedToRoster = questionnaire.IsQuestionLinkedToRoster(questionId.Value);

            switch (questionType)
            {
                case QuestionType.SingleOption:
                    if (!isLinkedToQuestion && !isLinkedToRoster)
                    {
                        var assignmentInt = ((AssignmentDoubleAnswer) value).Answer;
                        if (assignmentInt.HasValue)
                            answer.Answer = CategoricalFixedSingleOptionAnswer.FromDecimal(Convert.ToDecimal(assignmentInt.Value));
                    }
                    break;
                case QuestionType.MultyOption:
                {
                    var assignmentCategoricalMulti = ((AssignmentMultiAnswer) value)?.Values
                        ?.OfType<AssignmentDoubleAnswer>()
                        .Where(x => x.Answer.HasValue)
                        ?.Select(x => new {code = Convert.ToDecimal(x.VariableName), answer = Convert.ToInt32(x.Answer)})
                        .ToArray();

                        if (assignmentCategoricalMulti.Any())
                        {
                            if (questionnaire.ShouldQuestionRecordAnswersOrder(questionId.Value))
                            {
                                if (questionnaire.IsQuestionYesNo(questionId.Value))
                                {
                                    answer.Answer = YesNoAnswer.FromAnsweredYesNoOptions(assignmentCategoricalMulti
                                        .OrderBy(x => x.answer)
                                        .Select(x => new AnsweredYesNoOption(x.code, true)));
                                }

                                if (!isLinkedToQuestion && !isLinkedToRoster)
                                {
                                    answer.Answer = CategoricalFixedMultiOptionAnswer.FromDecimalArray(
                                        assignmentCategoricalMulti.OrderBy(x => x.answer).Select(x => x.code)
                                            .ToArray());
                                }
                            }
                            else
                            {
                                if (questionnaire.IsQuestionYesNo(questionId.Value))
                                {
                                    answer.Answer = YesNoAnswer.FromAnsweredYesNoOptions(
                                        assignmentCategoricalMulti.Select(x => new AnsweredYesNoOption(x.code, x.answer > 0)));
                                }

                                if (!isLinkedToQuestion && !isLinkedToRoster)
                                {
                                    answer.Answer = CategoricalFixedMultiOptionAnswer.FromDecimalArray(
                                        assignmentCategoricalMulti.Select(x => x.code).ToArray());
                                }
                            }
                        }
                    }
                    break;
                case QuestionType.DateTime:
                    var assignmentDateTime = ((AssignmentDateTimeAnswer) value).Answer;
                    if (assignmentDateTime.HasValue)
                        answer.Answer = DateTimeAnswer.FromDateTime(assignmentDateTime.Value);
                    break;
                case QuestionType.GpsCoordinates:
                    var assignmentGpsValues = ((AssignmentGpsAnswer)value)?.Values;
                    if (assignmentGpsValues != null)
                    {
                        var doubleAnswers = assignmentGpsValues.OfType<AssignmentDoubleAnswer>();
                        var longitude = doubleAnswers.FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Longitude).ToLower())?.Answer;
                        var latitude = doubleAnswers.FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Latitude).ToLower())?.Answer;
                        var altitude = doubleAnswers.FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Altitude).ToLower())?.Answer;
                        var accuracy = doubleAnswers.FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Accuracy).ToLower())?.Answer;
                        var timestamp = assignmentGpsValues.OfType<AssignmentDateTimeAnswer>().FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Timestamp).ToLower())?.Answer;

                        answer.Answer = GpsAnswer.FromGeoPosition(new GeoPosition(latitude ?? 0, longitude ?? 0,
                            accuracy ?? 0, altitude ?? 0, timestamp ?? DateTimeOffset.MinValue));
                    }
                    break;
                case QuestionType.Numeric:
                    if (questionnaire.IsQuestionInteger(questionId.Value))
                    {
                        var assignmentInt = ((AssignmentIntegerAnswer)value).Answer;
                        if (assignmentInt.HasValue)
                            answer.Answer = NumericIntegerAnswer.FromInt(assignmentInt.Value);
                    }
                    else
                    {
                        var assignmentDouble = ((AssignmentDoubleAnswer)value).Answer;
                        if (assignmentDouble.HasValue)
                            answer.Answer = NumericRealAnswer.FromDouble(assignmentDouble.Value);
                    }
                    break;
                case QuestionType.QRBarcode:
                    answer.Answer = QRBarcodeAnswer.FromString(((AssignmentTextAnswer) value)?.Value);
                    break;
                case QuestionType.Text:
                    answer.Answer = TextAnswer.FromString(((AssignmentTextAnswer) value)?.Value);
                    break;
                case QuestionType.TextList:
                {
                    // magic for text list question only
                    if(isRosterSizeQuestion)
                        answer.Answer = TextAnswer.FromString(((AssignmentTextAnswer)value)?.Value);
                    // ------------------------------------------------------------------------------
                    else
                    {
                        var textListAnswers = ((AssignmentMultiAnswer) value)?.Values
                            ?.OfType<AssignmentTextAnswer>()
                            ?.Where(x => !string.IsNullOrWhiteSpace(x.Value))
                            ?.Select(x => new Tuple<decimal, string>(Convert.ToDecimal(x.VariableName), x.Value))
                            ?.OrderBy(x => x.Item1)
                            ?.ToArray();

                        answer.Answer = TextListAnswer.FromTupleArray(textListAnswers);
                    }
                }
                    break;
            }

            return answer;
        }
    }
}
