using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public class AssignmentsImportService : IAssignmentsImportService
    {
        private readonly IUserViewFactory userViewFactory;
        private readonly IPreloadedDataVerifier verifier;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IPlainSessionProvider sessionProvider;
        private readonly IPlainStorageAccessor<AssignmentsImportProcess> importAssignmentsProcessRepository;
        private readonly IPlainStorageAccessor<AssignmentToImport> importAssignmentsRepository;
        private readonly IInterviewCreatorFromAssignment interviewCreatorFromAssignment;
        private readonly IPlainStorageAccessor<Assignment> assignmentsStorage;
        private readonly IAssignmentsImportFileConverter assignmentsImportFileConverter;

        public AssignmentsImportService(IUserViewFactory userViewFactory,
            IPreloadedDataVerifier verifier,
            IAuthorizedUser authorizedUser,
            IPlainSessionProvider sessionProvider,
            IPlainStorageAccessor<AssignmentsImportProcess> importAssignmentsProcessRepository,
            IPlainStorageAccessor<AssignmentToImport> importAssignmentsRepository,
            IInterviewCreatorFromAssignment interviewCreatorFromAssignment,
            IPlainStorageAccessor<Assignment> assignmentsStorage,
            IAssignmentsImportFileConverter assignmentsImportFileConverter)
        {
            this.userViewFactory = userViewFactory;
            this.verifier = verifier;
            this.authorizedUser = authorizedUser;
            this.sessionProvider = sessionProvider;
            this.importAssignmentsProcessRepository = importAssignmentsProcessRepository;
            this.importAssignmentsRepository = importAssignmentsRepository;
            this.interviewCreatorFromAssignment = interviewCreatorFromAssignment;
            this.assignmentsStorage = assignmentsStorage;
            this.assignmentsImportFileConverter = assignmentsImportFileConverter;
        }

        public IEnumerable<PanelImportVerificationError> VerifySimple(PreloadedFile file, IQuestionnaire questionnaire)
        {
            bool hasErrors = false;

            var assignmentRows = new List<PreloadingAssignmentRow>();

            foreach (var assignmentRow in this.assignmentsImportFileConverter.GetAssignmentRows(file, questionnaire))
            {
                foreach (var answerError in this.verifier.VerifyAnswers(assignmentRow, questionnaire))
                {
                    hasErrors = true;
                    yield return answerError;
                }

                assignmentRows.Add(assignmentRow);
            }

            if (hasErrors) yield break;

            this.Save(file.FileInfo.FileName, new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version), 
                assignmentRows.Select(row =>
                        ToAssignmentToImport(new[] {(RosterVector.Empty, row.Answers.OfType<AssignmentAnswer>())},
                            row.Responsible, row.Quantity, questionnaire, true))
                    .ToArray());
        }

        public IEnumerable<PanelImportVerificationError> VerifyPanel(string originalFileName,
            PreloadedFile[] allImportedFiles,
            IQuestionnaire questionnaire)
        {
            bool hasErrors = false;

            var assignmentRows = new List<PreloadingAssignmentRow>();

            foreach (var importedFile in allImportedFiles)
            {
                foreach (var assignmentRow in this.assignmentsImportFileConverter.GetAssignmentRows(importedFile, questionnaire))
                {
                    foreach (var answerError in this.verifier.VerifyAnswers(assignmentRow, questionnaire))
                    {
                        hasErrors = true;
                        yield return answerError;
                    }

                    assignmentRows.Add(assignmentRow);
                }
            }

            if (hasErrors) yield break;

            foreach (var rosterError in this.verifier.VerifyRosters(assignmentRows, questionnaire))
            {
                hasErrors = true;
                yield return rosterError;
            }

            if (hasErrors) yield break;

            this.Save(originalFileName, new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version), ConcatRosters(assignmentRows, questionnaire));
        }

        private List<AssignmentToImport> ConcatRosters(List<PreloadingAssignmentRow> assignmentRows,
            IQuestionnaire questionnaire)
        {
            var rowsGroupedByInterviews = assignmentRows.GroupBy(assignmentRow => assignmentRow.InterviewIdValue.Value)
                .Select(g => new
                {
                    assignmentId = g.Key,
                    quantity = g.Select(_ => _.Quantity).FirstOrDefault(_ => _ != null),
                    responsible = g.Select(_ => _.Responsible).FirstOrDefault(_ => _ != null),
                    rosters = g.Select(x => new
                    {
                        rosterVector = new RosterVector(x.RosterInstanceCodes.Select(y => y.Code.Value).ToArray()),
                        answers = x.Answers.OfType<AssignmentAnswer>()
                    })
                });
            
            return rowsGroupedByInterviews.Select(_=> ToAssignmentToImport(
                    _.rosters.Select(x=> (x.rosterVector, x.answers)), _.responsible, _.quantity, questionnaire, false)).ToList();
        }

        public AssignmentToImport GetAssignmentById(int assignmentId)
            => this.importAssignmentsRepository.Query(x => x.FirstOrDefault(_ => _.Id == assignmentId));

        public int[] GetAllAssignmentIdsToVerify()
            => this.importAssignmentsRepository.Query(x => x.Where(_ => !_.Verified).Select(_ => _.Id).ToArray());

        public int[] GetAllAssignmentIdsToImport()
            => this.importAssignmentsRepository.Query(x => x.Where(_ => _.Verified && _.Error == null).Select(_ => _.Id).ToArray());

        public AssignmentsImportStatus GetImportStatus()
        {
            var process = this.importAssignmentsProcessRepository.Query(x => x.FirstOrDefault());
            if (process == null) return null;

            if (!this.importAssignmentsRepository.Query(x => x.Any())) return null;

            var statistics = this.importAssignmentsRepository.Query(x =>
                x.Select(_ =>
                        new
                        {
                            Total = 1,
                            Verified = _.Verified ? 1 : 0,
                            VerifiedWithoutError = _.Verified && _.Error == null ? 1 : 0,
                            HasError = _.Error != null ? 1 : 0,
                            AssignedToInterviewer = _.Interviewer != null ? 1 : 0,
                            AssignedToSupervisor = _.Interviewer == null && _.Supervisor != null ? 1 : 0
                        })
                    .GroupBy(_ => 1)
                    .Select(_ => new
                    {
                        Total = _.Sum(y => y.Total),
                        WithErrors = _.Sum(y => y.HasError),
                        Verified = _.Sum(y => y.Verified),
                        AssignedToInterviewers = _.Sum(y => y.AssignedToInterviewer),
                        AssignedToSupervisors = _.Sum(y => y.AssignedToSupervisor),
                    })
                    .FirstOrDefault());

            return new AssignmentsImportStatus
            {
                IsOwnerOfRunningProcess = process.Responsible == this.authorizedUser.UserName,
                AssignedToInterviewersCount = statistics.AssignedToInterviewers,
                AssignedToSupervisorsCount = statistics.AssignedToSupervisors,
                TotalCount = process.TotalCount,
                InQueueCount = statistics.Total,
                ProcessedCount = process.TotalCount - statistics.Total,
                FileName = process.FileName,
                StartedDate = process.StartedDate,
                ResponsibleName = process.Responsible,
                QuestionnaireIdentity = QuestionnaireIdentity.Parse(process.QuestionnaireId),
                WithErrorsCount = statistics.WithErrors,
                VerifiedCount = statistics.Verified
            };
        }

        public void RemoveAllAssignmentsToImport()
        {
            this.sessionProvider.GetSession().Query<AssignmentToImport>().Delete();
            this.sessionProvider.GetSession().Query<AssignmentsImportProcess>().Delete();
        }

        public void SetResponsibleToAllImportedAssignments(Guid responsibleId)
        {
            var responsible = this.userViewFactory.GetUser(new UserViewInputModel(responsibleId));

            this.sessionProvider.GetSession().Query<AssignmentToImport>()
                .UpdateBuilder()
                .Set(c => c.Interviewer, c => responsible.IsInterviewer() ? responsible.PublicKey : (Guid?) null)
                .Set(c => c.Supervisor, c => responsible.IsInterviewer() ? responsible.Supervisor.Id : responsible.PublicKey)
                .Update();
        }

        public IEnumerable<string> GetImportAssignmentsErrors()
            => this.importAssignmentsRepository.Query(x => x.Where(_ => _.Error != null).Select(_ => _.Error));

        public void ImportAssignment(int assignmentId, IQuestionnaire questionnaire)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version);
            var assignmentToImport = this.GetAssignmentById(assignmentId);

            var responsibleId = assignmentToImport.Interviewer ?? assignmentToImport.Supervisor.Value;
            var identifyingQuestionIds = questionnaire.GetPrefilledQuestions().ToHashSet();

            var assignment = new Assignment(questionnaireIdentity, responsibleId, assignmentToImport.Quantity);
            var identifyingAnswers = assignmentToImport.Answers
                .Where(x => identifyingQuestionIds.Contains(x.Identity.Id)).Select(a =>
                    IdentifyingAnswer.Create(assignment, questionnaire, a.Answer.ToString(), a.Identity))
                .ToList();

            assignment.SetIdentifyingData(identifyingAnswers);
            assignment.SetAnswers(assignment.Answers);

            this.assignmentsStorage.Store(assignment, null);

            this.interviewCreatorFromAssignment.CreateInterviewIfQuestionnaireIsOld(responsibleId,
                questionnaireIdentity, assignment.Id, assignmentToImport.Answers);
        }

        public void SetVerifiedToAssignment(int assignmentId, string errorMessage = null)
            => this.sessionProvider.GetSession().Query<AssignmentToImport>()
                .Where(c => c.Id == assignmentId)
                .UpdateBuilder()
                .Set(c => c.Verified, c => true)
                .Set(c => c.Error, c => errorMessage)
                .Update();

        public void RemoveAssignmentToImport(int assignmentId)
            => this.importAssignmentsRepository.Remove(assignmentId);

        private void Save(string fileName, QuestionnaireIdentity questionnaireIdentity, IList<AssignmentToImport> assignments)
        {
            this.RemoveAllAssignmentsToImport();

            this.SaveProcess(fileName, questionnaireIdentity, assignments);
            this.SaveAssignments(assignments);
        }

        private void SaveProcess(string fileName, QuestionnaireIdentity questionnaireIdentity, IList<AssignmentToImport> assignments)
        {
            var process = this.importAssignmentsProcessRepository.Query(x => x.FirstOrDefault()) ??
                          new AssignmentsImportProcess();

            process.FileName = fileName;
            process.TotalCount = assignments.Count;
            process.Responsible = this.authorizedUser.UserName;
            process.StartedDate = DateTime.UtcNow;
            process.QuestionnaireId = questionnaireIdentity.ToString();

            this.importAssignmentsProcessRepository.Store(process, process.Id);
        }

        private void SaveAssignments(IList<AssignmentToImport> assignments)
        {
            this.importAssignmentsRepository.Store(assignments.Select(x =>
                new Tuple<AssignmentToImport, object>(x, x.Id)));
        }

        private AssignmentToImport ToAssignmentToImport(
            IEnumerable<(RosterVector rosterVector, IEnumerable<AssignmentAnswer> answers)> answers,
            AssignmentResponsible responsible, AssignmentQuantity quantity, IQuestionnaire questionnaire, bool verified) =>
            new AssignmentToImport
            {
                Interviewer = responsible?.Responsible?.InterviewerId,
                Supervisor = responsible?.Responsible?.SupervisorId,
                Quantity = quantity?.Quantity,
                Verified = verified,
                Answers = answers
                    .SelectMany(x => x.answers.Select(y => ToInterviewAnswer(y, x.rosterVector, questionnaire)))
                    .Where(y => y?.Answer != null)
                    .ToList()
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
                            ?.Select(x => new { code = Convert.ToInt32(x.VariableName), answer = Convert.ToInt32(x.Answer) })
                            .ToArray();

                        if (assignmentCategoricalMulti.Length > 0)
                        {
                            if (questionnaire.ShouldQuestionRecordAnswersOrder(questionId.Value))
                            {
                                assignmentCategoricalMulti = assignmentCategoricalMulti.Where(x => x.answer > 0).ToArray();

                                if (questionnaire.IsQuestionYesNo(questionId.Value))
                                {
                                    answer.Answer = YesNoAnswer.FromAnsweredYesNoOptions(assignmentCategoricalMulti
                                        .OrderBy(x => x.answer)
                                        .Select(x => new AnsweredYesNoOption(x.code, true)));
                                }
                                else if (!isLinkedToQuestion && !isLinkedToRoster)
                                {
                                    answer.Answer = CategoricalFixedMultiOptionAnswer.FromIntArray(
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
                                else if (!isLinkedToQuestion && !isLinkedToRoster)
                                {
                                    answer.Answer = CategoricalFixedMultiOptionAnswer.FromIntArray(
                                        assignmentCategoricalMulti.Where(x => x.answer == 1).Select(x => x.code).ToArray());
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
