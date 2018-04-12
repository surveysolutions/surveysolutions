using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Main.Core.Entities.SubEntities;
using Npgsql;
using NpgsqlTypes;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using AssignmentRow = WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier.AssignmentRow;

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
        private readonly IPlainStorageAccessor<AssignmentsImportProcess> importAssignmentsProcessRepository;
        private readonly IPlainStorageAccessor<AssignmentToImport> importAssignmentsRepository;
        private readonly IInterviewAnswerSerializer serializer;
        private readonly IInterviewTreeBuilder interviewTreeBuilder;
        private readonly IInterviewCreatorFromAssignment interviewCreatorFromAssignment;
        private readonly IPlainStorageAccessor<Assignment> assignmentsStorage;

        public AssignmentsImportService(ICsvReader csvReader, IArchiveUtils archiveUtils, 
            IQuestionnaireStorage questionnaireStorage, IUserViewFactory userViewFactory, 
            IFileSystemAccessor fileSystem,
            IPreloadedDataVerifier verifier,
            IAuthorizedUser authorizedUser,
            ISessionProvider sessionProvider,
            IPlainStorageAccessor<AssignmentsImportProcess> importAssignmentsProcessRepository,
            IPlainStorageAccessor<AssignmentToImport> importAssignmentsRepository,
            IInterviewAnswerSerializer serializer,
            IInterviewTreeBuilder interviewTreeBuilder,
            IInterviewCreatorFromAssignment interviewCreatorFromAssignment,
            IPlainStorageAccessor<Assignment> assignmentsStorage)
        {
            this.csvReader = csvReader;
            this.archiveUtils = archiveUtils;
            this.questionnaireStorage = questionnaireStorage;
            this.userViewFactory = userViewFactory;
            this.fileSystem = fileSystem;
            this.verifier = verifier;
            this.authorizedUser = authorizedUser;
            this.sessionProvider = sessionProvider;
            this.importAssignmentsProcessRepository = importAssignmentsProcessRepository;
            this.importAssignmentsRepository = importAssignmentsRepository;
            this.serializer = serializer;
            this.interviewTreeBuilder = interviewTreeBuilder;
            this.interviewCreatorFromAssignment = interviewCreatorFromAssignment;
            this.assignmentsStorage = assignmentsStorage;
        }

        public IEnumerable<PanelImportVerificationError> VerifySimple(PreloadedFile file, QuestionnaireIdentity questionnaireIdentity)
        {
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

            this.Save(file.FileInfo.FileName, questionnaireIdentity,
                assignmentRows.Select(row =>
                        ToAssignmentToImport(row.Answers.OfType<AssignmentAnswer>(), row.Responsible, row.Quantity, RosterVector.Empty, questionnaire))
                    .ToArray());

            this.SetVerifiedToAllImportedAssignments();
        }

        public IEnumerable<PanelImportVerificationError> VerifyPanel(string originalFileName,
            PreloadedFile[] allImportedFiles,
            QuestionnaireIdentity questionnaireIdentity)
        {
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
            {
                foreach (var assignmentRow in this.GetAssignmentRows(importedFile, questionnaire))
                {
                    foreach (var answerError in this.verifier.VerifyAnswers(assignmentRow, questionnaire))
                    {
                        hasErrors = true;
                        yield return answerError;
                    }

                    assignmentRows.Add(assignmentRow);
                }

                // hack for answers by roster size questions
                //var rosterName = importedFile.FileInfo.QuestionnaireOrRosterName;
                //var rosterId = questionnaire.GetRosterIdByVariableName(rosterName);
                //if (rosterId.HasValue)
                //{
                //    var rosterSizeQuestionId = questionnaire.GetRosterSizeQuestion(rosterId.Value);
                //    var rosterSizeQuestionName = questionnaire.GetQuestionVariableName(rosterSizeQuestionId);
                //    var questionType = questionnaire.GetQuestionType(rosterSizeQuestionId);

                //    foreach (var interviewRows in assignmentRows.GroupBy(x => x.InterviewIdValue.Value))
                //    {
                //        foreach (var rosterInstanceRows in interviewRows.GroupBy(x =>new RosterVector(x.RosterInstanceCodes.Where(_ => _.VariableName != rosterName).Select(_=>_.Code.Value).ToArray())))
                //        {
                //            var rosterSizeRow = new AssignmentRow
                //            {
                //                InterviewIdValue = new AssignmentInterviewId { Value = interviewRows.Key },
                //                QuestionnaireOrRosterName = rosterName,
                //                FileName = importedFile.FileInfo.FileName,
                //                RosterInstanceCodes = rosterInstanceRows.Key.Select(x=>new )
                //            }
                //            switch (questionType)
                //            {
                //                case QuestionType.MultyOption:
                //                    break;
                //                case QuestionType.Numeric:
                            
                //                    .Select(x => new AssignmentRow
                //                    {
                //                        InterviewIdValue = new AssignmentInterviewId { Value = x.Key },
                //                        QuestionnaireOrRosterName = rosterName,
                //                        FileName = importedFile.FileInfo.FileName,
                //                        RosterInstanceCodes = x.
                //                            Answers = new AssignmentValue[]
                //                        {
                //                            new AssignmentIntegerAnswer
                //                            {
                //                                Answer = x.SelectMany(_ => _.RosterInstanceCodes)
                //                                    .Where(_ => _.VariableName == rosterName).Select(_ => _.Code.Value)
                //                                    .Distinct()
                //                                    .Count(),
                //                                VariableName = rosterSizeQuestionName
                //                            }
                //                        }
                //                    });

                                    
                //                    break;
                //                case QuestionType.TextList:
                //                    break;

                //            }
                //            assignmentRows.Add();

                //        }
                //    }
                //}
            }

            if (hasErrors) yield break;

            foreach (var rosterError in this.verifier.VerifyRosters(assignmentRows, questionnaire))
            {
                hasErrors = true;
                yield return rosterError;
            }

            if (hasErrors) yield break;

            this.Save(originalFileName, questionnaireIdentity, ConcatRosters(assignmentRows, questionnaire));
        }

        private List<AssignmentToImport> ConcatRosters(List<AssignmentRow> assignmentRows,
            IQuestionnaire questionnaire)
        {
            var rowsGroupedByInterviews = assignmentRows.GroupBy(assignmentRow => assignmentRow.InterviewIdValue.Value)
                .Select(g => new
                {
                    quantity = g.Select(_ => _.Quantity).FirstOrDefault(_ => _ != null),
                    responsible = g.Select(_ => _.Responsible).FirstOrDefault(_ => _ != null),
                    rosters = g.Select(x => new
                    {
                        rosterVector = new RosterVector(x.RosterInstanceCodes.Select(y => y.Code.Value).ToArray()),
                        answers = x.Answers.OfType<AssignmentAnswer>()
                    })
                });
            
            return rowsGroupedByInterviews.SelectMany(z => z.rosters.GroupBy(x=>x.rosterVector).Select(_=> ToAssignmentToImport(
                    _.SelectMany(i=>i.answers), z.responsible, z.quantity, _.Key, questionnaire))).ToList();

            //return AddRosterSizeAnswersByRosterInstances(data, questionnaire);
        }

        //private List<AssignmentImportData> AddRosterSizeAnswersByRosterInstances(
        //    List<AssignmentImportData> assignmentImportData, IQuestionnaire questionnaire)
        //{
        //    var allAnsweredIdenities = assignmentImportData
        //        .SelectMany(x => x.PreloadedData.Answers.Select(y => y.Identity)).ToArray();

        //    var rosterSizeQuestions = questionnaire.GetAllRosterSizeQuestions();

        //    foreach (var rosterSizeQuestion in rosterSizeQuestions)
        //    {
        //        var questionType = questionnaire.GetQuestionType(rosterSizeQuestion);

        //        switch (questionType)
        //        {
        //            case QuestionType.MultyOption:
        //                break;
        //            case QuestionType.Numeric:
        //                break;
        //            case QuestionType.TextList:
        //                var allTextListIdentities = allAnsweredIdenities.Where(x => x.Id == rosterSizeQuestion);

        //                break;
        //        }
        //    }
            
        //    return assignmentImportData;
        //}

        public AssignmentToImport GetAssignmentToImport()
            => this.importAssignmentsRepository.Query(x => x.FirstOrDefault(_ => _.Verified && _.Error == null));

        public AssignmentToImport GetAssignmentToVerify()
            => this.importAssignmentsRepository.Query(x => x.FirstOrDefault(_ => !_.Verified));

        public void RemoveImportedAssignment(int assignmentId)
            => this.sessionProvider.GetSession().Connection
                .Execute($"DELETE FROM {AssignmentsToImportTableName} where id=@id", assignmentId);

        public AssignmentsImportStatus GetImportStatus()
        {
            var process = this.importAssignmentsProcessRepository.Query(x => x.FirstOrDefault());
            if (process == null) return null;

            var assignmentsInQueue = this.importAssignmentsRepository.Query(x => x.Count(_ => _.Verified && _.Error == null));
            var assignmentsWithErrors = this.importAssignmentsRepository.Query(x => x.Count(_ => _.Verified && _.Error != null));

            var elapsedTimeInMilliseconds = (DateTime.UtcNow - process.StartedDate).Milliseconds;
            var processedInterviews = process.TotalCount - assignmentsInQueue;
            var perInterviewMilliseconds = elapsedTimeInMilliseconds / processedInterviews;

            return new AssignmentsImportStatus
            {
                IsOwnerOfRunningProcess = process.Responsible == this.authorizedUser.UserName,
                IsInProgress = assignmentsInQueue > 0,
                AssignedToInterviewersCount = process.AssignedToInterviewersCount,
                AssignedToSupervisorsCount = process.AssignedToSupervisorsCount,
                TotalAssignments = process.TotalCount,
                AssignmentsInQueue = assignmentsInQueue,
                ProcessedCount = process.TotalCount - assignmentsInQueue,
                FileName = process.FileName,
                StartedDate = process.StartedDate,
                ResponsibleName = process.Responsible,
                QuestionnaireIdentity = QuestionnaireIdentity.Parse(process.QuestionnaireId),
                AssingmentsWithErrors = assignmentsWithErrors,
                EstimatedTime = TimeSpan.FromMilliseconds(perInterviewMilliseconds * assignmentsInQueue).ToString(@"dd\.hh\:mm\:ss"),
                ElapsedTime = TimeSpan.FromMilliseconds(elapsedTimeInMilliseconds).ToString(@"dd\.hh\:mm\:ss"),
            };
        }

        public void RemoveAllAssignmentsToImport()
        => this.sessionProvider.GetSession().Connection.Execute(
                $"DELETE FROM {AssignmentsToImportTableName};" +
                $"DELETE FROM {AssignmentsImportProcessTableName};");

        public void SetResponsibleToAllImportedAssignments(Guid responsibleId)
        {
            var responsible = this.userViewFactory.GetUser(new UserViewInputModel(responsibleId));

            this.sessionProvider.GetSession().Connection
                .Execute($"UPDATE {AssignmentsToImportTableName} SET interviewer=@interviewer, supervisor=@supervisor",
                    new
                    {
                        interviewer = responsible.IsInterviewer() ? responsible.PublicKey : (Guid?) null,
                        supervisor = responsible.IsSupervisor() ? responsible.PublicKey : (Guid?) null
                    });
        }

        public IEnumerable<string> GetImportAssignmentsErrors()
            => this.importAssignmentsRepository.Query(x => x.Where(_ => _.Error != null).Select(_ => _.Error));

        public void VerifyAssignment(AssignmentToImport assignment, QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            var answersGroupedByLevels = assignment.Answers.GroupedByLevels();

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
                            continue;

                        interviewTreeQuestion.SetAnswer(answer.Answer);

                        interviewTreeQuestion.RunImportInvariantsOrThrow(new InterviewQuestionInvariants(answer.Identity, questionnaire, tree));
                    }
                    tree.ActualizeTree();
                }

                this.SetVerifiedToAssignment(assignment.Id);
            }
            catch (Exception ex)
            {
                var allAnswersInString = string.Join(", ", answersGroupedByLevels.Where(x => x != null));
                var errorMessage = string.Format(Interviews.ImportInterviews_GenericError, allAnswersInString,
                    assignment.Interviewer, questionnaireIdentity, ex.Message);

                this.SetVerifiedToAssignment(assignment.Id, errorMessage);
            }
        }

        public void ImportAssignment(AssignmentToImport assignmentToImport, QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);
            
            var responsibleId = assignmentToImport.Interviewer ?? assignmentToImport.Supervisor.Value;
            var identifyingQuestionIds = questionnaire.GetPrefilledQuestions().ToHashSet();

            var assignment = new Assignment(questionnaireIdentity, responsibleId, assignmentToImport.Quantity);
            var identifyingAnswers = assignmentToImport.Answers
                .Where(x => identifyingQuestionIds.Contains(x.Identity.Id)).Select(a =>
                    IdentifyingAnswer.Create(assignment, questionnaire, a.Answer.ToString(), a.Identity))
                .ToList();

            assignment.SetIdentifyingData(identifyingAnswers);
            assignment.SetAnswers(assignment.Answers);

            this.assignmentsStorage.Store(assignment, Guid.NewGuid());

            this.interviewCreatorFromAssignment.CreateInterviewIfQuestionnaireIsOld(responsibleId,
                questionnaireIdentity, assignment.Id, assignmentToImport.Answers);

            this.RemoveImportedAssignment(assignment.Id);
        }

        private void SetVerifiedToAssignment(int assignmentId, string errorMessage = null)
            => this.sessionProvider.GetSession().Connection.Execute(
                $"UPDATE {AssignmentsToImportTableName} SET verified=true, error=@error WHERE id=@id",
                new { id = assignmentId, error = errorMessage });

        private void SetVerifiedToAllImportedAssignments()
            => this.sessionProvider.GetSession().Connection.Execute($"UPDATE {AssignmentsToImportTableName} SET verified=true");

        private void Save(string fileName, QuestionnaireIdentity questionnaireIdentity, IList<AssignmentToImport> assignments)
        {
            this.SaveProcess(fileName, questionnaireIdentity, assignments);
            this.SaveAssignments(assignments);
        }

        private void SaveProcess(string fileName, QuestionnaireIdentity questionnaireIdentity, IList<AssignmentToImport> assignments)
        {
            var process = this.importAssignmentsProcessRepository.Query(x => x.FirstOrDefault()) ??
                          new AssignmentsImportProcess();

            process.FileName = fileName;
            process.AssignedToInterviewersCount = assignments.Count(x => x.Interviewer.HasValue);
            process.AssignedToSupervisorsCount = assignments.Count(x => x.Supervisor.HasValue);
            process.TotalCount = assignments.Count;
            process.Responsible = this.authorizedUser.UserName;
            process.StartedDate = DateTime.UtcNow;
            process.QuestionnaireId = questionnaireIdentity.ToString();

            this.importAssignmentsProcessRepository.Store(process, process?.Id);
        }

        private void SaveAssignments(IList<AssignmentToImport> assignments)
        {
            var npgsqlConnection = this.sessionProvider.GetSession().Connection as NpgsqlConnection;

            using (var writer = npgsqlConnection.BeginBinaryImport($"COPY  {AssignmentsToImportTableName} (interviewer, supervisor, quantity, answers) " +
                                                                   "FROM STDIN BINARY;"))
            {
                foreach (var assignmentToImport in assignments)
                {
                    writer.StartRow();
                    writer.Write(assignmentToImport.Interviewer, NpgsqlDbType.Uuid);
                    writer.Write(assignmentToImport.Supervisor, NpgsqlDbType.Uuid);
                    writer.Write(assignmentToImport.Quantity, NpgsqlDbType.Integer);
                    writer.Write(this.serializer.Serialize(assignmentToImport.Answers), NpgsqlDbType.Jsonb);
                }
            }
        }

        private AssignmentToImport ToAssignmentToImport(IEnumerable<AssignmentAnswer> answers, AssignmentResponsible responsible,
            AssignmentQuantity quantity, RosterVector rosterVector, IQuestionnaire questionnaire) =>
            new AssignmentToImport
            {
                Interviewer = responsible?.Responsible?.InterviewerId,
                Supervisor = responsible?.Responsible?.SupervisorId,
                Quantity = quantity?.Quantity,
                Answers = answers.Select(x => ToInterviewAnswer(x, rosterVector, questionnaire)).Where(x => x != null).ToList()
            };

        private InterviewAnswer ToInterviewAnswer(AssignmentAnswer value, RosterVector rosterVector, IQuestionnaire questionnaire)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(value.VariableName);
            if (!questionId.HasValue) return null;

            var questionType = questionnaire.GetQuestionType(questionId.Value);
            
            var answer = new InterviewAnswer { Identity = Identity.Create(questionId.Value, rosterVector) };

            var isLinkedToQuestion = questionnaire.IsQuestionLinked(questionId.Value);
            var isLinkedToRoster = questionnaire.IsQuestionLinkedToRoster(questionId.Value);

            var isRosterSizeQuestion = questionnaire.IsRosterSizeQuestion(questionId.Value);
            // magic for text list question only
            if (isRosterSizeQuestion && value is AssignmentTextAnswer)
                //answer.Answer = TextAnswer.FromString(((AssignmentTextAnswer)value)?.Value);
                return null;
            //------------------------------------------------------------------------------

            switch (questionType)
            {
                case QuestionType.SingleOption:
                    if (!isLinkedToQuestion && !isLinkedToRoster)
                    {
                        var assignmentInt = ((AssignmentDoubleAnswer)value).Answer;
                        if (assignmentInt.HasValue)
                            answer.Answer = CategoricalFixedSingleOptionAnswer.FromDecimal(Convert.ToDecimal(assignmentInt.Value));
                    }
                    break;
                case QuestionType.MultyOption:
                    {
                        var assignmentCategoricalMulti = ((AssignmentMultiAnswer)value)?.Values
                            ?.OfType<AssignmentDoubleAnswer>()
                            .Where(x => x.Answer.HasValue)
                            ?.Select(x => new { code = Convert.ToDecimal(x.VariableName), answer = Convert.ToInt32(x.Answer) })
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
                    var assignmentDateTime = ((AssignmentDateTimeAnswer)value).Answer;
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
                    answer.Answer = QRBarcodeAnswer.FromString(((AssignmentTextAnswer)value)?.Value);
                    break;
                case QuestionType.Text:
                    answer.Answer = TextAnswer.FromString(((AssignmentTextAnswer)value)?.Value);
                    break;
                case QuestionType.TextList:
                    {
                        var textListAnswers = ((AssignmentMultiAnswer)value)?.Values
                            ?.OfType<AssignmentTextAnswer>()
                            ?.Where(x => !string.IsNullOrWhiteSpace(x.Value))
                            ?.Select(x => new Tuple<decimal, string>(Convert.ToDecimal(x.VariableName), x.Value))
                            ?.OrderBy(x => x.Item1)
                            ?.ToArray();

                        answer.Answer = TextListAnswer.FromTupleArray(textListAnswers);

                    }
                    break;
                default: return null;
            }

            return answer;
        }
    }
}
